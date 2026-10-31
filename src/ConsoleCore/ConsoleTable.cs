namespace App.src.ConsoleCore {
  internal static class ConsoleTable {
    internal const int MinimumCellWidth = 24;

    internal static void Write(
      IReadOnlyList<string> headers,
      IEnumerable<IReadOnlyList<string>> rows,
      int width = MinimumCellWidth,
      bool numberRows = false,
      bool highlightRows = false
    ) {
      if (headers.Count == 0 || width < 1) {
        throw new ArgumentException(
          "A table needs columns with a positive width."
        );
      }

      IReadOnlyList<string>[] tableRows = rows.ToArray();
      int cellWidth = Math.Max(
        MinimumCellWidth,
        Math.Max(width, headers.Max(static header => header.Length)
        )
      );
      foreach (IReadOnlyList<string> cells in tableRows) {
        if (cells.Count != headers.Count) {
          throw new ArgumentException("Each row must have one cell per column.",
            nameof(rows)
          );
        }

        cellWidth = Math.Max(cellWidth, cells.Max(static cell => cell.Length));
      }

      string headerPrefix = numberRows ? "|    " : "";
      string borderPrefix = numberRows ? "+----+" : "+";

      string segment = new('-', cellWidth + 2);
      string border =
        $"{borderPrefix}{string.Concat(Enumerable.Repeat($"{segment}+", headers.Count))}";
      Console.WriteLine(border);
      Console.WriteLine($"{headerPrefix}{FormatRow(headers, cellWidth)}");
      Console.WriteLine(border);

      int index = 0;
      foreach (IReadOnlyList<string> cells in tableRows) {
        string prefix = numberRows ? $"| {index + 1,2} " : "";
        string line = $"{prefix}{FormatRow(cells, cellWidth)}";
        if (highlightRows) {
          ConsoleSupport.WriteLineGreen(line);
        } else {
          Console.WriteLine(line);
        }

        index++;
      }

      Console.WriteLine(border);
      Console.WriteLine();
    }

    private static string FormatRow(IReadOnlyList<string> cells, int width) {
      return
        $"|{string.Concat(cells.Select(cell => $" {cell.PadRight(width)} |"))}";
    }
  }
}
