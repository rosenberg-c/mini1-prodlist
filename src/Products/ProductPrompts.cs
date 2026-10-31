using App.src.ConsoleCore;
using App.src.Validation;

namespace App.src.Products {
  internal sealed class ProductPrompts {
    internal static bool TryReadName(out string name) {
      name = string.Empty;

      while (true) {
        string? text = ConsoleSupport.ReadString("Enter Name: ");
        if (ConsoleSupport.IsExitOrNull(text)) {
          return false;
        }

        if (TextFieldValidation.TryField(text, out name)) {
          return true;
        }

        ConsoleSupport.WriteLineRed("ERROR: Name cannot be empty.");
      }
    }

    internal static bool TryReadCategory(out string category) {
      category = string.Empty;

      while (true) {
        string? text = ConsoleSupport.ReadString("Enter Category: ");
        if (ConsoleSupport.IsExitOrNull(text)) {
          return false;
        }

        if (TextFieldValidation.TryField(text, out category)) {
          return true;
        }

        ConsoleSupport.WriteLineRed("ERROR: Category cannot be empty.");
      }
    }

    internal static bool TryReadPrice(out decimal price) {
      price = 0m;

      while (true) {
        string? text = ConsoleSupport.ReadString("Enter Price: ");
        if (ConsoleSupport.IsExitOrNull(text)) {
          return false;
        }

        if (ProductPriceValidation.TryPrice(text, out price, out string e)) {
          return true;
        }

        ConsoleSupport.WriteLineRed($"ERROR: {e}");
      }
    }
  }
}
