using Authentication.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Authentication.Middlewares
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }

        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            ProblemDetails problemDetails;

            switch (exception)
            {
                case EmailAlreadyExistsException ex:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    problemDetails = new ProblemDetails
                    {
                        Title = "Confilct",
                        Detail = ex.Message,
                        Status = 409
                    };
                    break;

                case KeyNotFoundException ex:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    problemDetails = new ProblemDetails
                    {
                        Title = "Not Found",
                        Detail = ex.Message,
                        Status = 404
                    };
                    break;

                case IncorrectPasswordException ex:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    problemDetails = new ProblemDetails
                    {
                        Title = "Unauthorized",
                        Detail = ex.Message,
                        Status = 401
                    };
                    break;

                case AccessDeniedException ex:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    problemDetails = new ProblemDetails
                    {
                        Title = "Forbiden",
                        Detail = ex.Message,
                        Status = 403
                    };
                    break;

                case UnauthorizedAccessException ex:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    problemDetails = new ProblemDetails
                    {
                        Title = "Unauthorized",
                        Detail = ex.Message,
                        Status = 401
                    };
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    problemDetails = new ProblemDetails
                    {
                        Title = "Internal Server Error",
                        Detail = exception.Message,
                        Status = 500
                    };
                    break;


            }

            return context.Response.WriteAsJsonAsync(problemDetails);
        }

    }
}
