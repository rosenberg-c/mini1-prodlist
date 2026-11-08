using App.src.ConsoleCore;
using CI = App.src.ConsoleCore.ConsoleInput;
using System.ComponentModel;
using System.Reflection;

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

    internal async Task AddTaskAsync() {
      Console.WriteLine();
      List<TodoProject> projects = await _m.GetProjectsAsync();
      if (projects.Count == 0) {
        Console.WriteLine("No projects.");
        return;
      }

      NumberedSelectionResult choice = ProjectChoice(projects);

      if (choice.Status == SelectionStatus.Cancelled) {
        return;
      }

      TodoProject selected = projects[choice.Index];
      if (!CI.TryReadText(prompt: "Title", out string title)) {
        return;
      }

      if (!CI.TryReadOptionalText(prompt: "Description",
            out string? description
          )) {
        return;
      }

      if (!CI.TryReadOptionalDate(label: "Date",
            out DateOnly? date,
            current: null
          )) {
        return;
      }

      try {
        await _m.AddTaskAsync(
          id => new(
            Id: id,
            Status: TodoStatus.Pending,
            Title: title,
            Description: description,
            DueDate: date
          ),
          selected
        );
        Console.WriteLine("Task added.");
      }
      catch {
        Console.WriteLine("Could not create Task");
      }
    }

    internal async Task EditTaskAsync() {
      Console.WriteLine();

      (List<TodoProject> projects, List<TodoTask> tasks) =
        await _m.GetProjectsAndTasksAsync();

      if (projects.Count == 0) {
        Console.WriteLine("No projects.");
        return;
      }

      var selection = TaskChoice(projects);
      if (selection is null) {
        return;
      }

      (TodoTask task, TodoProject project) = selection.Value;

      Console.WriteLine();
      Console.WriteLine("Press Enter to keep the previous value.");
      Console.WriteLine();

      if (!ConsoleInput.TryReadText("Title", out string title, task.Title) ||
          !ConsoleInput.TryReadText("Description", out string desc,
            task.Description
          ) ||
          !TryReadStatus(out TodoStatus status, task.Status) ||
          !CI.TryReadOptionalDate(label: "Date", out DateOnly? date,
            current: task.DueDate
          )
         ) {
        return;
      }

      Console.WriteLine(title);
      Console.WriteLine(desc);
      Console.WriteLine(status);
      Console.WriteLine(date);
      Console.WriteLine(_m.UpdateTaskAsync());
    }

    private NumberedSelectionResult ProjectChoice(
      List<TodoProject> projects
    ) {
      NumberedSelectionResult choice;
      do {
        ConsoleTable.Write(
          ["Project"],
          projects.Select(project => (IReadOnlyList<string>) [
              project.Title
            ]
          )
        );

        choice = NumberedSelection.ReadIndex(projects.Count,
          "Select an index (q to cancel): "
        );
        if (choice.Status == SelectionStatus.Invalid) {
          ConsoleSupport.WriteLineYellow($"Choose 1-{projects.Count}.");
        }
      } while (choice.Status == SelectionStatus.Invalid);

      return choice;
    }

    private (TodoTask Task, TodoProject Project)? TaskChoice(
      List<TodoProject> projects
    ) {
      NumberedSelectionResult choice;
      var options = projects
        .SelectMany(project =>
          project.Tasks.Select(task => (Task: task, Project: project))
        )
        .ToList();

      do {
        ConsoleTable.Write(
          ["Task", "Project"],
          options.Select(option => (IReadOnlyList<string>) [
              option.Task.Title,
              option.Project.Title
            ]
          )
        );

        choice = NumberedSelection.ReadIndex(options.Count,
          "Select an index (q to cancel): "
        );

        if (choice.Status == SelectionStatus.Invalid) {
          ConsoleSupport.WriteLineYellow($"Choose 1-{options.Count}.");
        }
      } while (choice.Status == SelectionStatus.Invalid);

      return choice.Status == SelectionStatus.Cancelled
        ? null
        : options[choice.Index];
    }

    private static bool TryReadStatus(out TodoStatus status,
      TodoStatus? current = null) {
      TodoStatus[] statuses = Enum.GetValues<TodoStatus>();

      string label = current is TodoStatus saved
        ? $" [{GetStatusLabel(saved)}]"
        : "";

      Console.WriteLine($"Select type (1-{statuses.Length})");
      for (int i = 0; i < statuses.Length; i++) {
        Console.WriteLine($"{i + 1}. {GetStatusLabel(statuses[i])}");
      }

      while (true) {
        if (!ConsoleSupport.TryReadMenuChoice(out string? input, "> ")) {
          status = default;
          return false;
        }

        if (input.Length == 0 && current is TodoStatus existing) {
          status = existing;
          return true;
        }

        if (int.TryParse(input, out int selection) && selection >= 1 &&
            selection <= statuses.Length) {
          status = statuses[selection - 1];
          return true;
        }

        ConsoleSupport.WriteLineYellow($"Choose 1-{statuses.Length}");
      }
    }

    private static string GetStatusLabel(TodoStatus status) {
      return typeof(TodoStatus)
               .GetField(status.ToString())?
               .GetCustomAttribute<DescriptionAttribute>()?.Description
             ?? status.ToString();
    }
  }
}
