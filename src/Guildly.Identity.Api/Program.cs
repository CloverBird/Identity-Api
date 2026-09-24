using Guildly.Common.Database.Configurations;
using Guildly.Identity.Api.Configurations;
using Guildly.Identity.Api.DocumentTransformers;
using Guildly.Identity.Api.Endpoints;
using Guildly.Identity.Api.Extensions;
using Guildly.Identity.Application.Configurations;
using Guildly.Identity.Application.Extensions;
using Guildly.Identity.Infrastructure.Configurations;
using Guildly.Identity.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

var databaseConfiguration = builder.Configuration.GetRequiredSection("Database").Get<DatabaseConfiguration>();
builder.Services.Configure<VerificationSessionConfiguration>(builder.Configuration.GetSection("VerificationSession"));
builder.Services.Configure<VerificationCodeConfiguration>(builder.Configuration.GetSection("VerificationCode"));
builder.Services.Configure<RefreshTokenConfiguration>(builder.Configuration.GetSection("RefreshToken"));
        
var jwtConfiguration = builder.Configuration.GetSection("Jwt").Get<JwtConfiguration>();
builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<JwtBearerSecuritySchemeTransformer>());

builder.Services.AddApiServices()
       .AddApplicationServices()
       .AddInfrastructureServices(databaseConfiguration!);

builder.Services.AddAuth(jwtConfiguration!);

var app = builder.Build();

app.UseOpenApi(builder.Configuration.GetSection("OpenApi").Get<OpenApiConfiguration>()!);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityEndpoints();

app.Run();
