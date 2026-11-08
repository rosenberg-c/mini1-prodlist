using System.Text.Json;
using System.Text.Json.Nodes;
using App.src.Todo;
using Xunit;

namespace App.tests.Tests {
  public sealed class TodoValidationTests {
    private static TodoTask TaskItem() =>
      new(Guid.NewGuid(), TodoStatus.Pending, "Task", "", null);

    private static TodoProject Project(params TodoTask[] tasks) =>
      new(Guid.NewGuid(), "Project", [.. tasks]);

    [Fact]
    public void ValidProjectsAndTasksAreAccepted() {
      TodoValidator.Validate([Project(TaskItem()), Project()]);
    }

    [Fact]
    public void InvalidProjectsAreRejected() {
      TodoProject project = Project();
      Assert.Throws<InvalidDataException>(() =>
        TodoValidator.Validate([project with { Id = Guid.Empty }])
      );
      Assert.Throws<InvalidDataException>(() =>
        TodoValidator.Validate([project with { Title = "  " }])
      );
      Assert.Throws<InvalidDataException>(() =>
        TodoValidator.Validate([project with { Tasks = null! }])
      );
      Assert.Throws<InvalidDataException>(() =>
        TodoValidator.Validate([project, project])
      );
      Assert.Throws<InvalidDataException>(() =>
        TodoValidator.Validate([null!])
      );
    }

    [Fact]
    public void InvalidTasksAreRejected() {
      TodoTask task = TaskItem();
      Assert.Throws<InvalidDataException>(() =>
        TodoValidator.Validate([Project(task with { Id = Guid.Empty })])
      );
      Assert.Throws<InvalidDataException>(() =>
        TodoValidator.Validate([Project(task with { Title = " " })])
      );
      Assert.Throws<InvalidDataException>(() =>
        TodoValidator.Validate([Project(task with { Status = (TodoStatus) 99 })]
        )
      );
      Assert.Throws<InvalidDataException>(() =>
        TodoValidator.Validate([Project(new TodoTask[] { null! })])
      );
      Assert.Throws<InvalidDataException>(() =>
        TodoValidator.Validate([Project(task), Project(task)])
      );
    }

    [Fact]
    public async Task StoreRejectsInvalidJsonDataBeforeReturningIt() {
      string path = Path.Combine(Path.GetTempPath(),
        $"todo-validation-{Guid.NewGuid():N}.json"
      );
      try {
        TodoProject invalid = Project() with { Tasks = null! };
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(new {
              schemaVersion = TodoStore.SchemaVersion,
              data = new[] { invalid }
            }
          )
        );

        TodoStore store = new(path);
        await Assert.ThrowsAsync<InvalidDataException>(store.GetProjectsAsync);
      }
      finally {
        File.Delete(path);
      }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StoreRejectsExtraProjectOrTaskFields(bool onTask) {
      string path = Path.Combine(Path.GetTempPath(),
        $"todo-extra-field-{Guid.NewGuid():N}.json"
      );
      try {
        string json = JsonSerializer.Serialize(new {
            schemaVersion = TodoStore.SchemaVersion,
            data = new[] { Project(TaskItem()) }
          }
        );
        JsonNode document = JsonNode.Parse(json)!;
        JsonNode project = document["data"]![0]!;
        JsonNode target = onTask ? project["Tasks"]![0]! : project;
        target["CreatedDate"] = "2026-11-01";
        await File.WriteAllTextAsync(path, document.ToJsonString());

        TodoStore store = new(path);
        await Assert.ThrowsAsync<JsonException>(store.GetProjectsAsync);
      }
      finally {
        File.Delete(path);
      }
    }
  }
}
