using App.src.Todo;
using Xunit;

namespace App.tests.Tests {
  public sealed class TodoCreateTests {
    [Fact]
    public async Task CreateProject_PersistsNewProjectWithEmptyTaskList() {
      string path = Path.Combine(Path.GetTempPath(),
        $"todo-create-{Guid.NewGuid():N}.json"
      );
      try {
        TodoProject created =
          await new TodoStore(path).CreateProjectAsync(id =>
            new TodoProject(id, "New project", [])
          );

        TodoProject saved = Assert.Single(
          await new TodoStore(path).GetProjectsAsync()
        );
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(created.Id, saved.Id);
        Assert.Equal("New project", saved.Title);
        Assert.Empty(saved.Tasks);
      }
      finally {
        File.Delete(path);
      }
    }

    [Fact]
    public async Task AddTask_PersistsPendingTaskInSelectedProject() {
      string path = Path.Combine(Path.GetTempPath(),
        $"todo-add-task-{Guid.NewGuid():N}.json"
      );
      try {
        TodoStore store = new(path);
        TodoProject selected =
          await store.CreateProjectAsync(id =>
            new TodoProject(id, "Selected", [])
          );
        TodoProject other =
          await store.CreateProjectAsync(id => new TodoProject(id, "Other", [])
          );
        DateOnly dueDate = new(2026, 11, 10);

        await new TodoManager(store).AddTaskAsync(selected.Id, "Task",
          "Details", dueDate
        );

        List<TodoProject> saved = await new TodoStore(path).GetProjectsAsync();
        TodoTask task = Assert.Single(saved[0].Tasks);
        Assert.Equal(selected.Id, saved[0].Id);
        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(TodoStatus.Pending, task.Status);
        Assert.Equal("Task", task.Title);
        Assert.Equal("Details", task.Description);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal(other.Id, saved[1].Id);
        Assert.Empty(saved[1].Tasks);
      }
      finally {
        File.Delete(path);
      }
    }
  }
}
