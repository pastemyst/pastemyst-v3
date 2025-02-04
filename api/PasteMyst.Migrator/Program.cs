using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Models.V2;
using PasteMyst.Web.Serializers;
using PasteMyst.Web.Services;
using ShellProgressBar;

Console.WriteLine("Preprocessing the database...");

var process = new System.Diagnostics.Process
{
    StartInfo = new System.Diagnostics.ProcessStartInfo
    {
        FileName = "dub",
        Arguments = "preprocess.d",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    }
};

process.OutputDataReceived += (sender, args) => {};
process.ErrorDataReceived += (sender, args) => {};

process.Start();
process.BeginOutputReadLine();
process.BeginErrorReadLine();
process.WaitForExit();

if (process.ExitCode != 0)
{
    Console.WriteLine("Preprocessing failed.");
    return;
}

Console.WriteLine("Migrating the database...");

var connectionString = "mongodb://127.0.0.1:27017";

BsonSerializer.TryRegisterSerializer(new CustomEnumStringSerializer<ExpiresIn>());

var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);

var mongoClient = new MongoClient(connectionString);

var v2Db = mongoClient.GetDatabase("pastemyst-v2");
var v3Db = mongoClient.GetDatabase("pastemyst");

var usersV2 = v2Db.GetCollection<UserV2>("users");
var pastesV2 = v2Db.GetCollection<PasteV2>("pastes").Find(Builders<PasteV2>.Filter.Eq(p => p.Encrypted, false)).ToList();
var encryptedPastesV2 = v2Db.GetCollection<EncryptedPasteV2>("pastes").Find(Builders<EncryptedPasteV2>.Filter.Eq(p => p.Encrypted, true)).ToList();
var apiKeysV2 = v2Db.GetCollection<ApiKeyV2>("api-keys");

var usersV3 = v3Db.GetCollection<User>("users");
var basePastesV3 = v3Db.GetCollection<BasePaste>("pastes");
var pastesV3 = basePastesV3.OfType<Paste>();
var encryptedPastesV3 = basePastesV3.OfType<EncryptedPaste>();
var accessTokensV3 = v3Db.GetCollection<AccessToken>("accessTokens");
var actionLogsV3 = v3Db.GetCollection<ActionLog>("actionLogs");

var imagesV3 = new GridFSBucket(v3Db, new()
{
	BucketName = "images",
	ChunkSizeBytes = 1_000_000
});

var usernameIndex = Builders<User>.IndexKeys.Ascending(u => u.Username);

usersV3.Indexes.CreateOne(new CreateIndexModel<User>(usernameIndex, new()
{
	Unique = true
}));

var defaultAvatarId = await UploadDefaultAvatar();
await MigrateUsers(defaultAvatarId);
await MigrateUnencryptedPastes();
await MigrateEncryptedPastes();
await MigrateApiKeys();

Console.WriteLine("Migration completed successfully.");

async Task<ObjectId> UploadDefaultAvatar()
{
    var fileBytes = await File.ReadAllBytesAsync("Assets/default_avatar.png");
    using var stream = new MemoryStream(fileBytes);

    var options = new GridFSUploadOptions
    {
        Metadata = new BsonDocument
        {
            { "Content-Type", "image/png" }
        }
    };

    return await imagesV3.UploadFromStreamAsync("", stream, options);
}

async Task MigrateUsers(ObjectId defaultAvatarId)
{
    var httpClient = new HttpClient();

    var progressBarOptions = new ProgressBarOptions
    {
        ForegroundColor = ConsoleColor.Yellow,
        ForegroundColorDone = ConsoleColor.DarkGreen,
        BackgroundColor = ConsoleColor.DarkGray,
        ProgressCharacter = '─'
    };

    var allUsersV2 = await usersV2.Find(new BsonDocument()).ToListAsync();

    using var progressBar = new ProgressBar(allUsersV2.Count, "Migrating users", progressBarOptions);

    foreach (var userV2 in allUsersV2)
    {
        ObjectId avatarId = defaultAvatarId;

        try {
            var avatarResponse = await httpClient.GetAsync(userV2.AvatarUrl);
            if (avatarResponse.IsSuccessStatusCode)
            {
                var contentType = avatarResponse.Content.Headers.ContentType?.MediaType ?? "image/png";
                var buffer = await avatarResponse.Content.ReadAsByteArrayAsync();

                using var stream = new MemoryStream(buffer);
                var uploadOptions = new GridFSUploadOptions
                {
                    Metadata = new BsonDocument
                    {
                        { "Content-Type", contentType }
                    }
                };

                avatarId = await imagesV3.UploadFromStreamAsync(userV2.Username, stream, uploadOptions);
            }
        }
        catch {}

        var userV3 = new User
        {
            Id = userV2.Id,
            Username = userV2.Username,
            AvatarId = avatarId.ToString(),
            IsContributor = userV2.Contributor,
            IsSupporter = userV2.SupporterLength > 0,
            IsAdmin = false,
            ProviderName = userV2.ServiceIds.FirstOrDefault().Key,
            ProviderId = userV2.ServiceIds.FirstOrDefault().Value,
            UserSettings = new() {
                ShowAllPastesOnProfile = userV2.PublicProfile
            },
            Settings = new() {}
        };

        await usersV3.InsertOneAsync(userV3);

        var actionLog = new ActionLog
        {
            CreatedAt = DateTime.UtcNow,
            Type = ActionLogType.UserCreated,
            ObjectId = userV3.Id
        };

        await actionLogsV3.InsertOneAsync(actionLog);

        progressBar.Tick();

        await Task.Delay(250);
    }
}

async Task MigrateUnencryptedPastes()
{
    var progressBarOptions = new ProgressBarOptions
    {
        ForegroundColor = ConsoleColor.Yellow,
        ForegroundColorDone = ConsoleColor.DarkGreen,
        BackgroundColor = ConsoleColor.DarkGray,
        ProgressCharacter = '─'
    };

    using var progressBar = new ProgressBar(pastesV2.Count, "Migrating unencrypted pastes", progressBarOptions);

    foreach (var pasteV2 in pastesV2)
    {
        foreach (var pasty in pasteV2.Pasties)
        {
            pasty.Language = pasty.Language switch
            {
                "Vue.js Component" => "Vue",
                "TypeScript-JSX" => "TSX",
                "Asterisk" => "Text",
                "GitHub Flavored Markdown" => "Markdown",
                "JSON-LD" => "JSON",
                "SQLite" => "SQL",
                "Properties files" => "INI",
                "Z80" => "Assembly",
                "Solr" => "Text",
                "Spreadsheet" => "Text",
                "mscgen" => "Text",
                "MS SQL" => "SQL",
                _ => pasty.Language
            };
        }

        var starsFilter = Builders<UserV2>.Filter.ElemMatch(u => u.Stars, p => p == pasteV2.Id);
        var stars = (await usersV2.Find(starsFilter).ToListAsync()).Select(u => u.Id).ToList();

        var pasties = pasteV2.Pasties.Select(p => new Pasty
        {
            Id = p.Id,
            Title = p.Title == "" ? "untitled" : p.Title,
            Language = p.Language,
            Content = p.Code
        }).ToList();

        var paste = new Paste
        {
            Id = pasteV2.Id,
            Title = pasteV2.Title,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(pasteV2.CreatedAt).UtcDateTime,
            ExpiresIn = pasteV2.ExpiresIn,
            DeletesAt = pasteV2.DeletesAt == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(pasteV2.DeletesAt).UtcDateTime,
            OwnerId = pasteV2.OwnerId == "" ? null : pasteV2.OwnerId,
            Private = pasteV2.IsPrivate,
            Pinned = pasteV2.IsPublic,
            Tags = pasteV2.Tags,
            Stars = stars,
            Pasties = pasties
        };

        await pastesV3.InsertOneAsync(paste);

        var actionLog = new ActionLog
        {
            CreatedAt = paste.CreatedAt,
            Type = ActionLogType.PasteCreated,
            ObjectId = paste.Id
        };

        await actionLogsV3.InsertOneAsync(actionLog);

        progressBar.Tick();
    }
}

async Task MigrateEncryptedPastes()
{
    var progressBarOptions = new ProgressBarOptions
    {
        ForegroundColor = ConsoleColor.Yellow,
        ForegroundColorDone = ConsoleColor.DarkGreen,
        BackgroundColor = ConsoleColor.DarkGray,
        ProgressCharacter = '─'
    };

    using var progressBar = new ProgressBar(encryptedPastesV2.Count, "Migrating encrypted pastes", progressBarOptions);

    foreach (var pasteV2 in encryptedPastesV2)
    {
        var starsFilter = Builders<UserV2>.Filter.ElemMatch(u => u.Stars, p => p == pasteV2.Id);
        var stars = (await usersV2.Find(starsFilter).ToListAsync()).Select(u => u.Id).ToList();

        var paste = new EncryptedPaste
        {
            Id = pasteV2.Id,
            Title = "untitled",
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(pasteV2.CreatedAt).UtcDateTime,
            ExpiresIn = pasteV2.ExpiresIn,
            DeletesAt = pasteV2.DeletesAt == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(pasteV2.DeletesAt).UtcDateTime,
            OwnerId = pasteV2.OwnerId == "" ? null : pasteV2.OwnerId,
            Private = pasteV2.IsPrivate,
            Pinned = pasteV2.IsPublic,
            Tags = pasteV2.Tags,
            Stars = stars,
            EncryptedData = pasteV2.EncryptedData,
            Iv = pasteV2.EncryptedKey,
            Salt = pasteV2.Salt,
            EncryptionVersion = 2
        };

        await encryptedPastesV3.InsertOneAsync(paste);

        var actionLog = new ActionLog
        {
            CreatedAt = paste.CreatedAt,
            Type = ActionLogType.PasteCreated,
            ObjectId = paste.Id
        };

        await actionLogsV3.InsertOneAsync(actionLog);

        progressBar.Tick();
    }
}

async Task MigrateApiKeys()
{
    var progressBarOptions = new ProgressBarOptions
    {
        ForegroundColor = ConsoleColor.Yellow,
        ForegroundColorDone = ConsoleColor.DarkGreen,
        BackgroundColor = ConsoleColor.DarkGray,
        ProgressCharacter = '─'
    };

    var allApiKeysV2 = await apiKeysV2.Find(new BsonDocument()).ToListAsync();

    using var progressBar = new ProgressBar(allApiKeysV2.Count, "Migrating API keys", progressBarOptions);

    var idProvider = new IdProvider();

    foreach (var apiKeyV2 in allApiKeysV2)
    {
        var hashedToken = SHA512.HashData(Encoding.UTF8.GetBytes(apiKeyV2.Key));

        var hashStringBuilder = new StringBuilder();
        foreach (var b in hashedToken)
        {
            hashStringBuilder.Append(b.ToString("x2"));
        }

        var apiKey = new AccessToken
        {
            Id = await idProvider.GenerateId(async id => await accessTokensV3.Find(a => a.Id == id).FirstOrDefaultAsync() is not null),
            Description = "v2 api key",
            Hidden = false,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = null,
            Token = hashStringBuilder.ToString(),
            OwnerId = apiKeyV2.Id,
            Scopes = [Scope.Paste, Scope.User]
        };

        await accessTokensV3.InsertOneAsync(apiKey);

        progressBar.Tick();
    }
}
