using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;

public static class OpenApiServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApiServices(this IServiceCollection services, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        var settingsSection = config.GetSection(ConfigurationKeys.OpenApi);
        var settings = settingsSection.Get<OpenApiSettings>() ?? throw new InvalidOperationException($"Missing {ConfigurationKeys.OpenApi} section in configuration.");

        services.Configure<OpenApiSettings>(settingsSection);

        var productName = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName;
        var environment = config.GetEnvironment();
        var version = Assembly.GetExecutingAssembly().GetName().Version;

        foreach (var apiVersion in settings.SupportedApiVersions)
        {
            services.Configure<ScalarOptions>(options => options.AddDocument(apiVersion));

            services.AddOpenApi(apiVersion, options =>
            {
                // Add document info and version
                options.AddDocumentTransformer((document, context, _) =>
                {
                    document.Info = new OpenApiInfo
                    {
                        Version = apiVersion,
                        Title = productName,
                        Description = $"{productName} - {environment} ({version})"
                    };

                    return Task.CompletedTask;
                });

                // Add Bearer security scheme
                options.AddDocumentTransformer((document, _, _) =>
                {
                    var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                    {
                        [JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
                        {
                            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT"
                        }
                    };

                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes = securitySchemes;

                    // Add global security requirement to each operation
                    foreach (var pathItem in document.Paths.Values)
                    {
                        if (pathItem.Operations == null)
                        {
                            continue;
                        }

                        foreach (var operation in pathItem.Operations.Values)
                        {
                            operation.Security ??= [];
                            operation.Security.Add(new OpenApiSecurityRequirement
                            {
                                [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
                            });
                        }
                    }

                    return Task.CompletedTask;
                });

                // Remove version parameter and replace {version} in paths
                options.AddOperationTransformer((operation, context, _) =>
                {
                    // Remove version parameter from UI
                    var versionParameter = operation.Parameters?.FirstOrDefault(p => p.Name == "version");

                    if (versionParameter != null)
                    {
                        operation.Parameters?.Remove(versionParameter);
                    }

                    // Mark deprecated operations
                    operation.Deprecated = context.Description.IsDeprecated();

                    return Task.CompletedTask;
                });

                // Replace {version} with actual version in paths
                options.AddDocumentTransformer((document, _, _) =>
                {
                    var newPaths = new Dictionary<string, IOpenApiPathItem>();

                    foreach (var path in document.Paths)
                    {
                        newPaths.Add(path.Key.Replace("v{version}", document.Info.Version ?? apiVersion, StringComparison.Ordinal), path.Value);
                    }

                    document.Paths.Clear();

                    foreach (var path in newPaths)
                    {
                        document.Paths.Add(path.Key, path.Value);
                    }

                    return Task.CompletedTask;
                });
            });
        }

        return services;
    }
}