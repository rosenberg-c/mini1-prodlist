using App.src.Todo;
using Xunit;

namespace App.tests.Tests {
  public sealed class TodoRenameTests {
    [Fact]
    public async Task
      RenameProject_PersistsTitleWithoutChangingTasksOrOtherProjects() {
      string path = Path.Combine(Path.GetTempPath(),
        $"todo-rename-{Guid.NewGuid():N}.json"
      );
      try {
        TodoTask task = new(Guid.NewGuid(), TodoStatus.Pending, "Task", null,
          null
        );
        TodoStore store = new(path);
        TodoProject renamed =
          await store.CreateProjectAsync(id =>
            new TodoProject(id, "Original", [task])
          );
        TodoProject other =
          await store.CreateProjectAsync(id => new TodoProject(id, "Other", [])
          );

        await store.UpdateProjectAsync(renamed.Id,
          project => project.WithTitle("Renamed")
        );

        List<TodoProject> saved = await new TodoStore(path).GetProjectsAsync();
        Assert.Equal(2, saved.Count);
        Assert.Equal(renamed.Id, saved[0].Id);
        Assert.Equal("Renamed", saved[0].Title);
        Assert.Equal([task], saved[0].Tasks);
        Assert.Equal(other.Id, saved[1].Id);
        Assert.Equal(other.Title, saved[1].Title);
        Assert.Empty(saved[1].Tasks);
      }
      finally {
        File.Delete(path);
      }
    }
  }
}
