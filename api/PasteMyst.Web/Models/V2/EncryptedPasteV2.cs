using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PasteMyst.Web.Models.V2;

[BsonIgnoreExtraElements]
public class EncryptedPasteV2 : BasePasteV2
{
    public string EncryptedData { get; set; }

    public string EncryptedKey { get; set; }

    public string Salt { get; set; }
}
