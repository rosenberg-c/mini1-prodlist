using Product = App.src.Products.Product;
using App.src.Products;
using App.src.ConsoleCore;
using App.src.Validation;

namespace App.src {
  internal sealed class Actions {
    private readonly ProductManager _m;

    internal static void DisplayItemListAsync(
      List<Product> products,
      bool highlightMatches = false
    ) {
      ConsoleTable.Write(
        ["Category", "Name", "Price"],
        products.Select(static p => (IReadOnlyList<string>)[
            p.Category, p.Name, $"{p.Price:0.##} kr"
          ]
        ),
        numberRows: true,
        highlightRows: highlightMatches
      );
    }

    internal Actions(
      ProductManager manager
    ) {
      _m = manager;
    }

    internal async Task AddAsync() {
      while (true) {
        if (!ProductPrompts.TryReadCategory(out string category)) {
          return;
        }

        if (!ProductPrompts.TryReadName(out string name)) {
          return;
        }

        if (!ProductPrompts.TryReadPrice(out decimal price)) {
          return;
        }

        _ = await _m.CreateAsync(category, name, price);
        ConsoleSupport.WriteLineGreen("Product added successfully!");
      }
    }

    internal static void DisplayInit() {
      Console.WriteLine();
      ConsoleTable.Write(
        ["EDIT"],
        [["Category"], ["Name"], ["Price"]],
        numberRows: true
      );
    }

    internal static void DisplayUpdated(bool updated) {
      if (updated) {
        ConsoleSupport.WriteLineGreen("Product updated successfully.");
      } else {
        ConsoleSupport.WriteLineYellow("Product not found.");
      }
    }

    internal async Task EditAsync() {
      while (true) {
        ProductListResult result = await _m.ListProductsSortedAsync();
        List<Product> entries = result.Products;

        DisplayItemListAsync(result.Products);

        if (entries.Count == 0) {
          ConsoleSupport.WriteLineYellow("No products to edit.");
          Console.Write("Press any key to continue...");
          ConsoleSupport.HoldPressAnyKey();
          return;
        }

        string? intStr = ConsoleSupport.ReadString("Enter product to edit: ");
        if (ConsoleSupport.IsExitOrNull(intStr)) {
          return;
        }

        if (!int.TryParse(intStr, out int idx) || idx < 1 ||
            idx > entries.Count) {
          ConsoleSupport.WriteLineYellow("Invalid product number.");
          continue;
        }

        Product old = entries[idx - 1];

        while (true) {
          DisplayInit();
          string? choice = ConsoleSupport.ReadString("Select option: ");
          if (ConsoleSupport.IsExitOrNull(choice)) {
            return;
          }

          Product updated;
          switch (choice) {
            case "1":
              if (!ProductPrompts.TryReadCategory(out string category)) {
                return;
              }

              updated = old.WithCategory(category);
              break;

            case "2":
              if (!ProductPrompts.TryReadName(out string name)) {
                return;
              }

              updated = old.WithName(name);
              break;

            case "3":
              if (!ProductPrompts.TryReadPrice(out decimal price)) {
                return;
              }

              updated = old.WithPrice(price);
              break;

            default:
              ConsoleSupport.WriteLineYellow("Invalid option. Please try again.");
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
        ProductListResult results = await _m.ListProductsSortedAsync();

        DisplayItemListAsync(results.Products);
        ConsoleSupport.WriteLineYellow($"TOTAL PRICE: {results.Total:0.##} kr");
      }
      catch (OverflowException) {
        ConsoleSupport.WriteLineRed("ERROR: Total price is too large.");
      }
    }

    internal async Task DeleteAsync() {
      while (true) {
        ProductListResult result = await _m.ListProductsSortedAsync();
        List<Product> entries = result.Products;

        DisplayItemListAsync(result.Products);

        if (entries.Count == 0) {
          ConsoleSupport.WriteLineYellow("No products to delete.");
          Console.Write("Press any key to continue...");
          ConsoleSupport.HoldPressAnyKey();
          return;
        }

        string? intStr = ConsoleSupport.ReadString("Enter product to delete: ");
        if (ConsoleSupport.IsExitOrNull(intStr)) {
          return;
        }

        if (!int.TryParse(intStr, out int idx) || idx < 1 ||
            idx > entries.Count) {
          ConsoleSupport.WriteLineYellow("Invalid product number.");
          continue;
        }

        Guid id = entries[idx - 1].Id;
        bool deleted = await _m.DeleteAsync(id);

        if (deleted) {
          ConsoleSupport.WriteLineGreen("Product removed successfully.");
        } else {
          ConsoleSupport.WriteLineYellow("Product not found.");
        }
      }
    }

    internal async Task SearchAsync() {
      while (true) {
        string? term = ConsoleSupport.ReadString("Search category or name: ");
        if (ConsoleSupport.IsExitOrNull(term)) {
          break;
        }

        ProductListResult result = await _m.ListProductsSortedAsync();
        var entries = result.Products.Where(p =>
          p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
          p.Category.Contains(term, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        if (entries.Count == 0) {
          ConsoleSupport.WriteLineYellow("No products found.");
          continue;
        }

        if (term == string.Empty) {
          ConsoleSupport.WriteLineRed("Empty search query, list all.");
          Console.WriteLine();
          DisplayItemListAsync(entries);
          Console.WriteLine();

          continue;
        }

        if (TextFieldValidation.TryField(term, out string _)) {
          Console.WriteLine();
          DisplayItemListAsync(entries, highlightMatches: true);
          Console.WriteLine();

          continue;
        }
      }

      return;
    }

    internal async Task StatisticsAsync() {
      ProductListResult result = await _m.ListProductsSortedAsync();
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
