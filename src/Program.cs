using App.src.Todo;

namespace App.src {
  public class Program {
    public static async Task Main() {
      await TodoProgram.RunAsync();
      Console.WriteLine("Application closed.");
    }
  }
}
