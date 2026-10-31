using App.src.ConsoleCore;

namespace App.src.Products {
  internal sealed class ProductDisplay {
    internal static void DisplayTable(
      List<Product> products,
      bool highlightMatches = false
    ) {
      ConsoleTable.Write(
        ["Category", "Name", "Price"],
        products.Select(static p => (IReadOnlyList<string>)[
            p.Category, p.Name, $"{p.Price:0.##} kr"
          ]
        ),
        numberRows: true,
        highlightRows: highlightMatches
      );
    }

    internal static void DisplayEditOptions() {
      Console.WriteLine();
      ConsoleTable.Write(
        ["EDIT"],
        [["Category"], ["Name"], ["Price"]],
        numberRows: true
      );
    }

    internal static void DisplayStatistics(ProductListResult result) {
      List<Product> products = result.Products;
      Product cheapest = products[0];
      Product mostExpensive = products[^1];
      decimal average = result.Total / products.Count;

      IReadOnlyList<string>[] summary = [
        [
          "Most expensive",
          $"{mostExpensive.Name} - {mostExpensive.Price:0.##} kr"
        ],
        ["Cheapest", $"{cheapest.Name} - {cheapest.Price:0.##} kr"],
        ["Average price", $"{average:0.##} kr"]
      ];

      ConsoleTable.Write(["Statistic", "Value"], summary);

      Console.WriteLine("Products per category:");
      IReadOnlyList<string>[] categories = products
        .GroupBy(static p => p.Category)
        .OrderBy(static g => g.Key)
        .Select(static g => (IReadOnlyList<string>)[g.Key, g.Count().ToString()]
        )
        .ToArray();

      ConsoleTable.Write(["Category", "Count"], categories);
    }

    internal static void DisplayDidUpdate(bool updated) {
      if (updated) {
        ConsoleSupport.WriteLineGreen("Product updated successfully.");
      } else {
        ConsoleSupport.WriteLineYellow("Product not found.");
      }
    }
  }
}
