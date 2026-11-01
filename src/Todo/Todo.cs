namespace App.src.Todo {
  public enum TodoStatus {
    Pending,
    InProgress,
    Completed
  }

  public sealed record TodoTask(
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
    List<TodoTask> Tasks
  );
}
