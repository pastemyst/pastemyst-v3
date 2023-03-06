using Microsoft.EntityFrameworkCore;
using pastemyst.DbContexts;
using pastemyst.Models;
using pastemyst.Utils;

namespace pastemyst.Services;

public interface IPasteService
{
    public Task<Paste> CreatePasteAsync(PasteCreateInfo createInfo);

    public Task<bool> ExistsByIdAsync(string id);
}

public class PasteService : IPasteService
{
    private readonly IIdProvider _idProvider;
    private readonly ILanguageProvider _languageProvider;
    private readonly IPastyService _pastyService;
    private readonly DataContext _dbContext;

    public PasteService(IIdProvider idProvider, DataContext dbContext, ILanguageProvider languageProvider, IPastyService pastyService)
    {
        _idProvider = idProvider;
        _dbContext = dbContext;
        _languageProvider = languageProvider;
        _pastyService = pastyService;
    }

    public async Task<Paste> CreatePasteAsync(PasteCreateInfo createInfo)
    {
        // TODO: User stuff
        
        // TODO: Validate expires in?

        var paste = new Paste
        {
            Id = await _idProvider.GenerateId(async id => await ExistsByIdAsync(id)),
            CreatedAt = DateTime.UtcNow,
            ExpiresIn = createInfo.ExpiresIn,
            DeletesAt = ExpiresInUtils.ToDeletesAt(DateTime.UtcNow, createInfo.ExpiresIn),
            Title = createInfo.Title,
            Pasties = new List<Pasty>()
        };

        foreach (var pasty in createInfo.Pasties)
        {
            var langName = pasty.Language is null ? "Text" : _languageProvider.FindByName(pasty.Language).Name;

            paste.Pasties.Add(new Pasty
            {
                Id = await _idProvider.GenerateId(async id => await _pastyService.ExistsByIdAsync(id)),
                Tile = pasty.Title,
                Content = pasty.Content,
                Language = langName
            });
        }

        await _dbContext.Pastes.AddAsync(paste);
        await _dbContext.SaveChangesAsync();       

        return paste;
    }

    public async Task<bool> ExistsByIdAsync(string id)
    {
        return await _dbContext.Pastes.AnyAsync(p => p.Id == id);
    }
}