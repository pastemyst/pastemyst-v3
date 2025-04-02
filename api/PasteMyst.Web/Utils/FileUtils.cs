using Microsoft.AspNetCore.StaticFiles;

namespace PasteMyst.Web.Utils;

public static class FileUtils
{
    public static string GetFileExtensionFromMimeType(string mimeType)
    {
        var contentTypeProvider = new FileExtensionContentTypeProvider();
        foreach (var mapping in contentTypeProvider.Mappings)
        {
            if (mapping.Value.Equals(mimeType, StringComparison.OrdinalIgnoreCase))
            {
                return mapping.Key;
            }
        }

        return ".png";
    }
}