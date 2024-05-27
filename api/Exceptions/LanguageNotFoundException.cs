using System.Net;

namespace pastemyst.Exceptions;

public class LanguageNotFoundException() : HttpException(HttpStatusCode.NotFound, "The languages was not found.");