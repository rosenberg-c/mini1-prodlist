using App.src.ConsoleCore;

namespace App.src.Todo {
  internal class TodoManager {
    private readonly TodoStore _store;

    internal TodoManager(TodoStore store) {
      _store = store;
    }

    internal async Task<(
      List<TodoProject> Projects,
      List<TodoTask> Tasks
      )> GetProjectsAndTasksAsync() {
      List<TodoProject> projects = await GetProjectsAsync();
      List<TodoTask> tasks = [];

      for (int i = 0; i < projects.Count; i++) {
        for (int j = 0; j < projects[i].Tasks.Count; j++) {
          tasks.Add(projects[i].Tasks[j]);
        }
      }

      return (projects, tasks);
    }

    internal async Task<List<TodoProject>> GetProjectsAsync() {
      return await _store.GetProjectsAsync();
    }

    internal async Task CreateProjectAsync() {
      throw new NotImplementedException();
    }

    internal async Task RenameProjectAsync() {
      throw new NotImplementedException();
    }

    internal async Task DeleteProjectAsync() {
      throw new NotImplementedException();
    }

    internal async Task AddTaskAsync(
      Func<Guid, TodoTask> factory, TodoProject project
    ) {
      await _store.UpdateProjectAsync(project.Id,
        (project) => {
          project.Tasks.Add(factory(Guid.NewGuid()));
          return project;
        }
      );
    }

    internal async Task<bool> UpdateTaskAsync(TodoTask task,
      TodoProject project) {
      try {
        await _store.UpdateProjectAsync(project.Id, project => {
            int i = project.Tasks.FindIndex(existing => existing.Id == task.Id);
            if (i < 0) {
              throw new KeyNotFoundException("Task not found.");
            }

            project.Tasks[i] = task.Copy();
            return project;
          }
        );
        return true;
      }
      catch {
        return false;
      }
    }

    internal async Task SetTaskStatusAsync() {
      throw new NotImplementedException();
    }

    internal async Task MoveTaskAsync() {
      throw new NotImplementedException();
    }

    internal async Task DeleteTaskAsync() {
      throw new NotImplementedException();
    }
  }
}
