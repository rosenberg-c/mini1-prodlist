using System.Text.Json.Serialization;
using System.ComponentModel;

namespace App.src.Todo {
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum TodoStatus {
    Pending,
    [Description("In Progress")] InProgress,
    Completed
  }

  [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
  public record TodoTask(
    Guid Id,
    TodoStatus Status,
    string Title,
    string? Description,
    DateOnly? DueDate
  );

  [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
  public sealed record TodoProject(
    Guid Id,
    string Title,
    List<TodoTask> Tasks
  );
}
