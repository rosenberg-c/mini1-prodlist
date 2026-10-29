using Product = App.src.Products.Product;

namespace App.src {
  internal sealed class Actions {
    private readonly ProductManager _m;

    internal static void DisplayItemListAsync(List<Product> p) {
      for (int i = 0; i < p.Count; i++) {
        Console.Write($"{i + 1,2}. ");
        Console.WriteLine(p[i].GetListItemString());
      }

      Console.WriteLine();
    }

    private static bool TryReadName(out string name) {
      name = string.Empty;

      while (true) {
        string? text = Support.ReadString("Enter Name: ");
        if (Support.IsExitOrNull(text)) {
          return false;
        }

        if (ProductInput.TryField(text, out name)) {
          return true;
        }

        Support.WriteLineRed("ERROR: Name cannot be empty.");
      }
    }

    private static bool TryReadCategory(out string category) {
      category = string.Empty;

      while (true) {
        string? text = Support.ReadString("Enter Category: ");
        if (Support.IsExitOrNull(text)) {
          return false;
        }

        if (ProductInput.TryField(text, out category)) {
          return true;
        }

        Support.WriteLineRed("ERROR: Category cannot be empty.");
      }
    }

    private static bool TryReadPrice(out decimal price) {
      price = 0m;

      while (true) {
        string? text = Support.ReadString("Enter Price: ");
        if (Support.IsExitOrNull(text)) {
          return false;
        }

        if (ProductInput.TryPrice(text, out price, out string e)) {
          return true;
        }

        Support.WriteLineRed($"ERROR: {e}");
      }
    }

    internal Actions(
      ProductManager manager
    ) {
      _m = manager;
    }

    internal async Task AddAsync() {
      while (true) {
        if (!TryReadCategory(out string category)) {
          return;
        }

        if (!TryReadName(out string name)) {
          return;
        }

        if (!TryReadPrice(out decimal price)) {
          return;
        }

        _ = await _m.CreateAsync(category, name, price);
        Support.WriteLineGreen("Product added successfully!");
      }
    }

    internal static void DisplayInit() {
      Console.WriteLine();
      Console.WriteLine("|-- EDIT --");
      Console.WriteLine("| 1. Category");
      Console.WriteLine("| 2. Name");
      Console.WriteLine("| 3. Price");
      Console.WriteLine("|----------");
      Console.WriteLine();
    }

    internal static void DisplayUpdated(bool updated) {
      if (updated) {
        Support.WriteLineGreen("Product updated successfully.");
      } else {
        Support.WriteLineYellow("Product not found.");
      }
    }

    internal async Task EditAsync() {
      while (true) {
        ProductListResult result = await _m.ProductListSorted();
        List<Product> entries = result.Products;

        DisplayItemListAsync(result.Products);

        if (entries.Count == 0) {
          Support.WriteLineYellow("No products to edit.");
          Console.Write("Press any key to continue...");
          Support.HoldPressAnyKey();
          return;
        }

        string? intStr = Support.ReadString("Enter product to edit: ");
        if (Support.IsExitOrNull(intStr)) {
          return;
        }

        if (!int.TryParse(intStr, out int idx) || idx < 1 ||
            idx > entries.Count) {
          Support.WriteLineYellow("Invalid product number.");
          continue;
        }

        Product old = entries[idx - 1];

        while (true) {
          DisplayInit();
          string? choice = Support.ReadString("Select option: ");
          if (Support.IsExitOrNull(choice)) {
            return;
          }

          Product updated;
          switch (choice) {
            case "1":
              if (!TryReadCategory(out string category)) {
                return;
              }

              updated = old.WithCategory(category);
              break;

            case "2":
              if (!TryReadName(out string name)) {
                return;
              }

              updated = old.WithName(name);
              break;

            case "3":
              if (!TryReadPrice(out decimal price)) {
                return;
              }

              updated = old.WithPrice(price);
              break;

            default:
              Support.WriteLineYellow("Invalid option. Please try again.");
              continue;
          }

          bool saved = await _m.UpdateAsync(old.Id, updated);
          DisplayUpdated(saved);
          break;
        }
      }
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

    internal async Task DeleteAsync() {
      while (true) {
        ProductListResult result = await _m.ProductListSorted();
        List<Product> entries = result.Products;

        DisplayItemListAsync(result.Products);

        if (entries.Count == 0) {
          Support.WriteLineYellow("No products to delete.");
          Console.Write("Press any key to continue...");
          Support.HoldPressAnyKey();
          return;
        }

        string? intStr = Support.ReadString("Enter product to delete: ");
        if (Support.IsExitOrNull(intStr)) {
          return;
        }

        if (!int.TryParse(intStr, out int idx) || idx < 1 ||
            idx > entries.Count) {
          Support.WriteLineYellow("Invalid product number.");
          continue;
        }

        Guid id = entries[idx - 1].Id;
        bool deleted = await _m.DeleteAsync(id);

        if (deleted) {
          Support.WriteLineGreen("Product removed successfully.");
        } else {
          Support.WriteLineYellow("Product not found.");
        }
      }
    }

    internal async Task SearchAsync() {
      while (true) {
        string? term = Support.ReadString("Search category or name: ");
        if (Support.IsExitOrNull(term)) {
          break;
        }

        ProductListResult result = await _m.ProductListSorted();
        var entries = result.Products.Where(p =>
          p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
          p.Category.Contains(term, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        if (entries.Count == 0) {
          Support.WriteLineYellow("No products found.");
          continue;
        }

        if (term == string.Empty) {
          Support.WriteLineRed("Empty search query, list all.");
          Console.WriteLine();

          for (int i = 0; i < result.Products.Count; i++) {
            Support.WriteLineYellow(entries[i].GetListItemString());
          }

          Console.WriteLine();

          continue;
        }

        if (ProductInput.TryField(term, out string _)) {
          Console.WriteLine();

          for (int i = 0; i < entries.Count; i++) {
            Support.WriteLineGreen(entries[i].GetListItemString());
          }

          Console.WriteLine();

          continue;
        }
      }

      return;
    }

    internal async Task StatisticsAsync() {
      ProductListResult result = await _m.ProductListSorted();
      List<Product> entries = result.Products;

      if (entries.Count == 0) {
        Console.WriteLine("No products available for statistics.");
        return;
      }

      Product cheapest = entries[0];
      Product mostExpensive = entries[^1];

      Console.WriteLine(
        $"Most expensive: {mostExpensive.Name} - {mostExpensive.Price:0.##} kr"
      );
      Console.WriteLine($"Cheapest: {cheapest.Name} - {cheapest.Price:0.##} kr"
      );
      decimal average = result.Total / entries.Count;
      Console.WriteLine($"Average price: {average:0.##} kr");

      Console.WriteLine("Products per category:");
      foreach (IGrouping<string, Product>? group in entries
                 .GroupBy(static p => p.Category)
                 .OrderBy(static g => g.Key)) {
        Console.WriteLine($"{group.Key}: {group.Count()}");
      }
    }
  }
}
