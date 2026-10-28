using System.Text.Json;
using App.src.Products;
using Xunit;

namespace App.tests.Tests {
  public sealed class ProductStoreTests : IDisposable {
    private readonly string _dir = Path.Combine(
      Path.GetTempPath(), "prodlist-store-tests", Guid.NewGuid().ToString("N")
    );

    public ProductStoreTests() {
      Directory.CreateDirectory(_dir);
    }

    [Fact]
    public async Task CreateAsync_PersistsFieldsAndDistinctIds_ForRepeatedNames() {
      string path = Path.Combine(_dir, "products.json");
      ProductStore store = new(path);

      Product first = await store.CreateAsync("Electronics", "Mouse", 299.50m);
      Product second = await store.CreateAsync("Office", "Mouse", 120m);

      Assert.NotEqual(Guid.Empty, first.Id);
      Assert.NotEqual(first.Id, second.Id);
      Assert.Equal("Electronics", first.Category);
      Assert.Equal("Mouse", first.Name);
      Assert.Equal(299.50m, first.Price);
      Assert.Equal([first, second], await new ProductStore(path).GetAllAsync());

      using JsonDocument document = JsonDocument.Parse(await File.ReadAllTextAsync(path));
      Assert.Equal(Product.SchemaVersion,
        document.RootElement.GetProperty("schemaVersion").GetInt32());
      Assert.Equal(299.50m,
        document.RootElement.GetProperty("data")[0].GetProperty("Price").GetDecimal());
    }

    [Fact]
    public async Task DeleteAsync_RemovesOnlyTheSelectedId() {
      ProductStore store = new(Path.Combine(_dir, "products.json"));
      Product first = await store.CreateAsync("Office", "Pen", 1.25m);
      Product second = await store.CreateAsync("Office", "Pen", 1.25m);

      Assert.True(await store.DeleteAsync(second.Id));
      Assert.False(await store.DeleteAsync(second.Id));
      Assert.Equal([first], await store.GetAllAsync());
    }

    public void Dispose() {
      GC.SuppressFinalize(this);
      Directory.Delete(_dir, recursive: true);
    }
  }
}
