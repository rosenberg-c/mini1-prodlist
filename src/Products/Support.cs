namespace App.src.Products {
  public static class Ui {
    public static string MessageFrom(CreateStatus status) {
      return status switch {
        CreateStatus.Created => "Product added successfully.",
        CreateStatus.Duplicate => "WARNING: Product already exists.",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
      };
    }
  }
}
