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

        // 1. Check if the action result is an ObjectResult containing a Result<T>
        if (executed.Result is ObjectResult objResult && objResult.Value is Application.Common.Models.IResult result)
        {
            // --- SUCCESS CASE ---
            if (result.IsSuccess)
            {
                var content = result.GetValue();
                if (content is null) { executed.Result = new NoContentResult(); return; }

                var statusCode = context.HttpContext.Request.Method == HttpMethods.Post
                    ? StatusCodes.Status201Created
                    : StatusCodes.Status200OK;

                executed.Result = new ObjectResult(content) { StatusCode = statusCode };
            }
            // --- ERROR CASE ---
            else
            {
                var errorDictionary = new Dictionary<string, string[]>
                {
                    { "Logic", new[] { result.Error ?? "Business error occurred." } }
                };

                var validationProblem = new ValidationProblemDetails(errorDictionary)
                {
                    Instance = context.HttpContext.Request.Path,
                    Title = "One or more validation errors occurred.",
                    Status = result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true 
                        ? StatusCodes.Status404NotFound 
                        : StatusCodes.Status400BadRequest
                };

                executed.Result = new ObjectResult(validationProblem) { StatusCode = validationProblem.Status };
            }
        }
    }
}