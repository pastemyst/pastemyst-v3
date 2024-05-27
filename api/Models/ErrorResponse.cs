namespace pastemyst.Models;

public class ErrorResponse(int statusCode, string message)
{
    public int StatusCode { get; set; } = statusCode;
    public string Message { get; set; } = message;
}