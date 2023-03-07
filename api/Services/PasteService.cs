using System.Net;
using Microsoft.EntityFrameworkCore;
using pastemyst.DbContexts;
using pastemyst.Exceptions;
using pastemyst.Models;
using pastemyst.Utils;

namespace pastemyst.Services;

public interface IPasteService
{
    public Task<Paste> CreatePasteAsync(PasteCreateInfo createInfo);

    public Task<Paste> GetPasteAsync(string id);

    public Task<PasteStats> GetPasteStatsAsync(string id);

    public Task<int> GetActivePastesCountAsync();

    public Task DeletePasteAsync(string id);

    public Task ToggleStarPasteAsync(string id);

    public Task<bool> IsPasteStarredAsync(string id);

    public Task<bool> ExistsByIdAsync(string id);
}

public class PasteService : IPasteService
{
    private readonly IIdProvider _idProvider;
    private readonly ILanguageProvider _languageProvider;
    private readonly IPastyService _pastyService;
    private readonly IAuthService _authService;
    private readonly DataContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;

    public PasteService(IIdProvider idProvider, DataContext dbContext, ILanguageProvider languageProvider,
        IPastyService pastyService, IAuthService authService, IHttpContextAccessor contextAccessor)
    {
        _idProvider = idProvider;
        _dbContext = dbContext;
        _languageProvider = languageProvider;
        _pastyService = pastyService;
        _authService = authService;
        _contextAccessor = contextAccessor;
    }

    public async Task<Paste> CreatePasteAsync(PasteCreateInfo createInfo)
    {
        var user = await _authService.GetSelfAsync(_contextAccessor.HttpContext);

        if (createInfo.Private && user is null)
        {
            throw new HttpException(HttpStatusCode.Unauthorized,
                "Can't create a private paste while unauthorized.");
        }

        if (createInfo.Private && createInfo.Anonymous)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Can't create a private anonymous paste.");
        }

        var paste = new Paste
        {
            Id = await _idProvider.GenerateId(async id => await ExistsByIdAsync(id)),
            CreatedAt = DateTime.UtcNow,
            ExpiresIn = createInfo.ExpiresIn,
            DeletesAt = ExpiresInUtils.ToDeletesAt(DateTime.UtcNow, createInfo.ExpiresIn),
            Title = createInfo.Title,
            Owner = createInfo.Anonymous ? null : user,
            Private = createInfo.Private,
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

    public async Task<Paste> GetPasteAsync(string id)
    {
        var user = await _authService.GetSelfAsync(_contextAccessor.HttpContext);

        var paste = await _dbContext.Pastes
            .Include(p => p.Pasties)
            .Include(p => p.Stars)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paste is null) throw new HttpException(HttpStatusCode.NotFound, "Paste not found");

        if (paste.Private && (user is null || user.Id != paste.Owner.Id))
            throw new HttpException(HttpStatusCode.NotFound, "Paste not found");

        return paste;
    }

    public async Task<PasteStats> GetPasteStatsAsync(string id)
    {
        var paste = await GetPasteAsync(id);

        var res = new PasteStats();

        foreach (var pasty in paste.Pasties)
        {
            var lines = pasty.Content.Count(c => c == '\n') + 1;
            var words = pasty.Content
                .Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Length;
            var bytes = System.Text.Encoding.UTF8.GetByteCount(pasty.Content);

            res.Pasties[pasty.Id] = new Stats
            {
                Lines = lines,
                Words = words,
                Bytes = bytes
            };

            res.Lines += lines;
            res.Words += words;
            res.Bytes += bytes;
        }

        return res;
    }

    public async Task<int> GetActivePastesCountAsync()
    {
        return await _dbContext.Pastes.CountAsync();
    }

    public async Task DeletePasteAsync(string id)
    {
        var user = await _authService.GetSelfAsync(_contextAccessor.HttpContext);

        if (user is null)
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to delete pastes.");

        var paste = await GetPasteAsync(id);

        if (paste.Owner is null || paste.Owner.Id != user.Id)
        {
            // Returning not found instead of unauthorized to not expose that the paste exists.
            if (paste.Private)
                throw new HttpException(HttpStatusCode.NotFound, "Paste not found.");

            throw new HttpException(HttpStatusCode.Unauthorized, "You can only delete your own pastes.");
        }

        _dbContext.Pastes.Remove(paste);
        await _dbContext.SaveChangesAsync();
    }

    public async Task ToggleStarPasteAsync(string id)
    {
        var user = await _authService.GetSelfAsync(_contextAccessor.HttpContext);

        if (user is null)
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to star pastes.");

        var paste = await GetPasteAsync(id);

        if (paste.Owner is null || paste.Owner.Id != user.Id)
        {
            // Returning not found instead of unauthorized to not expose that the paste exists.
            if (paste.Private)
                throw new HttpException(HttpStatusCode.NotFound, "Paste not found.");
        }

        if (paste.Stars.Any(u => u.Id == user.Id))
        {
            paste.Stars.Remove(user);
        }
        else
        {
            paste.Stars.Add(user);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsPasteStarredAsync(string id)
    {
        var user = await _authService.GetSelfAsync(_contextAccessor.HttpContext);

        if (user is null)
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to star pastes.");

        var paste = await GetPasteAsync(id);

        if (paste.Owner is null || paste.Owner.Id != user.Id)
        {
            // Returning not found instead of unauthorized to not expose that the paste exists.
            if (paste.Private)
                throw new HttpException(HttpStatusCode.NotFound, "Paste not found.");
        }

        return paste.Stars.Any(u => u.Id == user.Id);
    }

    public async Task<bool> ExistsByIdAsync(string id)
    {
        return await _dbContext.Pastes.AnyAsync(p => p.Id == id);
    }
}