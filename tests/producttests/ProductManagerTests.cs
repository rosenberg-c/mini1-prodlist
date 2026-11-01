using App.src.Products;
using Xunit;

namespace App.tests.Tests {
  public sealed class ProductManagerTests : IDisposable {
    private readonly string _dir = Path.Combine(
      Path.GetTempPath(), "prodlist-manager-tests", Guid.NewGuid().ToString("N")
    );

    public ProductManagerTests() {
      Directory.CreateDirectory(_dir);
    }

    [Fact]
    public async Task
      ProductListSorted_OrdersByPriceAndTotalsWithoutChangingStorageOrder() {
      ProductStore store = new(Path.Combine(_dir, "products.json"));
      ProductManager manager = new(store);
      Product keyboard =
        await manager.CreateAsync("Electronics", "Keyboard", 599m);
      Product pizza = await manager.CreateAsync("Food", "Pizza", 120m);
      Product mouse = await manager.CreateAsync("Electronics", "Mouse", 299m);

      ProductListResult result = await manager.ListProductsSortedAsync();

      Assert.Equal([pizza, mouse, keyboard], result.Products);
      Assert.Equal(1018m, result.Total);
      Assert.Equal([keyboard, pizza, mouse], await store.GetAllAsync());
    }

    [Fact]
    public void SortForDisplay_BreaksEqualPriceTiesByCategoryNameAndId() {
      Product food = Item(1, "Food", "Z", 20m);
      Product office = Item(2, "Office", "A", 10m);
      Product electronicsB = Item(3, "Electronics", "B", 10m);
      Product electronicsA1 = Item(4, "Electronics", "A", 10m);
      Product electronicsA2 = Item(5, "Electronics", "A", 10m);

      List<Product> sorted = ProductManager.SortForDisplay([
          food, office, electronicsB, electronicsA2, electronicsA1
        ]
      );

      Assert.Equal([
          electronicsA1, electronicsA2, electronicsB, office, food
        ], sorted
      );
    }

    [Fact]
    public async Task ProductListSorted_EmptyInventoryHasZeroTotal() {
      ProductManager manager = new(new ProductStore(
          Path.Combine(_dir, "products.json")
        )
      );

      ProductListResult result = await manager.ListProductsSortedAsync();

      Assert.Empty(result.Products);
      Assert.Equal(0m, result.Total);
    }

    [Theory]
    [InlineData("", "Pizza", 1, "category")]
    [InlineData("   ", "Pizza", 1, "category")]
    [InlineData("Food", "", 1, "name")]
    [InlineData("Food", "   ", 1, "name")]
    [InlineData("Food", "Pizza", -1, "price")]
    public void CreateAsync_RejectsInvalidFieldsBeforeWriting(
      string category, string name, int price, string parameter
    ) {
      string path = Path.Combine(_dir, "products.json");
      ProductManager manager = new(new ProductStore(path));

      ArgumentException error = Assert.Throws<ArgumentException>(() => {
          _ = manager.CreateAsync(category, name, price);
        }
      );

      Assert.Equal(parameter, error.ParamName);
      Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task
      CreateAsync_TrimsFieldsAndAcceptsZeroAndFractionalPrices() {
      string path = Path.Combine(_dir, "products.json");
      ProductManager manager = new(new ProductStore(path));

      Product free = await manager.CreateAsync("  Food  ", "  Sample  ", 0m);
      Product paid = await manager.CreateAsync("Office", "Pen", 1.25m);

      Assert.Equal("Food", free.Category);
      Assert.Equal("Sample", free.Name);
      Assert.Equal(0m, free.Price);
      Assert.Equal(1.25m, paid.Price);
      Assert.Equal([free, paid], await new ProductStore(path).GetAllAsync());
    }

    [Fact]
    public async Task ProductListSorted_OverflowDoesNotChangeStoredProducts() {
      string path = Path.Combine(_dir, "products.json");
      ProductStore store = new(path);
      ProductManager manager = new(store);
      await manager.CreateAsync("Food", "First", decimal.MaxValue);
      await manager.CreateAsync("Food", "Second", decimal.MaxValue);

      await Assert.ThrowsAsync<OverflowException>(() =>
        manager.ListProductsSortedAsync()
      );
      Assert.Equal(2, (await store.GetAllAsync()).Count);
    }

    private static Product Item(int id, string category, string name,
      decimal price) {
      return new Product(new Guid(id, 0, 0, new byte[8]), category, name, price
      );
    }

    public void Dispose() {
      GC.SuppressFinalize(this);
      Directory.Delete(_dir, recursive: true);
    }
  }
}
