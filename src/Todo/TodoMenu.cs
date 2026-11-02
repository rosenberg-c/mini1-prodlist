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
            (List<Project> projects, List<ATask> tasks) =
              await _actions.GetProjectsAndTasksAsync();

            Console.WriteLine();

            while (true) {
              string? choice1 = ConsoleSupport.ReadString("Select option: ");
              if (ConsoleSupport.IsExitOrNull(choice1)) {
                // return;
                break;
              }

              switch (choice1) {
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

            break;
          //ConsoleSupport.Pause();

          //break;
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
