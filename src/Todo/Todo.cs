using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Globalization;

namespace App.src.Todo {
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum TodoStatus {
    Pending,
    [Description("In Progress")] InProgress,
    Completed
  }

  [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
  public sealed record TodoTask(
    Guid Id,
    TodoStatus Status,
    string Title,
    string? Description,
    DateOnly? DueDate
  ) {
    [JsonIgnore] public string dispDesc => Description ?? string.Empty;

    [JsonIgnore]
    public string dispDueDate => DueDate?.ToString(
      CultureInfo.InvariantCulture
    ) ?? string.Empty;

    [JsonIgnore] public string dispStatus => Status.ToString();

    public TodoTask Copy() {
      return new TodoTask(Id, Status, Title, Description, DueDate);
    }

    public TodoTask WithStatus(TodoStatus value) {
      return this with { Status = value };
    }

    public TodoTask WithTitle(string value) {
      return this with { Title = value };
    }

    public TodoTask WithDescription(string? value) {
      return this with { Description = value };
    }

    public TodoTask WithDueDate(DateOnly? value) {
      return this with { DueDate = value };
    }
  }

  [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
  public sealed record TodoProject(
    Guid Id,
    string Title,
    List<TodoTask> Tasks
  ) {
    public TodoProject WithTitle(string value) {
      return this with { Title = value };
    }
  }
}
