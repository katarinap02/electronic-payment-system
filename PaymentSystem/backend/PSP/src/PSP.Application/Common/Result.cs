namespace PSP.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; }

    protected Result(bool isSuccess, string? errorMessage)
    {
        if (isSuccess && errorMessage != null)
            throw new InvalidOperationException("Successful result cannot have an error message");
        if (!isSuccess && errorMessage == null)
            throw new InvalidOperationException("Failed result must have an error message");

        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new Result(true, null);
    public static Result Failure(string errorMessage) => new Result(false, errorMessage);

    public static Result<T> Success<T>(T value) => new Result<T>(value, true, null);
    public static Result<T> Failure<T>(string errorMessage) => new Result<T>(default!, false, errorMessage);
}

public class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, string? errorMessage)
        : base(isSuccess, errorMessage)
    {
        Value = value;
    }
}
