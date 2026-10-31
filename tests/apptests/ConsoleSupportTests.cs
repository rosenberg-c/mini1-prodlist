using App.src.ConsoleCore;
using Xunit;

namespace App.tests.Tests {
  public sealed class ConsoleSupportTests {
    [Theory]
    [InlineData(null)]
    [InlineData("q")]
    [InlineData(" Q ")]
    public void IsExitOrNull_RecognizesMenuQuit(string? input) {
      Assert.True(ConsoleSupport.IsExitOrNull(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("exit")]
    [InlineData("Mouse")]
    public void IsExitOrNull_DoesNotQuitForOtherInput(string input) {
      Assert.False(ConsoleSupport.IsExitOrNull(input));
    }
  }
}
