using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DerpCode.API.Constants;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        var settingsSection = config.GetSection(ConfigurationKeys.Swagger);
        var settings = settingsSection.Get<SwaggerSettings>() ?? throw new InvalidOperationException($"Missing {ConfigurationKeys.Swagger} section in configuration.");

        services.Configure<SwaggerSettings>(settingsSection);

        services.AddSwaggerGen(options =>
        {
            foreach (var version in settings.SupportedApiVersions)
            {
                options.SwaggerDoc(
                    version,
                    new OpenApiInfo
                    {
                        Version = version,
                        Title = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName,
                        Description = $"{FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName} - {config.GetEnvironment()} ({Assembly.GetExecutingAssembly().GetName().Version})"
                    });
            }

            // Remove version parameter from ui
            options.OperationFilter<RemoveVersionParameterFilter>();
            // Replace {version} with actual version in routes in swagger doc
            options.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();
            options.CustomSchemaIds(id => id.FullName);

            // Add the security token option to swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }, new List<string>()
                    }
            });

            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            options.SwaggerGeneratorOptions.DescribeAllParametersInCamelCase = true;
        });

        return services;
    }

    private sealed class RemoveVersionParameterFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var versionParameter = operation.Parameters.FirstOrDefault(p => p.Name == "version");

            if (versionParameter == null)
            {
                return;
            }

            operation.Parameters.Remove(versionParameter);
        }
    }

    private sealed class ReplaceVersionWithExactValueInPathFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = new OpenApiPaths();
            foreach (var path in swaggerDoc.Paths)
            {
                paths.Add(path.Key.Replace("v{version}", swaggerDoc.Info.Version, StringComparison.Ordinal), path.Value);
            }
            swaggerDoc.Paths = paths;
        }
    }
}