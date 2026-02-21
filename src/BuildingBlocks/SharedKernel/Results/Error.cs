using System.Net;

using SharedKernel.Enums;

namespace SharedKernel.Results
{
    public sealed record Error(
        string Code,
        string Message,
        ErrorType Type = ErrorType.Failure,
        string? Details = null,
        HttpStatusCode? HttpStatus = null)
    {

        public static readonly Error None = new("none", string.Empty, ErrorType.None);

        public static Error Validation(string code, string message, string? details = null)
            => new(code, message, ErrorType.Validation, details, HttpStatusCode.BadRequest);

        public static Error NotFound(string code, string message, string? details = null)
            => new(code, message, ErrorType.NotFound, details, HttpStatusCode.NotFound);

        public static Error Conflict(string code, string message, string? details = null)
            => new(code, message, ErrorType.Conflict, details, HttpStatusCode.Conflict);

        public static Error Unauthorized(string code, string message, string? details = null)
            => new(code, message, ErrorType.Unauthorized, details, HttpStatusCode.Unauthorized);

        public static Error Forbidden(string code, string message, string? details = null)
            => new(code, message, ErrorType.Forbidden, details, HttpStatusCode.Forbidden);

        public static Error Failure(string code, string message, string? details = null, HttpStatusCode? status = null)
            => new(code, message, ErrorType.Failure, details, status ?? HttpStatusCode.InternalServerError);
    }
}
