namespace App.src.Products {
  public sealed record Product(
    Guid Id,
    string Category,
    string Name,
    decimal Price
  ) {
    public const int SchemaVersion = 4;

    public string GetListItemString() {
      string c = Category.PadRight(14);
      string n = Name.PadRight(14);
      string p = $"{Price:0.##} kr".PadRight(14);

      return $"{c} | {n} | {p}";
    }
  }
}
