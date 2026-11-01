using System.Text.Json;
using App.src.Products;
using Xunit;

namespace App.tests.Tests {
  public sealed class ProductTests {
    [Fact]
    public void Product_SerializesCategoryNamePriceAndGuid() {
      Guid id = Guid.Parse("11111111-1111-4111-8111-111111111111");
      Product product = new(id, "Electronics", "Mouse", 299.50m);

      using JsonDocument json =
        JsonDocument.Parse(JsonSerializer.Serialize(product));
      Assert.Equal(id, json.RootElement.GetProperty("Id").GetGuid());
      Assert.Equal("Electronics",
        json.RootElement.GetProperty("Category").GetString()
      );
      Assert.Equal("Mouse", json.RootElement.GetProperty("Name").GetString());
      Assert.Equal(299.50m, json.RootElement.GetProperty("Price").GetDecimal());
    }

    [Fact]
    public void WithMethods_CreateNewProductsWithoutChangingOriginalOrId() {
      Product original = new(Guid.NewGuid(), "Food", "Pizza", 120.50m);

      Product category = original.WithCategory("Office");
      Product name = original.WithName("Pen");
      Product price = original.WithPrice(0m);

      Assert.Equal("Office", category.Category);
      Assert.Equal("Pen", name.Name);
      Assert.Equal(0m, price.Price);
      Assert.Equal(original.Id, category.Id);
      Assert.Equal(original.Id, name.Id);
      Assert.Equal(original.Id, price.Id);
      Assert.Equal("Food", original.Category);
      Assert.Equal("Pizza", original.Name);
      Assert.Equal(120.50m, original.Price);
      Assert.NotSame(original, category);
      Assert.NotSame(original, name);
      Assert.NotSame(original, price);
    }
  }
}
