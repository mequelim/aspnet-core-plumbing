using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNet.Plumbing.Shared.Middlewares
{
    /// <summary>
    /// Middleware that logs details about incoming HTTP requests, including the HTTP method, request path, and processing duration.
    /// </summary>
    /// <remarks>This middleware is designed to provide insights into request processing times and can help with monitoring and debugging.</remarks>
    public sealed partial class RequestLoggingMiddleware(RequestDelegate requestDelegate, ILogger<RequestLoggingMiddleware> logger)
    {
        // Methods:
        /// <summary>
        /// Logs an informational message indicating that a request with the specified HTTP method and path was processed in a given duration.
        /// </summary>
        /// <param name="method">The HTTP method of the request (e.g., GET, POST).</param>
        /// <param name="path">The path of the request.</param>
        /// <param name="duration">The time taken to process the request, in milliseconds.</param>
        [LoggerMessage(LogLevel.Information, "Request {Method} {Path} processed in {Duration} ms")]
        partial void LogRequestMethodPathProcessedInDurationMs(string method, PathString path, double duration);

        /// <summary>
        /// Processes an HTTP request, logs the request method, path, and the time taken to process the request.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> representing the current HTTP request.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            DateTime start = DateTime.UtcNow;

            await requestDelegate(context);

            TimeSpan duration = DateTime.UtcNow - start;

            LogRequestMethodPathProcessedInDurationMs(context.Request.Method, context.Request.Path, duration.TotalMilliseconds);
        }
    }
}