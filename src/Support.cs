using System.Diagnostics.CodeAnalysis;
using InputError = ProdList.src.Product.InputError;

namespace ProdList.src {
  public static class Support {
    public static bool IsExitOrNull([NotNullWhen(false)] string? value) {
      return value is null || string.Equals(
        value?.Trim(),
        "exit",
        StringComparison.OrdinalIgnoreCase
      );
    }

    public static string? ReadString(string prompt) {
      Console.Write(prompt);
      return Console.ReadLine()?.Trim();
    }
  }
}

namespace ProdList.src {
  public static class Ui {
    public static string MessageFrom(InputError error) {
      string detail = error switch {
        InputError.EmptyInput => "Input cannot be empty.",
        InputError.MissingDash =>
          "Product must contain a dash (-).",
        InputError.InvalidLeftSide =>
          "The left side must contain (A-Z) letters only.",
        InputError.InvalidNumber =>
          "The right side must contain numbers only.",
        InputError.NumberOutOfRange =>
          "The numeric part must be between 200 and 500.",
        _ => throw new ArgumentOutOfRangeException(nameof(error))
      };

      return $"ERROR: {detail}";
    }

    public static string MessageFrom(CreateStatus status) {
      return status switch {
        CreateStatus.Created => "Product added successfully.",
        CreateStatus.Duplicate => "WARNING: Product already exists.",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
      };
    }
  }
}
