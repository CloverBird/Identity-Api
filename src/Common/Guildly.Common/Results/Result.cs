using System;

namespace Guildly.Common.Results;

public readonly struct Result
{
    public Error Error { get; }
    
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    
    private Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new ArgumentException("Result cannot be success while having error");
        if (!isSuccess && error == Error.None)
            throw new ArgumentException("Error is required for failure.");
        
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public static Result Success => new(true, Error.None);
    
    public static Result Failure(Error error) => new(false, error);
    
    public static implicit operator Result(Error error) => Failure(error);
}

public readonly struct Result<TValue>
{
    private readonly TValue? _value;

    public Error Error { get; }

    public TValue Value => IsSuccess ? 
        _value! : 
        throw new InvalidOperationException("Cannot access the value of a failed result."); 
    
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private Result(TValue? value, bool isSuccess, Error error)
    {
        if (!isSuccess && error == Error.None) 
            throw new ArgumentException("Failed result must contain error");
        if (isSuccess && error != Error.None)
            throw new ArgumentException("Successful result cannot contain error");
        
        _value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result<TValue> Success(TValue value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        
        return new(value, true, Error.None);
    }
    
    public static Result<TValue> Failure(Error error) => new(default, false, error);
    
    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}