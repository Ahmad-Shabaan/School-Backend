namespace Application.Common;

public class Result
{
    public Pagination? Pagination { get; set; }  
    public bool Success { get; init; }

    public string Message { get; init; }
    public Error? Error { get; init; }

    public IEnumerable<Error>? Errors { get; init; }
    protected Result(bool success, string message, Error? error = null, IEnumerable<Error>? errors = null)
    {
        Success = success;
        Message = message;
        Error = error;
        Errors = errors;
    }

    public static Result Succeeded(string message = "")
    {
        return new Result(true, message, null, null);
    }

    public static Result Failed(string message, Error? error = null, IEnumerable<Error>? errors = null)
    {
        return new Result(false, message, error, errors);
    }
}
