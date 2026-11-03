using System.Diagnostics.CodeAnalysis;

namespace App.src.ConsoleCore {
  public static class ConsoleSupport {
    internal const string QuitCommand = "q";

    internal static void Pause() {
      ConsoleRender.Footer();
      Console.WriteLine("Press any key to return.");
      HoldPressAnyKey();
    }

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

    internal static bool TryReadMenuChoice(
      [NotNullWhen(true)] out string? choice,
      string prompt = "Select option: "
    ) {
      return TryReadInput(prompt, out choice);
    }

    internal static bool TryReadInput(
      string prompt, [NotNullWhen(true)] out string? value
    ) {
      value = ReadString(prompt);
      return !IsExitOrNull(value);
    }

    public static void WriteLineGreen(string prompt) {
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine(prompt);
      Console.ResetColor();
    }

    public static void WriteWhiteBackground(string prompt) {
      Console.BackgroundColor = ConsoleColor.White;
      Console.ForegroundColor = ConsoleColor.Black;
      Console.Write(prompt);
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
