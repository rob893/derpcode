using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DerpCode.API.ApplicationStartup.ApplicationBuilderExtensions;
using DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;
using DerpCode.API.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using System;
using System.IO;
using DerpCode.API.Core;
using CommandLine;
using Microsoft.Extensions.Logging;
using System.Linq;
using DerpCode.API.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DerpCode.API;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

        builder.Services.AddControllerServices()
            .AddHealthCheckServices()
            .AddIdentityServices()
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

                    if (o.Password != null && o.Password == GetSeederPasswordFromConfiguration())
                    {
                        var migrate = args.Contains(CommandLineOptions.MigrateArgument, StringComparer.OrdinalIgnoreCase);
                        var clearData = args.Contains(CommandLineOptions.ClearDataArgument, StringComparer.OrdinalIgnoreCase);
                        var seedData = args.Contains(CommandLineOptions.SeedDataArgument, StringComparer.OrdinalIgnoreCase);
                        var dropDatabase = args.Contains(CommandLineOptions.DropArgument, StringComparer.OrdinalIgnoreCase);

                        var seeder = serviceProvider.GetRequiredService<IDatabaseSeeder>();

                        logger.LogInformation($"Seeding database:\nDrop database: {dropDatabase}\nApply Migrations: {migrate}\nClear old data: {clearData}\nSeed new data: {seedData}");
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
            .UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            })
            .UseMiddleware<PathBaseRewriterMiddleware>()
            .UseAndConfigureCors(builder.Configuration)
            .UseAuthentication()
            .UseAuthorization()
            .UseAndConfigureSwagger(builder.Configuration)
            .UseAndConfigureEndpoints(builder.Configuration);

        await app.RunAsync();
    }

    private static string GetSeederPasswordFromConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Secrets.json", false, true)
            // Add local settings file last so it takes priority. 
            // This file should only be used for local development.
            .AddJsonFile("appsettings.Local.json", true, true);

        var config = builder.Build();

        return config.GetValue<string>("SeederPassword");
    }
}
