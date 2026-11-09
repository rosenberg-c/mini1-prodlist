using SharpStorage.src;

namespace App.src.Todo {
  internal sealed class TodoStore {
    public const int SchemaVersion = 1;

    private readonly StorageRepository<TodoProject> _storage;

    internal TodoStore(string filename) {
      _storage = new StorageRepository<TodoProject>(
        filename, static todo => todo.Id, SchemaVersion
      );
    }

    internal async Task<List<TodoProject>> GetProjectsAsync() {
      List<TodoProject> projects = await _storage.GetAllAsync();
      TodoValidator.Validate(projects);
      return projects;
    }

    internal async Task<TodoProject> CreateProjectAsync(Func<Guid, TodoProject> factory) {
      return await _storage.CreateAsync(factory);
    }

    internal async Task UpdateProjectAsync(
      Guid id,
      Func<TodoProject, TodoProject> update
    ) {
      var projects = await GetProjectsAsync();
      var project = projects.FirstOrDefault(p => p.Id == id);

      if (project is null) {
        throw new KeyNotFoundException("Project not found.");
      }

      if (!await _storage.UpdateAsync(id, update)) {
        throw new KeyNotFoundException("Could not update project.");
      }
    }

    internal async Task MoveProjectAsync(
      Guid fromId,
      Guid toId,
      Guid taskId
    ) {
      await _storage.UpdatePairAsync(fromId, toId, (fromProject, toProject) => {
          int index = fromProject.Tasks.FindIndex(t => t.Id == taskId);
          if (index < 0) {
            throw new KeyNotFoundException("Task not found.");
          }

          TodoTask task = fromProject.Tasks[index];
          fromProject.Tasks.RemoveAt(index);
          toProject.Tasks.Add(task);
          return (fromProject, toProject);
        }
      );
    }

    internal async Task DeleteProjectAsync() {
      throw new NotImplementedException();
    }
  }
}
