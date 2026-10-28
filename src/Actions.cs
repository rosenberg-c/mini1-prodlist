using App.src.Products;
using Product = App.src.Products.Product;

namespace App.src {
  internal enum MenuResult {
    Continue,
    Exit
  }

  internal sealed class Actions {
    private static void DisplayFor(Product product) {
      // Add tabs to space out line
      Console.WriteLine(
        $"{product.Category} | {product.Name} | {product.Price:0.##} kr"
      );
    }

    private readonly ProductStore _store;

    internal Actions(ProductStore store) {
      _store = store;
    }

    internal async Task<MenuResult> AddAsync() {
      string? category = Support.ReadString("Enter Category: ");
      if (Support.IsExitOrNull(category)) {
        return MenuResult.Exit;
      }

      string? name = Support.ReadString("Enter Product Name: ");
      if (name is null) {
        return MenuResult.Exit;
      }

      string? text = Support.ReadString("Enter Price: ");
      if (text is null) {
        return MenuResult.Exit;
      }

      if (category.Length == 0 || name.Length == 0 ||
          !decimal.TryParse(text, out decimal price) || price < 0m) {
        Console.WriteLine("ERROR: Invalid product fields.");
        return MenuResult.Continue;
      }

      _ = await _store.CreateAsync(category, name, price);
      Console.WriteLine("Product added successfully!");
      return MenuResult.Continue;
    }

    internal async Task ViewAsync() {
      List<Product> entries = await _store.GetAllAsync();
      Console.WriteLine("Products:");

      foreach (Product entry in entries) {
        DisplayFor(entry);
      }
    }

    internal async Task<MenuResult> SearchAsync() {
      string? query = Support.ReadString("Search product: ");

      if (Support.IsExitOrNull(query)) {
        return MenuResult.Exit;
      }

      if (query.Length == 0) {
        Console.WriteLine("Search cannot be empty.");
        return MenuResult.Continue;
      }

      List<Product> entries = await _store.GetAllAsync();
      var matches = entries;

      Console.WriteLine("Results:");

      if (matches.Count == 0) {
        Console.WriteLine("No products found.");
      }

      foreach (Product entry in matches) {
        DisplayFor(entry);
      }

      return MenuResult.Continue;
    }

    internal async Task<MenuResult> DeleteAsync() {
      string? name = Support.ReadString("Enter product to delete: ");

      if (Support.IsExitOrNull(name)) {
        return MenuResult.Exit;
      }

      List<Product> entries = await _store.GetAllAsync();
      Product? match = entries.FirstOrDefault(e =>
        string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase)
      );

      if (match is null) {
        Console.WriteLine("Product not found.");
        return MenuResult.Continue;
      }

      _ = await _store.DeleteAsync(match.Id);

      Console.WriteLine("Product removed successfully.");
      return MenuResult.Continue;
    }
  }
}
