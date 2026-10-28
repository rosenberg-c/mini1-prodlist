using SharpStorage.src;

namespace App.src.Products {
  public enum CreateStatus {
    Created,
    Duplicate
  }

  internal sealed class ProductStore {
    private readonly StorageRepository<Product> _storage;

    internal ProductStore(string filename) {
      _storage = new StorageRepository<Product>(
        filename, static p => p.Id, Product.SchemaVersion
      );
    }

    internal Task<List<Product>> GetAllAsync() {
      return _storage.GetAllAsync();
    }

    internal Task<bool> DeleteAsync(Guid id) {
      return _storage.DeleteAsync(id);
    }

    internal async Task<Product> CreateAsync(
      string category,
      string name,
      decimal price
    ) {
      return await _storage.CreateAsync(id => new Product(
          id,
          category,
          name,
          price
        )
      );
    }
  }
}
