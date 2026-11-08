namespace App.src.Todo {
  internal static class TodoValidator {
    internal static void Validate(IReadOnlyList<TodoProject> projects) {
      HashSet<Guid> projectIds = [];
      HashSet<Guid> taskIds = [];

      foreach (TodoProject project in projects) {
        if (project is null) {
          throw new InvalidDataException("A project cannot be null.");
        }

        if (project.Id == Guid.Empty || !projectIds.Add(project.Id)) {
          throw new InvalidDataException(
            $"Project ID {project.Id} is empty or duplicated."
          );
        }

        if (string.IsNullOrWhiteSpace(project.Title)) {
          throw new InvalidDataException(
            $"Project {project.Id} must have a title."
          );
        }

        if (project.Tasks is null) {
          throw new InvalidDataException(
            $"Project {project.Id} must have a tasks list."
          );
        }

        foreach (TodoTask task in project.Tasks) {
          if (task is null) {
            throw new InvalidDataException(
              $"Project {project.Id} contains a null task."
            );
          }

          if (task.Id == Guid.Empty || !taskIds.Add(task.Id)) {
            throw new InvalidDataException(
              $"Task ID {task.Id} is empty or duplicated."
            );
          }

          if (string.IsNullOrWhiteSpace(task.Title)) {
            throw new InvalidDataException(
              $"Task {task.Id} must have a title."
            );
          }

          if (!Enum.IsDefined(task.Status)) {
            throw new InvalidDataException(
              $"Task {task.Id} has an invalid status."
            );
          }
        }
      }
    }
  }
}
