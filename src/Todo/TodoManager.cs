namespace App.src.Todo {
  internal class TodoManager {
    private readonly TodoStore _store;

    internal TodoManager(TodoStore store) {
      _store = store;
    }

    internal async Task<List<Project>> GetAllAsync() {
      return await _store.GetAllAsync();
    }

    private void CreateAsync() {
    }

    internal Task<bool> DeleteAsync(Guid _) {
      return Task.FromResult(false);
    }

    internal Task<bool> UpdateAsync(Guid _) {
      return Task.FromResult(false);
    }

    internal static List<ATask> SortForDisplay(IEnumerable<ATask> todos) {
      return todos.ToList();
    }
  }
}
