using App.src.ConsoleCore;
using Xunit;

namespace App.tests.Tests {
  public sealed class ConsoleTableTests {
    private static readonly object ConsoleLock = new();

    [Fact]
    public void Write_NumberedRows_AlignsBordersHeadersAndCells() {
      string[] lines = Capture(() => ConsoleTable.Write(
          ["Category", "Name", "Price"],
          [
            ["Office", "Pen", "1.25 kr"],
            ["Food", "Pizza", "120.5 kr"]
          ]
        )
      );

      string segments = "----------+-------+----------+";
      Assert.Equal("     +" + segments, lines[0]);
      Assert.Equal("     | Category | Name  | Price    |", lines[1]);
      Assert.Equal("+----+" + segments, lines[2]);
      Assert.Equal("|  1 | Office   | Pen   | 1.25 kr  |", lines[3]);
      Assert.Equal("|  2 | Food     | Pizza | 120.5 kr |", lines[4]);
      Assert.Equal(lines[2], lines[5]);
      Assert.Equal("", lines[6]);
      Assert.Equal(lines[2].Length, lines[3].Length);
    }

    [Fact]
    public void Write_EditChoices_UsesWidestCellAndNumbersRows() {
      string[] lines = Capture(() => ConsoleTable.Write(
          ["EDIT"], [["Category"], ["Name"], ["Price"]]
        )
      );

      Assert.Equal("     +----------+", lines[0]);
      Assert.Equal("     | EDIT     |", lines[1]);
      Assert.Equal("+----+----------+", lines[2]);
      Assert.Equal("|  1 | Category |", lines[3]);
      Assert.Equal("|  2 | Name     |", lines[4]);
      Assert.Equal("|  3 | Price    |", lines[5]);
      Assert.Equal(lines[2], lines[6]);
    }

    [Fact]
    public void Write_LongValues_ExpandOnlyTheirOwnColumns() {
      const string longValue = "Extra long product name - 100 kr";
      string[] lines = Capture(() => ConsoleTable.Write(
          ["Statistic", "Value"], [["Most expensive", longValue]]
        )
      );

      string segments = new string('-', 16) + "+" +
                        new string('-', longValue.Length + 2) + "+";
      Assert.Equal("     +" + segments, lines[0]);
      Assert.Equal("     | Statistic      | " +
                   "Value".PadRight(longValue.Length) + " |", lines[1]
      );
      Assert.Equal("+----+" + segments, lines[2]);
      Assert.Equal("|  1 | Most expensive | " + longValue + " |", lines[3]);
      Assert.Equal(lines[2], lines[4]);
    }

    [Fact]
    public void Write_UnnumberedRows_HaveNoNumberColumn() {
      string[] lines = Capture(() => ConsoleTable.Write(
          ["Category", "Count"], [["Food", "2"], ["Office", "1"]],
          numberRows: false
        )
      );

      Assert.Equal("+----------+-------+", lines[0]);
      Assert.Equal("| Category | Count |", lines[1]);
      Assert.Equal(lines[0], lines[2]);
      Assert.Equal("| Food     | 2     |", lines[3]);
      Assert.Equal("| Office   | 1     |", lines[4]);
      Assert.Equal(lines[0], lines[5]);
    }

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

        return output.ToString().ReplaceLineEndings("\n").Split('\n');
      }
    }
  }
}
