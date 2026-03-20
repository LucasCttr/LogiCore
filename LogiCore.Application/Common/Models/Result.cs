using System;

namespace LogiCore.Application.Common.Models;
public interface IResult
{
    bool IsSuccess { get; }
    string? Error { get; }
    object? GetValue(); 
}

public class Result<T>: IResult
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public DateTime ResponseTime { get; init; } = DateTime.UtcNow;

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };

    public object? GetValue() => Value;
}

