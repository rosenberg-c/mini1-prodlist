using System.Diagnostics.CodeAnalysis;

namespace App.src {
  public static class Support {
    public static bool IsEmptyOrNull([NotNullWhen(false)] string? value) {
      return value == string.Empty || value is null;
    }

    public static bool IsExitOrNull([NotNullWhen(false)] string? value) {
      return value is null || string.Equals(
        value?.Trim(),
        "q",
        StringComparison.OrdinalIgnoreCase
      );
    }

    public static string? ReadString(string prompt) {
      Console.Write(prompt);
      return Console.ReadLine()?.Trim();
    }
  }
}

