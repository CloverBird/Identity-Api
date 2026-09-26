using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Guildly.Identity.Api.DocumentTransformers;

public class JwtBearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

    public JwtBearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    {
        _authenticationSchemeProvider = authenticationSchemeProvider;
    }

    public async Task TransformAsync(OpenApiDocument document, 
                                     OpenApiDocumentTransformerContext context,
                                     CancellationToken cancellationToken)
    {
        var authenticationSchemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(s => s.Name == JwtBearerDefaults.AuthenticationScheme))
        {
            document.Components ??= new OpenApiComponents();

            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>()
            {
                [JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
                {
                    Name = JwtBearerDefaults.AuthenticationScheme,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme.ToLowerInvariant(),
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                } 
            };
            
            document.Security ??= new List<OpenApiSecurityRequirement>();
            document.Security.Add(new OpenApiSecurityRequirement()
            {
                [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
            });
        }
    }
}