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
  }
}
