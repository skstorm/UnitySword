using GameCore.Models;

namespace GameCore.Repositories;

public interface IStorageRepository
{
    Task<PlayerData> LoadAsync();
    Task SaveAsync(PlayerData data);
}
