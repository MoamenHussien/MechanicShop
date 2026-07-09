using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MechanicShop.Api.OpenApi.Transformers;

internal sealed class BearerSecuritySchemeTransformer
    : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Enter JWT Bearer token"
            };

        return Task.CompletedTask;
    }

    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var hasAuthorize = context.Description
            .ActionDescriptor
            .EndpointMetadata
            .OfType<IAuthorizeData>()
            .Any();

        var hasAllowAnonymous = context.Description
            .ActionDescriptor
            .EndpointMetadata
            .OfType<IAllowAnonymous>()
            .Any();

        if (hasAuthorize && !hasAllowAnonymous)
        {
            operation.Security ??= [];

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(
                    JwtBearerDefaults.AuthenticationScheme,
                    context.Document)] = []
            });
        }

        return Task.CompletedTask;
    }
}