using System;

namespace AsmSpy.Core
{
    public abstract class Result<T>
    {
        public static Result<T> Succeed(T value) => new Success<T>(value);
        public static Result<T> Fail(string message) => new Failure<T>(message);

        public static implicit operator Result<T>(string message) => Fail(message);
        public static implicit operator Result<T>(T value) => Succeed(value);
    }

    public static class ResultExtensions
    {
        public static Result<B> Bind<A, B>(this Result<A> a, Func<A, Result<B>> func)
        {
            switch(a)
            {
                case Success<A> success: return func(success.Value);
                case Failure<A> failure: return failure.Message;
                default: throw new InvalidOperationException("Unexpected Result subtype.");
            }
        }

        public static Result<B> Map<A, B>(this Result<A> a, Func<A, B> func)
            => a.Bind(x => func(x).ToResult());

        public static Result<T> ToResult<T>(this T value) => Result<T>.Succeed(value);
    }

    public class Success<T> : Result<T>
    {
        public T Value { get; }
        public Success(T value) => Value = value;
    }

    public class Failure<T> : Result<T>
    {
        public string Message { get; }
        public Failure(string message) => Message = message 
            ?? throw new ArgumentNullException(nameof(message)); 
    }
}
