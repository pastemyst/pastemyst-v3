#!/usr/bin/env dub
/+ dub.sdl:
    name "pastemyst-db-preprocess"
    dependency "vibe-d" version="~>0.10.1"
    dependency "vibe-stream:tls" version="~>1.1.1"
    subConfiguration "vibe-stream:tls" "notls"
+/

import std.stdio;
import std.array;
import std.base64;
import vibe.d;

public enum ExpiresIn
{
    never = "never",
    oneHour = "1h",
    twoHours = "2h",
    tenHours = "10h",
    oneDay = "1d",
    twoDays = "2d",
    oneWeek = "1w",
    oneMonth = "1m",
    oneYear = "1y"
}

public struct EncryptedPaste
{
    @name("_id")
    public string id;
    public ulong createdAt;
    public ExpiresIn expiresIn;
    public ulong deletesAt;
    public string ownerId;
    public bool isPrivate;
    public bool isPublic;
    public string[] tags;
    public ulong stars;
    public bool encrypted;
    public string encryptedData;
    public string encryptedKey;
    public string salt;
}

void main()
{
    MongoDatabase mongo = connectMongoDB("mongodb://localhost:27017").getDatabase("pastemyst-v2");
	MongoCollection collection = mongo["pastes"];
    auto pastes = collection.find!EncryptedPaste(["encrypted": true]);

    foreach (paste; pastes.array)
    {
        try
        {
            Base64.decode(paste.encryptedData);
            Base64.decode(paste.encryptedKey);
            Base64.decode(paste.salt);
        }
        catch (Exception)
        {
            paste.encryptedData = Base64.encode(cast(const(ubyte[])) paste.encryptedData);
            paste.encryptedKey = Base64.encode(cast(const(ubyte[])) paste.encryptedKey);
            paste.salt = Base64.encode(cast(const(ubyte[])) paste.salt);
        }

        collection.replaceOne(["_id": paste.id], paste);
    }
}