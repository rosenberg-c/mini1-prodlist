namespace App.src.ConsoleCore {
  internal enum SelectionStatus {
    Selected,
    Invalid,
    Cancelled
  }

  internal readonly record struct NumberedSelectionResult(
    SelectionStatus Status,
    int Index
  );

  internal static class NumberedSelection {
    internal static NumberedSelectionResult
      ReadIndex(int count, string prompt) {
      return ParseIndex(count, ConsoleSupport.ReadString(prompt));
    }

    internal static NumberedSelectionResult
      ParseIndex(int count, string? input) {
      if (ConsoleSupport.IsExitOrNull(input)) {
        return new NumberedSelectionResult(SelectionStatus.Cancelled, -1);
      }

      if (!int.TryParse(input, out int num) || num < 1 || num > count) {
        return new NumberedSelectionResult(SelectionStatus.Invalid, -1);
      }

      return new NumberedSelectionResult(SelectionStatus.Selected, num - 1);
    }
  }
}
