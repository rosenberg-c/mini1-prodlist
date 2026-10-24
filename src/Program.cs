using System.Text.Json;
using ProdList.src;
using Entry = ProdList.src.Product.Entry;
using InputError = ProdList.src.Product.InputError;

string path = Path.Combine(Directory.GetCurrentDirectory(), "products.json");
StorageRepository<Entry> repo = new(
  path, static e => e.Id, Product.SchemaVersion
);

try {
  _ = await repo.GetAllAsync();
}
catch (Exception error) when (error is JsonException or InvalidDataException) {
  Console.WriteLine($"Cannot load products.json: {error.Message}");
  Console.WriteLine("Delete products.json to start with an empty inventory,");
  Console.WriteLine("or migrate a legacy file with:");
  Console.WriteLine("python3 scripts/migrate-products.py products.json");
  return;
}

Console.Clear();
await RunAsync();
Console.WriteLine("Application closed.");

async Task RunAsync() {
  Console.WriteLine("----------------------------------------");
  Console.WriteLine("PRODUCT INVENTORY SYSTEM - LEVEL 4");
  Console.WriteLine("----------------------------------------");

  while (true) {
    Console.WriteLine();
    Console.WriteLine("1. Add Product");
    Console.WriteLine("2. View Products");
    Console.WriteLine("3. Search Product");
    Console.WriteLine("4. Delete Product");
    Console.WriteLine("5. Statistics");
    Console.WriteLine("6. Exit");
    Console.WriteLine();

    string? choice = Support.ReadString("Select option: ");

    if (Support.IsExitOrNull(choice)) {
      return;
    }

    switch (choice) {
      case "1": {
        if (await AddAsync() == Menu.Exit) {
          return;
        }
        break;
      }

      case "2": {
        await ViewAsync();
        break;
      }

      case "3": {
        if (await SearchAsync() == Menu.Exit) {
          return;
        }
        break;
      }

      case "4":
        if (await DeleteAsync() == Menu.Exit) {
          return;
        }
        break;

      case "5": {
        await StatisticsAsync();
        break;
      }

      // Exit
      case "6":
        return;

      default:
        Console.WriteLine("Invalid option. Please try again.");
        break;
    }
  }
}

async Task<Menu> AddAsync() {
  string? product = Support.ReadString("Enter product: ");

  if (Support.IsExitOrNull(product)) {
    return Menu.Exit;
  }

  InputError? error = Product.Validate(product, out string left, out int right);
  if (error is InputError code) {
    Console.WriteLine(Ui.MessageFrom(code));
    return Menu.Continue;
  }

  CreateStatus result = await repo.TryCreateAsync(
    id => new Entry(id, left, right),
    entry => Product.IsDuplicate(entry, left, right)
  );

  Console.WriteLine(Ui.MessageFrom(result));
  return Menu.Continue;
}

async Task ViewAsync() {
  List<Entry> entries = await repo.GetAllAsync();
  Console.WriteLine("Sorted valid products:");

  foreach (Entry entry in Product.Sort(entries)) {
    Console.WriteLine("- " + entry.Name);
  }
}

async Task<Menu> SearchAsync() {
  string? query = Support.ReadString("Search product: ");

  if (Support.IsExitOrNull(query)) {
    return Menu.Exit;
  }

  if (query.Length == 0) {
    Console.WriteLine("Search cannot be empty.");
    return Menu.Continue;
  }

  List<Entry> entries = await repo.GetAllAsync();
  var matches = Product.Sort(entries)
    .Where(e => e.Name.Contains(
        query, StringComparison.OrdinalIgnoreCase
      )
    )
    .ToList();

  Console.WriteLine("Results:");

  if (matches.Count == 0) {
    Console.WriteLine("No products found.");
  }

  foreach (Entry entry in matches) {
    Console.WriteLine("- " + entry.Name);
  }

  return Menu.Continue;
}

async Task<Menu> DeleteAsync() {
  string? name = Support.ReadString("Enter product to delete: ");

  if (Support.IsExitOrNull(name)) {
    return Menu.Exit;
  }

  List<Entry> entries = await repo.GetAllAsync();
  Entry? match = entries.FirstOrDefault(e =>
    string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase)
  );

  if (match is null) {
    Console.WriteLine("Product not found.");
    return Menu.Continue;
  }

  _ = await repo.DeleteAsync(match.Id);

  Console.WriteLine("Product removed successfully.");
  return Menu.Continue;
}

async Task StatisticsAsync() {
  List<Entry> entries = await repo.GetAllAsync();
  var numbers = entries.Select(static entry => entry.Right).ToList();

  Console.WriteLine("Statistics:");
  Console.WriteLine($"- Total Products: {entries.Count}");

  if (numbers.Count == 0) {
    Console.WriteLine("- Lowest Number: N/A");
    Console.WriteLine("- Highest Number: N/A");
    Console.WriteLine("- Average Number: N/A");
  } else {
    Console.WriteLine($"- Lowest Number: {numbers.Min()}");
    Console.WriteLine($"- Highest Number: {numbers.Max()}");
    Console.WriteLine($"- Average Number: {numbers.Average():0.##}");
  }
}

internal enum Menu {
  Continue,
  Exit
}
