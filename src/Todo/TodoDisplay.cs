using System.Globalization;
using App.src.ConsoleCore;

namespace App.src.Todo {
  internal sealed class TodoDisplay {
    private static IReadOnlyList<string> TaskRow(ATask task) {
      string? finish = task.FinishDate?.ToString(CultureInfo.InvariantCulture);
      string created = task.CreatedDate.ToString(CultureInfo.InvariantCulture);
      string due = task.DueDate.ToString(CultureInfo.InvariantCulture);
      return [task.Title, task.Status.ToString(), created, due, finish ?? ""];
    }

    internal static void DisplayTasksByProject(List<Project> projects) {
      for (int i = 0; i < projects.Count; i++) {
        ConsoleTable.Write(
          ["Title", "Status", "Created", "Due", "FinishDate"],
          projects[i].Tasks.Select(TaskRow),
          header: projects[i].Title,
          numberRows: false
        );
      }
    }

    internal static void DisplayTasksByDate(List<ATask> tasks) {
      ConsoleTable.Write(
        ["Title", "Status", "Created", "Due", "FinishDate"],
        tasks
          .OrderBy(static task => task.DueDate)
          .Select(TaskRow),
        numberRows: false
      );
    }
  }
}
