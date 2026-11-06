using System.Text.Json;

namespace App.src.Assets {
  public class AssetsProgram {
    public static async Task RunAsync() {
      string path = Environment.GetEnvironmentVariable("APP_DATA_PATH")
                    ?? "assets.json";

      AssetsStore store = new(path);

      try {
        _ = await store.GetAllAsync();
      }
      catch (Exception e) when (e is JsonException or InvalidDataException) {
        Console.WriteLine($"Cannot load {path}: {e.Message}");
        Console.WriteLine(
          $"Delete {path} to start with an empty task list."
        );
        return;
      }

      AssetsManager manager = new(store);
      string dataPath = Path.GetFullPath(path);
      string reportPath = Path.Combine(
        Path.GetDirectoryName(dataPath)!,
        $"{Path.GetFileNameWithoutExtension(dataPath)}-report.json"
      );

      await new AssetsMenu(manager, reportPath).RunAsync();
    }
  }
}
