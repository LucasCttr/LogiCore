using LogiCore.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LogiCore.Api.Middlewares;

/// <summary>
/// A global exception handler that catches unhandled exceptions, logs them, and returns standardized error responses.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled error: {Message}", exception.Message);

        if (httpContext.Response.HasStarted) return false;

        var env = httpContext.RequestServices.GetService<Microsoft.Extensions.Hosting.IHostEnvironment>();
        var isDevelopment = env?.IsDevelopment() == true;

        // 1. Exception Mapping: Map specific exceptions to status codes and titles
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            DomainException => (StatusCodes.Status400BadRequest, "Domain Operation Failed"),
            FluentValidation.ValidationException => (StatusCodes.Status400BadRequest, "One or more validation errors occurred."), 
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        // 2. Special handling for FluentValidation exceptions to return a ValidationProblemDetails response
        if (exception is FluentValidation.ValidationException valEx)
        {
            var errors = valEx.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

            var validationProblem = new ValidationProblemDetails(errors)
            {
                Status = statusCode,
                Title = title,
                Instance = httpContext.Request.Path
            };
            validationProblem.Extensions["traceId"] = httpContext.TraceIdentifier;

            await httpContext.Response.WriteAsJsonAsync(validationProblem, cancellationToken);
            return true;
        }

        // 3. Caso General: Errores no esperados o de Dominio
        var detail = isDevelopment ? exception.Message : "An error occurred while processing the request.";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (isDevelopment)
        {
            problemDetails.Extensions["exception"] = exception.ToString();
            // Podés simplificar el bucle de InnerExceptions con una lista
            var inners = new List<string>();
            for (var i = exception.InnerException; i != null; i = i.InnerException) inners.Add(i.Message);
            if (inners.Count > 0) problemDetails.Extensions["innerExceptions"] = inners;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true; 
    }
}