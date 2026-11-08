using App.src.ConsoleCore;

namespace App.src.Todo {
  internal sealed class TodoMenu {
    private readonly TodoActions _actions;

    internal TodoMenu(TodoManager manager) {
      _actions = new TodoActions(manager);
    }

    private static void DisplayMeny() {
      Console.Clear();
      ConsoleRender.Header("Welcome to Todo App");
      Console.WriteLine("");
      Console.WriteLine("1. Show Task List (by date or project)");
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
            ConsoleRender.Header("Show Tasks");
            await _actions.ShowTaskAsync(
              DisplayMenu: () => {
                Console.WriteLine();
                Console.WriteLine("1. TasksByProject");
                Console.WriteLine("2. TasksByDate");
                Console.WriteLine();
              },
              DisplayInvalid: () => {
                ConsoleSupport.WriteLineYellow(
                  "Invalid option. Please try again."
                );
              }
            );
            break;
          default:
            ConsoleSupport.WriteLineYellow("Invalid option. Please try again.");
            break;
        }
      }
    }
  }
}
