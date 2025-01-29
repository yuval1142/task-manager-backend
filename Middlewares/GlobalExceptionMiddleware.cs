using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Middlewares
{
    /// <summary>
    /// Middleware for global exception handling, ensuring consistent API responses.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        /// <summary>
        /// Initializes the middleware with request handling pipeline and logging support.
        /// </summary>
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Middleware invocation method that intercepts unhandled exceptions.
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Proceed with the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // Handle different exception types with appropriate HTTP status codes
                switch (ex)
                {
                    case ArgumentNullException:
                        _logger.LogWarning(ex, "Bad request: missing required parameters.");
                        await HandleExceptionAsync(context, HttpStatusCode.BadRequest, "E400", "Missing required parameters.");
                        break;
                    case UnauthorizedAccessException:
                        _logger.LogWarning(ex, "Unauthorized access attempt.");
                        await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, "E401", "Unauthorized access.");
                        break;
                    case ForbiddenAccessException:
                        _logger.LogWarning(ex, "Forbidden access attempt.");
                        await HandleExceptionAsync(context, HttpStatusCode.Forbidden, "E403", "Forbidden access: You do not have permission.");
                        break;
                    case NotFoundException:
                        _logger.LogWarning(ex, "Requested resource not found.");
                        await HandleExceptionAsync(context, HttpStatusCode.NotFound, "E404", "The requested resource was not found.");
                        break;
                    default:
                        _logger.LogError(ex, "Unhandled exception occurred.");
                        await HandleExceptionAsync(
                            context,
                            HttpStatusCode.InternalServerError,
                            "E500",
                            "An internal server error occurred.",
                            _environment.IsDevelopment() ? ex.Message : string.Empty
                        );
                        break;
                }
            }
        }

        /// <summary>
        /// Generates a standardized JSON response for handled exceptions.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="statusCode">The HTTP status code to return.</param>
        /// <param name="errorID">A unique identifier for the error type.</param>
        /// <param name="errorMessage">A user-friendly error message.</param>
        /// <param name="details">Optional details (included in development mode).</param>
        /// <returns>A JSON-formatted error response.</returns>
        private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string errorID, string errorMessage, string details = "")
        {
            // Set the HTTP response status code and content type
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            // Create the error response object
            var errorResponse = new ErrorResponse
            {
                StatusCode = (int)statusCode,
                ErrorID = errorID,
                ErrorDescription = errorMessage,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            // Serialize the error response to JSON and return it
            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }


    /// <summary>
    /// Custom exception for handling 403 Forbidden access.
    /// </summary>
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException(string message = "Access to this resource is forbidden.")
            : base(message) { }
    }

    /// <summary>
    /// Custom exception for handling 404 Not Found errors.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message = "The requested resource was not found.")
            : base(message) { }
    }
}
