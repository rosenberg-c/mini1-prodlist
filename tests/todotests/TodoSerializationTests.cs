using System.Text.Json;
using App.src.Todo;
using Xunit;

namespace App.tests.Tests {
  public sealed class TodoSerializationTests {
    [Theory]
    [InlineData(TodoStatus.Pending, 0)]
    [InlineData(TodoStatus.InProgress, 1)]
    [InlineData(TodoStatus.Completed, 2)]
    public void Status_SerializesAsTextAndReadsLegacyNumber(
      TodoStatus status, int legacyValue
    ) {
      TodoTask task = new(
        Guid.NewGuid(), status, "Task", "Description",
        new DateOnly(2026, 11, 5)
      );
      TodoProject project = new(Guid.NewGuid(), "Project", [task]);

      string json = JsonSerializer.Serialize(project);
      Assert.Contains($"\"Status\":\"{status}\"", json);
      TodoProject? roundTrip = JsonSerializer.Deserialize<TodoProject>(json);
      Assert.Equal(status, Assert.Single(roundTrip!.Tasks).Status);

      string legacyJson = json.Replace(
        $"\"Status\":\"{status}\"", $"\"Status\":{legacyValue}"
      );
      TodoProject? loaded = JsonSerializer.Deserialize<TodoProject>(legacyJson);
      Assert.Equal(status, Assert.Single(loaded!.Tasks).Status);
    }
  }
}
