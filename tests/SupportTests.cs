using App.src;
using Xunit;

namespace App.tests.Tests {
  public sealed class SupportTests {
    [Theory]
    [InlineData(null)]
    [InlineData("q")]
    [InlineData(" Q ")]
    public void IsExitOrNull_RecognizesMenuQuit(string? input) {
      Assert.True(Support.IsExitOrNull(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("exit")]
    [InlineData("Mouse")]
    public void IsExitOrNull_DoesNotQuitForOtherInput(string input) {
      Assert.False(Support.IsExitOrNull(input));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(" ", false)]
    [InlineData("Food", false)]
    public void IsEmptyOrNull_ReportsRawEmptyInput(string? input, bool expected) {
      Assert.Equal(expected, Support.IsEmptyOrNull(input));
    }
  }
}
