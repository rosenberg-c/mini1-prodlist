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
      Console.WriteLine("2. Add Task");
      Console.WriteLine("3. Edit Task");
      Console.WriteLine("4. Delete Task");

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
            await _actions.ShowTaskAsync();
            break;
          case "2":
            ConsoleRender.Header("Add Task");
            await _actions.AddTaskAsync();
            ConsoleSupport.Pause();
            break;
          case "3":
            ConsoleRender.Header("Edit Task");
            await _actions.EditTaskAsync();
            ConsoleSupport.Pause();
            break;
          case "4":
            ConsoleRender.Header("Delete Task");
            await _actions.DeleteTaskAsync();
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
