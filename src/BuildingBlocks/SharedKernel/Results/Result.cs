namespace SharedKernel.Results
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error? Error { get; }
        protected Result(bool isSuccess, Error? error)
        {
            if (isSuccess && error is not null && error.Type != Enums.ErrorType.None)
                throw new InvalidOperationException("A successful result cannot have an error message.");
            if (!isSuccess && (error == null || error.Type == Enums.ErrorType.None))
                throw new InvalidOperationException("A failure result must have an error message.");
            IsSuccess = isSuccess;
            Error = error;
        }
        public static Result Success() => new Result(true, null);
        public static Result Failure(Error error) => new Result(false, error);
    }

    public sealed class Result<T> : Result
    {
        public readonly T? _value;

        private Result(bool isSuccess, Error? error, T? value) : base(isSuccess, error)
        {
            _value = value;
        }
        public T Value => IsSuccess ? _value! : throw new InvalidOperationException("The value of a failure result is not accessible.");
        public static Result<T> Success(T value) => new Result<T>(true, null, value);
        public static Result<T> Failure(Error error) => new Result<T>(false, error, default);
    }
}
