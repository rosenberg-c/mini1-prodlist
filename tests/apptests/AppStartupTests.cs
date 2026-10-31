using System.Diagnostics;
using App.src;
using Xunit;

namespace App.tests.Tests {
  public sealed class AppStartupTests {
    [Theory]
    [InlineData("q\n")]
    [InlineData("")]
    public async Task App_QuitsCleanlyOnQuitOrEndOfInput(string input) {
      string directory = Path.Combine(Path.GetTempPath(),
        "app-startup-tests", Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(directory);

      try {
        ProcessStartInfo start = new("dotnet") {
          WorkingDirectory = directory,
          RedirectStandardInput = true,
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          UseShellExecute = false
        };
        start.ArgumentList.Add(typeof(Program).Assembly.Location);

        using Process process = Process.Start(start)
          ?? throw new InvalidOperationException("Could not start the app.");
        try {
          Task<string> output = process.StandardOutput.ReadToEndAsync();
          Task<string> error = process.StandardError.ReadToEndAsync();
          await process.StandardInput.WriteAsync(input);
          process.StandardInput.Close();

          using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(15));
          await process.WaitForExitAsync(timeout.Token);
          string stdout = await output;
          string stderr = await error;
          Assert.True(process.ExitCode == 0,
            $"App exited with {process.ExitCode}: {stderr}");
          Assert.Contains("Application closed.", stdout);
        }
        finally {
          if (!process.HasExited) {
            process.Kill(entireProcessTree: true);
          }
        }
      }
      finally {
        Directory.Delete(directory, recursive: true);
      }
    }
  }
}
