using System.Globalization;

namespace App.src.Products {
  internal static class ProductPriceValidation {
    internal static bool TryPrice(string? text, out decimal price,
      out string error) {
      if (!decimal.TryParse(text,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.CurrentCulture, out price
          )) {
        error = "Invalid price. Please enter a numeric value.";
        return false;
      }

      if (price < 0) {
        error = "Price cannot be negative.";
        return false;
      }

      error = string.Empty;
      return true;
    }
  }
}
