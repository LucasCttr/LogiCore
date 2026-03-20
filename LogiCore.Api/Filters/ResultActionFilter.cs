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

        // 1. Solo interceptamos si el resultado es un ObjectResult
        if (executed.Result is not ObjectResult objResult || objResult.Value == null) 
            return;

        var value = objResult.Value;
        var type = value.GetType();

        // 2. Verificamos que sea nuestro Result<T> genérico
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Result<>)) 
            return;

        // 3. Extraemos las propiedades clave del Result<T>
        var isSuccess = (bool)(type.GetProperty("IsSuccess")?.GetValue(value) ?? false);
        var innerValue = type.GetProperty("Value")?.GetValue(value);
        var error = type.GetProperty("Error")?.GetValue(value) as string;

        // --- CASO ÉXITO (200 OK / 201 Created) ---
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

            // Retornamos el DTO interno directamente para un JSON limpio
            executed.Result = new ObjectResult(innerValue) { StatusCode = statusCode };
            return;
        }

        // --- CASO ERROR (400 / 404 con formato unificado de ValidationProblemDetails) ---

        // Creamos el diccionario de errores para que coincida con FluentValidation
        var errorDictionary = new Dictionary<string, string[]>
        {
            { "Logic", new[] { error ?? "Se produjo un error en la operación de negocio." } }
        };

        var validationProblem = new ValidationProblemDetails(errorDictionary)
        {
            Instance = context.HttpContext.Request.Path,
            Title = "One or more validation errors occurred.",
            Extensions = { ["traceId"] = context.HttpContext.TraceIdentifier }
        };

        // Determinamos el Status Code según el mensaje
        if (!string.IsNullOrEmpty(error) && error.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            validationProblem.Status = StatusCodes.Status404NotFound;
            executed.Result = new NotFoundObjectResult(validationProblem);
        }
        else
        {
            validationProblem.Status = StatusCodes.Status400BadRequest;
            executed.Result = new BadRequestObjectResult(validationProblem);
        }
    }
}