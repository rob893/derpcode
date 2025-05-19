using System;
using System.Linq;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using Microsoft.AspNetCore.Http;

namespace DerpCode.API.Middleware
{
    /// <summary>
    /// This middleware rewrites the request path base to whatever is in the X-Forwarded-Prefix.
    /// Useful if the app is running behind a reverse proxy or load balancer.
    /// </summary>
    public sealed class PathBaseRewriterMiddleware
    {
        private readonly RequestDelegate next;

        public PathBaseRewriterMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Request.Headers.TryGetValue(AppHeaderNames.ForwardedPrefix, out var value))
            {
                context.Request.PathBase = value.First();
            }

            await this.next(context);
        }
    }
}