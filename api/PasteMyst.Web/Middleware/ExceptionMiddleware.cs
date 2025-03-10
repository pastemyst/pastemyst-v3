using System.Net;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.V2;

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

        var isV2 = context.Request.Path.StartsWithSegments("/api/v2");

        if (isV2)
        {
            var response = new ErrorResponseV2(exception.Message);
            await context.Response.WriteAsJsonAsync(response);
        }
        else
        {
            var response = new ErrorResponse((int)exception.Status, exception.Message);
            await context.Response.WriteAsJsonAsync(response);
        }
    }

    private async Task HandleDefaultExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError("Unhandled exception: {}", exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var isV2 = context.Request.Path.StartsWithSegments("/api/v2");

        if (isV2)
        {
            var response = new ErrorResponseV2("Internal server error.");
            await context.Response.WriteAsJsonAsync(response);
        }
        else
        {
            var response = new ErrorResponse((int)HttpStatusCode.InternalServerError, "Internal server error.");
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}