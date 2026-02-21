using SharedKernel.Results;

namespace SharedKernel.Api.Extensions
{
    public static class ResultExtensions
    {
        public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> map)
            => result.IsSuccess
                ? Result<TOut>.Success(map(result.Value))
                : Result<TOut>.Failure(result.Error);

        public static Result Bind(this Result result, Func<Result> next)
            => result.IsSuccess ? next() : result;

        public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> next)
            => result.IsSuccess ? next(result.Value) : Result<TOut>.Failure(result.Error);

        public static Result Ensure(this Result result, Func<bool> predicate, Error error)
       => result.IsSuccess && !predicate()
           ? Result.Failure(error)
           : result;

        public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error)
            => result.IsSuccess && !predicate(result.Value)
                ? Result<T>.Failure(error)
                : result;

        public static Result Tap(this Result result, Action action)
        {
            if (result.IsSuccess) action();
            return result;
        }

        public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
        {
            if (result.IsSuccess) action(result.Value);
            return result;
        }
    }
}
