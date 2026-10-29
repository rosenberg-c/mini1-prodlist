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

      ProductManager manager = new(store);

      Console.Clear();
      await new Main(manager).RunAsync();
      Console.WriteLine("Application closed.");
    }
  }
}

internal sealed class Main {
  private readonly Actions _actions;

  internal Main(ProductManager manager) {
    _actions = new Actions(manager);
  }

  internal static void DisplayInit() {
    int pad = 40;
    Console.WriteLine("".PadRight(pad, '-'));
    Console.WriteLine("PRODUCT INVENTORY SYSTEM");
    Console.WriteLine("".PadRight(pad, '-'));
    Console.WriteLine("Enter q to exit/back");
    Console.WriteLine();
    Console.WriteLine("1. Add Product");
    Console.WriteLine("2. Delete Product");
    Console.WriteLine("3. Update Product");
    Console.WriteLine("4. View Products");
    Console.WriteLine("5. Search Products");
    Console.WriteLine("6. Statistics");
    Console.WriteLine();
    Console.WriteLine("".PadRight(pad, '-'));

  }

  internal async Task RunAsync() {
    string? choice = string.Empty;

    while (true) {
      DisplayInit();

      if (Support.IsExitOrNull(choice)) {
        return;
      }

      switch (choice) {
        case "": {
          break;
        }

        case "1": {
          Console.Clear();
          await _actions.AddAsync();
          Console.Clear();
          DisplayInit();

          break;
        }
        case "2":
          Console.Clear();
          await _actions.DeleteAsync();
          Console.Clear();
          DisplayInit();

          break;

        case "3": {
          Console.Clear();
          await _actions.EditAsync();
          Console.Clear();
          DisplayInit();

          break;
        }

        case "4": {
          await _actions.ViewAsync();
          break;
        }

        case "5": {
          Console.Clear();
          await _actions.SearchAsync();
          Console.Clear();
          DisplayInit();

          break;
        }

        case "6": {
          Console.Clear();
          await _actions.StatisticsAsync();
          Console.Clear();
          DisplayInit();

          break;
        }

        default:
          Support.WriteLineYellow("Invalid option. Please try again.");
          break;
      }

      choice = Support.ReadString("Select option: ");

      Console.Clear();
    }
  }
}
