using System.Text.Json;

namespace App.src.Products {
  public class ProductProgram {
    internal static async Task RunAsync() {
      string path = Environment.GetEnvironmentVariable("APP_DATA_PATH")
        ?? "products.json";

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
      await new ProductMenu(manager).RunAsync();
    }
  }
}
