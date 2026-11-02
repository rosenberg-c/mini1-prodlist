using App.src.Products;
using App.src.Todo;

namespace App.src {
  public class Program {
    public static async Task Main() {
      // await ProductProgram.RunAsync();
      await TodoProgram.RunAsync();
      Console.WriteLine("Application closed.");
    }
  }
}
