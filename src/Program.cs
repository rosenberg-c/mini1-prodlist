using App.src.Products;

namespace App.src {
  public class Program {
    public static async Task Main() {
      await ProductProgram.RunProductAsync();
      Console.WriteLine("Application closed.");
    }
  }
}
