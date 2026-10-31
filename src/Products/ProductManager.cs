using App.src.Products;
using App.src.Validation;

internal readonly record struct ProductListResult(
  List<Product> Products,
  decimal Total
);

internal class ProductManager {
  private readonly ProductStore _store;

  internal ProductManager(ProductStore store) {
    _store = store;
  }

  internal Task<Product> CreateAsync(
    string category,
    string name,
    decimal price
  ) {
    if (!TextFieldValidation.TryField(category, out string cat)) {
      throw new ArgumentException("Category cannot be empty.", nameof(category)
      );
    }

    if (!TextFieldValidation.TryField(name, out string n)) {
      throw new ArgumentException("Name cannot be empty.", nameof(name));
    }

    if (price < 0m) {
      throw new ArgumentException("Price cannot be negative.", nameof(price));
    }

    return _store.CreateAsync(cat, n, price);
  }

  private Task<List<Product>> GetAllAsync() {
    return _store.GetAllAsync();
  }

  internal Task<bool> DeleteAsync(Guid id) {
    return _store.DeleteAsync(id);
  }

  internal Task<bool> UpdateAsync(Guid id, Product product) {
    return _store.UpdateAsync(id, product);
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
