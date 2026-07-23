namespace BankingConsole.Common;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    protected AppException(string message, int statusCode) : base(message)
        => StatusCode = statusCode;
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404) { }
}

public class ValidationException : AppException
{
    public IReadOnlyList<string> Errors { get; }
    public ValidationException(string message, IReadOnlyList<string>? errors = null)
        : base(message, 400)
        => Errors = errors ?? [message];
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}
