using System.Text.Json;
using App.src.Products;
using App.src;
using Entry = App.src.Products.Product.Entry;
using InputError = App.src.Products.Product.InputError;

namespace App.src {
  public class Program {
    public static async Task Main() {
      string path =
        Path.Combine(Directory.GetCurrentDirectory(), "products.json");
      ProductStore store = new(path);

      try {
        _ = await store.GetAllAsync();
      }
      catch (Exception error) when (error is JsonException
                                      or InvalidDataException) {
        Console.WriteLine($"Cannot load products.json: {error.Message}");
        Console.WriteLine(
          "Delete products.json to start with an empty inventory,"
        );
        return;
      }

      Console.Clear();
      await new Main(store).RunAsync();
      Console.WriteLine("Application closed.");
    }
  }
}

internal sealed class Main {
  private readonly Actions _actions;

  internal Main(ProductStore store) {
    _actions = new Actions(store);
  }

  internal async Task RunAsync() {
    Console.WriteLine("----------------------------------------");
    Console.WriteLine("PRODUCT INVENTORY SYSTEM - LEVEL 4");
    Console.WriteLine("----------------------------------------");

    while (true) {
      Console.WriteLine();
      Console.WriteLine("1. Add Product");
      Console.WriteLine("2. View Products");
      Console.WriteLine("3. Search Product");
      Console.WriteLine("4. Delete Product");
      Console.WriteLine("5. Statistics");
      Console.WriteLine("6. Exit");
      Console.WriteLine();

      string? choice = Support.ReadString("Select option: ");

      if (Support.IsExitOrNull(choice)) {
        return;
      }

      switch (choice) {
        case "1": {
          if (await _actions.AddAsync() == MenuResult.Exit) {
            return;
          }

          break;
        }

        case "2": {
          await _actions.ViewAsync();
          break;
        }

        case "3": {
          if (await _actions.SearchAsync() == MenuResult.Exit) {
            return;
          }

          break;
        }

        case "4":
          if (await _actions.DeleteAsync() == MenuResult.Exit) {
            return;
          }

          break;

        case "5": {
          await _actions.StatisticsAsync();
          break;
        }

        // Exit
        case "6":
          return;

        default:
          Console.WriteLine("Invalid option. Please try again.");
          break;
      }
    }
  }
}
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
      string? product = Support.ReadString("Enter product: ");

      if (Support.IsExitOrNull(product)) {
        return MenuResult.Exit;
      }

      InputError? error =
        Product.Validate(product, out string left, out int right);
      if (error is InputError code) {
        Console.WriteLine(Ui.MessageFrom(code));
        return MenuResult.Continue;
      }

      CreateStatus result = await _store.TryCreateAsync(left, right);

      Console.WriteLine(Ui.MessageFrom(result));
      return MenuResult.Continue;
    }

    internal async Task ViewAsync() {
      List<Entry> entries = await _store.GetAllAsync();
      Console.WriteLine("Sorted valid products:");

      foreach (Entry entry in Product.Sort(entries)) {
        Console.WriteLine("- " + entry.Name);
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

      List<Entry> entries = await _store.GetAllAsync();
      var matches = Product.Sort(entries)
        .Where(e => e.Name.Contains(
            query, StringComparison.OrdinalIgnoreCase
          )
        )
        .ToList();

      Console.WriteLine("Results:");

      if (matches.Count == 0) {
        Console.WriteLine("No products found.");
      }

      foreach (Entry entry in matches) {
        Console.WriteLine("- " + entry.Name);
      }

      return MenuResult.Continue;
    }

    internal async Task<MenuResult> DeleteAsync() {
      string? name = Support.ReadString("Enter product to delete: ");

      if (Support.IsExitOrNull(name)) {
        return MenuResult.Exit;
      }

      List<Entry> entries = await _store.GetAllAsync();
      Entry? match = entries.FirstOrDefault(e =>
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

    internal async Task StatisticsAsync() {
      List<Entry> entries = await _store.GetAllAsync();
      var numbers = entries.Select(static entry => entry.Right).ToList();

      Console.WriteLine("Statistics:");
      Console.WriteLine($"- Total Products: {entries.Count}");

      if (numbers.Count == 0) {
        Console.WriteLine("- Lowest Number: N/A");
        Console.WriteLine("- Highest Number: N/A");
        Console.WriteLine("- Average Number: N/A");
      } else {
        Console.WriteLine($"- Lowest Number: {numbers.Min()}");
        Console.WriteLine($"- Highest Number: {numbers.Max()}");
        Console.WriteLine($"- Average Number: {numbers.Average():0.##}");
      }
    }
  }
}
