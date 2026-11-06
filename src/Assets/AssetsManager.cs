namespace App.src.Assets {
  internal class AssetsManager {
    private readonly AssetsStore _store;

    internal AssetsManager(AssetsStore store) {
      _store = store;
    }

    internal async Task<List<Asset>> GetAllAsync() {
      return await _store.GetAllAsync();
    }

    internal Task<Asset> CreateAsync(Func<Guid, Asset> factory) {
      return _store.CreateAsync(factory);
    }

    internal Task<bool> DeleteAsync(Guid id) {
      return _store.DeleteAsync(id);
    }

    internal Task<bool> UpdateAsync(Guid _) {
      return Task.FromResult(false);
    }

    internal static List<Asset> SortForDisplay(IEnumerable<Asset> assets) {
      return assets.ToList();
    }
  }
}
