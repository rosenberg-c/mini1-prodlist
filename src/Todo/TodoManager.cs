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

    internal async Task AddTaskAsync() {
      throw new NotImplementedException();
    }

    internal async Task UpdateTaskAsync() {
      throw new NotImplementedException();
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
