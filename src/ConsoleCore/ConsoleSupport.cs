using System.Diagnostics.CodeAnalysis;

namespace App.src.ConsoleCore {
  public static class ConsoleSupport {
    internal const string QuitCommand = "q";

    public static bool IsExitOrNull([NotNullWhen(false)] string? value) {
      return value is null || string.Equals(
        value?.Trim(),
        QuitCommand,
        StringComparison.OrdinalIgnoreCase
      );
    }

    public static string? ReadString(string prompt) {
      Console.Write(prompt);
      return Console.ReadLine()?.Trim();
    }

    public static void HoldPressAnyKey() {
      if (Console.IsInputRedirected) {
        _ = Console.ReadLine();
      } else {
        _ = Console.ReadKey(intercept: true);
      }
    }

    public static void WriteLineGreen(string prompt) {
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine(prompt);
      Console.ResetColor();
    }

    public static void WriteLineRed(string prompt) {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine(prompt);
      Console.ResetColor();
    }

    public static void WriteLineYellow(string prompt) {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine(prompt);
      Console.ResetColor();
    }
  }
}
