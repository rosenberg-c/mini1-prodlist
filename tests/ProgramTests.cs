using System.Diagnostics;
using System.Globalization;
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
        "1\nElectronics\nMouse\n299.50\nOffice\nMouse\n120\nq\n4\nx\nq\n"
      );
      Assert.Equal(2, first.Split("Product added successfully!").Length - 1);
      Assert.Contains(" 1. | Office", first);
      Assert.Contains(" 2. | Electronics", first);
      Assert.True(first.IndexOf(" 1. | Office", StringComparison.Ordinal) <
        first.IndexOf(" 2. | Electronics", StringComparison.Ordinal));
      Assert.Contains("299.5 kr", first);
      Assert.Contains("TOTAL PRICE: 419.5 kr", first);

      string path = Path.Combine(_dir, "products.json");
      List<Product> saved = await ReadProductsAsync(path);
      Assert.Equal(2, saved.Count);
      Assert.Equal("Mouse", saved[0].Name);
      Assert.Equal("Mouse", saved[1].Name);
      Assert.NotEqual(saved[0].Id, saved[1].Id);

      string second = await RunAsync("2\n2\nq\n4\nx\nq\n");
      Assert.Contains("Product removed successfully.", second);
      Assert.Contains(" 1. | Office", second);
      Assert.Contains(" 2. | Electronics", second);
      Assert.Contains("TOTAL PRICE: 120 kr", second);
      Assert.Equal([saved[1]], await ReadProductsAsync(path));
    }

    [Theory]
    [InlineData("1\n\nq\n")]
    [InlineData("1\nFood\n\nq\n")]
    [InlineData("1\nFood\nPizza\n\nq\n")]
    [InlineData("1\nFood\nPizza\nabc\nq\n")]
    [InlineData("1\nFood\nPizza\n-100\nq\n")]
    [InlineData("1\n   \nq\n")]
    [InlineData("1\nFood\n   \nq\n")]
    [InlineData("1\nFood\nPizza\n   \nq\n")]
    [InlineData("1\n")]
    [InlineData("1\nFood\n")]
    [InlineData("1\nFood\nPizza\n")]
    [InlineData("1\nq\n")]
    [InlineData("1\nFood\nq\n")]
    [InlineData("1\nFood\nPizza\nq\n")]
    public async Task Menu_IncompleteOrInvalidEntry_DoesNotWriteFile(string input) {
      string output = await RunAsync(input);

      Assert.DoesNotContain("Product added successfully!", output);
      Assert.Contains("Application closed.", output);
      Assert.False(File.Exists(Path.Combine(_dir, "products.json")));
    }

    [Theory]
    [InlineData("1\n\nFood\nPizza\n120\nq\n",
      "ERROR: Category cannot be empty.")]
    [InlineData("1\nFood\n   \nPizza\n120\nq\n",
      "ERROR: Name cannot be empty.")]
    [InlineData("1\nFood\nPizza\nabc\n120\nq\n",
      "ERROR: Invalid price. Please enter a numeric value.")]
    [InlineData("1\nFood\nPizza\n\n120\nq\n",
      "ERROR: Invalid price. Please enter a numeric value.")]
    [InlineData("1\nFood\nPizza\n999999999999999999999999999999999\n120\nq\n",
      "ERROR: Invalid price. Please enter a numeric value.")]
    [InlineData("1\nFood\nPizza\n-100\n120\nq\n",
      "ERROR: Price cannot be negative.")]
    public async Task Menu_InvalidField_RetriesSamePromptAndSavesOnce(
      string input, string error
    ) {
      string output = await RunAsync(input);

      Assert.Contains(error, output);
      Assert.Equal(1, output.Split("Product added successfully!").Length - 1);
      Product product = Assert.Single(await ReadProductsAsync(
        Path.Combine(_dir, "products.json")
      ));
      Assert.Equal("Food", product.Category);
      Assert.Equal("Pizza", product.Name);
      Assert.Equal(120m, product.Price);
    }

    [Fact]
    public async Task Menu_ZeroAndFractionalPrices_DisplayAndPersist() {
      string output = await RunAsync(
        "1\nFood\nSample\n0\nq\n4\nx\n1\nOffice\nPen\n299.50\nq\n4\nx\nq\n"
      );

      Assert.Contains("TOTAL PRICE: 0 kr", output);
      Assert.Contains("299.5 kr", output);
      Assert.Contains("TOTAL PRICE: 299.5 kr", output);
      List<Product> products = await ReadProductsAsync(
        Path.Combine(_dir, "products.json")
      );
      Assert.Equal([0m, 299.50m], products.Select(p => p.Price));
    }

    [Fact]
    public async Task Menu_AfterViewing_CanAddAgainAndViewUpdatedTotal() {
      string output = await RunAsync(
        "1\nFood\nPizza\n120\nq\n4\nx\n1\nOffice\nPen\n1.25\nq\n4\nx\nq\n"
      );

      Assert.Equal(2, output.Split("Product added successfully!").Length - 1);
      Assert.True(output.IndexOf("TOTAL PRICE: 120 kr", StringComparison.Ordinal) <
        output.IndexOf("TOTAL PRICE: 121.25 kr", StringComparison.Ordinal));
      Assert.Contains("Application closed.", output);
      Assert.Equal(2, (await ReadProductsAsync(
        Path.Combine(_dir, "products.json")
      )).Count);
    }

    [Fact]
    public async Task Menu_OverflowingTotal_ReportsErrorAndKeepsProducts() {
      string maximum = decimal.MaxValue.ToString(CultureInfo.InvariantCulture);
      string output = await RunAsync(
        $"1\nFood\nFirst\n{maximum}\nFood\nSecond\n{maximum}\nq\n4\nx\nq\n"
      );

      Assert.Contains("ERROR: Total price is too large.", output);
      Assert.DoesNotContain("TOTAL PRICE:", output);
      Assert.Contains("Application closed.", output);
      Assert.Equal(2, (await ReadProductsAsync(
        Path.Combine(_dir, "products.json")
      )).Count);
    }

    [Fact]
    public async Task Menu_BadOptionAndInvalidIndexes_KeepProducts() {
      string output = await RunAsync(
        "9\n1\nFood\nPizza\n120\nq\n2\nabc\n0\n-1\n2\nq\n4\nq\n"
      );

      Assert.Contains("Invalid option. Please try again.", output);
      Assert.Equal(4, output.Split("Invalid product number.").Length - 1);
      Assert.Contains(" 1. | Food", output);
      Assert.Single(await ReadProductsAsync(Path.Combine(_dir, "products.json")));
    }

    [Fact]
    public async Task Menu_DeleteSeveral_RedisplaysRemainingNumbersAndReturnsOnQ() {
      string output = await RunAsync(
        "1\nFood\nPizza\n120\nOffice\nPen\n1.25\nElectronics\nMouse\n299\nq\n2\n2\n1\nq\n4\nq\n"
      );

      Assert.Equal(2, output.Split("Product removed successfully.").Length - 1);
      Assert.Contains(" 2. | Food", output);
      Assert.Contains(" 1. | Office", output);
      Assert.Contains("TOTAL PRICE: 299 kr", output);
      Product remaining = Assert.Single(await ReadProductsAsync(
        Path.Combine(_dir, "products.json")
      ));
      Assert.Equal("Mouse", remaining.Name);
    }

    [Fact]
    public async Task Menu_DeletePromptEof_ReturnsToMenuWithoutRemovingProduct() {
      string output = await RunAsync("1\nFood\nPizza\n120\nq\n2\n");

      Assert.Contains("Application closed.", output);
      Assert.DoesNotContain("Product removed successfully.", output);
      Assert.Single(await ReadProductsAsync(Path.Combine(_dir, "products.json")));
    }

    [Fact]
    public async Task Menu_EmptyViewAndDelete_PausesBeforeReturningToMenu() {
      string output = await RunAsync("4\nx\n2\nx\n4\nx\nq\n");

      Assert.Equal(2, output.Split("TOTAL PRICE: 0 kr").Length - 1);
      Assert.Contains("No products to delete.", output);
      Assert.Contains("Press any key to continue...", output);
      Assert.Contains("Application closed.", output);
      Assert.False(File.Exists(Path.Combine(_dir, "products.json")));
    }

    [Fact]
    public async Task Menu_AfterDeletingLastProduct_PausesBeforeReturningToMenu() {
      string output = await RunAsync("1\nFood\nPizza\n120\nq\n2\n1\nx\n4\nq\n");

      Assert.Contains("Product removed successfully.", output);
      Assert.Contains("No products to delete.", output);
      Assert.Contains("Press any key to continue...", output);
      Assert.Contains("TOTAL PRICE: 0 kr", output);
      Assert.Empty(await ReadProductsAsync(Path.Combine(_dir, "products.json")));
    }

    [Fact]
    public async Task Menu_SearchByNameAndCategory_IsCaseInsensitiveAndLeavesProductsUnchanged() {
      string output = await RunAsync(
        "1\nElectronics\nMouse\n299\nFood\nMousepad\n120\nElectronics\nKeyboard\n30\nq\n5\nmOuSe\nELECTRONICS\nq\nq\n"
      );
      string[] searches = output.Split("Search category or name: ");

      Assert.True(searches.Length >= 3);
      Assert.Matches(@"Electronics\s+\|\s+Mouse\s+\|\s+299 kr", searches[1]);
      Assert.Matches(@"Food\s+\|\s+Mousepad\s+\|\s+120 kr", searches[1]);
      Assert.DoesNotContain("Keyboard", searches[1]);
      Assert.Matches(@"Electronics\s+\|\s+Mouse\s+\|\s+299 kr", searches[2]);
      Assert.Matches(@"Electronics\s+\|\s+Keyboard\s+\|\s+30 kr", searches[2]);
      Assert.DoesNotContain("Mousepad", searches[2]);
      Assert.Equal(3, (await ReadProductsAsync(
        Path.Combine(_dir, "products.json")
      )).Count);
    }

    [Fact]
    public async Task Menu_SearchNoMatchesThenEmptyQuery_ListsAllProducts() {
      string output = await RunAsync(
        "1\nFood\nPizza\n120\nOffice\nPen\n1.25\nq\n5\nmissing\n\nq\nq\n"
      );
      string[] searches = output.Split("Search category or name: ");

      Assert.True(searches.Length >= 3);
      Assert.Contains("No products found.", searches[1]);
      Assert.DoesNotContain("Pizza", searches[1]);
      Assert.Contains("Empty search query, list all.", searches[2]);
      Assert.Matches(@"Office\s+\|\s+Pen\s+\|\s+1.25 kr", searches[2]);
      Assert.Matches(@"Food\s+\|\s+Pizza\s+\|\s+120 kr", searches[2]);
      Assert.Contains("Application closed.", output);
    }

    [Fact]
    public async Task Menu_EditEachField_ReordersByPriceAndKeepsTheSameIdAcrossRuns() {
      string path = Path.Combine(_dir, "products.json");
      ProductStore store = new(path);
      Product food = await store.CreateAsync("Food", "Pizza", 120m);
      Product office = await store.CreateAsync("Office", "Pen", 777m);

      string output = await RunAsync(
        "3\n2\n3\n0\n1\n1\nElectronics\n1\n2\nStylus\nq\n4\nx\nq\n"
      );

      Assert.Equal(3, output.Split("Product updated successfully.").Length - 1);
      Assert.Contains(" 1. | Electronics", output);
      Assert.Contains("TOTAL PRICE: 120 kr", output);
      Assert.Equal([food, office with {
        Category = "Electronics", Name = "Stylus", Price = 0m
      }], await ReadProductsAsync(path));

      string nextRun = await RunAsync("4\nx\nq\n");
      Assert.Matches(@"Electronics\s+\|\s+Stylus\s+\|\s+0 kr", nextRun);
      Assert.Contains("TOTAL PRICE: 120 kr", nextRun);
    }

    [Theory]
    [InlineData("3\n1\n1\nq\n")]
    [InlineData("3\n1\n2\n")]
    [InlineData("3\n1\n3\n-1\nq\n")]
    [InlineData("3\n0\nq\n")]
    public async Task Menu_CancelOrIncompleteEdit_DoesNotChangeStoredProduct(
      string input
    ) {
      string path = Path.Combine(_dir, "products.json");
      ProductStore store = new(path);
      Product original = await store.CreateAsync("Food", "Pizza", 120m);
      string before = await File.ReadAllTextAsync(path);

      string output = await RunAsync(input);

      Assert.DoesNotContain("Product updated successfully.", output);
      Assert.Contains("Application closed.", output);
      Assert.Equal(before, await File.ReadAllTextAsync(path));
      Assert.Equal([original], await ReadProductsAsync(path));
    }

    [Fact]
    public async Task Menu_InvalidEditFields_RetryWithoutSavingUntilValid() {
      string path = Path.Combine(_dir, "products.json");
      ProductStore store = new(path);
      Product original = await store.CreateAsync("Food", "Pizza", 120m);

      string output = await RunAsync("3\n1\n9\n2\n\nRevised\nq\nq\n");

      Assert.Contains("Invalid option. Please try again.", output);
      Assert.Contains("ERROR: Name cannot be empty.", output);
      Assert.Equal(1, output.Split("Product updated successfully.").Length - 1);
      Assert.Equal([original with { Name = "Revised" }],
        await ReadProductsAsync(path));
    }

    [Fact]
    public async Task Menu_EditEmptyInventory_PausesAndReturnsToMenu() {
      string output = await RunAsync("3\nx\n4\nx\nq\n");

      Assert.Contains("No products to edit.", output);
      Assert.Contains("Press any key to continue...", output);
      Assert.Contains("TOTAL PRICE: 0 kr", output);
      Assert.False(File.Exists(Path.Combine(_dir, "products.json")));
    }

    [Fact]
    public async Task Menu_ViewTable_AlignsHeaderBordersAndNumberedRows() {
      string path = Path.Combine(_dir, "products.json");
      ProductStore store = new(path);
      await store.CreateAsync("Food", "Pizza", 120.50m);
      await store.CreateAsync("Office", "Pen", 1.25m);

      string output = await RunAsync("4\nx\nq\n");
      string[] lines = output.Split('\n');
      string border = "    +" + new string('-', 18) + "+" +
        new string('-', 18) + "+" + new string('-', 18) + "+";
      string header = "    | " + "Category".PadRight(16) + " | " +
        "Name".PadRight(16) + " | " + "Price".PadRight(16) + " |";
      string first = " 1. | " + "Office".PadRight(16) + " | " +
        "Pen".PadRight(16) + " | " + "1.25 kr".PadRight(16) + " |";
      string second = " 2. | " + "Food".PadRight(16) + " | " +
        "Pizza".PadRight(16) + " | " + "120.5 kr".PadRight(16) + " |";

      Assert.Contains(border, lines);
      Assert.Contains(header, lines);
      Assert.Contains(first, lines);
      Assert.Contains(second, lines);
      Assert.Equal(border.Length, header.Length);
      Assert.Equal(border.Length, first.Length);
      Assert.Equal(border.Length, second.Length);
    }

    [Fact]
    public async Task Menu_Statistics_DisplaysExtremesAverageAndCategoryCounts() {
      string path = Path.Combine(_dir, "products.json");
      ProductStore store = new(path);
      Product food = await store.CreateAsync("Food", "Banana", 10m);
      Product office = await store.CreateAsync("Office", "Pen", 20m);
      Product foodAgain = await store.CreateAsync("Food", "Pizza", 30m);

      string output = await RunAsync("6\nx\nq\n");

      Assert.Contains("Most expensive: Pizza - 30 kr", output);
      Assert.Contains("Cheapest: Banana - 10 kr", output);
      Assert.Contains("Average price: 20 kr", output);
      Assert.Contains("Food: 2", output);
      Assert.Contains("Office: 1", output);
      Assert.Contains("Press any key to return.", output);
      Assert.Equal([food, office, foodAgain], await ReadProductsAsync(path));
    }

    [Fact]
    public async Task Menu_Statistics_EmptyInventoryExplainsNoProducts() {
      string output = await RunAsync("6\nx\nq\n");

      Assert.Contains("No products available for statistics.", output);
      Assert.DoesNotContain("Average price:", output);
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
