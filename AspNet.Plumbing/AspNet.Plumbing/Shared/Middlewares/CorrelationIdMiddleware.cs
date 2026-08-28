using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AspNet.Plumbing.Shared.Middlewares
{
    /// <summary>
    /// Middleware that ensures every HTTP request has a unique correlation ID.
    /// This ID is used for tracking and logging purposes across the request lifecycle.
    /// </summary>
    public class CorrelationIdMiddleware(RequestDelegate requestDelegate)
    {
        private const string DefaultHeaderName = "X-Correlation-Id";

        // Method:
        /// <summary>
        /// Processes an incoming HTTP request to ensure a correlation ID is present.
        /// If a correlation ID is provided in the request headers, it is validated and used.
        /// Otherwise, a new correlation ID is generated and added to the request and response headers.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> representing the current HTTP request.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            Guid correlationId;

            if(
                (context.Request.Headers.TryGetValue(DefaultHeaderName, out StringValues headerValue)) &&
                (Guid.TryParse(headerValue, out Guid parsedHeader))
            )
            {
                correlationId = parsedHeader;
            }
            else
            {
                correlationId = Guid.Empty;
                context.Request.Headers[DefaultHeaderName] = correlationId.ToString("D");
            }

            context.TraceIdentifier = correlationId.ToString();
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[DefaultHeaderName] = correlationId.ToString();

                return Task.CompletedTask;
            });

            await requestDelegate(context);
        }
    }
}