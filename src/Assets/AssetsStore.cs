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

    internal async Task<List<Asset>> GetAllAsync() {
      List<Asset> assets = await _storage.GetAllAsync();
      ValidateUniqueIds(assets);
      return assets;
    }

    internal async Task<Asset> CreateAsync(Func<Guid, Asset> factory) {
      return await _storage.CreateAsync(factory);
    }

    internal async Task<bool> DeleteAsync(Guid id) {
      return await _storage.DeleteAsync(id);
    }

    internal async Task<bool> UpdateAsync(Guid id, Func<Asset, Asset> update) {
      return await _storage.UpdateAsync(id, update);
    }

    private static void ValidateUniqueIds(IEnumerable<Asset> assets) {
      HashSet<Guid> ids = [];
      foreach (Asset asset in assets) {
        if (!ids.Add(asset.Id)) {
          throw new InvalidDataException(
            $"\n\n  -- Duplicate asset ID: {asset.Id}. --\n"
          );
        }
      }
    }
  }
}
