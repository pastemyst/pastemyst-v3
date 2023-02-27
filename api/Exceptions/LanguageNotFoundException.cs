using System.Net;

namespace pastemyst.Exceptions;

public class LanguageNotFoundException : HttpException
{
    public LanguageNotFoundException() : base(HttpStatusCode.NotFound, "The languages was not found.")
    {
    }
}