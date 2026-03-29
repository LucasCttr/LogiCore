using LogiCore.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogiCore.Api.Filters;

/// <summary>
/// A filter for handling Result<T> responses and converting them to appropriate HTTP responses.
/// </summary>
public class ResultActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        // Only intervene if the result is a Result<T> and it FAILED
        if (executed.Result is ObjectResult objResult &&
            objResult.Value is Application.Common.Models.IResult result &&
            !result.IsSuccess)
        {
            var status = result.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };

            var problem = new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            { "Logic", new[] { result.Error ?? "Business error occurred." } }
        })
            {
                Instance = context.HttpContext.Request.Path,
                Title = "A business logic error occurred.",
                Status = status
            };

            executed.Result = new ObjectResult(problem) { StatusCode = status };
        }
    }
}
