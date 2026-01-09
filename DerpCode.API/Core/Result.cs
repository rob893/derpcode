using System;

namespace DerpCode.API.Core;

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, DomainErrorType? errorType, string? errorMessage)
    {
        this.IsSuccess = isSuccess;
        this.Value = value;
        this.ErrorType = errorType;
        this.ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public T? Value { get; }

    public T ValueOrThrow => this.Value ?? throw new InvalidOperationException("Result does not contain a value.");

    public DomainErrorType? ErrorType { get; }

    public string? ErrorMessage { get; }

    public static Result<T> Success(T value) => new(true, value, null, null);

    public static Result<T> Failure(DomainErrorType errorType, string message) =>
        new(false, default, errorType, message);
}
