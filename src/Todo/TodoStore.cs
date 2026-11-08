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

    internal async Task UpdateProjectAsync() {
      throw new NotImplementedException();
    }

    internal async Task DeleteProjectAsync() {
      throw new NotImplementedException();
    }
  }
}
