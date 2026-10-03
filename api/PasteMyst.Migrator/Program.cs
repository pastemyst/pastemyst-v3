using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Models;
using PasteMyst.Web.Models.Auth;
using PasteMyst.Web.Models.V2;
using PasteMyst.Web.Serializers;
using PasteMyst.Web.Services;
using PasteMyst.Web.Utils;
using PasteMyst.Migrator;
using ShellProgressBar;

var connectionArg = Array.FindIndex(args, a => a == "--connection");
var avatarsArg = Array.FindIndex(args, a => a == "--v2-avatars");
if (connectionArg == -1 || connectionArg + 1 >= args.Length || avatarsArg == -1 || avatarsArg + 1 >= args.Length)
{
    Console.Error.WriteLine("Usage: dotnet run -- --connection <mongodb-connection-string> --v2-avatars <v2 public/assets/avatars dir> [--drop-existing]");
    Environment.ExitCode = 1;
    return;
}

var connectionString = args[connectionArg + 1];
var dropExisting = Array.IndexOf(args, "--drop-existing") != -1;

// v2 serves uploaded avatars from this directory, and v2 is down during the migration, so they're
// read from disk instead of over HTTP.
var v2AvatarsDir = args[avatarsArg + 1];
if (!Directory.Exists(v2AvatarsDir))
{
    Console.Error.WriteLine($"v2 avatars directory not found: {v2AvatarsDir}");
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine("Preprocessing the database...");

// preprocess.d runs in docker with a pinned D toolchain (see preprocess.Dockerfile), on the host
// network so it reaches Mongo at the same address as the migrator.
const string preprocessImage = "pastemyst-migrator-preprocess";

if (RunDocker("build", "-t", preprocessImage, "-f", "preprocess.Dockerfile", ".") != 0 ||
    RunDocker("run", "--rm", "--network", "host", preprocessImage, connectionString) != 0)
{
    Console.Error.WriteLine("Preprocessing failed.");
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine("Migrating the database...");

BsonSerializer.TryRegisterSerializer(new CustomEnumStringSerializer<ExpiresIn>());
BsonSerializer.TryRegisterSerializer(new CustomEnumStringSerializer<Scope>());

var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);

var languageProvider = new LanguageProvider();
await languageProvider.StartAsync(CancellationToken.None);

var mongoClient = new MongoClient(connectionString);

var v2Db = mongoClient.GetDatabase("pastemyst-v2");
var v3Db = mongoClient.GetDatabase("pastemyst");

if (dropExisting)
{
    Console.Write("Dropping existing v3 database... ");
    await mongoClient.DropDatabaseAsync("pastemyst");
    Console.WriteLine("done.");
}

var usersV2 = v2Db.GetCollection<UserV2>("users");
var pastesV2 = v2Db.GetCollection<PasteV2>("pastes");
var encryptedPastesV2 = v2Db.GetCollection<EncryptedPasteV2>("pastes");
var unencryptedFilter = Builders<PasteV2>.Filter.Eq(p => p.Encrypted, false);
var encryptedFilter = Builders<EncryptedPasteV2>.Filter.Eq(p => p.Encrypted, true);

// Pastes are streamed and inserted in batches: loading them all at once took ~10 GB on prod data.
const int batchSize = 1000;
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
var starsByPaste = Mappings.BuildStarsByPaste(await usersV2.Find(FilterDefinition<UserV2>.Empty).ToListAsync());
await MigrateUnencryptedPastes();
await MigrateEncryptedPastes();
await MigrateApiKeys();

if (!await Verify())
{
    Console.Error.WriteLine("Migration finished, but verification FAILED. Do not open traffic to v3.");
    Environment.ExitCode = 1;
    return;
}

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

    var unmappedDefaultLanguages = new Dictionary<string, int>();
    var avatarFallbacks = new List<string>();

    // v2 has no join date for users, so the oldest paste they own stands in for it. Computed in Mongo
    // so the pastes don't have to be loaded.
    var oldestPasteByOwner = (await v2Db.GetCollection<BsonDocument>("pastes").Aggregate()
            .Match(new BsonDocument
            {
                { "ownerId", new BsonDocument("$nin", new BsonArray { "", BsonNull.Value }) },
                { "createdAt", new BsonDocument("$gt", 0) }
            })
            .Group(new BsonDocument
            {
                { "_id", "$ownerId" },
                { "oldest", new BsonDocument("$min", "$createdAt") }
            })
            .ToListAsync())
        .ToDictionary(d => d["_id"].AsString, d => d["oldest"].ToInt64());

    foreach (var userV2 in allUsersV2)
    {
        ObjectId avatarId = defaultAvatarId;

        var (avatar, avatarContentType, avatarProblem) = await LoadAvatar(userV2.AvatarUrl);
        if (avatar is not null)
        {
            using var stream = new MemoryStream(avatar);
            var uploadOptions = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    { "Content-Type", avatarContentType }
                }
            };

            avatarId = await imagesV3.UploadFromStreamAsync(userV2.Username, stream, uploadOptions);
        }
        else
        {
            avatarFallbacks.Add($"{userV2.Username}: {avatarProblem}");
        }

        var defaultLanguage = Mappings.MapDefaultLanguage(userV2.DefaultLang, ResolveLanguage);
        if (defaultLanguage == Mappings.AutodetectLanguage && !string.IsNullOrEmpty(userV2.DefaultLang) &&
            userV2.DefaultLang != Mappings.AutodetectLanguage)
        {
            unmappedDefaultLanguages.TryGetValue(userV2.DefaultLang, out var existingCount);
            unmappedDefaultLanguages[userV2.DefaultLang] = existingCount + 1;
        }

        var createdAt = Mappings.MapUserCreatedAt(oldestPasteByOwner.TryGetValue(userV2.Id, out var oldest) ? oldest : null);

        var userV3 = Mappings.MapUser(userV2, avatarId.ToString(), defaultLanguage, createdAt);

        await usersV3.InsertOneAsync(userV3);

        var actionLog = new ActionLog
        {
            CreatedAt = userV3.CreatedAt,
            Type = ActionLogType.UserCreated,
            ObjectId = userV3.Id
        };

        await actionLogsV3.InsertOneAsync(actionLog);

        progressBar.Tick();
    }

    if (unmappedDefaultLanguages.Count > 0)
    {
        progressBar.WriteLine("\nWarning: the following default languages were not recognised and fell back to \"Autodetect\":");
        foreach (var (lang, count) in unmappedDefaultLanguages.OrderByDescending(x => x.Value))
            progressBar.WriteLine($"  {lang} ({count} users)");
    }

    if (avatarFallbacks.Count > 0)
    {
        progressBar.WriteLine($"\nWarning: {avatarFallbacks.Count} users got the default avatar:");
        foreach (var fallback in avatarFallbacks)
            progressBar.WriteLine($"  {fallback}");
    }

    // Returns the avatar and its content type, or why it couldn't be used.
    async Task<(byte[]? Avatar, string? ContentType, string? Problem)> LoadAvatar(string avatarUrl)
    {
        byte[] avatar;

        var v2FileName = Mappings.GetV2HostedAvatarFileName(avatarUrl);
        if (v2FileName is not null)
        {
            var path = Path.Combine(v2AvatarsDir, v2FileName);
            if (!File.Exists(path)) return (null, null, $"{v2FileName} not found in {v2AvatarsDir}");

            avatar = await File.ReadAllBytesAsync(path);
        }
        else
        {
            try
            {
                var response = await httpClient.GetAsync(avatarUrl);
                if (!response.IsSuccessStatusCode) return (null, null, $"HTTP {(int)response.StatusCode} from {avatarUrl}");

                avatar = await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception e)
            {
                return (null, null, $"{e.GetType().Name} fetching {avatarUrl}");
            }
            finally
            {
                // Go easy on GitHub / GitLab / Gravatar.
                await Task.Delay(250);
            }
        }

        // Don't trust file extensions or response headers: an error page served with 200 would
        // otherwise end up as someone's avatar.
        var contentType = Mappings.DetectImageContentType(avatar);
        return contentType is null ? (null, null, "not a recognised image") : (avatar, contentType, null);
    }
}

string? ResolveLanguage(string name)
{
    try
    {
        return languageProvider.FindByName(name).Name;
    }
    catch (LanguageNotFoundException)
    {
        return null;
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

    var total = await pastesV2.CountDocumentsAsync(unencryptedFilter);
    using var progressBar = new ProgressBar((int)total, "Migrating unencrypted pastes", progressBarOptions);

    var unmappedLanguages = new Dictionary<string, int>();

    using var cursor = await pastesV2.Find(unencryptedFilter, new FindOptions { BatchSize = batchSize }).ToCursorAsync();
    while (await cursor.MoveNextAsync())
    {
        var pastes = new List<Paste>();

        foreach (var pasteV2 in cursor.Current)
        {
            foreach (var pasty in pasteV2.Pasties)
            {
                pasty.Language = V2LanguageMapper.MapLanguage(pasty.Language);

                // Validate the language is known to v3; fall back to Text if not
                try
                {
                    languageProvider.FindByName(pasty.Language);
                }
                catch (LanguageNotFoundException)
                {
                    unmappedLanguages.TryGetValue(pasty.Language, out var existingCount);
                    unmappedLanguages[pasty.Language] = existingCount + 1;
                    pasty.Language = "Text";
                }
            }

            var pasties = pasteV2.Pasties.Select(Mappings.MapPasty).ToList();

            pastes.Add(Mappings.MapUnencryptedPaste(pasteV2, pasties, StarsOf(pasteV2.Id)));
        }

        if (pastes.Count == 0) continue;

        await pastesV3.InsertManyAsync(pastes);
        await actionLogsV3.InsertManyAsync(pastes.Select(PasteCreatedLog));

        progressBar.Tick(progressBar.CurrentTick + pastes.Count);
    }

    if (unmappedLanguages.Count > 0)
    {
        progressBar.WriteLine("\nWarning: the following language names were not recognised and fell back to \"Text\":");
        foreach (var (lang, count) in unmappedLanguages.OrderByDescending(x => x.Value))
            progressBar.WriteLine($"  {lang} ({count} pasties)");
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

    var total = await encryptedPastesV2.CountDocumentsAsync(encryptedFilter);
    using var progressBar = new ProgressBar((int)total, "Migrating encrypted pastes", progressBarOptions);

    using var cursor = await encryptedPastesV2.Find(encryptedFilter, new FindOptions { BatchSize = batchSize }).ToCursorAsync();
    while (await cursor.MoveNextAsync())
    {
        var pastes = cursor.Current.Select(p => Mappings.MapEncryptedPaste(p, StarsOf(p.Id))).ToList();

        if (pastes.Count == 0) continue;

        await encryptedPastesV3.InsertManyAsync(pastes);
        await actionLogsV3.InsertManyAsync(pastes.Select(PasteCreatedLog));

        progressBar.Tick(progressBar.CurrentTick + pastes.Count);
    }
}

// A fresh list per paste, so pastes never share one.
List<string> StarsOf(string pasteId) => starsByPaste.TryGetValue(pasteId, out var users) ? [..users] : [];

ActionLog PasteCreatedLog(BasePaste paste) => new()
{
    CreatedAt = paste.CreatedAt,
    Type = ActionLogType.PasteCreated,
    ObjectId = paste.Id
};

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
        var id = await idProvider.GenerateId(async id => await accessTokensV3.Find(a => a.Id == id).FirstOrDefaultAsync() is not null);

        var apiKey = Mappings.MapApiKey(apiKeyV2, id);

        await accessTokensV3.InsertOneAsync(apiKey);

        progressBar.Tick();
    }
}

// Compares v2 and v3 counts after the migration. Also catches v2 pastes that neither the
// encrypted nor the unencrypted filter picked up, and leftovers in a v3 DB that wasn't empty.
async Task<bool> Verify()
{
    Console.WriteLine("Verifying...");

    async Task<long> CountPasties(IMongoDatabase db, BsonDocument filter)
    {
        var result = await db.GetCollection<BsonDocument>("pastes").Aggregate()
            .Match(filter)
            .Group(new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "count", new BsonDocument("$sum", new BsonDocument("$size", new BsonDocument("$ifNull", new BsonArray { "$pasties", new BsonArray() }))) }
            })
            .FirstOrDefaultAsync();

        return result?["count"].ToInt64() ?? 0;
    }

    long CountLogs(ActionLogType type) => actionLogsV3.CountDocuments(l => l.Type == type);

    var allPastesV2 = await v2Db.GetCollection<BsonDocument>("pastes").CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
    var allUsersV2 = await usersV2.CountDocumentsAsync(FilterDefinition<UserV2>.Empty);

    var checks = new List<(string Name, long V2, long V3)>
    {
        ("users", allUsersV2, await usersV3.CountDocumentsAsync(FilterDefinition<User>.Empty)),
        ("pastes (all)", allPastesV2, await basePastesV3.CountDocumentsAsync(FilterDefinition<BasePaste>.Empty)),
        ("unencrypted pastes", await pastesV2.CountDocumentsAsync(unencryptedFilter), await pastesV3.CountDocumentsAsync(FilterDefinition<Paste>.Empty)),
        ("encrypted pastes", await encryptedPastesV2.CountDocumentsAsync(encryptedFilter), await encryptedPastesV3.CountDocumentsAsync(FilterDefinition<EncryptedPaste>.Empty)),
        ("pasties", await CountPasties(v2Db, new BsonDocument("encrypted", false)), await CountPasties(v3Db, new BsonDocument())),
        ("api keys", await apiKeysV2.CountDocumentsAsync(FilterDefinition<ApiKeyV2>.Empty), await accessTokensV3.CountDocumentsAsync(FilterDefinition<AccessToken>.Empty)),
        ("UserCreated logs", allUsersV2, CountLogs(ActionLogType.UserCreated)),
        ("PasteCreated logs", allPastesV2, CountLogs(ActionLogType.PasteCreated))
    };

    Console.WriteLine($"  {"",-20} {"v2",10} {"v3",10}");
    foreach (var (name, v2, v3) in checks)
        Console.WriteLine($"  {name,-20} {v2,10} {v3,10}{(v2 == v3 ? "" : "   MISMATCH")}");

    return checks.All(c => c.V2 == c.V3);
}

int RunDocker(params string[] arguments)
{
    using var docker = System.Diagnostics.Process.Start("docker", arguments);
    docker.WaitForExit();
    return docker.ExitCode;
}
