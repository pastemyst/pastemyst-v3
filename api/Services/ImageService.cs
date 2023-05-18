using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace pastemyst.Services;

public interface IImageService
{
    Task<string> UploadImageAsync(byte[] bytes, string contentType);

    Task<GridFSFileInfo<ObjectId>> FindByIdAsync(string id);

    Task<byte[]> DownloadByIdAsync(string id);

    Task DeleteAsync(string id);

    Task<bool> ExistsByIdAsync(string id);
}

public class ImageService : IImageService
{
    private readonly IMongoService _mongo;

    private readonly IIdProvider _idProvider;

    public ImageService(IIdProvider idProvider, IMongoService mongo)
    {
        _idProvider = idProvider;
        _mongo = mongo;
    }

    public async Task<string> UploadImageAsync(byte[] bytes, string contentType)
    {
        var image = await _mongo.Images.UploadFromBytesAsync("", bytes, new()
        {
            Metadata = new BsonDocument(new Dictionary<string, string>
            {
                { "Content-Type", contentType }
            })
        });

        return image.ToString();
    }

    public async Task<GridFSFileInfo<ObjectId>> FindByIdAsync(string id)
    {
        var filter = Builders<GridFSFileInfo<ObjectId>>.Filter.Eq(fs => fs.Id, ObjectId.Parse(id));
        return await _mongo.Images.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<byte[]> DownloadByIdAsync(string id)
    {
        return await _mongo.Images.DownloadAsBytesAsync(new ObjectId(id));
    }

    public async Task DeleteAsync(string id)
    {
        await _mongo.Images.DeleteAsync(new ObjectId(id));
    }

    public async Task<bool> ExistsByIdAsync(string id) => await FindByIdAsync(id) is not null;
}
