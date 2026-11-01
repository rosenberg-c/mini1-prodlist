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
      Console.WriteLine("2. Add New Task");
      Console.WriteLine("3. Edit Task (update, mark as done, remove)");
      Console.WriteLine("4. Manage Projects");
      Console.WriteLine("");
      Console.WriteLine("q to quit or go back");
      ConsoleRender.Footer();
    }

    internal async Task RunAsync() {
      while (true) {
        DisplayMeny();
        string? choice = ConsoleSupport.ReadString("Select option: ");

        if (ConsoleSupport.IsExitOrNull(choice)) {
          return;
        }

        switch (choice) {
          case "1":
            ConsoleRender.Header("Show");
            _actions.Show();
            ConsoleSupport.Pause();
            break;
          case "2":
            ConsoleRender.Header("Add");
            _actions.Add();
            ConsoleSupport.Pause();
            break;
          case "3":
            ConsoleRender.Header("Edit");
            _actions.Edit();
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
