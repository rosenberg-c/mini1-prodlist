namespace App.src.ConsoleCore {
  internal static class ConsoleRender {
    private const int Pad = 40;

    internal static void Header(string text) {
      Console.Clear();
      Console.WriteLine("".PadRight(Pad, '-'));
      Console.WriteLine(text);
      Console.WriteLine("".PadRight(Pad, '-'));
    }

    internal static void Footer() {
      Console.WriteLine("".PadRight(Pad, '-'));
    }

  }
}
