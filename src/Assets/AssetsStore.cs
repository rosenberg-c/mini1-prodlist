using SharpStorage.src;

namespace App.src.Assets {
  internal sealed class AssetsStore {
    public const int SchemaVersion = 1;

    private readonly StorageRepository<Asset> _storage;

    internal AssetsStore(string filename) {
      _storage = new StorageRepository<Asset>(
        filename, static Assets => Assets.Id, SchemaVersion
      );
    }

    internal Task<List<Asset>> GetAllAsync() {
      return _storage.GetAllAsync();
    }

    internal Task<Asset> CreateAsync(Func<Guid, Asset> factory) {
      return _storage.CreateAsync(factory);
    }
  }
}
