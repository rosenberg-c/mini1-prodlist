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
          "2\n3\n1\n \nApple\nMacBook Pro\n2024-22-15\n2024-03-15\n-1\n1500\n9\n1\n\nq\n"
        );

        Assert.Contains("1. Computer", output);
        Assert.Contains("2. Mobile phone", output);
        Assert.Contains("Choose 1-2.", output);
        Assert.Contains("This field cannot be empty.", output);
        Assert.Contains("Enter a valid date", output);
        Assert.Contains("Enter a non-negative price.", output);
        Assert.Contains("Choose 1-3.", output);
        Assert.Contains("1. Sweden", output);
        Assert.Contains("2. USA", output);
        Assert.Contains("3. Turkey", output);
        Assert.Contains("Asset added.", output);

        Asset asset = Assert.Single(await new AssetsStore(path).GetAllAsync());
        Computer computer = Assert.IsType<Computer>(asset);
        Assert.NotEqual(Guid.Empty, computer.Id);
        Assert.Equal("Apple", computer.Brand);
        Assert.Equal("MacBook Pro", computer.Model);
        Assert.Equal(new DateOnly(2024, 3, 15), computer.PurchaseDate);
        Assert.Equal(1500m, computer.PriceUsd);
        Assert.Equal(Office.Sweden, computer.Office);
        Assert.DoesNotContain("Price (local):", output);
        Assert.DoesNotContain("\"PriceLocal\"",
          await File.ReadAllTextAsync(path)
        );
        Assert.Contains("\"Office\": \"Sweden\"",
          await File.ReadAllTextAsync(path)
        );
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
          "2\n2\nSamsung\nGalaxy S23\n2025-01-10\n900\n3\n\nq\n"
        );

        Assert.Contains("Asset added.", output);
        Asset asset = Assert.Single(await new AssetsStore(path).GetAllAsync());
        Assert.IsType<MobilePhone>(asset);
        Assert.Equal(Office.Turkey, asset.Office);
      }
      finally {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Theory]
    [InlineData(Office.Sweden, 1234.50, "SEK")]
    [InlineData(Office.USA, 123.45, "USD")]
    [InlineData(Office.Turkey, 7407.00, "TRY")]
    public void ConvertPrice_UsesOfficeRate(
      Office office, double expectedAmount, string expectedCurrency
    ) {
      var (amount, currency) = CurrencyConverter.Convert(123.45m, office);
      Assert.Equal((decimal) expectedAmount, amount);
      Assert.Equal(expectedCurrency, currency);
    }

    [Theory]
    [InlineData(2022, 3, 15, 2024, 3, 14, 1, 11)]
    [InlineData(2022, 3, 15, 2024, 3, 15, 2, 0)]
    [InlineData(2024, 2, 29, 2025, 2, 27, 0, 11)]
    [InlineData(2024, 2, 29, 2025, 2, 28, 1, 0)]
    [InlineData(2024, 1, 31, 2024, 2, 28, 0, 0)]
    [InlineData(2024, 1, 31, 2024, 2, 29, 0, 1)]
    [InlineData(2027, 3, 15, 2026, 3, 15, 0, 0)]
    public void Age_CountsCompletedYearsAndMonths(
      int purchaseYear, int purchaseMonth, int purchaseDay,
      int todayYear, int todayMonth, int todayDay,
      int expectedYears, int expectedMonths
    ) {
      Asset asset = new Computer(Guid.NewGuid(), "Apple", "MacBook Pro",
        new DateOnly(purchaseYear, purchaseMonth, purchaseDay),
        100m, Office.USA
      );

      Assert.Equal((expectedYears, expectedMonths), asset.Age(
          new DateOnly(todayYear, todayMonth, todayDay)
        )
      );
    }

    [Fact]
    public async Task ShowAsset_IgnoresSavedLocalPriceAndRecalculates() {
      string directory = CreateTempDirectory();
      try {
        string path = Path.Combine(directory, "assets.json");
        AssetsStore store = new(path);
        await store.CreateAsync(id => new Computer(
            id, "Lenovo", "ThinkPad", new DateOnly(2024, 3, 15),
            1200m, Office.Turkey
          )
        );
        string json = await File.ReadAllTextAsync(path);
        await File.WriteAllTextAsync(path,
          json.Replace("\"PriceUsd\": 1200,",
            "\"PriceUsd\": 1200,\n      \"PriceLocal\": 1,"
          )
        );

        string output = await RunAppAsync(directory, path, "1\n1\nq\nq\n");
        Assert.Contains("72000.00 TRY", output);
        Assert.DoesNotContain("PriceLocal", output);
        Assert.Equal(1200m, Assert.Single(await store.GetAllAsync()).PriceUsd);
      }
      finally {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Fact]
    public async Task ShowAsset_DisplaysCalculatedAge() {
      string directory = CreateTempDirectory();
      try {
        string path = Path.Combine(directory, "assets.json");
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        DateOnly purchaseDate = today.AddMonths(-35);
        AssetsStore store = new(path);
        await store.CreateAsync(id => new Computer(
            id, "Apple", "Age check", purchaseDate, 100m, Office.USA
          )
        );

        string output = await RunAppAsync(directory, path, "1\n1\nq\nq\n");
        Assert.Contains("| Age ", output);
        string row = Assert.Single(output.Split('\n'),
          line => line.StartsWith("| Computer ")
        );
        Assert.Contains($"| {purchaseDate:yyyy-MM-dd} ", row);
        Assert.Contains("| 2y 11m ", row);
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
    public async Task AddAsset_QuitAtOfficeDoesNotSave() {
      string directory = CreateTempDirectory();
      try {
        string path = Path.Combine(directory, "assets.json");
        string output = await RunAppAsync(directory, path,
          "2\n1\nApple\nMacBook Pro\n2024-03-15\n1500\nq\n\nq\n"
        );

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
            1500m, Office.Sweden
          )
        );
        await store.CreateAsync(id => new MobilePhone(
            id, "Samsung", "Galaxy S23", new DateOnly(2025, 1, 10),
            900m, Office.USA
          )
        );

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
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        int ageWidth = (await store.GetAllAsync()).Max(asset => {
            var (years, months) = asset.Age(today);
            return $"{years}y {months}m".Length;
          }
        );
        int[] expectedWidths = [11, 7, 11, 13, ageWidth, 11, 13, 6, 6];
        string[] headerRows = rows.Where(row => row.StartsWith("| Type "))
          .ToArray();
        Assert.Equal(2, headerRows.Length);
        string headerRow = headerRows[0];
        Assert.Equal(expectedWidths.Select(width => width + 2),
          headerRow.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(cell => cell.Length)
        );
        Assert.Contains("+" + string.Concat(
            expectedWidths.Select(width => $"{new string('-', width + 2)}+"
            )
          ), rows
        );
        string[] computerRows = rows.Where(row => row.Contains("MacBook Pro"))
          .ToArray();
        Assert.Equal(2, computerRows.Length);
        Assert.All(computerRows, row => {
            Assert.Equal(headerRow.Length, row.Length);
            Assert.Contains("Computer", row);
            Assert.Contains("Apple", row);
            Assert.Contains("2024-03-15", row);
            Assert.Contains("1500.00", row);
            Assert.Contains("15000.00 SEK", row);
            Assert.Contains("Sweden", row);
          }
        );
        string[] phoneRows = rows.Where(row => row.Contains("Galaxy S23"))
          .ToArray();
        Assert.Equal(2, phoneRows.Length);
        Assert.All(phoneRows, row => {
            Assert.Equal(headerRow.Length, row.Length);
            Assert.Contains("MobilePhone", row);
            Assert.Contains("Samsung", row);
            Assert.Contains("2025-01-10", row);
            Assert.Contains("900.00", row);
            Assert.Contains("900.00 USD", row);
            Assert.Contains("USA", row);
          }
        );
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
            id, "Nokia", "Expired", today.AddYears(-4), 100m, Office.USA
          )
        );
        await store.CreateAsync(id => new Computer(
            id, "Apple", "Three months", today.AddMonths(3).AddYears(-3),
            100m, Office.USA
          )
        );
        await store.CreateAsync(id => new Computer(
            id, "Lenovo", "Six months", today.AddMonths(6).AddYears(-3),
            100m, Office.USA
          )
        );
        await store.CreateAsync(id => new Computer(
            id, "Dell", "Before red",
            today.AddMonths(3).AddDays(1).AddYears(-3),
            100m, Office.USA
          )
        );
        await store.CreateAsync(id => new Computer(
            id, "HP", "Before yellow",
            today.AddMonths(6).AddDays(1).AddYears(-3),
            100m, Office.USA
          )
        );
        await store.CreateAsync(id => new Computer(
            id, "Asus", "Safe", today.AddMonths(7).AddYears(-3),
            100m, Office.USA
          )
        );

        string output = await RunAppAsync(directory, path, "1\n3\nq\nq\n");
        Assert.Contains("3. By asset type, then purchase date", output);
        string[] rows = output.Split('\n')
          .Where(row =>
            row.StartsWith("| Computer ") || row.StartsWith("| MobilePhone ")
          )
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
    public async Task ShowByOffice_SortsByOfficeThenPurchaseDate() {
      string directory = CreateTempDirectory();
      try {
        string path = Path.Combine(directory, "assets.json");
        AssetsStore store = new(path);
        foreach (Office office in new[]
                   { Office.USA, Office.Turkey, Office.Sweden }) {
          await store.CreateAsync(id => new Computer(
              id, "Apple", office.ToString(), new DateOnly(2024, 3, 15),
              100m, office
            )
          );
        }

        await store.CreateAsync(id => new Computer(
            id, "Dell", "Older Sweden", new DateOnly(2023, 3, 15),
            100m, Office.Sweden
          )
        );

        string output = await RunAppAsync(directory, path, "1\n1\nq\nq\n");
        string[] rows = output.Split('\n')
          .Where(row => row.StartsWith("| Computer ")).ToArray();
        Assert.Equal(4, rows.Length);
        Assert.Contains("Sweden", rows[0]);
        Assert.Contains("Older Sweden", rows[0]);
        Assert.Contains("Sweden", rows[1]);
        Assert.Contains("Turkey", rows[2]);
        Assert.Contains("USA", rows[3]);
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

    [Fact]
    public async Task RemoveAsset_DeletesOnlySelectedAssetFromFile() {
      string directory = CreateTempDirectory();
      try {
        string path = Path.Combine(directory, "assets.json");
        AssetsStore store = new(path);
        Asset keep = await store.CreateAsync(id => new Computer(
            id, "Apple", "Keep", new DateOnly(2024, 3, 15),
            100m, Office.Sweden
          )
        );
        await store.CreateAsync(id => new MobilePhone(
            id, "Samsung", "Remove", new DateOnly(2025, 1, 10),
            200m, Office.Turkey
          )
        );

        string output = await RunAppAsync(directory, path,
          "4\n9\n2\ny\n\nq\n"
        );

        Assert.Contains("4. Remove", output);
        Assert.Contains("Purchase Date", output);
        Assert.Contains("|  1 | Computer", output);
        Assert.Contains("|  2 | MobilePhone", output);
        Assert.Contains("Choose 1-2.", output);
        Assert.Contains("Asset removed.", output);
        Asset remaining =
          Assert.Single(await new AssetsStore(path).GetAllAsync());
        Assert.Equal(keep.Id, remaining.Id);
      }
      finally {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Theory]
    [InlineData("4\nq\n\nq\n")]
    [InlineData("4\n1\nn\n\nq\n")]
    public async Task RemoveAsset_CancelLeavesFileUnchanged(string input) {
      string directory = CreateTempDirectory();
      try {
        string path = Path.Combine(directory, "assets.json");
        AssetsStore store = new(path);
        await store.CreateAsync(id => new Computer(
            id, "Apple", "Keep", new DateOnly(2024, 3, 15),
            100m, Office.Sweden
          )
        );
        string before = await File.ReadAllTextAsync(path);

        string output = await RunAppAsync(directory, path, input);

        Assert.DoesNotContain("Asset removed.", output);
        Assert.Equal(before, await File.ReadAllTextAsync(path));
      }
      finally {
        Directory.Delete(directory, recursive: true);
      }
    }

    [Fact]
    public async Task RemoveAsset_EmptyInventoryDoesNotCreateFile() {
      string directory = CreateTempDirectory();
      try {
        string path = Path.Combine(directory, "assets.json");
        string output = await RunAppAsync(directory, path, "4\n\nq\n");

        Assert.Contains("No assets to remove.", output);
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
