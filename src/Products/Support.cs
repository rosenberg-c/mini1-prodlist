using InputError = App.src.Products.Product.InputError;

namespace App.src.Products {
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
          $"The numeric part must be between {Product.minNum} "
            + $"and {Product.maxNum}.",
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
