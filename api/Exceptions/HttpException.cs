using System.Net;
using System.Text.Json.Serialization;

namespace pastemyst.Exceptions;

public class HttpException : Exception
{
    [JsonIgnore]
    public HttpStatusCode Status { get; set; }

    public HttpException(HttpStatusCode status, string message) : base(message)
    {
        Status = status;
    }
}
