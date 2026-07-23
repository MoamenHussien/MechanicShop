using System.Net.Http.Headers;

namespace MechanicShop.Client.Identity;

public class BearerTokenHandler(IAccountManagement accountManagement) : DelegatingHandler
{
    private readonly IAccountManagement _accountManagement = accountManagement;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authResult = await _accountManagement.LoadAccessTokenFromStorage();

        if (authResult?.AccessToken is null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        var response = await base.SendAsync(request, cancellationToken);

        // Prevent infinite retries by checking if the request has already been retried
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !request.Headers.Contains("X-Retry"))
        {
            var newTokenResponse = await _accountManagement.RefreshTokenAsync();

            if (newTokenResponse is not null)
            {
                // Clone request asynchronously
                var newRequest = await CloneRequestAsync(request, cancellationToken);
                newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokenResponse.AccessToken);
                newRequest.Headers.Add("X-Retry", "true");

                // Free the original 401 response resource before retrying
                response.Dispose();

                return await base.SendAsync(newRequest, cancellationToken);
            }
        }

        return response;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var newRequest = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy // copy الـ VersionPolicy (.NET 5+)
        };

        // 1. copy الـ Options if exsits
        foreach (var option in request.Options)
        {
            newRequest.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        // Clone content asynchronously if present
        if (request.Content != null)
        {
            var memoryStream = new MemoryStream();
            await request.Content.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;
            newRequest.Content = new StreamContent(memoryStream);

            foreach (var header in request.Content.Headers)
            {
                newRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // 3. نسخ الـ Headers الأصلية للطلب
        foreach (var header in request.Headers)
        {
            newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return newRequest;
    }
}