namespace App.src.Products {
  public sealed record Product(
    Guid Id,
    string Category,
    string Name,
    decimal Price
  ) {
    public const int SchemaVersion = 4;

    public string GetListItemString() {
      return $"{Category.PadRight(14)} | {Name.PadRight(14)} | {Price:0.##} kr";
    }
  }
}
