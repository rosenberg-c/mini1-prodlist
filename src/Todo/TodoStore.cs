using SharpStorage.src;

namespace App.src.Todo {
  internal sealed class TodoStore {
    public const int SchemaVersion = 1;

    private readonly StorageRepository<Project> _storage;

    internal TodoStore(string filename) {
      _storage = new StorageRepository<Project>(
        filename, static todo => todo.Id, SchemaVersion
      );
    }

    internal Task<List<Project>> GetAllAsync() {
      return _storage.GetAllAsync();
    }
  }
}
