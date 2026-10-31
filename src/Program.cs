using System.Text.Json;
using App.src.Products;

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
      await new ProductMenu(manager).RunAsync();
      Console.WriteLine("Application closed.");
    }
  }
}
