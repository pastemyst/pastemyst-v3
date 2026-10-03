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
if (connectionArg == -1 || connectionArg + 1 >= args.Length)
{
    Console.Error.WriteLine("Usage: dotnet run -- --connection <mongodb-connection-string> [--drop-existing]");
    Environment.ExitCode = 1;
    return;
}

var connectionString = args[connectionArg + 1];
var dropExisting = Array.IndexOf(args, "--drop-existing") != -1;

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

    // v2 has no join date for users, so the oldest paste they own stands in for it.
    var oldestPasteByOwner = pastesV2.Cast<BasePasteV2>().Concat(encryptedPastesV2)
        .Where(p => p.OwnerId != "" && p.CreatedAt > 0)
        .GroupBy(p => p.OwnerId)
        .ToDictionary(g => g.Key, g => g.Min(p => p.CreatedAt));

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

        await Task.Delay(250);
    }

    if (unmappedDefaultLanguages.Count > 0)
    {
        progressBar.WriteLine("\nWarning: the following default languages were not recognised and fell back to \"Autodetect\":");
        foreach (var (lang, count) in unmappedDefaultLanguages.OrderByDescending(x => x.Value))
            progressBar.WriteLine($"  {lang} ({count} users)");
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
{    var progressBarOptions = new ProgressBarOptions
    {
        ForegroundColor = ConsoleColor.Yellow,
        ForegroundColorDone = ConsoleColor.DarkGreen,
        BackgroundColor = ConsoleColor.DarkGray,
        ProgressCharacter = '─'
    };

    using var progressBar = new ProgressBar(pastesV2.Count, "Migrating unencrypted pastes", progressBarOptions);

    var unmappedLanguages = new Dictionary<string, int>();

    foreach (var pasteV2 in pastesV2)
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

        var starsFilter = Builders<UserV2>.Filter.ElemMatch(u => u.Stars, p => p == pasteV2.Id);
        var stars = (await usersV2.Find(starsFilter).ToListAsync()).Select(u => u.Id).ToList();

        var pasties = pasteV2.Pasties.Select(Mappings.MapPasty).ToList();

        var paste = Mappings.MapUnencryptedPaste(pasteV2, pasties, stars);

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

    using var progressBar = new ProgressBar(encryptedPastesV2.Count, "Migrating encrypted pastes", progressBarOptions);

    foreach (var pasteV2 in encryptedPastesV2)
    {
        var starsFilter = Builders<UserV2>.Filter.ElemMatch(u => u.Stars, p => p == pasteV2.Id);
        var stars = (await usersV2.Find(starsFilter).ToListAsync()).Select(u => u.Id).ToList();

        var paste = Mappings.MapEncryptedPaste(pasteV2, stars);

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

    var pastiesV3 = await v3Db.GetCollection<BsonDocument>("pastes").Aggregate()
        .Group(new BsonDocument
        {
            { "_id", BsonNull.Value },
            { "count", new BsonDocument("$sum", new BsonDocument("$size", new BsonDocument("$ifNull", new BsonArray { "$pasties", new BsonArray() }))) }
        })
        .FirstOrDefaultAsync();

    long CountLogs(ActionLogType type) => actionLogsV3.CountDocuments(l => l.Type == type);

    var allPastesV2 = await v2Db.GetCollection<BsonDocument>("pastes").CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
    var allUsersV2 = await usersV2.CountDocumentsAsync(FilterDefinition<UserV2>.Empty);

    var checks = new List<(string Name, long V2, long V3)>
    {
        ("users", allUsersV2, await usersV3.CountDocumentsAsync(FilterDefinition<User>.Empty)),
        ("pastes (all)", allPastesV2, await basePastesV3.CountDocumentsAsync(FilterDefinition<BasePaste>.Empty)),
        ("unencrypted pastes", pastesV2.Count, await pastesV3.CountDocumentsAsync(FilterDefinition<Paste>.Empty)),
        ("encrypted pastes", encryptedPastesV2.Count, await encryptedPastesV3.CountDocumentsAsync(FilterDefinition<EncryptedPaste>.Empty)),
        ("pasties", pastesV2.Sum(p => p.Pasties.Count), pastiesV3?["count"].ToInt64() ?? 0),
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
