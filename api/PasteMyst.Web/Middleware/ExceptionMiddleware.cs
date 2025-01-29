using System.Net;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Models;

namespace PasteMyst.Web.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
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
        logger.LogError("Unhandled exception: {}", exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new ErrorResponse((int)HttpStatusCode.InternalServerError, "Internal server error.");

        await context.Response.WriteAsJsonAsync(response);
    }
}