using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Api.Middlewares
{
    public sealed class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
            => _logger = logger;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
            catch (Exception ex)
            {
                var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

                _logger.LogError(ex,
               "Unhandled exception. TraceId={traceId} Path={Path}",
               traceId, context.Request.Path);

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Unexpected error",
                    Detail = "An unexpected error occurred.",
                    Instance = context.Request.Path
                };

                problem.Extensions["traceId"] = traceId;

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = problem.Status.Value;

                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }
}
