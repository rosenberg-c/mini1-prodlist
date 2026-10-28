using System.Text.Json;
using App.src.Products;
using App.src;

namespace App.src {
  public class Program {
    public static async Task Main() {
      string path = Path.Combine(
        Directory.GetCurrentDirectory(),
        "products.json"
      );
      ProductStore store = new(path);

      try {
        _ = await store.GetAllAsync();
      }
      catch (Exception e) when (e is JsonException or InvalidDataException) {
        Console.WriteLine($"Cannot load products.json: {e.Message}");
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
    Console.WriteLine("PRODUCT INVENTORY SYSTEM");
    Console.WriteLine("----------------------------------------");

    Console.WriteLine("Enter q to exit");
    while (true) {
      Console.WriteLine();
      Console.WriteLine("1. Add Product");
      Console.WriteLine("2. Delete Product");
      Console.WriteLine("3. View Products");

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
        case "2":
          if (await _actions.DeleteAsync() == MenuResult.Exit) {
            return;
          }

          break;

        case "3": {
          await _actions.ViewAsync();
          break;
        }

        default:
          Console.WriteLine("Invalid option. Please try again.");
          break;
      }
    }
  }
}
