using Xunit;
using ProdList.src;
using InputError = ProdList.src.Product.InputError;

namespace ProdList.tests.Tests {
  public sealed class UiTests {
    [Theory]
    [InlineData(InputError.EmptyInput,
      "ERROR: Input cannot be empty.")]
    [InlineData(InputError.MissingDash,
      "ERROR: Product must contain a dash (-).")]
    [InlineData(InputError.InvalidLeftSide,
      "ERROR: The left side must contain (A-Z) letters only.")]
    [InlineData(InputError.InvalidNumber,
      "ERROR: The right side must contain numbers only.")]
    [InlineData(InputError.NumberOutOfRange,
      "ERROR: The numeric part must be between 200 and 500.")]
    public void ValidationMessage_KeepsPrefixAndMessage_Literal(
      InputError code,
      string expected
    ) {
      Assert.Equal(expected, Ui.MessageFrom(code));
    }

    [Theory]
    [InlineData(CreateStatus.Created, "Product added successfully.")]
    [InlineData(CreateStatus.Duplicate,
      "WARNING: Product already exists.")]
    public void CreateMessage_UsesStatus(CreateStatus status, string expected) {
      Assert.Equal(expected, Ui.MessageFrom(status));
    }
  }
}
