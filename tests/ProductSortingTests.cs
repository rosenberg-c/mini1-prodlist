using ProdList.src.Products;
using Xunit;
using Entry = ProdList.src.Products.Product.Entry;

namespace ProdList.tests.Tests {
  public sealed class ProductSortingTests {
    [Fact]
    public void ForDisplay_Sorts_Alphabetically() {
      List<Entry> entries = [
        Item(1, "PC", 300),
        Item(2, "MAC", 500),
        Item(3, "LABAN", 200)
      ];

      Assert.Equal([
          Item(3, "LABAN", 200),
          Item(2, "MAC", 500),
          Item(1, "PC", 300)
        ],
        Product.Sort(entries)
      );
    }

    [Fact]
    public void ForDisplay_SortsIgnoringCase_ThenBreaksTiesOrdinally() {
      List<Entry> entries = [
        Item(1, "b", 300),
        Item(2, "a", 200),
        Item(3, "B", 300),
        Item(4, "A", 200)
      ];

      Assert.Equal([
          Item(4, "A", 200),
          Item(2, "a", 200),
          Item(3, "B", 300),
          Item(1, "b", 300)
        ],
        Product.Sort(entries)
      );
    }

    private static Entry Item(int id, string left, int right) {
      return new Entry(new Guid(id, 0, 0, new byte[8]), left, right);
    }
  }
}
