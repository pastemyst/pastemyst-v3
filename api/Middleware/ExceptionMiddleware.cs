using System.Net;
using pastemyst.Exceptions;
using pastemyst.Models;

namespace pastemyst.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HttpException e)
        {
            await HandleHttpExceptionAsync(context, e);
        }
        catch (Exception e)
        {
            await HandleDefaultExceptionAsync(context, e);
        }
    }

    private async Task HandleHttpExceptionAsync(HttpContext context, HttpException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)exception.Status;

        var response = new ErrorResponse((int)exception.Status, exception.Message);

        await context.Response.WriteAsJsonAsync(response);
    }

    private async Task HandleDefaultExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError("Unhandled exception: {}", exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new ErrorResponse((int)HttpStatusCode.InternalServerError, "Internal server error.");

        await context.Response.WriteAsJsonAsync(response);
    }
}