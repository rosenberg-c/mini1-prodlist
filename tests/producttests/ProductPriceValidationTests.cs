using System.Globalization;
using App.src.Products;
using Xunit;

namespace App.tests.Tests {
  public sealed class ProductPriceValidationTests {
    [Fact]
    public void TryPrice_AcceptsFractionalValueInCurrentCulture() {
      string input = 1.25m.ToString(CultureInfo.CurrentCulture);

      bool result = ProductPriceValidation.TryPrice(input, out decimal price,
        out string error
      );

      Assert.True(result);
      Assert.Equal(1.25m, price);
      Assert.Equal(string.Empty, error);
    }

    [Theory]
    [InlineData(null, 0, "Invalid price. Please enter a numeric value.")]
    [InlineData("abc", 0, "Invalid price. Please enter a numeric value.")]
    [InlineData("-1", -1, "Price cannot be negative.")]
    public void TryPrice_RejectsInvalidOrNegativeValues(
      string? input, int expectedPrice, string expectedError
    ) {
      bool result = ProductPriceValidation.TryPrice(input, out decimal price,
        out string error
      );

      Assert.False(result);
      Assert.Equal(expectedPrice, price);
      Assert.Equal(expectedError, error);
    }
  }
}
