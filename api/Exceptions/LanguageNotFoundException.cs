using System.Net;

namespace pastemyst.Exceptions;

public class LanguageNotFoundException() : HttpException(HttpStatusCode.NotFound, "The language was not found.");
