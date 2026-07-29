using System.Net.Http.Headers;
using System.Net.Http.Json;

using MechanicShop.Infrastructure.Identity;

namespace MechanicShop.Api.IntegrationTests.Common;

public class AppHttpClient
{
    private readonly HttpClient _httpClient;

    public AppHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateTokenAsync(AppUser user)
    {
        var request = new GenerateTokenCommand(user.Email!, user.Email!);

        var response = await _httpClient.PostAsJsonAsync("identity/token/generate", request);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"Token generation failed. Status Code: {(int)response.StatusCode} ({response.StatusCode}){Environment.NewLine}" +
                $"Response:{Environment.NewLine}{responseBody}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>()
            ?? throw new InvalidOperationException("Token response is null.");

        return tokenResponse.AccessToken!;
    }

    public void SetAuthorizationHeader(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public Task<HttpResponseMessage> GetAsync(
        string requestUri,
        CancellationToken cancellationToken = default) =>
        _httpClient.GetAsync(requestUri, cancellationToken);

    public Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default) =>
        _httpClient.SendAsync(request, cancellationToken);

    public Task<HttpResponseMessage> PostAsJsonAsync<T>(
        string requestUri,
        T value,
        CancellationToken cancellationToken = default) =>
        _httpClient.PostAsJsonAsync(requestUri, value, cancellationToken);

    public Task<HttpResponseMessage> PutAsJsonAsync<T>(
        string requestUri,
        T value,
        CancellationToken cancellationToken = default) =>
        _httpClient.PutAsJsonAsync(requestUri, value, cancellationToken);

    public Task<HttpResponseMessage> DeleteAsync(
        string requestUri,
        CancellationToken cancellationToken = default) =>
        _httpClient.DeleteAsync(requestUri, cancellationToken);

    public Task<HttpResponseMessage> PatchAsJsonAsync<T>(
        string requestUri,
        T value,
        CancellationToken cancellationToken = default) =>
        _httpClient.PatchAsJsonAsync(requestUri, value, cancellationToken);

    public async Task<T?> GetFromJsonAsync<T>(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        var response = await GetAsync(requestUri, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    public async Task<TResponse?> PostAndGetFromJsonAsync<TRequest, TResponse>(
        string requestUri,
        TRequest value,
        CancellationToken cancellationToken = default)
    {
        var response = await PostAsJsonAsync(requestUri, value, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
