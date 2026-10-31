using System.Text.Json;
using App.src.Products;
using App.src.ConsoleCore;

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
  private readonly ProductActions _actions;

  internal Main(ProductManager manager) {
    _actions = new ProductActions(manager);
  }

  internal static void Header(string header) {
    int pad = 40;
    Console.Clear();
    Console.WriteLine("".PadRight(pad, '-'));
    Console.WriteLine(header);
    Console.WriteLine("".PadRight(pad, '-'));
  }

  internal static void Footer() {
    int pad = 40;
    Console.WriteLine("".PadRight(pad, '-'));
  }

  internal static void DisplayInit() {
    Header("PRODUCT INVENTORY SYSTEM");
    Console.WriteLine("Enter q to exit/back");
    Console.WriteLine();
    Console.WriteLine("1. Add Product");
    Console.WriteLine("2. Delete Product");
    Console.WriteLine("3. Update Product");
    Console.WriteLine("4. View Products");
    Console.WriteLine("5. Search Products");
    Console.WriteLine("6. Statistics");
    Console.WriteLine();
    Footer();
  }

  private static void Pause() {
    Footer();
    Console.WriteLine("Press any key to return.");
    ConsoleSupport.HoldPressAnyKey();
  }

  internal async Task RunAsync() {
    while (true) {
      DisplayInit();
      string? choice = ConsoleSupport.ReadString("Select option: ");

      if (ConsoleSupport.IsExitOrNull(choice)) {
        return;
      }

      switch (choice) {
        case "": {
          break;
        }

        case "1": {
          Header("Add product");
          await _actions.AddAsync();

          break;
        }
        case "2":
          Header("Delete product");
          await _actions.DeleteAsync();

          break;

        case "3": {
          Header("Edit product");
          await _actions.EditAsync();

          break;
        }

        case "4": {
          Header("View");
          await _actions.ViewAsync();
          Pause();

          break;
        }

        case "5": {
          Header("Search product");
          await _actions.SearchAsync();

          break;
        }

        case "6": {
          Header("Statistics");
          await _actions.StatisticsAsync();
          Pause();

          break;
        }

        default:
          ConsoleSupport.WriteLineYellow("Invalid option. Please try again.");
          break;
      }
    }
  }
}
