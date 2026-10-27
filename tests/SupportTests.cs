using Xunit;
using App.src;

namespace App.tests.Tests {
  public sealed class SupportTests {
    [Theory]
    [InlineData(null)]
    [InlineData("exit")]
    [InlineData(" EXIT ")]
    [InlineData("eXiT")]
    public void IsExitOrNull_RecognizesExit(string? input) {
      Assert.True(Support.IsExitOrNull(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("AB-300")]
    [InlineData("exiting")]
    public void IsExitOrNull_DoesNotExitForOtherInput(string input) {
      Assert.False(Support.IsExitOrNull(input));
    }
  }
}
