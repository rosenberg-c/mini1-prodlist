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
  }
}
