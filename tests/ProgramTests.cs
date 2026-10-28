using System.Diagnostics;
using System.Text.Json;
using App.src.Products;
using Xunit;

namespace App.tests.Tests {
  public sealed class ProgramTests : IDisposable {
    private readonly string _dir = Path.Combine(
      Path.GetTempPath(), "prodlist-program-tests", Guid.NewGuid().ToString("N")
    );

    public ProgramTests() {
      Directory.CreateDirectory(_dir);
    }

    [Fact]
    public async Task Menu_AddViewAndDeleteByIndex_PersistAcrossRuns() {
      string first = await RunAsync(
        "1\nElectronics\nMouse\n299.50\n1\nOffice\nMouse\n120\n3\nq\n"
      );
      Assert.Equal(2, first.Split("Product added successfully!").Length - 1);
      Assert.Contains(" 1. Office", first);
      Assert.Contains(" 2. Electronics", first);
      Assert.True(first.IndexOf(" 1. Office", StringComparison.Ordinal) <
        first.IndexOf(" 2. Electronics", StringComparison.Ordinal));
      Assert.Contains("299.5 kr", first);
      Assert.Contains("TOTAL PRICE: 419.5 kr", first);

      string path = Path.Combine(_dir, "products.json");
      List<Product> saved = await ReadProductsAsync(path);
      Assert.Equal(2, saved.Count);
      Assert.Equal("Mouse", saved[0].Name);
      Assert.Equal("Mouse", saved[1].Name);
      Assert.NotEqual(saved[0].Id, saved[1].Id);

      string second = await RunAsync("2\n2\n3\nq\n");
      Assert.Contains("Product removed successfully.", second);
      Assert.Contains(" 1. Office", second);
      Assert.Contains(" 2. Electronics", second);
      Assert.Contains("TOTAL PRICE: 120 kr", second);
      Assert.Equal([saved[1]], await ReadProductsAsync(path));
    }

    [Theory]
    [InlineData("1\n\nq\n")]
    [InlineData("1\nFood\n\nq\n")]
    [InlineData("1\nFood\nPizza\n\nq\n")]
    [InlineData("1\nFood\nPizza\nabc\nq\n")]
    [InlineData("1\nFood\nPizza\n-100\nq\n")]
    [InlineData("1\n")]
    public async Task Menu_IncompleteOrInvalidEntry_DoesNotWriteFile(string input) {
      string output = await RunAsync(input);

      Assert.DoesNotContain("Product added successfully!", output);
      Assert.False(File.Exists(Path.Combine(_dir, "products.json")));
    }

    [Fact]
    public async Task Menu_BadOptionAndInvalidIndexes_KeepProducts() {
      string output = await RunAsync(
        "9\n1\nFood\nPizza\n120\n2\nabc\n2\n0\n2\n-1\n2\n2\n3\nq\n"
      );

      Assert.Contains("Invalid option. Please try again.", output);
      Assert.Equal(4, output.Split("Invalid product number.").Length - 1);
      Assert.Contains(" 1. Food", output);
      Assert.Single(await ReadProductsAsync(Path.Combine(_dir, "products.json")));
    }

    [Fact]
    public async Task Menu_EmptyViewAndDelete_ThenQuitWithoutWriting() {
      string output = await RunAsync("3\n2\nq\n");

      Assert.Contains("TOTAL PRICE: 0 kr", output);
      Assert.Contains("No products to delete.", output);
      Assert.Contains("Application closed.", output);
      Assert.False(File.Exists(Path.Combine(_dir, "products.json")));
    }

    [Fact]
    public async Task Startup_RejectsIncompatibleSchemaWithoutChangingFile() {
      string path = Path.Combine(_dir, "products.json");
      const string legacyJson = "{\"schemaVersion\":3,\"data\":[]}";
      await File.WriteAllTextAsync(path, legacyJson);

      string output = await RunAsync("");

      Assert.Contains("Cannot load products.json", output);
      Assert.Equal(legacyJson, await File.ReadAllTextAsync(path));
    }

    private static async Task<List<Product>> ReadProductsAsync(string path) {
      using JsonDocument document = JsonDocument.Parse(
        await File.ReadAllTextAsync(path)
      );
      Assert.Equal(Product.SchemaVersion,
        document.RootElement.GetProperty("schemaVersion").GetInt32());
      return document.RootElement.GetProperty("data")
        .Deserialize<List<Product>>() ?? [];
    }

    private async Task<string> RunAsync(string input) {
      ProcessStartInfo start = new("dotnet") {
        WorkingDirectory = _dir,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
      };
      start.Environment["DOTNET_SYSTEM_GLOBALIZATION_INVARIANT"] = "1";
      start.ArgumentList.Add(typeof(Product).Assembly.Location);

      using Process process = Process.Start(start)
        ?? throw new InvalidOperationException("Could not start the app.");
      try {
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        await process.StandardInput.WriteAsync(input);
        process.StandardInput.Close();

        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(15));
        await process.WaitForExitAsync(timeout.Token);
        string stdout = await output;
        string stderr = await error;
        Assert.True(process.ExitCode == 0,
          $"App exited with {process.ExitCode}: {stderr}");
        return stdout;
      }
      finally {
        if (!process.HasExited) {
          process.Kill(entireProcessTree: true);
        }
      }
    }

    public void Dispose() {
      GC.SuppressFinalize(this);
      Directory.Delete(_dir, recursive: true);
    }
  }
}
