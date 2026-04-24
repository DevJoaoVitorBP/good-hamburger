using GoodHamburger.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.API.Middlewares;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessRuleValidationException exception)
        {
            logger.LogWarning(exception, "Business validation failed for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemDetailsAsync(context, StatusCodes.Status400BadRequest, "Business rule validation failed", exception.Message);
        }
        catch (ResourceNotFoundException exception)
        {
            logger.LogWarning(exception, "Resource was not found for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemDetailsAsync(context, StatusCodes.Status404NotFound, "Resource not found", exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error for {Method} {Path}", context.Request.Method, context.Request.Path);

            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred. Please try again."
            );
        }
    }

    private static Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;

        ProblemDetails problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["correlationId"] = context.TraceIdentifier;

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
