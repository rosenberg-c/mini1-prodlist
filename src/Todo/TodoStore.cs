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

    internal async Task CreateProjectAsync() {
      throw new NotImplementedException();
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

    internal async Task DeleteProjectAsync() {
      throw new NotImplementedException();
    }
  }
}
