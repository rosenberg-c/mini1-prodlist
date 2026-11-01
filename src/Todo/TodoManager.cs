namespace App.src.Todo {
  internal class TodoManager {
    private readonly TodoStore _store;

    internal TodoManager(TodoStore store) {
      _store = store;
    }

    internal Task<List<TodoTask>> GetAllAsync() {
      return Task.FromResult(new List<TodoTask>());
    }

    private void CreateAsync() {
    }

    internal Task<bool> DeleteAsync(Guid _) {
      return Task.FromResult(false);
    }

    internal Task<bool> UpdateAsync(Guid _) {
      return Task.FromResult(false);
    }

    internal static List<TodoTask> SortForDisplay(IEnumerable<TodoTask> todos) {
      return todos.ToList();
    }
  }
}
