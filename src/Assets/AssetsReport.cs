using System.Text.Json;

namespace App.src.Assets {
  internal static class AssetsReport {
    internal static async Task ExportAsync(
      IEnumerable<Asset> assets, string path, DateOnly today
    ) {
      var rows = assets.Select(asset => {
          var (priceLocal, currency) = CurrencyConverter.Convert(
            asset.PriceUsd, asset.Office
          );
          var (years, months) = asset.Age(today);
          return new {
            asset.Id,
            Type = asset.Type.ToString(),
            asset.Brand,
            asset.Model,
            asset.PurchaseDate,
            AgeYears = years,
            AgeMonths = months,
            asset.PriceUsd,
            PriceLocal = priceLocal,
            Currency = currency,
            Office = asset.Office.ToString(),
            Status = AssetsDisplay.StatusLabel(asset, today)
          };
        }
      ).ToList();

      string json = JsonSerializer.Serialize(rows, new JsonSerializerOptions(
          JsonSerializerDefaults.Web
        ) { WriteIndented = true }
      );

      string fullPath = Path.GetFullPath(path);
      string temporaryPath = Path.Combine(Path.GetDirectoryName(fullPath)!,
        $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp"
      );

      try {
        await File.WriteAllTextAsync(temporaryPath, json);
        File.Move(temporaryPath, fullPath, overwrite: true);
      }

      finally {
        if (File.Exists(temporaryPath)) {
          File.Delete(temporaryPath);
        }
      }
    }
  }
}
