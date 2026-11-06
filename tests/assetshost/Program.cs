using App.src.Assets;

namespace AssetsHost {
  public static class Program {
    public static async Task Main() {
      await AssetsProgram.RunAsync();
      Console.WriteLine("Application closed.");
    }
  }
}
