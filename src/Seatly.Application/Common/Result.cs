namespace Seatly.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string ErrorMessage { get; }

    protected Result(bool isSuccess, string errorMessage)
    {
        this.IsSuccess = IsSuccess;
        this.ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, string.Empty);

    public static Result Failure(string errorMessage) => new(false, errorMessage);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string errorMessage)
        : base(isSuccess, errorMessage)
    {
        this.Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);

    public static new Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
}
