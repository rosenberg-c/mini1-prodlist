using System.Text.Json;
using ProdList.src.Products;
using Xunit;
using Entry = ProdList.src.Products.Product.Entry;

namespace ProdList.tests.Tests {
  public sealed class ProductDuplicateTests {
    [Fact]
    public void Entry_ComputesNameWithoutSerializingIt() {
      Guid id = Guid.Parse("11111111-1111-4111-8111-111111111111");
      Entry entry = new(id, "AB", 300);

      Assert.Equal("AB-300", entry.Name);
      Assert.Equal("{\"Id\":\"11111111-1111-4111-8111-111111111111\",\"Left\":\"AB\",\"Right\":300}",
        JsonSerializer.Serialize(entry));
    }

    [Theory]
    [InlineData("AB", 300, true)]
    [InlineData("ab", 300, true)]
    [InlineData("AB", 301, false)]
    [InlineData("XAB", 300, false)]
    public void IsDuplicate_ComparesPartsIgnoringLeftCase(
      string left, int right, bool expected
    ) {
      Assert.Equal(expected,
        Product.IsDuplicate(new Entry(Guid.Empty, "AB", 300), left, right));
    }
  }
}
