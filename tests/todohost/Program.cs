using App.src.Todo;

namespace TodoHost {
  public static class Program {
    public static async Task Main() {
      await TodoProgram.RunAsync();
      Console.WriteLine("Application closed.");
    }
  }
}
