namespace App.src.Products {
  public sealed record Product(
    Guid Id,
    string Category,
    string Name,
    decimal Price
  ) {
    public const int SchemaVersion = 4;

    public Product Copy() {
      return new Product(Id, Category, Name, Price);
    }

    public Product WithCategory(string category) {
      return this with { Category = category };
    }

    public Product WithName(string name) {
      return this with { Name = name };
    }

    public Product WithPrice(decimal price) {
      return this with { Price = price };
    }

    public string GetListItemString() {
      string c = Category.PadRight(14);
      string n = Name.PadRight(14);
      string p = $"{Price:0.##} kr".PadRight(14);

      return $"{c} | {n} | {p}";
    }
  }
}
