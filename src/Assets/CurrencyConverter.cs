namespace App.src.Assets {
  internal static class CurrencyConverter {
    internal static (decimal Amount, string Currency) Convert(
      decimal priceUsd, Office office
    ) {
      return office switch {
        Office.Sweden => (priceUsd * 10m, "SEK"),
        Office.USA => (priceUsd, "USD"),
        Office.Turkey => (priceUsd * 60m, "TRY"),
        _ => throw new ArgumentOutOfRangeException(nameof(office), office,
          "Unsupported office."
        )
      };
    }
  }
}
