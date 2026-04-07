using System.Threading.Tasks;
using GameCore.Models;

namespace GameCore.Repositories
{
    public class InMemoryRepository : IStorageRepository
    {
        private PlayerData _data;

        public InMemoryRepository(PlayerData initialData = null)
        {
            _data = initialData ?? new PlayerData();
        }

        public Task<PlayerData> LoadAsync()
        {
            return Task.FromResult(_data);
        }

        public Task SaveAsync(PlayerData data)
        {
            _data = data;
            return Task.CompletedTask;
        }
    }
}
