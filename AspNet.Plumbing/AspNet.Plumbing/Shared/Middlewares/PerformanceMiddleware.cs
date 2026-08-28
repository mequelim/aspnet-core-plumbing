using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNet.Plumbing.Shared.Middlewares
{
    /// <summary>
    /// Middleware that monitors the performance of HTTP requests by measuring their execution time.
    /// Logs a warning if the processing time exceeds a predefined threshold.
    /// </summary>
    public sealed class PerformanceMiddleware(RequestDelegate requestDelegate, ILogger<PerformanceMiddleware> logger)
    {
        private const int ThresholdInMilliseconds = 500;

        // Method:
        /// <summary>
        /// Processes an incoming HTTP request and logs a warning if the request processing time exceeds a predefined threshold.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> representing the current HTTP request.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            DateTime start = DateTime.UtcNow;

            await requestDelegate(context);

            TimeSpan duration = DateTime.UtcNow - start;

            if(duration.TotalMilliseconds > ThresholdInMilliseconds)
            {
                logger.LogWarning(
                    "##### [AspNetCore.Plumbing.Shared.Middlewares.PerformanceMiddleware.cs] [InvokeAsync()] Slow request detected: {method} {path} took {duration}ms #####",
                    context.Request.Method,
                    context.Request.Path,
                    duration.TotalMilliseconds
                );
            }
        }
    }
}