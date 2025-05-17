
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DerpCode.API.ApplicationStartup.ApplicationBuilderExtensions;
using DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;
using DerpCode.API.Middleware;

namespace DerpCode.API;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

        builder.Services.AddControllerServices()
            .AddHealthCheckServices()
            .AddSwaggerServices(builder.Configuration)
            .AddCors()
            .AddHttpClient()
            .AddDockerServices();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseExceptionHandler(x => x.UseMiddleware<GlobalExceptionHandlerMiddleware>())
            .UseRouting()
            .UseHsts()
            .UseHttpsRedirection()
            .UseAndConfigureCors(builder.Configuration)
            .UseAuthentication()
            .UseAuthorization()
            .UseAndConfigureSwagger(builder.Configuration)
            .UseAndConfigureEndpoints(builder.Configuration);

        app.Run();
    }
}
