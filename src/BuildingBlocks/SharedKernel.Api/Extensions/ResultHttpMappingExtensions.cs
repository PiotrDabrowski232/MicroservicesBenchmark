using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SharedKernel.Enums;
using SharedKernel.Results;

namespace SharedKernel.Api.Extensions
{
    public static class ResultHttpMappingExtensions
    {
        public static ProblemDetails ToProblemDetails(this Result result, HttpContext httpContext)
        {
            var error = result.Error;
            var status = (int)(error.HttpStatus ?? MapStatus(error.Type));

            var problem = new ProblemDetails
            {
                Status = status,
                Title = error.Message,
                Type = error.Code,
                Detail = error.Details,
                Instance = httpContext.Request.Path
            };

            problem.Extensions["errorCode"] = error.Code;
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;

            return problem;
        }

        public static IResult ToIResult(this Result result, HttpContext httpContext)
            => result.IsSuccess
                ? Microsoft.AspNetCore.Http.Results.NoContent()
                : Microsoft.AspNetCore.Http.Results.Problem(result.ToProblemDetails(httpContext));

        public static IResult ToIResult<T>(this Result<T> result, HttpContext httpContext)
            => result.IsSuccess
                ? Microsoft.AspNetCore.Http.Results.Ok(result.Value)
                : Microsoft.AspNetCore.Http.Results.Problem(result.ToProblemDetails(httpContext));

        private static HttpStatusCode MapStatus(ErrorType type) =>
            type switch
            {
                ErrorType.Validation => HttpStatusCode.BadRequest,
                ErrorType.NotFound => HttpStatusCode.NotFound,
                ErrorType.Conflict => HttpStatusCode.Conflict,
                ErrorType.Unauthorized => HttpStatusCode.Unauthorized,
                ErrorType.Forbidden => HttpStatusCode.Forbidden,
                _ => HttpStatusCode.InternalServerError
            };
    }
}
