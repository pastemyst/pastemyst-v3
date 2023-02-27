using pastemyst.DbContexts;
using pastemyst.Models;

namespace pastemyst.Services;

public interface IImageService
{
    Task<Image> UploadImageAsync(byte[] bytes);
    
    Task<Image?> GetImageAsync(string id);
}

public class ImageService : IImageService
{
    private readonly DataContext _context;

    private readonly IIdProvider _idProvider;

    public ImageService(DataContext context, IIdProvider idProvider)
    {
        _context = context;
        _idProvider = idProvider;
    }

    public async Task<Image> UploadImageAsync(byte[] bytes)
    {
        var image = new Image
        {
            Id = _idProvider.GenerateId(ExistsById),
            CreatedAt = DateTime.UtcNow,
            Bytes = bytes
        };

        await _context.Images.AddAsync(image);

        await _context.SaveChangesAsync();

        return image;
    }

    public async Task<Image?> GetImageAsync(string id) => await _context.Images.FindAsync(id);

    public bool ExistsById(string id) => _context.Images.Any(img => img.Id == id);
}