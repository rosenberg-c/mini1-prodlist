using System.Diagnostics;
using System.Text.Json;
using Xunit;
using ProdList.src;
using Entry = ProdList.src.Product.Entry;

namespace ProdList.tests.Tests {
  public sealed class ProgramTests : IDisposable {
    private readonly string _dir = Path.Combine(
      Path.GetTempPath(), "prodlist-program-tests", Guid.NewGuid().ToString("N")
    );

    public ProgramTests() {
      Directory.CreateDirectory(_dir);
    }

    [Fact]
    public async Task Menu_AddSearchDeleteAndStatistics_PersistAcrossRuns() {
      string first = await RunAsync("1\nAB-300\n1\nAB-300\n1\nXY-500\n6\n");
      Assert.Contains("Product added successfully.", first);
      Assert.Contains("WARNING: Product already exists.", first);

      string path = Path.Combine(_dir, "products.json");
      List<Entry> saved = await ReadEntriesAsync(path);
      Assert.Equal(2, saved.Count);
      Assert.Equal(("AB", 300), (saved[0].Left, saved[0].Right));
      Assert.Equal(("XY", 500), (saved[1].Left, saved[1].Right));
      Assert.NotEqual(Guid.Empty, saved[0].Id);
      Assert.NotEqual(saved[0].Id, saved[1].Id);

      string second = await RunAsync("2\n3\n500\n3\nAB\n5\n4\nAB-300\n5\n6\n");
      string lines = second.ReplaceLineEndings("\n");
      Assert.Contains("Sorted valid products:\n- AB-300\n- XY-500", lines);
      Assert.Contains("Results:\n- XY-500", lines);
      Assert.Contains("Results:\n- AB-300", lines);
      Assert.Contains("- Total Products: 2", second);
      Assert.Contains("- Lowest Number: 300", second);
      Assert.Contains("- Highest Number: 500", second);
      Assert.Contains("- Average Number: 400", second);
      Assert.Contains("Product removed successfully.", second);
      Assert.Contains("- Total Products: 1", second);
      Assert.Equal([saved[1]], await ReadEntriesAsync(path));
    }

    [Fact]
    public async Task Menu_InvalidProductAndExitAtPrompt_DoNotWriteFile() {
      string output = await RunAsync("1\ninvalid\n1\n EXIT \n");

      Assert.Contains("ERROR: Product must contain a dash (-).", output);
      Assert.Contains("Application closed.", output);
      Assert.False(File.Exists(Path.Combine(_dir, "products.json")));
    }

    [Fact]
    public async Task Startup_RejectsLegacyProductsWithMigrationInstructions() {
      string path = Path.Combine(_dir, "products.json");
      const string legacyJson = "[{\"Id\":3,\"Name\":\"AB-300\"}]";
      await File.WriteAllTextAsync(path, legacyJson);

      string output = await RunAsync("");

      Assert.Contains("Cannot load products.json", output);
      Assert.Equal(legacyJson, await File.ReadAllTextAsync(path));
      Assert.False(File.Exists(path + ".v1.bak"));
    }

    private static async Task<List<Entry>> ReadEntriesAsync(string path) {
      using JsonDocument document = JsonDocument.Parse(
        await File.ReadAllTextAsync(path)
      );
      Assert.Equal(Product.SchemaVersion,
        document.RootElement.GetProperty("schemaVersion").GetInt32());
      return document.RootElement.GetProperty("data")
        .Deserialize<List<Entry>>() ?? [];
    }

    private async Task<string> RunAsync(string input) {
      ProcessStartInfo start = new("dotnet") {
        WorkingDirectory = _dir,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
      };
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
