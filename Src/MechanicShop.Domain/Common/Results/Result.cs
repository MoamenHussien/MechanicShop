using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;

public readonly record struct Success;
public readonly record struct Created;
public readonly record struct Deleted;
public readonly record struct Updated;

public static class Result
{
    public static Success Success => default;
    public static Created Created => default;
    public static Deleted Deleted => default;
    public static Updated Updated => default;
}

public sealed class Result<Tvalue> : IResult<Tvalue>
{
    private readonly Tvalue _Value = default!;
    private readonly List<Error> _Errors = [];

    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;
    public Tvalue Value => IsSuccess ? _Value : default!;
    public List<Error> Errors => IsError ? _Errors : [];

    public Error TopError => _Errors.Count() > 0 ? _Errors[0] : default;


    [JsonConstructor]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("For serializer only.", true)]
    public Result(Tvalue? value, List<Error>? errors, bool isSuccess)
    {
        if (isSuccess)
        {
            _Value = value ?? throw new ArgumentNullException(nameof(value));
            _Errors = [];
            IsSuccess = true;
        }
        else
        {
            if (errors == null || errors.Count == 0)
            {
                throw new ArgumentException("Provide at least one error.", nameof(errors));
            }

            _Errors = errors;
            _Value = default!;
            IsSuccess = false;
        }
    }
    public Result(Tvalue value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        IsSuccess = true;
        _Value = value;
    }
    public Result(Error error)
    {
        _Errors.Add(error);
        IsSuccess = false;
    }

    public Result(List<Error> errors)
    {
        if (Errors.Count() < 0 || Errors is null)
        {
            throw new ArgumentException("The list Is empty pro");
        }

        _Errors = errors;
        IsSuccess = false;
    }

    public ChosenType Match<ChosenType>(Func<Tvalue, ChosenType> OnSuccess, Func<List<Error>, ChosenType> OnFailure)
    {
        return IsSuccess ? OnSuccess(_Value) : OnFailure(_Errors);
    }


    public static implicit operator Result<Tvalue>(Tvalue value)
    {
        return new Result<Tvalue>(value);
    }
    public static implicit operator Result<Tvalue>(Error error)
    {
        return new Result<Tvalue>(error);
    }

    public static implicit operator Result<Tvalue>(List<Error> errors)
    {
        return new Result<Tvalue>(errors);
    }




}