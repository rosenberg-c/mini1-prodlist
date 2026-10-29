using App.src.Products;

internal readonly record struct ProductListResult(
  List<Product> Products,
  decimal Total
);

internal class ProductManager {
  private readonly ProductStore _store;

  internal ProductManager(ProductStore store) {
    _store = store;
  }

  internal Task<Product> CreateAsync(string c, string n, decimal p) {
    return _store.CreateAsync(c, n, p);
  }

  private Task<List<Product>> GetAllAsync() {
    return _store.GetAllAsync();
  }

  internal Task<bool> DeleteAsync(Guid id) {
    return _store.DeleteAsync(id);
  }

  internal static List<Product> SortForDisplay(IEnumerable<Product> products) {
    return products.OrderBy(static p => p.Price)
      .ThenBy(static p => p.Category, StringComparer.OrdinalIgnoreCase)
      .ThenBy(static p => p.Name, StringComparer.OrdinalIgnoreCase)
      .ThenBy(static p => p.Id)
      .ToList();
  }

  internal static decimal CalculateTotal(IEnumerable<Product> products) {
    return products.Sum(static p => p.Price);
  }

  internal async Task<ProductListResult> ProductListSorted() {
    List<Product> products = await GetAllAsync();
    List<Product> sorted = SortForDisplay(products);
    return new ProductListResult(sorted, CalculateTotal(sorted));
  }
}
