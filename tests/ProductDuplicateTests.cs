using System.Text.Json;
using ProdList.src;
using Xunit;
using Entry = ProdList.src.Product.Entry;

namespace ProdList.tests.Tests {
  public sealed class ProductDuplicateTests {
    [Fact]
    public void Entry_ComputesNameWithoutSerializingIt() {
      Entry entry = new(1, "AB", 300);

      Assert.Equal("AB-300", entry.Name);
      Assert.Equal("{\"Id\":1,\"Left\":\"AB\",\"Right\":300}",
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
        Product.IsDuplicate(new Entry(1, "AB", 300), left, right));
    }
  }
}
