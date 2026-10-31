using App.src.Products;

namespace ProductHost {
  public static class Program {
    public static async Task Main() {
      await ProductProgram.RunAsync();
      Console.WriteLine("Application closed.");
    }
  }
}
