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
            var isNotFound = result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true;

            var validationProblem = new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            { "Logic", new[] { result.Error ?? "Business error occurred." } }
        })
            {
                Instance = context.HttpContext.Request.Path,
                Title = "One or more validation errors occurred.",
                Status = isNotFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest
            };

            executed.Result = new ObjectResult(validationProblem) { StatusCode = validationProblem.Status };
        }
    }
}
