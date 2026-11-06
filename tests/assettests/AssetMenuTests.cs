using System.Diagnostics;
using App.src.Assets;
using Xunit;

namespace App.tests.Tests {
  public sealed class AssetMenuTests {
    [Fact]
    public async Task AddComputer_RetriesInvalidFieldsAndPersistsAsset() {
      string directory = CreateTempDirectory();
      try {
        string path = Path.Combine(directory, "assets.json");
        string output = await RunAppAsync(directory, path,
          "2\n3\n1\n \nApple\nMacBook Pro\n2024-22-15\n2024-03-15\n-1\n1500\n15000\nSweden\n\nq\n"
        );

        Assert.Contains("Choose 1 or 2.", output);
        Assert.Contains("This field cannot be empty.", output);
        Assert.Contains("Enter a valid date", output);
        Assert.Contains("Enter a non-negative price.", output);
        Assert.Contains("Asset added.", output);

        Asset asset = Assert.Single(await new AssetsStore(path).GetAllAsync());
        Computer computer = Assert.IsType<Computer>(asset);
        Assert.NotEqual(Guid.Empty, computer.Id);
        Assert.Equal("Apple", computer.Brand);
        Assert.Equal("MacBook Pro", computer.Model);
        Assert.Equal(new DateOnly(2024, 3, 15), computer.PurchaseDate);
        Assert.Equal(1500m, computer.PriceUsd);
        Assert.Equal(15000m, computer.PriceLocal);
        Assert.Equal("Sweden", computer.Office);
      }
      finally {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Fact]
    public async Task AddMobilePhone_PersistsConcreteType() {
      string directory = CreateTempDirectory();
      try {
        string path = Path.Combine(directory, "assets.json");
        string output = await RunAppAsync(directory, path,
          "2\n2\nSamsung\nGalaxy S23\n2025-01-10\n900\n9000\nSweden\n\nq\n"
        );

        Assert.Contains("Asset added.", output);
        Asset asset = Assert.Single(await new AssetsStore(path).GetAllAsync());
        Assert.IsType<MobilePhone>(asset);
      }
      finally {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Fact]
    public async Task AddAsset_QuitAtPromptDoesNotSave() {
      string directory = CreateTempDirectory();
      try {
        string path = Path.Combine(directory, "assets.json");
        string output = await RunAppAsync(directory, path, "2\n1\nq\n\nq\n");

        Assert.DoesNotContain("Asset added.", output);
        Assert.False(File.Exists(path));
      }
      finally {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Fact]
    public async Task ShowMenu_DisplaysBothViewsAndReturnsToMainMenu() {
      string directory = CreateTempDirectory();
      try {
        string path = Path.Combine(directory, "assets.json");
        AssetsStore store = new(path);
        await store.CreateAsync(id => new Computer(
          id, "Apple", "MacBook Pro", new DateOnly(2024, 3, 15),
          1500m, 15000m, "Sweden"
        ));
        await store.CreateAsync(id => new MobilePhone(
          id, "Samsung", "Galaxy S23", new DateOnly(2025, 1, 10),
          900m, 9000m, "USA"
        ));

        string output = await RunAppAsync(directory, path,
          "1\n9\n1\n2\nq\n2\nq\n\nq\n"
        );

        Assert.Contains("1. By office", output);
        Assert.Contains("2. By purchase date", output);
        Assert.Contains("Invalid option. Please try again.", output);
        Assert.Contains("Price (USD)", output);
        Assert.Contains("Price (Local)", output);
        Assert.Equal(2, output.Split("| Sweden ").Length - 1);
        string[] rows = output.Split('\n');
        int[] expectedWidths = [11, 7, 11, 13, 11, 13, 6, 6];
        string[] headerRows = rows.Where(row => row.StartsWith("| Type "))
          .ToArray();
        Assert.Equal(2, headerRows.Length);
        string headerRow = headerRows[0];
        Assert.Equal(expectedWidths.Select(width => width + 2),
          headerRow.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(cell => cell.Length)
        );
        Assert.Contains("+" + string.Concat(expectedWidths.Select(
          width => $"{new string('-', width + 2)}+"
        )), rows);
        string[] computerRows = rows.Where(row => row.Contains("MacBook Pro"))
          .ToArray();
        Assert.Equal(2, computerRows.Length);
        Assert.All(computerRows, row => {
          Assert.Equal(headerRow.Length, row.Length);
          Assert.Contains("Computer", row);
          Assert.Contains("Apple", row);
          Assert.Contains("2024-03-15", row);
          Assert.Contains("1500", row);
          Assert.Contains("15000", row);
          Assert.Contains("Sweden", row);
        });
        string[] phoneRows = rows.Where(row => row.Contains("Galaxy S23"))
          .ToArray();
        Assert.Equal(2, phoneRows.Length);
        Assert.All(phoneRows, row => {
          Assert.Equal(headerRow.Length, row.Length);
          Assert.Contains("MobilePhone", row);
          Assert.Contains("Samsung", row);
          Assert.Contains("2025-01-10", row);
          Assert.Contains("900", row);
          Assert.Contains("9000", row);
          Assert.Contains("USA", row);
        });
        Assert.Contains("\nAdd\n", output);
      }
      finally {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Fact]
    public async Task ShowByTypeAndDate_SortsAndAppliesEndOfLifeWarnings() {
      string directory = CreateTempDirectory();
      try {
        string path = Path.Combine(directory, "assets.json");
        AssetsStore store = new(path);
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        await store.CreateAsync(id => new MobilePhone(
          id, "Nokia", "Expired", today.AddYears(-4), 100m, 100m, "USA"
        ));
        await store.CreateAsync(id => new Computer(
          id, "Apple", "Three months", today.AddMonths(3).AddYears(-3),
          100m, 100m, "USA"
        ));
        await store.CreateAsync(id => new Computer(
          id, "Lenovo", "Six months", today.AddMonths(6).AddYears(-3),
          100m, 100m, "USA"
        ));
        await store.CreateAsync(id => new Computer(
          id, "Dell", "Before red", today.AddMonths(3).AddDays(1).AddYears(-3),
          100m, 100m, "USA"
        ));
        await store.CreateAsync(id => new Computer(
          id, "HP", "Before yellow", today.AddMonths(6).AddDays(1).AddYears(-3),
          100m, 100m, "USA"
        ));
        await store.CreateAsync(id => new Computer(
          id, "Asus", "Safe", today.AddMonths(7).AddYears(-3),
          100m, 100m, "USA"
        ));

        string output = await RunAppAsync(directory, path, "1\n3\nq\nq\n");
        Assert.Contains("3. By asset type, then purchase date", output);
        string[] rows = output.Split('\n')
          .Where(row => row.StartsWith("| Computer ") || row.StartsWith("| MobilePhone "))
          .ToArray();
        Assert.Equal(6, rows.Length);
        Assert.Contains("Three months", rows[0]);
        Assert.Contains("RED", rows[0]);
        Assert.Contains("Before red", rows[1]);
        Assert.Contains("YELLOW", rows[1]);
        Assert.Contains("Six months", rows[2]);
        Assert.Contains("YELLOW", rows[2]);
        Assert.Contains("Before yellow", rows[3]);
        Assert.DoesNotContain("RED", rows[3]);
        Assert.DoesNotContain("YELLOW", rows[3]);
        Assert.Contains("Safe", rows[4]);
        Assert.DoesNotContain("RED", rows[4]);
        Assert.DoesNotContain("YELLOW", rows[4]);
        Assert.Contains("Expired", rows[5]);
        Assert.Contains("RED", rows[5]);
      }
      finally {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Fact]
    public async Task ShowMenu_EmptyInventoryExplainsAndReturns() {
      string directory = CreateTempDirectory();
      try {
        string path = Path.Combine(directory, "assets.json");
        string output = await RunAppAsync(directory, path, "1\n\nq\n");

        Assert.Contains("No assets to show.", output);
        Assert.Contains("Application closed.", output);
        Assert.False(File.Exists(path));
      }
      finally {
        Directory.Delete(directory, recursive: true);
      }
    }

    private static string CreateTempDirectory() {
      string path = Path.Combine(Path.GetTempPath(),
        "asset-menu-tests", Guid.NewGuid().ToString("N")
      );
      Directory.CreateDirectory(path);
      return path;
    }

    private static async Task<string> RunAppAsync(
      string directory, string path, string input
    ) {
      ProcessStartInfo start = new("dotnet") {
        WorkingDirectory = directory,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
      };
      start.ArgumentList.Add(typeof(AssetsHost.Program).Assembly.Location);
      start.Environment["APP_DATA_PATH"] = path;

      using Process process = Process.Start(start)
                              ?? throw new InvalidOperationException(
                                "Could not start the app."
                              );
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
          $"App exited with {process.ExitCode}: {stderr}"
        );
        return stdout;
      }
      finally {
        if (!process.HasExited) {
          process.Kill(entireProcessTree: true);
        }
      }
    }
  }
}
