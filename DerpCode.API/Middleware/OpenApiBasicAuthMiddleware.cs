using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DerpCode.API.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DerpCode.API.Middleware;

public sealed class OpenApiBasicAuthMiddleware
{
    private readonly RequestDelegate next;

    public OpenApiBasicAuthMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<OpenApiSettings> openApiSettings)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(openApiSettings);

        var settings = openApiSettings.Value;
        var authSettings = settings.AuthSettings;

        // Make sure we are hitting the swagger/openapi/scalar paths
        if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.Ordinal) ||
            context.Request.Path.StartsWithSegments("/openapi", StringComparison.Ordinal) ||
            context.Request.Path.StartsWithSegments("/scalar", StringComparison.Ordinal))
        {
            if (!settings.Enabled)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (!authSettings.RequireAuth)
            {
                await this.next.Invoke(context);
                return;
            }

            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) && authHeader.ToString().StartsWith("Basic ", StringComparison.Ordinal))
            {
                // Get the encoded username and password
                var encodedUsernamePassword = authHeader.ToString().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();

                // Decode from Base64 to string
                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword ?? ""));

                // Split username and password
                var username = decodedUsernamePassword.Split(':', 2)[0];
                var password = decodedUsernamePassword.Split(':', 2)[1];

                // Check if login is correct
                if (IsAuthorized(username, password, authSettings))
                {
                    await this.next.Invoke(context);
                    return;
                }
            }

            // Return authentication type (causes browser to show login dialog)
            context.Response.Headers["WWW-Authenticate"] = "Basic";

            // Return unauthorized
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
        else
        {
            await this.next.Invoke(context);
        }
    }

    private static bool IsAuthorized(string username, string password, OpenApiAuthSettings authSettings)
    {
        // Check that username and password are correct
        return username.Equals(authSettings.Username, StringComparison.Ordinal) && password.Equals(authSettings.Password, StringComparison.Ordinal);
    }
}