using System.IO.Compression;
using System.Net;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using pastemyst.Exceptions;
using pastemyst.Models;
using pastemyst.Utils;

namespace pastemyst.Services;

public class PasteService(
    IdProvider idProvider,
    LanguageProvider languageProvider,
    UserContext userContext,
    ActionLogger actionLogger,
    MongoService mongo)
{
    public async Task<Paste> CreateAsync(PasteCreateInfo createInfo)
    {
        switch (createInfo.Pinned)
        {
            case true when !userContext.IsLoggedIn():
                throw new HttpException(HttpStatusCode.Unauthorized,
                    "Can't create a pinned paste while unauthorized.");
            case true when createInfo.Private || createInfo.Anonymous:
                throw new HttpException(HttpStatusCode.Unauthorized,
                    "Can't create a private or anonymous pinned paste.");
        }

        switch (createInfo.Private)
        {
            case true when !userContext.IsLoggedIn():
                throw new HttpException(HttpStatusCode.Unauthorized,
                    "Can't create a private paste while unauthorized.");
            case true when createInfo.Anonymous:
                throw new HttpException(HttpStatusCode.BadRequest,
                    "Can't create a private anonymous paste.");
        }

        switch (createInfo.Tags.Count != 0)
        {
            case true when !userContext.IsLoggedIn():
                throw new HttpException(HttpStatusCode.Unauthorized,
                    "Can't create a tagged paste while unauthorized.");
            case true when createInfo.Anonymous:
                throw new HttpException(HttpStatusCode.BadRequest,
                    "Can't create a tagged anonymous paste.");
        }

        var createdAt = DateTime.UtcNow;

        var paste = new Paste
        {
            Id = await idProvider.GenerateId(async id => await ExistsByIdAsync(id)),
            CreatedAt = createdAt,
            ExpiresIn = createInfo.ExpiresIn,
            DeletesAt = ExpiresInUtils.ToDeletesAt(createdAt, createInfo.ExpiresIn),
            Title = createInfo.Title,
            OwnerId = createInfo.Anonymous ? null : userContext.Self?.Id,
            Private = createInfo.Private,
            Pinned = createInfo.Pinned,
            Tags = createInfo.Tags.Select(t => t.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList(),
            Pasties = new List<Pasty>()
        };

        foreach (var pasty in createInfo.Pasties)
        {
            var langName = pasty.Language is null ? "Text" : languageProvider.FindByName(pasty.Language).Name;

            if (langName == "Autodetect")
            {
                langName = (await languageProvider.AutodetectLanguageAsync(pasty.Content)).Name;
            }

            paste.Pasties.Add(new Pasty
            {
                Id = idProvider.GenerateId(id => paste.Pasties.Any(p => p.Id == id)),
                Title = pasty.Title ?? "",
                Content = pasty.Content,
                Language = langName
            });
        }

        await mongo.Pastes.InsertOneAsync(paste);

        await actionLogger.LogActionAsync(ActionLogType.PasteCreated, paste.Id);

        return paste;
    }

    public async Task<Paste> GetAsync(string id)
    {
        var paste = await mongo.Pastes.Find(p => p.Id == id).FirstOrDefaultAsync();

        if (paste is null) throw new HttpException(HttpStatusCode.NotFound, "Paste not found");

        if (paste.DeletesAt <= DateTime.UtcNow)
        {
            await mongo.Pastes.DeleteOneAsync(p => p.Id == paste.Id);

            throw new HttpException(HttpStatusCode.NotFound, "Paste not found");
        }

        if (paste.Private && (!userContext.IsLoggedIn() || userContext.Self.Id != paste.OwnerId))
            throw new HttpException(HttpStatusCode.NotFound, "Paste not found");

        // only the paste owner can see the tags
        if (!userContext.IsLoggedIn() || userContext.Self.Id != paste.OwnerId)
            paste.Tags = new();

        return paste;
    }

    public async Task<PasteStats> GetStatsAsync(string id)
    {
        var paste = await GetAsync(id);

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

    public async Task<List<LanguageStat>> GetLanguageStatsAsync(string id)
    {
        var paste = await GetAsync(id);

        return GetLanguageStats(paste);
    }

    public List<LanguageStat> GetLanguageStats(Paste paste)
    {
        var stats = new List<LanguageStat>();

        var charsPerLanguage = new Dictionary<string, int>();
        var totalChars = 0;

        foreach (var pasty in paste.Pasties)
        {
            if (charsPerLanguage.ContainsKey(pasty.Language))
            {
                charsPerLanguage[pasty.Language] += pasty.Content.Length;
            }
            else
            {
                charsPerLanguage.Add(pasty.Language, pasty.Content.Length);
            }

            totalChars += pasty.Content.Length;
        }

        if (totalChars == 0) return stats;

        foreach (var entry in charsPerLanguage)
        {
            var percentage = entry.Value / (float)totalChars * 100;

            if (percentage == 0) continue;

            var language = languageProvider.FindByName(entry.Key);

            stats.Add(new LanguageStat
            {
                Language = language,
                Percentage = percentage
            });
        }

        stats.Sort((a, b) => b.Percentage.CompareTo(a.Percentage));

        return stats;
    }

    public async Task<long> GetActiveCountAsync()
    {
        return await mongo.Pastes.CountDocumentsAsync(new BsonDocument());
    }

    public async Task DeleteAsync(string id)
    {
        if (!userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to delete pastes.");

        var paste = await GetAsync(id);

        if (paste.OwnerId is null || paste.OwnerId != userContext.Self.Id)
        {
            // Returning not found instead of unauthorized to not expose that the paste exists.
            if (paste.Private)
                throw new HttpException(HttpStatusCode.NotFound, "Paste not found.");

            throw new HttpException(HttpStatusCode.Unauthorized, "You can only delete your own pastes.");
        }

        await mongo.Pastes.DeleteOneAsync(p => p.Id == paste.Id);

        await actionLogger.LogActionAsync(ActionLogType.PasteDeleted, paste.Id);
    }

    public async Task<bool> IsStarredAsync(string id)
    {
        if (!userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to star pastes.");

        var paste = await GetAsync(id);

        if (paste.OwnerId is null || paste.OwnerId != userContext.Self.Id)
        {
            // Returning not found instead of unauthorized to not expose that the paste exists.
            if (paste.Private)
                throw new HttpException(HttpStatusCode.NotFound, "Paste not found.");
        }

        return paste.Stars.Any(u => u == userContext.Self.Id);
    }

    public async Task ToggleStarAsync(string id)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to star pastes.");
        }

        var paste = await GetAsync(id);

        if (paste.OwnerId is null || paste.OwnerId != userContext.Self.Id)
        {
            // Returning not found instead of unauthorized to not expose that the paste exists.
            if (paste.Private)
            {
                throw new HttpException(HttpStatusCode.NotFound, "Paste not found.");
            }
        }

        if (paste.Stars.Any(u => u == userContext.Self.Id))
        {
            paste.Stars.Remove(userContext.Self.Id);
        }
        else
        {
            paste.Stars.Add(userContext.Self.Id);
        }

        var update = Builders<Paste>.Update.Set(p => p.Stars, paste.Stars);
        await mongo.Pastes.UpdateOneAsync(p => p.Id == paste.Id, update);
    }

    public async Task TogglePinnedAsync(string id)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to pin/unpin pastes.");
        }

        var paste = await GetAsync(id);

        if (paste.OwnerId is null)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Only owned pastes can be pinned.");
        }

        if (paste.OwnerId != userContext.Self.Id)
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You can only pin/unpin your own pastes.");
        }

        if (paste.Private)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "You can't pin private pastes.");
        }

        var update = Builders<Paste>.Update.Set(p => p.Pinned, !paste.Pinned);
        await mongo.Pastes.UpdateOneAsync(p => p.Id == paste.Id, update);
    }

    public async Task TogglePrivateAsync(string id)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to change the private status of pastes.");
        }

        var paste = await GetAsync(id);

        if (paste.OwnerId is null)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Only owned pastes can be set/unset to private.");
        }

        if (paste.OwnerId != userContext.Self.Id)
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You can only change the private status of your own pastes.");
        }

        if (paste.Pinned)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "You can't private pinned pastes.");
        }

        var update = Builders<Paste>.Update.Set(p => p.Private, !paste.Private);
        await mongo.Pastes.UpdateOneAsync(p => p.Id == paste.Id, update);
    }

    public async Task<bool> ExistsByIdAsync(string id)
    {
        return await mongo.Pastes.Find(p => p.Id == id).FirstOrDefaultAsync() is not null;
    }

    public async Task<(byte[] zip, string title)> GetPasteAsZip(string id)
    {
        var paste = await GetAsync(id);

        using var memoryStream = new MemoryStream();

        using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var pasty in paste.Pasties)
            {
                var lang = languageProvider.FindByName(pasty.Language);

                var zipEntry = zipArchive.CreateEntry(pasty.Title + lang.Extensions.First());

                using var entryStream = zipEntry.Open();
                using var writer = new StreamWriter(entryStream, Encoding.UTF8);
                await writer.WriteAsync(pasty.Content);
            }
        }

        memoryStream.Position = 0;

        return (memoryStream.ToArray(), paste.Title);
    }

    public async Task<Paste> EditAsync(string id, PasteEditInfo editInfo)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to edit pastes.");
        }

        var paste = await GetAsync(id);

        if (paste.OwnerId is null)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Only owned pastes can be edited.");
        }

        if (paste.OwnerId != userContext.Self.Id)
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You can only edit your own pastes.");
        }

        var pasteHistory = new PasteHistory
        {
            EditedAt = DateTime.UtcNow,
            Title = paste.Title.Clone() as string,
            Pasties = new List<Pasty>(paste.Pasties)
        };

        paste.Title = editInfo.Title;
        paste.Pasties = new();

        foreach (var pasty in editInfo.Pasties)
        {
            var langName = pasty.Language is null ? "Text" : languageProvider.FindByName(pasty.Language).Name;

            if (langName == "Autodetect")
            {
                langName = (await languageProvider.AutodetectLanguageAsync(pasty.Content)).Name;
            }

            paste.Pasties.Add(new Pasty
            {
                Id = pasty.Id ?? idProvider.GenerateId(id => paste.Pasties.Any(p => p.Id == id)),
                Title = pasty.Title ?? "",
                Content = pasty.Content,
                Language = langName
            });
        }

        paste.History.Add(pasteHistory);

        var update = Builders<Paste>.Update
            .Set(p => p.Title, paste.Title ?? "")
            .Set(p => p.Pasties, paste.Pasties)
            .Set(p => p.History, paste.History)
            .Set(p => p.Tags, editInfo.Tags.Select(t => t.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList());
        await mongo.Pastes.UpdateOneAsync(p => p.Id == paste.Id, update);

        return paste;
    }
}
