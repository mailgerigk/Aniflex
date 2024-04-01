using Aniflex.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Aniflex.Services;

public sealed class WebCache
{
    private const string PathPart = "/.cache/cache.json";
    private static readonly string Path = $"{AppContext.BaseDirectory}{PathPart}";

    private struct CacheEntry
    {
        public DateTime ValidUntil { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    private readonly Dictionary<string, (DateTime validUntil, string value)> data = new();

    public WebCache()
    {
        try
        {
            if (File.Exists(Path))
            {
                string fileContent = File.ReadAllText(Path);
                CacheEntry[] entries = JsonSerializer.Deserialize<CacheEntry[]>(fileContent)!;
                DateTime now = DateTime.Now;
                foreach (CacheEntry entry in entries)
                {
                    if (entry.ValidUntil >= now)
                        data.Add(entry.Key, (entry.ValidUntil, entry.Value));
                }
            }
        }
        catch
        {
            data = new();
        }

        OnExit.Do += () =>
        {
            try
            {
                EnsurePath.Exists(Path);
                string fileContent = JsonSerializer.Serialize(data.Select(kv => new CacheEntry{ Key = kv.Key, ValidUntil = kv.Value.validUntil, Value = kv.Value.value }));
                File.WriteAllText(Path, fileContent);
            }
            catch (Exception)
            {
                throw;
            }
        };
    }

    public async Task<string> Get(string key, Func<Task<string>> getter, TimeSpan validFor)
    {
        DateTime now = DateTime.Now;
        lock (data)
        {
            if (data.TryGetValue(key, out (DateTime validUntil, string value) tuple) && now < tuple.validUntil)
                return tuple.value;
        }

        string value = await getter();
        DateTime validUntil = now+ validFor;
        lock (data)
        {
            if (data.ContainsKey(key))
                data.Remove(key);
            data.Add(key, (validUntil, value));
        }
        return value;
    }

    public void Clear()
    {
        data.Clear();
    }
}
