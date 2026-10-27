using App.src.Products;
using Xunit;

namespace App.tests.Tests {
  public sealed class ProductStoreTests : IDisposable {
    private readonly string _dir = Path.Combine(
      Path.GetTempPath(), "prodlist-store-tests", Guid.NewGuid().ToString("N")
    );

    public ProductStoreTests() {
      _ = Directory.CreateDirectory(_dir);
    }

    [Fact]
    public async Task TryCreateAsync_RejectsDuplicateProductWithoutWriting() {
      string path = Path.Combine(_dir, "products.json");
      ProductStore store = new(path);
      Assert.Equal(CreateStatus.Created,
        await store.TryCreateAsync("AB", 300));
      string before = await File.ReadAllTextAsync(path);

      Assert.Equal(CreateStatus.Duplicate,
        await store.TryCreateAsync("ab", 300));
      Assert.Equal(before, await File.ReadAllTextAsync(path));
    }

    [Fact]
    public async Task TryCreateAsync_TwoInstancesOnlyCreateOneMatchingProduct() {
      string path = Path.Combine(_dir, "products.json");
      ProductStore first = new(path);
      ProductStore second = new(path);

      CreateStatus[] results = await Task.WhenAll(
        first.TryCreateAsync("AB", 300),
        second.TryCreateAsync("ab", 300)
      );

      Assert.Equal(1, results.Count(status => status == CreateStatus.Created));
      Assert.Equal(1, results.Count(status => status == CreateStatus.Duplicate));
      Assert.Single(await first.GetAllAsync());
    }

    [Fact]
    public async Task ProductStore_CreatesReadsAndDeletesProduct() {
      string path = Path.Combine(_dir, "products.json");
      ProductStore store = new(path);

      Assert.Equal(CreateStatus.Created,
        await store.TryCreateAsync("AB", 300));
      Product.Entry entry = Assert.Single(await store.GetAllAsync());
      Assert.Equal("AB-300", entry.Name);

      Assert.True(await store.DeleteAsync(entry.Id));
      Assert.Empty(await store.GetAllAsync());
    }

    public void Dispose() {
      GC.SuppressFinalize(this);
      Directory.Delete(_dir, recursive: true);
    }
  }
}
