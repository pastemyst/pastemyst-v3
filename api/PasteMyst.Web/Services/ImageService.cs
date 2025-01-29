using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace PasteMyst.Web.Services;

public class ImageService(MongoService mongo)
{
    public async Task<string> UploadImageAsync(byte[] bytes, string contentType)
    {
        var image = await mongo.Images.UploadFromBytesAsync("", bytes, new()
        {
            Metadata = new BsonDocument(new Dictionary<string, string>
            {
                { "Content-Type", contentType }
            })
        });

        return image.ToString();
    }

    public async Task<GridFSFileInfo<ObjectId>> FindByIdAsync(string id, CancellationToken cancellationToken)
    {
        var filter = Builders<GridFSFileInfo<ObjectId>>.Filter.Eq(fs => fs.Id, ObjectId.Parse(id));
        return await (await mongo.Images.FindAsync(filter, cancellationToken: cancellationToken)).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<byte[]> DownloadByIdAsync(string id, CancellationToken cancellationToken)
    {
        return await mongo.Images.DownloadAsBytesAsync(new ObjectId(id), cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id)
    {
        await mongo.Images.DeleteAsync(new ObjectId(id));
    }

    public async Task<bool> ExistsByIdAsync(string id, CancellationToken cancellationToken) => await FindByIdAsync(id, cancellationToken) is not null;
}
