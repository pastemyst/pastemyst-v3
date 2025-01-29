using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using PasteMyst.Web.Extensions;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Utils;

namespace PasteMyst.Web.Services;

public class PasteService(
    IdProvider idProvider,
    LanguageProvider languageProvider,
    UserContext userContext,
    EncryptionContext encryptionContext,
    ActionLogger actionLogger,
    MongoService mongo)
{
    public async Task<Paste> CreateAsync(PasteCreateInfo createInfo, CancellationToken cancellationToken)
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

        if (userContext.IsLoggedIn() && !createInfo.Anonymous && !userContext.HasScope(Scope.Paste))
        {
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.Paste.ToEnumString()}.");
        }

        List<Pasty> pasties = [];
        foreach (var pasty in createInfo.Pasties)
        {
            var langName = pasty.Language is null ? "Text" : languageProvider.FindByName(pasty.Language).Name;

            if (langName == "Autodetect")
            {
                langName = (await languageProvider.AutodetectLanguageAsync(pasty.Content, cancellationToken)).Name;
            }

            pasties.Add(new Pasty
            {
                Id = idProvider.GenerateId(id => pasties.Any(p => p.Id == id)),
                Title = pasty.Title ?? "",
                Content = pasty.Content,
                Language = langName
            });
        }

        if (createInfo.Encrypted)
        {
            if (encryptionContext.EncryptionKey is null)
            {
                throw new HttpException(HttpStatusCode.BadRequest, "Missing encryption key");
            }

            var decryptedPasteData = new DecryptedPasteData
            {
                Pasties = pasties
            };

            var decryptedPasteDataJson = JsonSerializer.Serialize(decryptedPasteData);

            var salt = RandomNumberGenerator.GetBytes(32);
            using var deriveBytes = new Rfc2898DeriveBytes(encryptionContext.EncryptionKey, salt, 100_000, HashAlgorithmName.SHA512);
            var key = deriveBytes.GetBytes(32);

            using var aes = Aes.Create();
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(key, aes.IV);
            using var ms = new MemoryStream();
            await using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            await using (var sw = new StreamWriter(cs))
            {
                await sw.WriteAsync(decryptedPasteDataJson);
            }

            var paste = await CreateBasePaste<EncryptedPaste>(createInfo);
            paste.EncryptedData = Convert.ToBase64String(ms.ToArray());
            paste.Iv = Convert.ToBase64String(aes.IV);
            paste.Salt = Convert.ToBase64String(salt);

            await mongo.EncryptedPastes.InsertOneAsync(paste);
            await actionLogger.LogActionAsync(ActionLogType.PasteCreated, paste.Id);

            return new Paste
            {
                Id = paste.Id,
                Title = paste.Title,
                CreatedAt = paste.CreatedAt,
                ExpiresIn = paste.ExpiresIn,
                DeletesAt = paste.DeletesAt,
                OwnerId = paste.OwnerId,
                Private = paste.Private,
                Pinned = paste.Pinned,
                Tags = paste.Tags,
                Stars = paste.Stars,
                Pasties = decryptedPasteData.Pasties,
                History = decryptedPasteData.History
            };
        }
        else
        {
            var paste = await CreateBasePaste<Paste>(createInfo);
            paste.Pasties = pasties;

            await mongo.Pastes.InsertOneAsync(paste);
            await actionLogger.LogActionAsync(ActionLogType.PasteCreated, paste.Id);

            return paste;
        }
    }

    public async Task<Paste> GetAsync(string id)
    {
        var paste = await mongo.BasePastes.Find(p => p.Id == id).FirstOrDefaultAsync();

        if (paste is null) throw new HttpException(HttpStatusCode.NotFound, "Paste not found");

        if (paste.DeletesAt <= DateTime.UtcNow)
        {
            await mongo.Pastes.DeleteOneAsync(p => p.Id == paste.Id);

            throw new HttpException(HttpStatusCode.NotFound, "Paste not found");
        }

        if (paste.Private && (!userContext.IsLoggedIn() || userContext.Self.Id != paste.OwnerId || !userContext.HasScope(Scope.Paste, Scope.PasteRead)))
            throw new HttpException(HttpStatusCode.NotFound, "Paste not found");

        // only the paste owner can see the tags
        if (!userContext.IsLoggedIn() || userContext.Self.Id != paste.OwnerId)
            paste.Tags = [];

        switch (paste)
        {
            case Paste regularPaste:
                return regularPaste;
            case EncryptedPaste encryptedPaste:
            {
                var encryptionKey = encryptionContext.EncryptionKey;
                if (encryptionKey is null)
                {
                    if (encryptionContext.EncryptionKeys.TryGetValue(paste.Id, out var value))
                    {
                        encryptionKey = value;
                    }
                    else
                    {
                        throw new HttpException(HttpStatusCode.BadRequest, "Missing encryption key");
                    }
                }

                try
                {
                    var salt = Convert.FromBase64String(encryptedPaste.Salt);
                    using var deriveBytes = new Rfc2898DeriveBytes(encryptionKey, salt, 100_000, HashAlgorithmName.SHA512);
                    var key = deriveBytes.GetBytes(32);

                    using var aes = Aes.Create();
                    aes.Key = key;
                    aes.IV = Convert.FromBase64String(encryptedPaste.Iv);
                    using var decryptor = aes.CreateDecryptor(key, Convert.FromBase64String(encryptedPaste.Iv));
                    using var ms = new MemoryStream(Convert.FromBase64String(encryptedPaste.EncryptedData));
                    await using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                    using var sr = new StreamReader(cs);

                    var decryptedPasteDataJson = await sr.ReadToEndAsync();

                    var decryptedPasteData = JsonSerializer.Deserialize<DecryptedPasteData>(decryptedPasteDataJson);

                    return new Paste
                    {
                        Id = paste.Id,
                        Title = paste.Title,
                        CreatedAt = paste.CreatedAt,
                        ExpiresIn = paste.ExpiresIn,
                        DeletesAt = paste.DeletesAt,
                        OwnerId = paste.OwnerId,
                        Private = paste.Private,
                        Pinned = paste.Pinned,
                        Tags = paste.Tags,
                        Stars = paste.Stars,
                        Pasties = decryptedPasteData.Pasties,
                        History = decryptedPasteData.History
                    };
                }
                catch(CryptographicException)
                {
                    throw new HttpException(HttpStatusCode.BadRequest, "Invalid encryption key");
                }
            }
        }

        throw new InvalidOperationException();
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
            var bytes = Encoding.UTF8.GetByteCount(pasty.Content);

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

    public async Task<List<LanguageStat>> GetLanguageStatsAsync(string id, string encryptionKey = null)
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

    public async Task<long> GetActiveCountAsync(CancellationToken cancellationToken)
    {
        return await mongo.Pastes.CountDocumentsAsync(new BsonDocument(), cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id)
    {
        if (!userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to delete pastes.");

        if (!userContext.HasScope(Scope.Paste))
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.Paste.ToEnumString()}.");

        var paste = await mongo.BasePastes.Find(p => p.Id == id).FirstOrDefaultAsync();

        if (paste is null) throw new HttpException(HttpStatusCode.NotFound, "Paste not found");

        if (paste.DeletesAt <= DateTime.UtcNow)
        {
            await mongo.Pastes.DeleteOneAsync(p => p.Id == paste.Id);

            throw new HttpException(HttpStatusCode.NotFound, "Paste not found");
        }

        if (paste.Private && (!userContext.IsLoggedIn() || userContext.Self.Id != paste.OwnerId || !userContext.HasScope(Scope.Paste)))
            throw new HttpException(HttpStatusCode.NotFound, "Paste not found");

        if (paste.OwnerId is null || paste.OwnerId != userContext.Self.Id)
        {
            // Returning not found instead of unauthorized to not expose that the paste exists.
            if (paste.Private)
                throw new HttpException(HttpStatusCode.NotFound, "Paste not found.");

            throw new HttpException(HttpStatusCode.Unauthorized, "You can only delete your own pastes.");
        }

        await mongo.BasePastes.DeleteOneAsync(p => p.Id == paste.Id);

        await actionLogger.LogActionAsync(ActionLogType.PasteDeleted, paste.Id);
    }

    public async Task<bool> IsStarredAsync(string id)
    {
        if (!userContext.IsLoggedIn())
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to star pastes.");

        var paste = await GetAsync(id);

        if (paste.OwnerId is not null && paste.OwnerId == userContext.Self.Id)
            return paste.Stars.Any(u => u == userContext.Self.Id);

        // Returning not found instead of unauthorized to not expose that the paste exists.
        if (paste.Private)
            throw new HttpException(HttpStatusCode.NotFound, "Paste not found.");

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

        var update = Builders<BasePaste>.Update.Set(p => p.Stars, paste.Stars);
        await mongo.BasePastes.UpdateOneAsync(p => p.Id == paste.Id, update);
    }

    public async Task TogglePinnedAsync(string id)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to pin/unpin pastes.");
        }

        if (!userContext.HasScope(Scope.User))
        {
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.User.ToEnumString()}.");
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

        var update = Builders<BasePaste>.Update.Set(p => p.Pinned, !paste.Pinned);
        await mongo.BasePastes.UpdateOneAsync(p => p.Id == paste.Id, update);
    }

    public async Task TogglePrivateAsync(string id)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to change the private status of pastes.");
        }

        if (!userContext.HasScope(Scope.Paste))
        {
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.Paste.ToEnumString()}.");
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

        var update = Builders<BasePaste>.Update.Set(p => p.Private, !paste.Private);
        await mongo.BasePastes.UpdateOneAsync(p => p.Id == paste.Id, update);
    }

    public async Task<bool> ExistsByIdAsync(string id)
    {
        return await mongo.Pastes.Find(p => p.Id == id).FirstOrDefaultAsync() is not null;
    }

    public async Task<(byte[] zip, string title)> GetPasteAsZip(string id, CancellationToken cancellationToken)
    {
        var paste = await GetAsync(id);

        using var memoryStream = new MemoryStream();

        using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var pasty in paste.Pasties)
            {
                var lang = languageProvider.FindByName(pasty.Language);

                var zipEntry = zipArchive.CreateEntry(pasty.Title + lang.Extensions.First());

                await using var entryStream = zipEntry.Open();
                await using var writer = new StreamWriter(entryStream, Encoding.UTF8);
                await writer.WriteAsync(pasty.Content);
            }
        }

        memoryStream.Position = 0;

        return (memoryStream.ToArray(), paste.Title);
    }

    public async Task<Paste> EditTagsAsync(string id, List<string> tags)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to edit tags.");
        }

        if (!userContext.HasScope(Scope.Paste))
        {
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.Paste.ToEnumString()}.");
        }

        var paste = await GetAsync(id);

        if (paste.OwnerId is null)
        {
            throw new HttpException(HttpStatusCode.BadRequest, "Only owned pastes can have their tags edited.");
        }

        if (paste.OwnerId != userContext.Self.Id)
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You can only edit tags of your own pastes.");
        }

        paste.Tags = tags.Select(t => t.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

        var update = Builders<BasePaste>.Update.Set(p => p.Tags, paste.Tags);
        await mongo.BasePastes.UpdateOneAsync(p => p.Id == paste.Id, update);

        return paste;
    }

    public async Task<Paste> EditAsync(string id, PasteEditInfo editInfo, CancellationToken cancellationToken)
    {
        if (!userContext.IsLoggedIn())
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "You must be authorized to edit pastes.");
        }

        if (!userContext.HasScope(Scope.Paste))
        {
            throw new HttpException(HttpStatusCode.Forbidden, $"Missing required scope {Scope.Paste.ToEnumString()}.");
        }

        var paste = await GetAsync(id);
        var isEncrypted = (await mongo.BasePastes.FindAsync(p => p.Id == paste.Id, cancellationToken: cancellationToken)).FirstOrDefault(cancellationToken: cancellationToken) is EncryptedPaste;

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
            Id = idProvider.GenerateId(id => paste.History.Any(h => h.Id == id)),
            EditedAt = DateTime.UtcNow,
            Title = paste.Title.Clone() as string,
            Pasties = [..paste.Pasties]
        };

        paste.Title = editInfo.Title;
        paste.Pasties = [];

        foreach (var pasty in editInfo.Pasties)
        {
            var langName = pasty.Language is null ? "Text" : languageProvider.FindByName(pasty.Language).Name;

            if (langName == "Autodetect")
            {
                langName = (await languageProvider.AutodetectLanguageAsync(pasty.Content, cancellationToken)).Name;
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

        if (isEncrypted)
        {
            var encryptionKey = encryptionContext.EncryptionKey;
            if (encryptionKey is null)
            {
                if (encryptionContext.EncryptionKeys.TryGetValue(paste.Id, out var value))
                {
                    encryptionKey = value;
                }
                else
                {
                    throw new HttpException(HttpStatusCode.BadRequest, "Missing encryption key");
                }
            }

            var decryptedPasteData = new DecryptedPasteData
            {
                Pasties = paste.Pasties,
                History = paste.History
            };

            var decryptedPasteDataJson = JsonSerializer.Serialize(decryptedPasteData);

            try
            {
                var salt = RandomNumberGenerator.GetBytes(32);
                using var deriveBytes = new Rfc2898DeriveBytes(encryptionKey, salt, 100_000, HashAlgorithmName.SHA512);
                var key = deriveBytes.GetBytes(32);

                using var aes = Aes.Create();
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor(key, aes.IV);
                using var ms = new MemoryStream();
                await using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                await using (var sw = new StreamWriter(cs))
                {
                    await sw.WriteAsync(decryptedPasteDataJson);
                }

                var update = Builders<EncryptedPaste>.Update
                    .Set(p => p.Title, paste.Title ?? "")
                    .Set(p => p.EncryptedData, Convert.ToBase64String(ms.ToArray()))
                    .Set(p => p.Iv, Convert.ToBase64String(aes.IV))
                    .Set(p => p.Salt, Convert.ToBase64String(salt));
                await mongo.EncryptedPastes.UpdateOneAsync(p => p.Id == paste.Id, update, cancellationToken: cancellationToken);
            }
            catch(CryptographicException)
            {
                throw new HttpException(HttpStatusCode.BadRequest, "Invalid encryption key");
            }
        }
        else
        {
            var update = Builders<Paste>.Update
                .Set(p => p.Title, paste.Title ?? "")
                .Set(p => p.Pasties, paste.Pasties)
                .Set(p => p.History, paste.History);
            await mongo.Pastes.UpdateOneAsync(p => p.Id == paste.Id, update, cancellationToken: cancellationToken);
        }

        return paste;
    }

    public async Task<List<PasteHistoryCompact>> GetHistoryCompactAsync(string id)
    {
        var paste = await GetAsync(id);

        var history = paste.History
            .Select(h => new PasteHistoryCompact() { Id = h.Id, EditedAt = h.EditedAt })
            .ToList();

        history.Sort((a, b) => b.EditedAt.CompareTo(a.EditedAt));

        return history;
    }

    public async Task<Paste> GetAtEditAsync(string id, string historyId)
    {
        var paste = await GetAsync(id);

        var edit = paste.History.FirstOrDefault(h => h.Id == historyId);

        if (edit is null)
        {
            throw new HttpException(HttpStatusCode.NotFound, "Edit not found.");
        }

        paste.Title = edit.Title;
        paste.Pasties = edit.Pasties;

        return paste;
    }

    public async Task<PasteDiff> GetDiffAsync(string id, string historyId)
    {
        var paste = await GetAsync(id);

        var editIndex = paste.History.FindIndex(h => h.Id == historyId);

        if (editIndex == -1)
        {
            throw new HttpException(HttpStatusCode.NotFound, "Edit not found.");
        }

        PasteHistory newEdit;
        if (editIndex != paste.History.Count - 1)
        {
            newEdit = paste.History[editIndex + 1];
        }
        else
        {
            newEdit = new PasteHistory
            {
                Title = paste.Title,
                Pasties = paste.Pasties
            };
        }

        return new PasteDiff
        {
            CurrentPaste = paste,
            OldPaste = paste.History[editIndex],
            NewPaste = newEdit
        };
    }

    public async Task<bool> IsEncryptedAsync(string id)
    {
        var paste = await mongo.BasePastes.Find(p => p.Id == id).FirstOrDefaultAsync();

        if (paste is null) throw new HttpException(HttpStatusCode.NotFound, "Paste not found");

        if (paste.DeletesAt <= DateTime.UtcNow)
        {
            await mongo.BasePastes.DeleteOneAsync(p => p.Id == paste.Id);

            throw new HttpException(HttpStatusCode.NotFound, "Paste not found");
        }

        if (paste.Private && (!userContext.IsLoggedIn() || userContext.Self.Id != paste.OwnerId ||
                              !userContext.HasScope(Scope.Paste, Scope.PasteRead)))
        {
            throw new HttpException(HttpStatusCode.NotFound, "Paste not found");
        }

        return paste is EncryptedPaste;
    }

    private async Task<T> CreateBasePaste<T>(PasteCreateInfo createInfo) where T : BasePaste, new()
    {
        var createdAt = DateTime.UtcNow;

        return new T
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
        };
    }
}
