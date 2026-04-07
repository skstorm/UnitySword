using System.Text.Json;
using System.Text.Json.Serialization;
using GameCore.Models;
using GameCore.Repositories;

namespace CuiApp.Repositories;

public class JsonFileRepository : IStorageRepository
{
    private readonly string _savePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonFileRepository(string? savePath = null)
    {
        _savePath = savePath ?? "save.json";
    }

    public Task<PlayerData> LoadAsync()
    {
        if (!File.Exists(_savePath))
            return Task.FromResult(PlayerData.CreateDefault());

        var json = File.ReadAllText(_savePath);
        var data = JsonSerializer.Deserialize<PlayerData>(json, JsonOptions);
        return Task.FromResult(data ?? PlayerData.CreateDefault());
    }

    public Task SaveAsync(PlayerData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(_savePath, json);
        return Task.CompletedTask;
    }

    public PlayerData Load()
    {
        if (!File.Exists(_savePath))
            return PlayerData.CreateDefault();

        var json = File.ReadAllText(_savePath);
        return JsonSerializer.Deserialize<PlayerData>(json, JsonOptions) ?? PlayerData.CreateDefault();
    }

    public void Save(PlayerData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(_savePath, json);
    }
}
