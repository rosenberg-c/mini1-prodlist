using App.src.ConsoleCore;

namespace App.src.Todo {
  internal sealed class TodoActions {
    private readonly TodoManager _m;

    internal TodoActions(
      TodoManager manager
    ) {
      _m = manager;
    }

    internal async Task ShowTaskAsync() {
      (List<TodoProject> projects, List<TodoTask> tasks) =
        await _m.GetProjectsAndTasksAsync();
      while (true) {
        Console.WriteLine();
        Console.WriteLine("1. TasksByProject");
        Console.WriteLine("2. TasksByDate");
        Console.WriteLine();

        if (!ConsoleSupport.TryReadMenuChoice(out string? choice)) {
          return;
        }

        switch (choice) {
          case "1":
            TodoDisplay.DisplayTasksByProject(projects);
            break;
          case "2":
            TodoDisplay.DisplayTasksByDate(tasks);
            break;
          default:
            ConsoleSupport.WriteLineYellow(
              "Invalid option. Please try again."
            );

            break;
        }
      }
    }
  }
}
