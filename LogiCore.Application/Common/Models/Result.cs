using System;

namespace LogiCore.Application.Common.Models;
public interface IResult
{
    bool IsSuccess { get; }
    string? Error { get; }
    ErrorType Type { get; }
    object? GetValue(); 
}

public enum ErrorType { None, Validation, NotFound, Conflict, Unauthorized }

public class Result<T>: IResult
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public ErrorType Type { get; init; } = ErrorType.None;
    public DateTime ResponseTime { get; init; } = DateTime.UtcNow;

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error, ErrorType type = ErrorType.Validation) => new() { IsSuccess = false, Error = error, Type = type };

    public object? GetValue() => Value;
}

