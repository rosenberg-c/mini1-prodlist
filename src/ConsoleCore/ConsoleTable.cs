namespace App.src.ConsoleCore {
  internal static class ConsoleTable {
    internal const int MinimumCellWidth = 24;

    internal static void Write(
      IReadOnlyList<string> headers,
      IEnumerable<IReadOnlyList<string>> rows,
      int width = MinimumCellWidth,
      bool numberRows = true,
      bool highlightRows = false,
      string? header = null
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

      string headerPrefix = numberRows ? "     " : "";
      string borderPrefix = numberRows ? "+----+" : "+";

      string segment = new('-', cellWidth + 2);
      string borderH =
        $"{headerPrefix}+{string.Concat(Enumerable.Repeat($"{segment}+", headers.Count))}";

      string border =
        $"{borderPrefix}{string.Concat(Enumerable.Repeat($"{segment}+", headers.Count))}";

      if (header != null) {
        Console.Write($"{"".PadRight(numberRows ? 5 : 0, ' ')}");
        ConsoleSupport.WriteWhiteBackground($" {header} ");
        Console.WriteLine();
      }

      Console.WriteLine(borderH);
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
