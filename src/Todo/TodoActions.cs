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

    internal async Task ShowShortTaskAsync() {
      (List<TodoProject> _, List<TodoTask> tasks) =
        await _m.GetProjectsAndTasksAsync();
      TodoDisplay.DisplaySimpleTasksByDate(tasks);
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
        await _m.AddTaskAsync(selected.Id, title, description, date);
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
          !ConsoleInput.TryReadOptionalText("Description", out string? desc,
            task.Description
          ) ||
          !TryReadStatus(out TodoStatus status, task.Status) ||
          !CI.TryReadOptionalDate(label: "Date", out DateOnly? date,
            current: task.DueDate
          )
         ) {
        return;
      }

      bool saved = await _m.UpdateTaskAsync(
        taskId: task.Id,
        title: title,
        description: desc,
        dueDate: date,
        status: status,
        projectId: project.Id
      );
      if (saved) {
        Console.WriteLine("Updated.");
      } else {
        ConsoleSupport.WriteLineYellow("Could not update");
      }
    }

    internal async Task DeleteTaskAsync() {
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

      try {
        await _m.DeleteTaskAsync(project.Id, task.Id);
        Console.WriteLine("Success.");
      }
      catch {
        ConsoleSupport.WriteLineYellow("Could not remove.");
      }
    }

    internal async Task MoveTaskAsync() {
      Console.WriteLine();

      (List<TodoProject> currProjects, List<TodoTask> currTasks) =
        await _m.GetProjectsAndTasksAsync();

      if (currProjects.Count == 0) {
        Console.WriteLine("No projects.");
        return;
      }

      var currSelection = TaskChoice(currProjects);
      if (currSelection is null) {
        return;
      }

      (TodoTask moveTask, TodoProject fromProject) = currSelection.Value;

      NumberedSelectionResult toChoice = ProjectChoice(currProjects);
      if (toChoice.Status == SelectionStatus.Cancelled) {
        return;
      }

      if (fromProject.Id == currProjects[toChoice.Index].Id) {
        Console.WriteLine("Cannot move to same, select another project.");
        return;
      }

      TodoProject toProject = currProjects[toChoice.Index];
      try {
        await _m.MoveTaskAsync(fromProject.Id, toProject.Id, moveTask.Id);

        Console.WriteLine("Success.");
      }
      catch {
        ConsoleSupport.WriteLineYellow("Could not move.");
      }
    }

    // Project
    internal async Task CreateProjectAsync() {
      Console.WriteLine();
      if (!ConsoleInput.TryReadText("Title", out string title)) {
        return;
      }

      try {
        await _m.CreateProjectAsync(title);
        Console.WriteLine("Created Project");
      }
      catch {
        ConsoleSupport.WriteLineRed("Could not create project");
      }
    }

    internal async Task RenameProjectAsync() {
      (List<TodoProject> projects, List<TodoTask> _) =
        await _m.GetProjectsAndTasksAsync();

      if (projects.Count == 0) {
        Console.WriteLine("No projects.");
        return;
      }

      NumberedSelectionResult choice = ProjectChoice(projects);
      if (choice.Status == SelectionStatus.Cancelled) {
        return;
      }

      Console.WriteLine();
      Console.WriteLine("Press Enter to keep the previous value.");
      Console.WriteLine();
      var project = projects[choice.Index];
      if (!ConsoleInput.TryReadText("Title", out string title, project.Title)) {
        return;
      }

      try {
        await _m.RenameProjectAsync(project.Id, title);
        Console.WriteLine("Success.");
      }
      catch {
        ConsoleSupport.WriteLineYellow("Could not rename.");
      }
    }

    internal async Task DeleteProjectAsync() {
      (List<TodoProject> projects, List<TodoTask> _) =
        await _m.GetProjectsAndTasksAsync();

      if (projects.Count == 0) {
        Console.WriteLine("No projects.");
        return;
      }

      NumberedSelectionResult choice = ProjectChoice(projects);
      if (choice.Status == SelectionStatus.Cancelled) {
        return;
      }

      Console.WriteLine();
      var project = projects[choice.Index];
      if (project.Tasks.Count > 0) {
        var shouldRemove = ConsoleSupport.TryReadInput(
          $"There are still tasks, comfirm to remove? (y/N): ",
          out string? confirmation
        );
        if (!shouldRemove || !string.Equals(confirmation, "y",
              StringComparison.OrdinalIgnoreCase
            )
           ) {
          Console.WriteLine("Removal cancelled.");
          return;
        }
      }

      try {
        await _m.DeleteProjectAsync(project.Id);
        Console.WriteLine("Success.");
      }
      catch {
        ConsoleSupport.WriteLineYellow("Could not remove.");
      }
    }

    // Support
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
