using System.Diagnostics.CodeAnalysis;

namespace App.src {
  public static class Support {
    public static bool IsExitOrNull([NotNullWhen(false)] string? value) {
      return value is null || string.Equals(
        value?.Trim(),
        "exit",
        StringComparison.OrdinalIgnoreCase
      );
    }

    public static string? ReadString(string prompt) {
      Console.Write(prompt);
      return Console.ReadLine()?.Trim();
    }
  }
}

