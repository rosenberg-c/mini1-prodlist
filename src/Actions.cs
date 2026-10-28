using App.src.Products;
using Product = App.src.Products.Product;

namespace App.src {
  internal enum MenuResult {
    Continue,
    Exit
  }

  internal sealed class Actions {
    private readonly ProductStore _store;

    internal Actions(ProductStore store) {
      _store = store;
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

      _ = await _store.CreateAsync(category, name, price);
      Console.WriteLine("Product added successfully!");
      return MenuResult.Continue;
    }

    internal async Task ViewAsync() {
      List<Product> entries = await _store.GetAllAsync();
      Console.WriteLine("");
      Console.WriteLine(
        $"{"".PadRight(14, '=')} PRODUCTS {"".PadRight(14, '=')}"
      );
      Console.WriteLine("");
      if (entries.Count == 0) {
        Console.WriteLine("No products to delete.");
        return;
      }

      for (int i = 0; i < entries.Count; i++) {
        Console.Write($"[{i + 1:D2}] ");
        Console.WriteLine(entries[i].GetListItemString());
      }
    }

    internal async Task<MenuResult> DeleteAsync() {
      List<Product> entries = await _store.GetAllAsync();

      if (entries.Count == 0) {
        Console.WriteLine("No products to delete.");
        return MenuResult.Continue;
      }

      await ViewAsync();

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
      bool deleted = await _store.DeleteAsync(id);
      Console.WriteLine(deleted
        ? "Product removed successfully."
        : "Product not found."
      );
      return MenuResult.Continue;
    }
  }
}
