using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using CommandLine;
using DerpCode.API.ApplicationStartup.ApplicationBuilderExtensions;
using DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;
using DerpCode.API.Constants;
using DerpCode.API.Core;
using DerpCode.API.Data;
using DerpCode.API.Extensions;
using DerpCode.API.Middleware;
using DerpCode.API.Models.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using IPNetwork = System.Net.IPNetwork;

namespace DerpCode.API;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var keyVaultUrl = builder.Configuration[ConfigurationKeys.KeyVaultUrl] ?? throw new InvalidOperationException("KeyVaultUrl not found in configuration.");

        if (builder.Configuration.GetEnvironment() != EnvironmentNames.Development)
        {
            builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential(), new PrefixKeyVaultSecretManager(["DerpCode", "All"]));

            var appInsightsConnectionString = builder.Configuration[ConfigurationKeys.ApplicationInsightsConnectionString] ?? throw new InvalidOperationException("ApplicationInsightsConnectionString not found in configuration.");

            var productName = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName;
            var environment = builder.Configuration.GetEnvironment();
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            builder.Logging.AddOpenTelemetry(options =>
            {
                options.IncludeScopes = true;
            });
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName: $"{productName}-{environment}-{version}"))
                .UseAzureMonitor(options =>
                {
                    options.ConnectionString = appInsightsConnectionString;
                    options.Credential = new DefaultAzureCredential();
                    options.SamplingRatio = 1f;
                });
        }

        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

        builder.Services.AddControllerServices()
            .AddHealthCheckServices()
            .AddMemoryCache()
            .AddIdentityServices()
            .AddRateLimiterServices(builder.Configuration)
            .AddCoreServices()
            .AddGitHubServices(builder.Configuration)
            .AddEmailServices(builder.Configuration)
            .AddAuthenticationServices(builder.Configuration)
            .AddDatabaseServices(builder.Configuration)
            .AddRepositoryServices()
            .AddDomainServices()
            .AddOpenApiServices(builder.Configuration)
            .AddCors()
            .AddHttpClient()
            .AddDockerServices();

        builder.Services.Configure<ProgressionSettings>(builder.Configuration.GetSection(ConfigurationKeys.Progression));

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            var seeder = serviceProvider.GetRequiredService<IDatabaseSeeder>();

            if (args.Contains(CommandLineOptions.SeedArgument, StringComparer.OrdinalIgnoreCase))
            {
                await Parser.Default.ParseArguments<CommandLineOptions>(args)
                    .WithParsedAsync(async o =>
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();
                        var seederPassword = app.Configuration.GetValue<string>("SeederPassword") ?? throw new InvalidOperationException("Seeder password not found in configuration.");


                        if (o.Password != null && o.Password == seederPassword)
                        {
                            var migrate = args.Contains(CommandLineOptions.MigrateArgument, StringComparer.OrdinalIgnoreCase);
                            var clearData = args.Contains(CommandLineOptions.ClearDataArgument, StringComparer.OrdinalIgnoreCase);
                            var seedData = args.Contains(CommandLineOptions.SeedDataArgument, StringComparer.OrdinalIgnoreCase);
                            var dropDatabase = args.Contains(CommandLineOptions.DropArgument, StringComparer.OrdinalIgnoreCase);

                            logger.LogInformation("Seeding database:\nDrop database: {DropDatabase}\nApply Migrations: {Migrate}\nClear old data: {ClearData}\nSeed new data: {SeedData}", dropDatabase, migrate, clearData, seedData);
                            logger.LogWarning("Are you sure you want to apply these actions to the database in that order? Only 'yes' will continue.");

                            var answer = Console.ReadLine();

                            if (answer == "yes")
                            {
                                await seeder.SeedDatabaseAsync(seedData, clearData, migrate, dropDatabase, CancellationToken.None);
                            }
                            else
                            {
                                logger.LogWarning("Aborting database seed process...");
                            }
                        }
                        else
                        {
                            logger.LogWarning("Invalid seeder password");
                        }
                    });
            }

            await seeder.SyncProblemsFromFolderAsync(CancellationToken.None);
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseExceptionHandler(x => x.UseMiddleware<GlobalExceptionHandlerMiddleware>())
            .UseRouting()
            .UseHsts()
            .UseHttpsRedirection()
            .UseMiddleware<CorrelationIdMiddleware>()
            .UseForwardedHeaders(BuildForwardedHeadersOptions(builder.Configuration))
            .UseMiddleware<PathBaseRewriterMiddleware>()
            .UseAndConfigureCors(builder.Configuration)
            .UseAuthentication()
            .UseAuthorization()
            .UseMiddleware<LoggingScopeMiddleware>() // Ensure this is after UseAuthentication and UseAuthorization to capture user information.
            .UseRateLimiter(); // Ensure this is after UseAuthentication and UseAuthorization to apply rate limiting based on user identity.

        app.UseAndConfigureOpenApi(builder.Configuration)
            .UseAndConfigureEndpoints(builder.Configuration);

        await app.RunAsync();
    }

    // ForwardedHeaders middleware rewrites Connection.RemoteIpAddress, Request.Scheme, and
    // Request.Host based on X-Forwarded-* headers, but only when the immediate connection comes
    // from a trusted proxy. We pin the trust list to loopback by default (the API sits behind an
    // on-host nginx in production) and allow ops to extend it via configuration without redeploys.
    // Without this, malicious clients could spoof their source IP for the rate limiter and any
    // other middleware that reads RemoteIpAddress.
    private static ForwardedHeadersOptions BuildForwardedHeadersOptions(ConfigurationManager configuration)
    {
        var settings = configuration.GetSection(ConfigurationKeys.ForwardedHeaders).Get<ForwardedHeadersSettings>() ?? new ForwardedHeadersSettings();

        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.All,
            ForwardLimit = settings.ForwardLimit
        };

        // Replace the framework defaults with an explicit, documented trust list. Loopback is
        // always included so the on-host nginx reverse proxy keeps working.
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();
        options.KnownIPNetworks.Add(new IPNetwork(IPAddress.Parse("127.0.0.0"), 8));
        options.KnownIPNetworks.Add(new IPNetwork(IPAddress.IPv6Loopback, 128));

        foreach (var proxy in settings.KnownProxies)
        {
            if (IPAddress.TryParse(proxy, out var address))
            {
                options.KnownProxies.Add(address);
            }
        }

        foreach (var network in settings.KnownNetworks)
        {
            if (IPNetwork.TryParse(network, out var ipNetwork))
            {
                options.KnownIPNetworks.Add(ipNetwork);
            }
        }

        return options;
    }
}