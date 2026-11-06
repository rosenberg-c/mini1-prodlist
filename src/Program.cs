using App.src.Assets;

namespace App.src {
  public class Program {
    public static async Task Main() {
      await AssetsProgram.RunAsync();
      Console.WriteLine("Application closed.");
    }
  }
}
