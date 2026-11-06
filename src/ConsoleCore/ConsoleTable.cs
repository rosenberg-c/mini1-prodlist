namespace App.src.ConsoleCore {
  internal static class ConsoleTable {
    internal static void Write(
      IReadOnlyList<string> headers,
      IEnumerable<IReadOnlyList<string>> rows,
      int width = 1,
      bool numberRows = true,
      bool highlightRows = false,
      string? header = null,
      Func<int, ConsoleColor?>? rowColor = null
    ) {
      // Validate the table and requested minimum width.
      if (headers.Count == 0 || width < 1) {
        throw new ArgumentException(
          "A table needs columns with a positive width."
        );
      }

      // Size each column to its header or widest cell.
      IReadOnlyList<string>[] tableRows = rows.ToArray();
      int[] columnWidths = headers
        .Select(header => Math.Max(width, header.Length)).ToArray();

      foreach (IReadOnlyList<string> cells in tableRows) {
        if (cells.Count != headers.Count) {
          throw new ArgumentException("Each row must have one cell per column.",
            nameof(rows)
          );
        }

        for (int i = 0; i < cells.Count; i++) {
          columnWidths[i] = Math.Max(columnWidths[i], cells[i].Length);
        }
      }

      // Build borders with room for optional row numbers.
      string headerPrefix = numberRows ? "     " : "";
      string borderPrefix = numberRows ? "+----+" : "+";

      string segments = string.Concat(
        columnWidths.Select(columnWidth =>
          $"{new string('-', columnWidth + 2)}+"
        )
      );
      string borderH =
        $"{headerPrefix}+{segments}";

      string border = $"{borderPrefix}{segments}";

      // Write the optional title and column headings.
      if (header != null) {
        Console.Write($"{"".PadRight(numberRows ? 5 : 0, ' ')}");
        ConsoleSupport.WriteWhiteBackground($" {header} ");
        Console.WriteLine();
      }

      Console.WriteLine(borderH);
      Console.WriteLine($"{headerPrefix}{FormatRow(headers, columnWidths)}");
      Console.WriteLine(border);

      // Write each row, highlighting when requested.
      int index = 0;
      foreach (IReadOnlyList<string> cells in tableRows) {
        string prefix = numberRows ? $"| {index + 1,2} " : "";
        string line = $"{prefix}{FormatRow(cells, columnWidths)}";
        ConsoleColor? color = rowColor?.Invoke(index);
        if (color is ConsoleColor.Red) {
          ConsoleSupport.WriteLineRed(line);
        } else if (color is ConsoleColor.Yellow) {
          ConsoleSupport.WriteLineYellow(line);
        } else if (highlightRows) {
          ConsoleSupport.WriteLineGreen(line);
        } else {
          Console.WriteLine(line);
        }

        index++;
      }

      Console.WriteLine(border);
      Console.WriteLine();
    }

    // Pad cells to their corresponding column widths.
    private static string FormatRow(
      IReadOnlyList<string> cells, IReadOnlyList<int> widths
    ) {
      return $"|{string.Concat(cells.Select(
        (cell, index) => $" {cell.PadRight(widths[index])} |"
      ))}";
    }
  }
}
