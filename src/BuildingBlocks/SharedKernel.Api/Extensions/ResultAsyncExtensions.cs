using SharedKernel.Results;

namespace SharedKernel.Api.Extensions
{
    public static class ResultAsyncExtensions
    {
        public static async Task<Result> BindAsync(this Result result, Func<Task<Result>> next)
            => result.IsSuccess ? await next() : result;

        public static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> next)
            => result.IsSuccess ? await next(result.Value) : Result<TOut>.Failure(result.Error);

        public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> next)
        {
            var result = await resultTask;
            return await result.BindAsync(next);
        }

        public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
            this Result<TIn> result,
            Func<TIn, Task<TOut>> map)
            => result.IsSuccess
                ? Result<TOut>.Success(await map(result.Value))
                : Result<TOut>.Failure(result.Error);

        public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
            this Task<Result<TIn>> resultTask,
            Func<TIn, TOut> map)
        {
            var result = await resultTask;
            return result.IsSuccess ? Result<TOut>.Success(map(result.Value)) : Result<TOut>.Failure(result.Error);
        }

        public static async Task<Result<T>> EnsureAsync<T>(
            this Result<T> result,
            Func<T, Task<bool>> predicate,
            Error error)
            => result.IsSuccess && !await predicate(result.Value)
                ? Result<T>.Failure(error)
                : result;

        public static async Task<Result<T>> TapAsync<T>(this Result<T> result, Func<T, Task> action)
        {
            if (result.IsSuccess) await action(result.Value);
            return result;
        }
    }
}
