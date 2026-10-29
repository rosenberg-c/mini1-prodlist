using SharpStorage.src;

namespace App.src.Products {
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

    internal Task<bool> UpdateAsync(Guid id, Product product) {
      return _storage.UpdateAsync(id, old => product.Copy());
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
