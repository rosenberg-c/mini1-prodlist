using Xunit;
using ProdList.src;
using InputError = ProdList.src.Product.InputError;

namespace ProdList.tests.Tests {
  public sealed class ProductValidatorTests {
    [Theory]
    [InlineData("AB-300", "AB", "300")]
    [InlineData("-300", "", "300")]
    [InlineData("AB-", "AB", "")]
    [InlineData("AB-C-400", "AB", "C-400")]
    public void TrySplit_SplitsOnFirstSeparator(
      string name,
      string expectedLeft,
      string expectedRight
    ) {
      bool result = Product.TrySplit(name, out string left, out string right);

      Assert.True(result);
      Assert.Equal(expectedLeft, left);
      Assert.Equal(expectedRight, right);
    }

    [Fact]
    public void TrySplit_ReturnsFalseWithoutSeparator() {
      bool result = Product.TrySplit("AB300", out string left, out string right);

      Assert.False(result);
      Assert.Empty(left);
      Assert.Empty(right);
    }

    [Theory]
    [InlineData("CE-400")]
    [InlineData("XX-480")]
    [InlineData("LABAN-231")]
    [InlineData("A-200")]
    [InlineData("SSS-200")]
    [InlineData("Z-500")]
    public void Validate_Accepts_ValidNames(string name) {
      Assert.Null(Product.Validate(name));
    }

    [Fact]
    public void Validate_ReturnsParsedParts() {
      InputError? error = Product.Validate(
        " AB-300 ", out string left, out int right
      );

      Assert.Null(error);
      Assert.Equal("AB", left);
      Assert.Equal(300, right);
    }

    [Fact]
    public void Validate_ClearsParts_WhenInvalid() {
      InputError? error = Product.Validate(
        "invalid", out string left, out int right
      );

      Assert.Equal(InputError.MissingDash, error);
      Assert.Empty(left);
      Assert.Equal(0, right);
    }

    [Theory]
    [InlineData("", InputError.EmptyInput)]
    [InlineData("CE400", InputError.MissingDash)]
    [InlineData("-300", InputError.InvalidLeftSide)]
    [InlineData("XX3-480", InputError.InvalidLeftSide)]
    [InlineData("q-222", InputError.InvalidLeftSide)]
    [InlineData("AbC-300", InputError.InvalidLeftSide)]
    [InlineData("AB-300B", InputError.InvalidNumber)]
    [InlineData("XY-199", InputError.NumberOutOfRange)]
    [InlineData("XY-501", InputError.NumberOutOfRange)]
    [InlineData("XY-700", InputError.NumberOutOfRange)]
    public void Validate_Rejects_WithExpectedCode(
      string name,
      InputError expected
    ) {
      Assert.Equal(expected, Product.Validate(name));
    }

    [Theory]
    [InlineData("AB--50", InputError.InvalidNumber)]
    [InlineData("AB-C-400", InputError.InvalidNumber)]
    [InlineData("AB-99999999999", InputError.NumberOutOfRange)]
    public void Validate_HandlesEdgeSemantics(
      string name,
      InputError expected
    ) {
      Assert.Equal(expected, Product.Validate(name));
    }

    [Fact]
    public void Validate_Rejects_LowercaseLeft_BeforeRange() {
      Assert.Equal(InputError.InvalidLeftSide,
        Product.Validate("sss-123"));
    }

    [Fact]
    public void Validate_AcceptsUppercase_ThenAppliesRange() {
      Assert.Null(Product.Validate("SSS-200"));
      Assert.Equal(InputError.NumberOutOfRange,
        Product.Validate("SSS-123"));
    }
  }
}
