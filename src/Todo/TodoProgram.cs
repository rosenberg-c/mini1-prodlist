using System.Text.Json;

namespace App.src.Todo {
  public class TodoProgram {
    public static async Task RunAsync() {
      string path = Environment.GetEnvironmentVariable("APP_DATA_PATH")
                    ?? "data.json";

      TodoStore store = new(path);

      try {
        _ = await store.GetProjectsAsync();
      }
      catch (Exception e) when (e is JsonException or InvalidDataException) {
        Console.WriteLine($"Cannot load {path}: {e.Message}");
        Console.WriteLine(
          $"Delete {path} to start with an empty task list."
        );
        return;
      }

      TodoManager manager = new(store);
      await new TodoMenu(manager).RunAsync();
    }
  }
}
