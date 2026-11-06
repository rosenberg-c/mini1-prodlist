using System.Globalization;
using App.src.ConsoleCore;

namespace App.src.Assets {
  internal sealed class AssetsDisplay {
    private enum EndOfLifeStatus {
      None,
      Yellow,
      Red
    }

    private static readonly IReadOnlyList<string> headers = [
      "Type", "Brand", "Model", "Purchase Date", "Price (USD)",
      "Price (Local)", "Office", "Status"
    ];

    private static EndOfLifeStatus Status(Asset asset, DateOnly today) {
      DateOnly endOfLife = asset.PurchaseDate.AddYears(3);

      if (today >= endOfLife.AddMonths(-3)) {
        return EndOfLifeStatus.Red;
      }

      return today >= endOfLife.AddMonths(-6)
        ? EndOfLifeStatus.Yellow
        : EndOfLifeStatus.None;
    }

    private static IReadOnlyList<string> ToRow(
      Asset asset, EndOfLifeStatus status
    ) {
      var (amount, currency) = CurrencyConverter.Convert(
        asset.PriceUsd, asset.Office
      );
      return [
        asset.Type.ToString(),
        asset.Brand,
        asset.Model,
        asset.PurchaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        asset.PriceUsd.ToString("F2", CultureInfo.CurrentCulture),
        $"{amount.ToString("F2", CultureInfo.CurrentCulture)} {currency}",
        asset.Office.ToString(),
        status switch {
          EndOfLifeStatus.Red => "RED",
          EndOfLifeStatus.Yellow => "YELLOW",
          _ => ""
        }
      ];
    }

    private static void Display(IEnumerable<Asset> assets) {
      DateOnly today = DateOnly.FromDateTime(DateTime.Today);
      var entries = assets.Select(asset => (
          Asset: asset, Status: Status(asset, today)
        )
      ).ToArray();

      ConsoleTable.Write(
        headers,
        entries.Select(entry => ToRow(entry.Asset, entry.Status)),
        numberRows: false,
        rowColor: index => entries[index].Status switch {
          EndOfLifeStatus.Red => ConsoleColor.Red,
          EndOfLifeStatus.Yellow => ConsoleColor.Yellow,
          _ => null
        }
      );
    }

    internal static void DisplayByTypeAndDate(List<Asset> assets) {
      Display(
        assets
          .OrderBy(static asset => asset.Type)
          .ThenBy(static asset => asset.PurchaseDate)
      );
    }

    internal static void DisplayByOffice(List<Asset> tasks) {
      Display(tasks.OrderBy(static task => task.Office.ToString()));
    }

    internal static void DisplayByDate(List<Asset> tasks) {
      Display(tasks.OrderBy(static task => task.PurchaseDate));
    }
  }
}
