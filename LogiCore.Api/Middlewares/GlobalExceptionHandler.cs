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

        if (httpContext.Response.HasStarted)
        {
            logger.LogWarning("The response has already started, the global exception handler will not modify the response.");
            return false;
        }

        // determine if we are in development to include exception details
        var env = httpContext.RequestServices.GetService<Microsoft.Extensions.Hosting.IHostEnvironment>();
        var isDevelopment = env?.IsDevelopment() == true;

        // Map excepcions to status codes and titles
        var (statusCode, title) = exception switch
        {
            PackageWeightException => (StatusCodes.Status400BadRequest, "Invalid Package Weight"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            DomainException => (StatusCodes.Status400BadRequest, "Invalid Domain Operation"),
            FluentValidation.ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        // Build ProblemDetails (or ValidationProblemDetails)
        if (exception is FluentValidation.ValidationException valEx)
        {
            var errors = new Dictionary<string, string[]>();
            foreach (var failure in valEx.Errors)
            {
                if (!errors.TryGetValue(failure.PropertyName, out var list))
                {
                    errors[failure.PropertyName] = new[] { failure.ErrorMessage };
                }
                else
                {
                    var merged = list.Concat(new[] { failure.ErrorMessage }).ToArray();
                    errors[failure.PropertyName] = merged;
                }
            }

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

        // Keep `detail` concise; in Development include inner exceptions separately in extensions
        var detail = isDevelopment ? exception.Message : "An error occurred while processing the request.";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        // In Development include the full exception (stack trace) and inner exception messages in extensions to aid debugging
        if (isDevelopment)
        {
            problemDetails.Extensions["exception"] = exception.ToString();

            var innerMessages = new System.Collections.Generic.List<string>();
            var inner = exception.InnerException;
            while (inner != null)
            {
                innerMessages.Add(inner.Message);
                inner = inner.InnerException;
            }

            if (innerMessages.Count > 0)
            {
                problemDetails.Extensions["innerExceptions"] = innerMessages.ToArray();
            }
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; 
    }
}