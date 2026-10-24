using Xunit;
using Entry = ProdList.src.Product.Entry;
using ProdList.src;

namespace ProdList.tests.Tests {
  public sealed class ProductSortingTests {
    [Fact]
    public void ForDisplay_Sorts_Alphabetically() {
      List<Entry> entries = [
        new Entry(1, "PC", 300),
        new Entry(2, "MAC", 500),
        new Entry(3, "LABAN", 200)
      ];

      Assert.Equal([
          new Entry(3, "LABAN", 200),
          new Entry(2, "MAC", 500),
          new Entry(1, "PC", 300)
        ],
        Product.Sort(entries)
      );
    }

    [Fact]
    public void ForDisplay_SortsIgnoringCase_ThenBreaksTiesOrdinally() {
      List<Entry> entries = [
        new Entry(1, "b", 300),
        new Entry(2, "a", 200),
        new Entry(3, "B", 300),
        new Entry(4, "A", 200)
      ];

      Assert.Equal([
          new Entry(4, "A", 200),
          new Entry(2, "a", 200),
          new Entry(3, "B", 300),
          new Entry(1, "b", 300)
        ],
        Product.Sort(entries)
      );
    }
  }
}
