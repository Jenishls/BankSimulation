namespace BankingConsole.Common;

public class ErrorResult
{
    public int Code { get; set; }
    public string Description { get; set; }

    public ErrorResult(int code, string description)
    {
        Code = code;
        Description = description;
    }

}