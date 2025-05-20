using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Core;
using DerpCode.API.Models.Settings;
using DerpCode.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;

public static class AuthenticationServiceCollectionExtensions
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.Configure<AuthenticationSettings>(config.GetSection(ConfigurationKeys.Authentication));

        var authSettings = config.GetSection(ConfigurationKeys.Authentication).Get<AuthenticationSettings>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Set token validation options. These will be used when validating all tokens.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.APISecret)),
                    RequireSignedTokens = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ValidAudience = authSettings.TokenAudience,
                    ValidIssuers = [authSettings.TokenIssuer]
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var errorMessage = string.IsNullOrWhiteSpace(context.ErrorDescription) ? context.Error : $"{context.Error}. {context.ErrorDescription}.";

                        var problem = new ProblemDetailsWithErrors(errorMessage ?? "Invalid token", StatusCodes.Status401Unauthorized, context.Request);

                        return context.Response.WriteAsync(JsonSerializer.Serialize(problem, jsonOptions));
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var problem = new ProblemDetailsWithErrors("Forbidden", StatusCodes.Status403Forbidden, context.Request);

                        return context.Response.WriteAsync(JsonSerializer.Serialize(problem, jsonOptions));
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Add(AppHeaderNames.TokenExpired, "true");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicyName.RequireAdminRole, policy => policy.RequireRole(UserRoleName.Admin));
            options.AddPolicy(AuthorizationPolicyName.RequireUserRole, policy => policy.RequireRole(UserRoleName.User));
        });

        return services;
    }
}