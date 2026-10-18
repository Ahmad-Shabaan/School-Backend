using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Common;

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool success, T? data, string message, Error? error = null, IEnumerable<Error>? errors = null)
        : base(success, message, error, errors)
    {
        Data = data;
    }

    public static Result<T> Succeeded(T data, string message = "")
    {
        return new Result<T>(true, data, message);
    }

    public static new Result<T> Failed(string message, Error? error = null, IEnumerable<Error>? errors = null)
    {
        return new Result<T>(false, default, message, error, errors);
    }
}
