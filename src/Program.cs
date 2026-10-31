using App.src.Products;

namespace App.src {
  public class Program {
    public static async Task Main() {
      await ProductProgram.RunAsync();
      Console.WriteLine("Application closed.");
    }
  }
}
