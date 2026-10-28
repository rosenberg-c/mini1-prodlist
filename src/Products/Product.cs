namespace App.src.Products {
  public sealed record Product(
    Guid Id,
    string Category,
    string Name,
    decimal Price
  ) {
    public const int SchemaVersion = 4;
  }
}
