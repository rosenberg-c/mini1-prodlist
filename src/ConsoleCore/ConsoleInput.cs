using System.Globalization;
using App.src.Validation;

namespace App.src.ConsoleCore {
  internal sealed class ConsoleInput {
    internal static bool TryReadOptionalText(
      string prompt, out string? value, string? current = null
    ) {
      while (true) {
        string label = current is null
          ? $"{prompt} (optional) "
          : $"{prompt} (optional) [{current}] ";
        label += "/reset to clear: ";
        if (!ConsoleSupport.TryReadInput(label, out string? input)) {
          value = string.Empty;
          return false;
        }

        string trimmed = input.Trim();
        if (trimmed.Length == 0) {
          value = current;
          return true;
        }

        if (trimmed.Equals("/reset", StringComparison.OrdinalIgnoreCase)) {
          value = null;
          return true;
        }

        if (TextFieldValidation.TryField(trimmed, out value)) {
          return true;
        }

        ConsoleSupport.WriteLineYellow("This field cannot be empty.");
      }
    }


    internal static bool TryReadText(
      string prompt, out string value, string? current = null
    ) {
      while (true) {
        string label = current is null
          ? $"{prompt}: "
          : $"{prompt} [{current}]: ";
        if (!ConsoleSupport.TryReadInput(label, out string? input)) {
          value = string.Empty;
          return false;
        }

        if (input.Length == 0 && current is not null) {
          value = current;
          return true;
        }

        if (TextFieldValidation.TryField(input, out value)) {
          return true;
        }

        ConsoleSupport.WriteLineYellow("This field cannot be empty.");
      }
    }

    internal static bool TryReadDate(
      string label,
      out DateOnly date, DateOnly? current = null
    ) {
      while (true) {
        string prompt = current is DateOnly saved
          ? $"{label} (yyyy-MM-dd) [{saved.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}]: "
          : $"{label} (yyyy-MM-dd): ";

        if (!ConsoleSupport.TryReadInput(prompt, out string? input)) {
          date = default;
          return false;
        }

        if (input.Length == 0 && current is DateOnly existing) {
          date = existing;
          return true;
        }

        if (DateOnly.TryParseExact(
              input, "yyyy-MM-dd", CultureInfo.InvariantCulture,
              DateTimeStyles.None, out date
            )) {
          return true;
        }

        ConsoleSupport.WriteLineYellow(
          "Enter a valid date in yyyy-MM-dd format."
        );
      }
    }

    internal static bool TryReadOptionalDate(
      string label,
      out DateOnly? date, DateOnly? current = null
    ) {
      while (true) {
        string prompt = current is DateOnly saved
          ? $"{label} (yyyy-MM-dd) [{saved.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}] "
          : $"{label} (yyyy-MM-dd, optional) ";
        prompt += "/reset to clear: ";
        if (!ConsoleSupport.TryReadInput(prompt, out string? input)) {
          date = default;
          return false;
        }

        string trimmed = input.Trim();
        if (trimmed.Length == 0) {
          date = current;
          return true;
        }

        if (trimmed.Equals("/reset", StringComparison.OrdinalIgnoreCase)) {
          date = null;
          return true;
        }

        if (DateOnly.TryParseExact(
              trimmed, "yyyy-MM-dd", CultureInfo.InvariantCulture,
              DateTimeStyles.None, out DateOnly parsed
            )) {
          date = parsed;
          return true;
        }

        ConsoleSupport.WriteLineYellow(
          "Enter a valid date in yyyy-MM-dd format, or leave blank."
        );
      }
    }
  }
}
