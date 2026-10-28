using Product = App.src.Products.Product;

namespace App.src {
  internal enum MenuResult {
    Continue,
    Exit
  }

  internal sealed class Actions {
    private readonly ProductManager _m;

    internal Actions(
      ProductManager manager
    ) {
      _m = manager;
    }

    internal async Task<MenuResult> AddAsync() {
      string? category = Support.ReadString("Enter Category: ");
      if (Support.IsEmptyOrNull(category)) {
        return MenuResult.Continue;
      }

      string? name = Support.ReadString("Enter Product Name: ");
      if (Support.IsEmptyOrNull(name)) {
        return MenuResult.Continue;
      }

      string? priceStr = Support.ReadString("Enter Price: ");
      if (Support.IsEmptyOrNull(priceStr)) {
        return MenuResult.Continue;
      }

      if (category.Length == 0 || name.Length == 0 ||
          !decimal.TryParse(priceStr, out decimal price) || price < 0m) {
        Console.WriteLine("ERROR: Invalid product fields.");
        return MenuResult.Continue;
      }

      _ = await _m.CreateAsync(category, name, price);
      Console.WriteLine("Product added successfully!");
      return MenuResult.Continue;
    }

    internal static void ItemListAsync(List<Product> p) {
      for (int i = 0; i < p.Count; i++) {
        Console.Write($"{i + 1,2}. ");
        Console.WriteLine(p[i].GetListItemString());
      }
      Console.WriteLine();

    }

    internal async Task ViewAsync() {
      ProductListResult results = await _m.ProductListSorted();

      ItemListAsync(results.Products);
      Console.WriteLine($"TOTAL PRICE: {results.Total:0.##} kr");
    }

    internal async Task<MenuResult> DeleteAsync() {
      ProductListResult result = await _m.ProductListSorted();
      List<Product> entries = result.Products;

      ItemListAsync(result.Products);

      if (entries.Count == 0) {
        Console.WriteLine("No products to delete.");
        return MenuResult.Continue;
      }

      string? intStr = Support.ReadString("Enter product to delete: ");

      if (Support.IsExitOrNull(intStr)) {
        return MenuResult.Continue;
      }

      if (!int.TryParse(intStr, out int idx) || idx < 1 ||
          idx > entries.Count) {
        Console.WriteLine("Invalid product number.");
        return MenuResult.Continue;
      }

      Guid id = entries[idx - 1].Id;
      bool deleted = await _m.DeleteAsync(id);
      Console.WriteLine(deleted
        ? "Product removed successfully."
        : "Product not found."
      );
      return MenuResult.Continue;
    }
  }
}
