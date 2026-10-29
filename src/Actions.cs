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
      string category;
      while (true) {
        string? text = Support.ReadString("Enter Category: ");
        if (Support.IsExitOrNull(text)) {
          return MenuResult.Continue;
        }

        if (ProductInput.TryField(text, out category)) {
          break;
        }

        Support.WriteLineRed("ERROR: Category cannot be empty.");
      }

      string name;
      while (true) {
        string? text = Support.ReadString("Enter Product Name: ");
        if (Support.IsExitOrNull(text)) {
          return MenuResult.Continue;
        }

        if (ProductInput.TryField(text, out name)) {
          break;
        }

        Support.WriteLineRed("ERROR: Product name cannot be empty.");
      }

      decimal price;
      while (true) {
        string? text = Support.ReadString("Enter price: ");
        if (Support.IsExitOrNull(text)) {
          return MenuResult.Continue;
        }

        if (ProductInput.TryPrice(text, out price, out string error)) {
          break;
        }

        Support.WriteLineRed($"ERROR: {error}");
      }

      _ = await _m.CreateAsync(category, name, price);
      Support.WriteLineGreen("Product added successfully!");
      return MenuResult.Continue;
    }

    internal static void DisplayItemListAsync(List<Product> p) {
      for (int i = 0; i < p.Count; i++) {
        Console.Write($"{i + 1,2}. ");
        Console.WriteLine(p[i].GetListItemString());
      }

      Console.WriteLine();
    }

    internal async Task ViewAsync() {
      try {
        ProductListResult results = await _m.ProductListSorted();

        DisplayItemListAsync(results.Products);
        Support.WriteLineYellow($"TOTAL PRICE: {results.Total:0.##} kr");
      }
      catch (OverflowException) {
        Support.WriteLineRed("ERROR: Total price is too large.");
      }
    }

    internal async Task<MenuResult> DeleteAsync() {
      ProductListResult result = await _m.ProductListSorted();
      List<Product> entries = result.Products;

      DisplayItemListAsync(result.Products);

      if (entries.Count == 0) {
        Support.WriteLineYellow("No products to delete.");
        return MenuResult.Continue;
      }

      string? intStr = Support.ReadString("Enter product to delete: ");

      if (Support.IsExitOrNull(intStr)) {
        return MenuResult.Continue;
      }

      if (!int.TryParse(intStr, out int idx) || idx < 1 ||
          idx > entries.Count) {
        Support.WriteLineYellow("Invalid product number.");
        return MenuResult.Continue;
      }

      Guid id = entries[idx - 1].Id;
      bool deleted = await _m.DeleteAsync(id);

      if (deleted) {
        Support.WriteLineGreen("Product removed successfully.");
      } else {
        Support.WriteLineYellow("Product not found.");
      }

      return MenuResult.Continue;
    }
  }
}
