using Guildly.Identity.Api.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;

namespace Guildly.Identity.Api.Extensions;

public static class OpenApiExtensions
{
    public static WebApplication UseOpenApi(this WebApplication app, OpenApiConfiguration openApiConfiguration)
    {
        if (openApiConfiguration.EnableScalar)
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme);
                options.AddHttpAuthentication(JwtBearerDefaults.AuthenticationScheme, auth =>
                {
                    auth.Token = "header.payload.signature";
                }).EnablePersistentAuthentication();
            });
        }
    
        return app;
    }
}
