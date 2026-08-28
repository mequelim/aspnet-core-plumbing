using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AspNet.Plumbing.Shared.Middlewares
{
    /// <summary>
    /// Middleware for handling exceptions in an ASP.NET Core application.
    /// Captures unhandled exceptions, logs the error details, and returns a standardized JSON response.
    /// </summary>
    public class ExceptionHandlingMiddleware(RequestDelegate requestDelegate, ILogger<ExceptionHandlingMiddleware> logger)
    {
        // Method:
        /// <summary>
        /// Handles exceptions that occur during the processing of HTTP requests.
        /// Captures unhandled exceptions, logs the error, and returns a standardized JSON response with an HTTP 500 status code.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current HTTP request.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await requestDelegate(context);
            }
            catch(Exception exception)
            {
                logger.LogError(
                    exception,
                    "##### [AspNetCore.Plumbing.Shared.Middlewares.ExceptionHandlingMiddleware.cs] [InvokeAsync()] Unhandled exception caught by middleware! #####"
                );

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                string exceptionMessage = (exception.InnerException is not null)
                    ? exception.InnerException.Message
                    : exception.Message;

                object response = new
                {
                    error = "Internal Server Error",
                    message = exceptionMessage,
                    traceId = context.TraceIdentifier
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}