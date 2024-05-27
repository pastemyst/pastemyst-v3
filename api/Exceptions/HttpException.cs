using System.Net;
using System.Text.Json.Serialization;

namespace pastemyst.Exceptions;

public class HttpException(HttpStatusCode status, string message) : Exception(message)
{
    [JsonIgnore] public HttpStatusCode Status { get; } = status;
}