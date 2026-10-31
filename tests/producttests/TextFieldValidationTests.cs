using App.src.Validation;
using Xunit;

namespace App.tests.Tests {
  public sealed class TextFieldValidationTests {
    [Theory]
    [InlineData(null, "", false)]
    [InlineData("   ", "", false)]
    [InlineData("  Novel  ", "Novel", true)]
    public void TryField_TrimsAndRejectsBlankText(
      string? input, string expected, bool accepted
    ) {
      bool result = TextFieldValidation.TryField(input, out string value);

      Assert.Equal(accepted, result);
      Assert.Equal(expected, value);
    }
  }
}
