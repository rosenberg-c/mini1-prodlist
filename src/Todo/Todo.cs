using System.Text.Json.Serialization;

namespace App.src.Todo {
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum TodoStatus {
    Pending,
    InProgress,
    Completed
  }

  public record ATask(
    Guid Id,
    TodoStatus Status,
    string Title,
    string Description,
    DateOnly DueDate,
    DateOnly CreatedDate,
    DateOnly? FinishDate
  );

  public sealed record Project(
    Guid Id,
    string Title,
    List<ATask> Tasks
  );
}
