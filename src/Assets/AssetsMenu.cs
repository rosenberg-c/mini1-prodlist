using App.src.ConsoleCore;

namespace App.src.Assets {
  internal sealed class AssetsMenu {
    private readonly AssetsActions _actions;

    internal AssetsMenu(AssetsManager manager) {
      _actions = new AssetsActions(manager);
    }

    private static void DisplayMeny() {
      Console.Clear();
      ConsoleRender.Header("Welcome to Assets App");
      Console.WriteLine("");
      Console.WriteLine("1. Show");
      Console.WriteLine("2. Add");
      Console.WriteLine("3. Edit");
      Console.WriteLine("4. Remove");
      Console.WriteLine("");
      Console.WriteLine("q to quit or go back");
      ConsoleRender.Footer();
    }

    internal async Task RunAsync() {
      while (true) {
        DisplayMeny();

        if (!ConsoleSupport.TryReadMenuChoice(out string? choice)) {
          return;
        }

        switch (choice) {
          case "1":
            ConsoleRender.Header("Show");
            await _actions.ShowAsync();
            break;
          case "2":
            ConsoleRender.Header("Add");
            await _actions.AddAsync();
            ConsoleSupport.Pause();
            break;
          case "3":
            ConsoleRender.Header("Edit");
            await _actions.EditAsync();
            ConsoleSupport.Pause();
            break;
          case "4":
            ConsoleRender.Header("Remove");
            await _actions.RemoveAsync();
            ConsoleSupport.Pause();
            break;
          default:
            ConsoleSupport.WriteLineYellow("Invalid option. Please try again.");
            break;
        }
      }
    }
  }
}
