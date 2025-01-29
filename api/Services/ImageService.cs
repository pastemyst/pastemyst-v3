using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace pastemyst.Services;

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

    public async Task<GridFSFileInfo<ObjectId>> FindByIdAsync(string id, CancellationToken token)
    {
        var filter = Builders<GridFSFileInfo<ObjectId>>.Filter.Eq(fs => fs.Id, ObjectId.Parse(id));
        return await (await mongo.Images.FindAsync(filter, cancellationToken: token)).FirstOrDefaultAsync(cancellationToken: token);
    }

    public async Task<byte[]> DownloadByIdAsync(string id, CancellationToken token)
    {
        return await mongo.Images.DownloadAsBytesAsync(new ObjectId(id), cancellationToken: token);
    }

    public async Task DeleteAsync(string id)
    {
        await mongo.Images.DeleteAsync(new ObjectId(id));
    }

    public async Task<bool> ExistsByIdAsync(string id, CancellationToken token) => await FindByIdAsync(id, token) is not null;
}
