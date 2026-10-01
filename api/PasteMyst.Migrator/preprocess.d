#!/usr/bin/env dub
/+ dub.sdl:
    name "pastemyst-db-preprocess"
    dependency "vibe-d" version="~>0.10.1"
    dependency "vibe-stream:tls" version="~>1.1.1"
    subConfiguration "vibe-stream:tls" "notls"
+/

// Some v2 encrypted pastes store encryptedData / encryptedKey / salt as raw bytes instead of base64.
// v3 (and the pastemyst-decryptor it shells out to) expects base64, so this re-encodes them.
//
// It's written in D because vibe.d reads those strings as raw bytes, while the C# driver refuses
// strings that aren't valid UTF-8, which raw bytes may well not be.
//
// It modifies documents in place, so it must only ever run against the restored *copy* of the v2 DB
// (`pastemyst-v2` in the v3 Mongo), never the live v2 DB. The live v2 DB is named `pastemyst`, so
// pointing this at the wrong instance finds nothing to change.
//
// Usage: pastemyst-db-preprocess <mongodb-connection-string>

import std.array;
import std.stdio;
import std.base64;
import vibe.d;

enum databaseName = "pastemyst-v2";
immutable fields = ["encryptedData", "encryptedKey", "salt"];

int main(string[] args)
{
    if (args.length != 2)
    {
        stderr.writeln("Usage: pastemyst-db-preprocess <mongodb-connection-string>");
        return 1;
    }

    auto collection = connectMongoDB(args[1]).getDatabase(databaseName)["pastes"];

    size_t total, reencoded;

    foreach (paste; collection.find(Bson(["encrypted": Bson(true)])).array)
    {
        total++;

        string[string] values;
        foreach (field; fields)
        {
            auto value = paste[field];
            if (value.type != Bson.Type.string)
            {
                stderr.writefln("Paste %s: %s is not a string.", paste["_id"], field);
                return 1;
            }
            values[field] = value.get!string;
        }

        // Same rule as before: if any of the three isn't valid base64, re-encode all three.
        try
        {
            foreach (field; fields) Base64.decode(values[field]);
            continue;
        }
        catch (Exception) {}

        Bson[string] set;
        foreach (field; fields)
            set[field] = Bson(Base64.encode(cast(const(ubyte)[]) values[field]).idup);

        // $set only the three fields, so nothing else in the document is touched.
        collection.updateOne(Bson(["_id": paste["_id"]]), Bson(["$set": Bson(set)]));
        reencoded++;
    }

    writefln("Checked %s encrypted pastes, re-encoded %s.", total, reencoded);
    return 0;
}
