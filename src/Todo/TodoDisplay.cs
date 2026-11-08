using System.Globalization;
using App.src.ConsoleCore;

namespace App.src.Todo {
  internal sealed class TodoDisplay {
    internal static void DisplayTasksByProject(List<TodoProject> projects) {
      for (int i = 0; i < projects.Count; i++) {
        ConsoleTable.Write(
          ["Title", "Status", "Due"],
          projects[i].Tasks.Select(TaskRow),
          header: projects[i].Title,
          numberRows: false
        );
      }
    }

    internal static void DisplayTasksByDate(List<TodoTask> tasks) {
      ConsoleTable.Write(
        ["Title", "Status", "Due"],
        tasks
          .OrderBy(static task => task.DueDate)
          .Select(TaskRow),
        numberRows: false
      );
    }

    private static IReadOnlyList<string> TaskRow(TodoTask task) {
      string? due = task.DueDate?.ToString(CultureInfo.InvariantCulture);
      return [task.Title, task.Status.ToString(), due ?? ""];
    }
  }
}
