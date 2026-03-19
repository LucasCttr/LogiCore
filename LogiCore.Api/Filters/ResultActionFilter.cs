using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace LogiCore.Api.Filters;

/// <summary>
/// Converts Result&lt;T&gt; returned from actions into proper IActionResult (200/404/400).
/// Registered globally to avoid repeating mapping in controllers.
/// </summary>
public class ResultActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        var objResult = executed.Result as ObjectResult;
        if (objResult == null) return;

        var value = objResult.Value;
        if (value == null) return;

        var type = value.GetType();
        if (!type.IsGenericType) return;

        var genDef = type.GetGenericTypeDefinition();
        if (genDef != typeof(LogiCore.Application.Common.Models.Result<>)) return;

        var isSuccessProp = type.GetProperty("IsSuccess");
        var valueProp = type.GetProperty("Value");
        var errorProp = type.GetProperty("Error");

        if (isSuccessProp == null || valueProp == null || errorProp == null) return;

        var isSuccess = (bool)(isSuccessProp.GetValue(value) ?? false);
        var innerValue = valueProp.GetValue(value);
        var error = errorProp.GetValue(value) as string;

        if (isSuccess)
        {
            if (innerValue is null)
            {
                executed.Result = new NoContentResult();
                return;
            }

            var statusCode = context.HttpContext.Request.Method == HttpMethods.Post
                ? StatusCodes.Status201Created
                : StatusCodes.Status200OK;

            var objectResult = new ObjectResult(value) { StatusCode = statusCode };

            if (statusCode == StatusCodes.Status201Created)
            {
                string? idPart = null;
                if (innerValue is Guid g) idPart = g.ToString();
                else
                {
                    var idProp = innerValue.GetType().GetProperty("Id");
                    if (idProp != null) idPart = idProp.GetValue(innerValue)?.ToString();
                    else if (innerValue is string s) idPart = s;
                }

                if (!string.IsNullOrEmpty(idPart))
                {
                    var basePath = context.HttpContext.Request.Path.Value?.TrimEnd('/') ?? string.Empty;
                    var location = $"{basePath}/{idPart}";
                    context.HttpContext.Response.Headers["Location"] = location;
                }
            }

            executed.Result = objectResult;
            return;
        }

        if (!string.IsNullOrEmpty(error) && error.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            executed.Result = new NotFoundObjectResult(error);
            return;
        }

        executed.Result = new BadRequestObjectResult(error ?? "An error occurred");
    }
}
