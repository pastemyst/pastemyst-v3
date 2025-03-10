namespace PasteMyst.Web.Models.V2;

public class ErrorResponseV2(string message)
{
    public string StatusMessage { get; set; } = message;
}