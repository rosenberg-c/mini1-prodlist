using System.Globalization;
using App.src.ConsoleCore;

namespace App.src.Assets {
  internal sealed class AssetsDisplay {
    private static readonly IReadOnlyList<string> headers = [
      "Type", "Brand", "Model", "Purchase Date", "Price (USD)",
      "Price (Local)", "Office"
    ];

    private static IReadOnlyList<string> ToRow(Asset asset) {
      return [
        asset.Type.ToString(),
        asset.Brand,
        asset.Model,
        asset.PurchaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        asset.PriceUsd.ToString(CultureInfo.CurrentCulture),
        asset.PriceLocal.ToString(CultureInfo.CurrentCulture),
        asset.Office
      ];
    }

    internal static void DisplayByOffice(List<Asset> tasks) {
      ConsoleTable.Write(
        headers,
        tasks
          .OrderBy(static task => task.Office)
          .Select(ToRow),
        numberRows: false
      );
    }

    internal static void DisplayByDate(List<Asset> tasks) {
      ConsoleTable.Write(
        headers,
        tasks
          .OrderBy(static task => task.PurchaseDate)
          .Select(ToRow),
        numberRows: false
      );
    }
  }
}
