using LogiCore.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LogiCore.Api.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled error occurred: {Message}", exception.Message);

        if (httpContext.Response.HasStarted) return false;

        var env = httpContext.RequestServices.GetService<IHostEnvironment>();
        var isDevelopment = env?.IsDevelopment() == true;

        // Mapping Exceptions to Status Codes and Titles
        var (statusCode, title) = exception switch
        {
            // 404: Cuando el ID del paquete o envío no existe en la DB
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),

            // 400: Cuando el camión está lleno o el estado es inválido (Reglas de LogiCore)
            DomainException => (StatusCodes.Status400BadRequest, "Business Rule Violation"),

            // 422: Cuando el JSON tiene errores (ej: TrackingNumber vacío o Peso < 0)
            FluentValidation.ValidationException => (StatusCodes.Status422UnprocessableEntity, "Validation Error"),

            // 500: Errores no controlados (explosión de base de datos, nulos inesperados, etc.)
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        // Initialize the errors dictionary 
        IDictionary<string, string[]> errors = new Dictionary<string, string[]>();

        if (exception is FluentValidation.ValidationException valEx)
        {
            errors = valEx.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
        }
        else if (exception is DomainException domainEx)
        {
            errors.Add("Domain", new[] { domainEx.Message });
        }

        // If we have specific errors to report (validation or domain), return a ValidationProblemDetails response
        if (errors.Count > 0)
        {
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

        // General error response for unhandled exceptions or those without specific error details
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
            // Extra details for development environment
            var inners = new List<string>();
            for (var i = exception.InnerException; i != null; i = i.InnerException)
                inners.Add(i.Message);

            if (inners.Count > 0)
                problemDetails.Extensions["innerExceptions"] = inners;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}