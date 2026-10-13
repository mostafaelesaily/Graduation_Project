using MedLink.Application.Exceptions;
using System.Text.Json;

namespace MedLink.Presentation.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> logger;
        public ExceptionMiddleware(RequestDelegate next , ILogger<ExceptionMiddleware> logger)
        {
            this.logger = logger;
            this._next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                logger.LogError
                    (
                    ex,
                    "An unhandled exception has occurred while processing {Method} {Path}.",
                    context.Request.Method,
                    context.Request.Path
                    );
                switch (ex) 
                {
                    case NotFoundException:
                        logger.LogWarning(
                            ex,
                            "NotFoundException occurred while processing {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path);
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        break;
                    case BadRequestException:
                        logger.LogWarning(
                            ex,
                            "BadRequestException occurred while processing {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path);
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        break;

                    case ConflictException:
                        logger.LogWarning(
                            ex,
                            "ConflictException occurred while processing {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path);
                        context.Response.StatusCode = StatusCodes.Status409Conflict;
                        break;

                    case ForbiddenException:
                        logger.LogWarning(
                            ex,
                            "ForbiddenException occurred while processing {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path);
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        break;

                    case UnauthorizedException:
                        logger.LogWarning(
                            ex,
                            "ForbiddenException occurred while processing {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path);
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        break;

                    default:
                        logger.LogError(
                            ex,
                            "Unhandled exception occurred while processing {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path);
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        break;
                }
                var response = new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = ex.Message,
                };
                await context.Response.WriteAsync
                    (
                     JsonSerializer.Serialize(response)
                    );
            }
        }
    }
}
