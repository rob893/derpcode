namespace DerpCode.API.Core;

public sealed class Result<T>
{
    public bool IsSuccess { get; }

    public T? Value { get; }

    public DomainErrorType? ErrorType { get; }

    public string? ErrorMessage { get; }

    public static Result<T> Success(T value) => new(true, value, null, null);

    public static Result<T> Failure(DomainErrorType errorType, string message) =>
        new(false, default, errorType, message);

    private Result(bool isSuccess, T? value, DomainErrorType? errorType, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }
}
