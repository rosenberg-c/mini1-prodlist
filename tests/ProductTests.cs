using System.Globalization;
using System.Text.Json;
using App.src.Products;
using Xunit;

namespace App.tests.Tests {
  public sealed class ProductTests {
    [Fact]
    public void Product_SerializesCategoryNamePriceAndGuid() {
      Guid id = Guid.Parse("11111111-1111-4111-8111-111111111111");
      Product product = new(id, "Electronics", "Mouse", 299.50m);

      using JsonDocument json = JsonDocument.Parse(JsonSerializer.Serialize(product));
      Assert.Equal(id, json.RootElement.GetProperty("Id").GetGuid());
      Assert.Equal("Electronics", json.RootElement.GetProperty("Category").GetString());
      Assert.Equal("Mouse", json.RootElement.GetProperty("Name").GetString());
      Assert.Equal(299.50m, json.RootElement.GetProperty("Price").GetDecimal());
    }

    [Fact]
    public void GetListItemString_IncludesCategoryNameAndPrice() {
      Product product = new(Guid.NewGuid(), "Food", "Pizza", 120.50m);

      string line = product.GetListItemString();

      Assert.Contains("Food", line);
      Assert.Contains("Pizza", line);
      Assert.Contains(120.50m.ToString("0.##", CultureInfo.CurrentCulture) + " kr", line);
    }
  }
}
