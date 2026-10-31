using System.Text.Json;
using App.src.Products;

namespace App.src {
  public class Program {
    public static Task Main() {
      string path = Environment.GetEnvironmentVariable("APP_DATA_PATH")
        ?? "products.json";
      return RunAsync(path);
    }

    internal static async Task RunAsync(string path) {
      ProductStore store = new(path);

      try {
        _ = await store.GetAllAsync();
      }
      catch (Exception e) when (e is JsonException or InvalidDataException) {
        Console.WriteLine($"Cannot load {path}: {e.Message}");
        Console.WriteLine(
          $"Delete {path} to start with an empty inventory,"
        );
        return;
      }

      ProductManager manager = new(store);

      Console.Clear();
      await new ProductMenu(manager).RunAsync();
      Console.WriteLine("Application closed.");
    }
  }
}
