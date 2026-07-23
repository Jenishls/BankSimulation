using BankingConsole.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BankingConsole.Middleware;

public class AppExceptionHandler : IExceptionHandler
{
    private readonly ILogger<AppExceptionHandler> _logger;

    public AppExceptionHandler(ILogger<AppExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if(exception is not AppException appException)
            return false;
        
        _logger.LogWarning(
            exception,
            "Handled application exception: {Message}",
            exception.Message 
        );

        httpContext.Response.StatusCode = appException.StatusCode;

        var errors = appException is ValidationException ve ? ve.Errors : [appException.Message];

        var result = new OperationResult(
            appException.StatusCode,
            false,
            appException.Message,
            errors.Select(e => new ErrorResult(appException.StatusCode,e)).ToList()
        );

        // await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        // {
        //     Status = appException.StatusCode,
        //     Title = appException.Message,
        //     Extensions = {["errors"] = errors}
        // }, cancellationToken);

        httpContext.Response.StatusCode = appException.StatusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken);
        
        return true;
    }
}