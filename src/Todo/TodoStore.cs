using SharpStorage.src;

namespace App.src.Todo {
  internal sealed class TodoStore {
    public const int SchemaVersion = 1;

    private readonly StorageRepository<TodoTask> _storage;

    internal TodoStore(string filename) {
      _storage = new StorageRepository<TodoTask>(
        filename, static todo => todo.Id, SchemaVersion
      );
    }

    internal Task<List<TodoTask>> GetAllAsync() {
      return _storage.GetAllAsync();
    }
  }
}
