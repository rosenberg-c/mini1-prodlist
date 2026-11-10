using App.src.ConsoleCore;

namespace App.src.Todo {
  internal sealed class TodoDisplay {
    internal static void DisplayTasksByProject(List<TodoProject> projects) {
      var ordered = projects.OrderBy(static p => p.Title).ToList();
      for (int i = 0; i < ordered.Count; i++) {
        ConsoleTable.Write(
          ["Title", "Status", "Due", "Description"],
          ordered[i].Tasks
            .OrderBy(static task => task.DueDate)
            .Select(TaskRow),
          header: ordered[i].Title,
          numberRows: false
        );
      }
    }

    internal static void DisplayTasksByDate(List<TodoTask> tasks) {
      ConsoleTable.Write(
        ["Title", "Status", "Due", "Description"],
        tasks
          .OrderBy(static task => task.DueDate)
          .Select(TaskRow),
        numberRows: false
      );
    }

    internal static void DisplaySimpleTasksByDate(List<TodoTask> tasks) {
      var header = new[] { "Title", "Status", "Due", "Description" };
      const int columnWidth = 20;
      const int margin = 2;

      var headerString = string.Concat(
        header.Select(h => h.PadRight(columnWidth))
      );

      Console.WriteLine(new string('-', headerString.Length));
      Console.WriteLine(headerString);
      Console.WriteLine(new string('-', headerString.Length));

      var ordered = tasks.OrderBy(static task => task.DueDate).ToList();
      for (int i = 0; i < ordered.Count; i++) {
        var task = ordered[i];

        static List<string> Wrap(string value, int width) =>
          value.Replace("\r\n", "\n")
            .Split('\n')
            .SelectMany(line => line.Length == 0
              ? [""]
              : line.Chunk(width - margin).Select(chars => new string(chars))
            )
            .ToList();

        var cells = new[]
            { task.Title, task.dispStatus, task.dispDueDate, task.dispDesc }
          .Select(value => Wrap(value, columnWidth))
          .ToArray();

        for (int line = 0; line < cells.Max(cell => cell.Count); line++) {
          foreach (var cell in cells) {
            Console.Write(
              (line < cell.Count ? cell[line] : "").PadRight(columnWidth)
            );
          }

          Console.WriteLine();
        }
      }
    }

    private static IReadOnlyList<string> TaskRow(TodoTask task) {
      return [task.Title, task.dispStatus, task.dispDueDate, task.dispDesc];
    }
  }
}
