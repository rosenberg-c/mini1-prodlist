using App.src.Todo;
using Xunit;

namespace App.tests.Tests {
  public sealed class TodoDisplayTests {
    private static readonly object ConsoleLock = new();

    [Fact]
    public void SimpleTasksByDate_OrdersAndWrapsCells() {
      TodoTask later = new(
        Guid.NewGuid(), TodoStatus.Pending, new string('L', 19),
        "First line\r\nSecond line", new DateOnly(2026, 11, 10)
      );
      TodoTask earlier = new(
        Guid.NewGuid(), TodoStatus.Completed, "Earlier", null,
        new DateOnly(2026, 11, 9)
      );

      string[] lines = Capture(() =>
        TodoDisplay.DisplaySimpleTasksByDate([later, earlier])
      );

      Assert.Equal(6, lines.Length);
      Assert.Equal(new string('-', 80), lines[0]);
      Assert.Equal(Row("Title", "Status", "Due", "Description"), lines[1]);
      Assert.Equal(lines[0], lines[2]);
      Assert.Equal(Row("Earlier", "Completed", earlier.dispDueDate, ""),
        lines[3]
      );
      Assert.Equal(Row(new string('L', 18), "Pending", later.dispDueDate,
          "First line"
        ), lines[4]
      );
      Assert.Equal(Row("L", "", "", "Second line"), lines[5]);
    }

    [Fact]
    public void SimpleTasksByDate_EmptyListStillDisplaysHeader() {
      string[] lines = Capture(() =>
        TodoDisplay.DisplaySimpleTasksByDate([])
      );

      Assert.Equal(3, lines.Length);
      Assert.Equal(new string('-', 80), lines[0]);
      Assert.Equal(Row("Title", "Status", "Due", "Description"), lines[1]);
      Assert.Equal(lines[0], lines[2]);
    }

    [Fact]
    public void SearchResults_ShowsProjectAndTaskDetails() {
      TodoTask task = new(Guid.NewGuid(), TodoStatus.Pending, "Plan",
        "Details", new DateOnly(2026, 11, 10)
      );
      TodoProject project = new(Guid.NewGuid(), "Home", [task]);

      string[] lines = Capture(() => TodoDisplay.DisplaySearchResults(
          [(task, project)], highlightMatches: true
        )
      );

      Assert.Contains(lines, line => line.Contains("Project") &&
                                     line.Contains("Title")
      );
      Assert.Contains(lines, line => line.Contains("Home") &&
                                     line.Contains("Plan") &&
                                     line.Contains("Details")
      );
    }

    private static string Row(params string[] cells) =>
      string.Concat(cells.Select(cell => cell.PadRight(20)));

    private static string[] Capture(Action write) {
      lock (ConsoleLock) {
        TextWriter original = Console.Out;
        using StringWriter output = new();
        try {
          Console.SetOut(output);
          write();
        }
        finally {
          Console.SetOut(original);
        }

        return output.ToString().ReplaceLineEndings("\n")
          .Split('\n', StringSplitOptions.RemoveEmptyEntries);
      }
    }
  }
}
