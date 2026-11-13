using App.src.Todo;
using Xunit;

namespace App.tests.Tests {
  public sealed class TodoSearchTests {
    [Fact]
    public void FindTasks_MatchesTitleOrProjectIgnoringCase_AndSortsByDate() {
      TodoTask later = new(Guid.NewGuid(), TodoStatus.Pending, "WRITE report",
        null, new DateOnly(2026, 12, 2)
      );
      TodoTask earlier = new(Guid.NewGuid(), TodoStatus.Completed, "Review",
        null, new DateOnly(2026, 12, 1)
      );
      TodoTask unrelated = new(Guid.NewGuid(), TodoStatus.Pending, "Other",
        null, null
      );
      TodoProject writing = new(Guid.NewGuid(), "Work", [later, unrelated]);
      TodoProject projectMatch = new(Guid.NewGuid(), "Writers", [earlier]);

      var matches = TodoActions.FindTasks([writing, projectMatch], "wri");

      Assert.Equal([earlier, later], matches.Select(match => match.Task));
      Assert.Equal([projectMatch, writing],
        matches.Select(match => match.Project)
      );
    }

    [Fact]
    public void FindTasks_EmptyQueryListsAllAndMissingQueryReturnsNothing() {
      TodoTask task = new(Guid.NewGuid(), TodoStatus.Pending, "Plan",
        null, null
      );
      TodoProject project = new(Guid.NewGuid(), "Home", [task]);

      Assert.Equal(task,
        Assert.Single(TodoActions.FindTasks([project], "")).Task
      );
      Assert.Empty(TodoActions.FindTasks([project], "missing"));
      Assert.Empty(TodoActions.FindTasks([], ""));
    }
  }
}
