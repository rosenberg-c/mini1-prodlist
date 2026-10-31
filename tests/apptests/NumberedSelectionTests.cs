using App.src.ConsoleCore;
using Xunit;

namespace App.tests.Tests {
  public sealed class NumberedSelectionTests {
    [Fact]
    public void ParseIndex_SelectsFromNonProductList() {
      string[] books = ["Dune", "The Hobbit"];

      NumberedSelectionResult result = NumberedSelection.ParseIndex(books.Length, "2");

      Assert.Equal(SelectionStatus.Selected, result.Status);
      Assert.Equal("The Hobbit", books[result.Index]);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("3")]
    [InlineData("abc")]
    [InlineData("")]
    public void ParseIndex_RejectsInvalidSelections(string input) {
      NumberedSelectionResult result = NumberedSelection.ParseIndex(2, input);

      Assert.Equal(SelectionStatus.Invalid, result.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" Q ")]
    public void ParseIndex_DistinguishesCancellationFromInvalidInput(string? input) {
      NumberedSelectionResult result = NumberedSelection.ParseIndex(2, input);

      Assert.Equal(SelectionStatus.Cancelled, result.Status);
    }
  }
}
