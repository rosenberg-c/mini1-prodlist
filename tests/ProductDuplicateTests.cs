using ProdList.src;
using Xunit;
using Entry = ProdList.src.Product.Entry;

namespace ProdList.tests.Tests {
  public sealed class ProductDuplicateTests {
    [Theory]
    [InlineData("AB-300", true)]
    [InlineData("ab-300", true)]
    [InlineData("AB-301", false)]
    [InlineData("XAB-300", false)]
    public void IsDuplicate_ComparesWholeNameIgnoringCase(
      string name, bool expected
    ) {
      Assert.Equal(expected, Product.IsDuplicate(new Entry(1, "AB-300"), name));
    }
  }
}
