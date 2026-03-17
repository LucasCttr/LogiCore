using System;
using Microsoft.AspNetCore.Mvc;
using LogiCore.Application.Common.Models;

namespace LogiCore.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, IActionResult> successRoute)
    {
        if (result is null) return new BadRequestObjectResult("Result was null");

        if (result.IsSuccess)
            return successRoute(result.Value!);

        var error = result.Error ?? "An error occurred";

        return error.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? new NotFoundObjectResult(error)
            : new BadRequestObjectResult(error);
    }
}
