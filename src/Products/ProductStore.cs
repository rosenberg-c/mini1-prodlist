using System.Collections.Concurrent;
using SharpStorage.src;
using Entry = App.src.Products.Product.Entry;

namespace App.src.Products {
  public enum CreateStatus {
    Created,
    Duplicate
  }

  internal sealed class ProductStore {
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates =
      new(StringComparer.OrdinalIgnoreCase);

    private readonly StorageRepository<Entry> _storage;
    private readonly SemaphoreSlim _gate;

    internal ProductStore(string filename) {
      _storage = new StorageRepository<Entry>(
        filename, static entry => entry.Id, Product.SchemaVersion
      );
      _gate = Gates.GetOrAdd(Path.GetFullPath(filename),
        static _ => new SemaphoreSlim(1, 1)
      );
    }

    internal Task<List<Entry>> GetAllAsync() {
      return _storage.GetAllAsync();
    }

    internal Task<bool> DeleteAsync(Guid id) {
      return _storage.DeleteAsync(id);
    }

    internal async Task<CreateStatus> TryCreateAsync(string left, int right) {
      await _gate.WaitAsync();
      try {
        List<Entry> entries = await _storage.GetAllAsync();
        if (entries.Any(entry => Product.IsDuplicate(entry, left, right))) {
          return CreateStatus.Duplicate;
        }

        _ = await _storage.CreateAsync(id => new Entry(id, left, right));
        return CreateStatus.Created;
      }
      finally {
        _ = _gate.Release();
      }
    }
  }
}
