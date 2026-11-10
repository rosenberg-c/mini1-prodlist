using App.src.Todo;
using Xunit;

namespace App.tests.Tests {
  public sealed class TodoUpdateTests {
    [Fact]
    public async Task
      UpdateTask_PersistsEditedFieldsWithoutChangingIdentityOrOtherTasks() {
      string path = Path.Combine(Path.GetTempPath(),
        $"todo-update-{Guid.NewGuid():N}.json"
      );
      try {
        TodoTask original = new(Guid.NewGuid(), TodoStatus.Pending,
          "Original", "Old details", null
        );
        TodoTask other = new(Guid.NewGuid(), TodoStatus.Completed,
          "Other", null, null
        );
        TodoStore store = new(path);
        TodoProject project = await store.CreateProjectAsync(id =>
          new TodoProject(id, "Project", [original, other])
        );
        DateOnly dueDate = new(2026, 11, 10);

        bool saved = await new TodoManager(store).UpdateTaskAsync(
          original.Id, "Updated", null, dueDate, TodoStatus.InProgress,
          project.Id
        );

        Assert.True(saved);
        TodoProject reloaded = Assert.Single(
          await new TodoStore(path).GetProjectsAsync()
        );
        Assert.Equal(project.Id, reloaded.Id);
        Assert.Equal(2, reloaded.Tasks.Count);
        TodoTask updated = reloaded.Tasks[0];
        Assert.Equal(original.Id, updated.Id);
        Assert.Equal("Updated", updated.Title);
        Assert.Null(updated.Description);
        Assert.Equal(dueDate, updated.DueDate);
        Assert.Equal(TodoStatus.InProgress, updated.Status);
        Assert.Equal(other, reloaded.Tasks[1]);
      }
      finally {
        File.Delete(path);
      }
    }

    [Fact]
    public async Task
      UpdateTask_ReturnsFalseAndLeavesProjectUnchangedWhenTaskIsMissing() {
      string path = Path.Combine(Path.GetTempPath(),
        $"todo-update-missing-{Guid.NewGuid():N}.json"
      );
      try {
        TodoTask existing = new(Guid.NewGuid(), TodoStatus.Pending,
          "Existing", null, null
        );
        TodoStore store = new(path);
        TodoProject project = await store.CreateProjectAsync(id =>
          new TodoProject(id, "Project", [existing])
        );

        bool saved = await new TodoManager(store).UpdateTaskAsync(
          Guid.NewGuid(), "Updated", null, null, TodoStatus.Completed,
          project.Id
        );

        Assert.False(saved);
        TodoProject reloaded = Assert.Single(
          await new TodoStore(path).GetProjectsAsync()
        );
        Assert.Equal(existing, Assert.Single(reloaded.Tasks));
      }
      finally {
        File.Delete(path);
      }
    }
  }
}
