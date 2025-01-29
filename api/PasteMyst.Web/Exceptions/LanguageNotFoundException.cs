using System.Net;

namespace PasteMyst.Web.Exceptions;

public class LanguageNotFoundException() : HttpException(HttpStatusCode.NotFound, "The language was not found.");
