using System.Text.Json;
using App.src.Todo;
using Xunit;

namespace App.tests.Tests {
  public sealed class TodoMoveTests {
    [Fact]
    public async Task MoveTask_MovesOnlySelectedTaskBetweenProjects() {
      string path = Path.Combine(Path.GetTempPath(),
        $"todo-move-{Guid.NewGuid():N}.json"
      );
      try {
        TodoTask keep = new(Guid.NewGuid(), TodoStatus.Pending, "Keep", null,
          null
        );
        TodoTask move = new(Guid.NewGuid(), TodoStatus.InProgress, "Move",
          "Details", new DateOnly(2026, 11, 9)
        );
        TodoTask existing = new(Guid.NewGuid(), TodoStatus.Completed,
          "Existing", null, null
        );
        TodoProject source = new(Guid.NewGuid(), "Source", [keep, move]);
        TodoProject destination =
          new(Guid.NewGuid(), "Destination", [existing]);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new {
              schemaVersion = TodoStore.SchemaVersion,
              data = new[] { source, destination }
            }
          )
        );

        await new TodoStore(path).MoveTaskAsync(source.Id, destination.Id,
          move.Id
        );

        List<TodoProject> saved = await new TodoStore(path).GetProjectsAsync();
        Assert.Equal(2, saved.Count);
        Assert.Equal(source.Id, saved[0].Id);
        Assert.Equal([keep], saved[0].Tasks);
        Assert.Equal(destination.Id, saved[1].Id);
        Assert.Equal([existing, move], saved[1].Tasks);
      }
      finally {
        File.Delete(path);
      }
    }
  }
}
