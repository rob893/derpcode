using System;
using System.IO;
using System.Linq;
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
using DerpCode.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

            builder.Logging.AddOpenTelemetry(options =>
            {
                options.IncludeScopes = true;
            });
            builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
                options.Credential = new DefaultAzureCredential();
                options.SamplingRatio = 0.5f;
            });
        }

        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

        builder.Services.AddControllerServices()
            .AddHealthCheckServices()
            .AddIdentityServices()
            .AddScoped<ICorrelationIdService, CorrelationIdService>()
            .AddAuthenticationServices(builder.Configuration)
            .AddDatabaseServices(builder.Configuration)
            .AddRepositoryServices()
            .AddSwaggerServices(builder.Configuration)
            .AddCors()
            .AddHttpClient()
            .AddDockerServices();

        var app = builder.Build();

        if (args.Contains(CommandLineOptions.SeedArgument, StringComparer.OrdinalIgnoreCase))
        {
            await Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsedAsync(async o =>
                {
                    using var scope = app.Services.CreateScope();
                    var serviceProvider = scope.ServiceProvider;
                    var logger = serviceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();
                    var seederPassword = app.Configuration.GetValue<string>("SeederPassword") ?? throw new InvalidOperationException("Seeder password not found in configuration.");

                    if (o.Password != null && o.Password == seederPassword)
                    {
                        var migrate = args.Contains(CommandLineOptions.MigrateArgument, StringComparer.OrdinalIgnoreCase);
                        var clearData = args.Contains(CommandLineOptions.ClearDataArgument, StringComparer.OrdinalIgnoreCase);
                        var seedData = args.Contains(CommandLineOptions.SeedDataArgument, StringComparer.OrdinalIgnoreCase);
                        var dropDatabase = args.Contains(CommandLineOptions.DropArgument, StringComparer.OrdinalIgnoreCase);

                        var seeder = serviceProvider.GetRequiredService<IDatabaseSeeder>();

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
            .UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            })
            .UseMiddleware<PathBaseRewriterMiddleware>()
            .UseAndConfigureCors(builder.Configuration)
            .UseAuthentication()
            .UseAuthorization()
            .UseMiddleware<LoggingScopeMiddleware>() // Ensure this is after UseAuthentication and UseAuthorization to capture user information
            .UseAndConfigureSwagger(builder.Configuration)
            .UseAndConfigureEndpoints(builder.Configuration);

        await app.RunAsync();
    }
}