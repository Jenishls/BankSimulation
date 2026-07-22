namespace BankingConsole.Common;

public class OperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int StatusCode { get; set; }
    public  List<ErrorResult> Errors { get; set; } = new List<ErrorResult>();
    public OperationResult( int statusCode, bool success, string message)
    {
        StatusCode = statusCode;
        Success = success;
        Message = message;
    }

    public OperationResult(int statusCode, bool success, string message,  List<ErrorResult> errors)
    {
        Success = success;
        Message = message;
        StatusCode = statusCode;
        Errors = errors;
    }
}