using System.Diagnostics;
using System.Text.Json;
using App.src.Todo;
using Xunit;

namespace App.tests.Tests {
  public sealed class TodoDeleteTests {
    [Fact]
    public async Task DeleteTask_RemovesOnlySelectedTask() {
      string path = TempPath();
      try {
        TodoTask keep = TaskItem("Keep");
        TodoTask remove = TaskItem("Remove");
        TodoTask otherProjectTask = TaskItem("Other project task");
        TodoProject project = new(Guid.NewGuid(), "First", [keep, remove]);
        TodoProject other = new(Guid.NewGuid(), "Second", [otherProjectTask]);
        await SeedAsync(path, project, other);

        string output = await RunAppAsync(path, "5\n2\n\nq\n");

        Assert.Contains("Success.", output);
        List<TodoProject> saved = await new TodoStore(path).GetProjectsAsync();
        Assert.Equal(2, saved.Count);
        Assert.Equal(project.Id, saved[0].Id);
        Assert.Equal(keep.Id, Assert.Single(saved[0].Tasks).Id);
        Assert.Equal(other.Id, saved[1].Id);
        Assert.Equal(otherProjectTask.Id, Assert.Single(saved[1].Tasks).Id);
      }
      finally {
        File.Delete(path);
      }
    }

    [Fact]
    public async Task DeleteTask_CancelLeavesFileUnchanged() {
      string path = TempPath();
      try {
        await SeedAsync(path, new TodoProject(Guid.NewGuid(), "Project",
            [TaskItem("Keep")]
          )
        );
        string before = await File.ReadAllTextAsync(path);

        string output = await RunAppAsync(path, "5\nq\n\nq\n");

        Assert.DoesNotContain("Success.", output);
        Assert.Equal(before, await File.ReadAllTextAsync(path));
      }
      finally {
        File.Delete(path);
      }
    }

    [Fact]
    public async Task DeleteTask_MissingId_ThrowsAndLeavesFileUnchanged() {
      string path = TempPath();
      try {
        TodoTask kept = TaskItem("Keep");
        TodoProject project = new(Guid.NewGuid(), "Project", [kept]);
        await SeedAsync(path, project);
        string before = await File.ReadAllTextAsync(path);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
          new TodoManager(new TodoStore(path)).DeleteTaskAsync(project.Id,
            Guid.NewGuid()
          )
        );

        Assert.Equal(before, await File.ReadAllTextAsync(path));
        TodoProject saved = Assert.Single(
          await new TodoStore(path).GetProjectsAsync()
        );
        Assert.Equal(kept, Assert.Single(saved.Tasks));
      }
      finally {
        File.Delete(path);
      }
    }

    [Fact]
    public async Task
      DeleteProject_WithTasksAndConfirmation_RemovesOnlySelectedProject() {
      string path = TempPath();
      try {
        TodoProject removed = new(Guid.NewGuid(), "Remove", [TaskItem("Task")]);
        TodoTask keptTask = TaskItem("Keep task");
        TodoProject kept = new(Guid.NewGuid(), "Keep", [keptTask]);
        await SeedAsync(path, removed, kept);

        string output = await RunAppAsync(path, "9\n1\ny\n\nq\n");

        Assert.Contains("Success.", output);
        List<TodoProject> saved = await new TodoStore(path).GetProjectsAsync();
        TodoProject remaining = Assert.Single(saved);
        Assert.Equal(kept.Id, remaining.Id);
        Assert.Equal(keptTask.Id, Assert.Single(remaining.Tasks).Id);
      }
      finally {
        File.Delete(path);
      }
    }

    [Fact]
    public async Task
      DeleteProject_WithTasksAndDeclinedConfirmation_LeavesFileUnchanged() {
      string path = TempPath();
      try {
        await SeedAsync(path,
          new TodoProject(Guid.NewGuid(), "Keep", [TaskItem("Keep task")])
        );
        string before = await File.ReadAllTextAsync(path);

        string output = await RunAppAsync(path, "9\n1\nn\n\nq\n");

        Assert.Contains("Removal cancelled.", output);
        Assert.Equal(before, await File.ReadAllTextAsync(path));
      }
      finally {
        File.Delete(path);
      }
    }

    [Fact]
    public async Task DeleteProject_EmptyProject_DoesNotRequireConfirmation() {
      string path = TempPath();
      try {
        await SeedAsync(path, new TodoProject(Guid.NewGuid(), "Empty", []));

        string output = await RunAppAsync(path, "9\n1\n\nq\n");

        Assert.Contains("Success.", output);
        Assert.DoesNotContain("really remove?", output);
        Assert.Empty(await new TodoStore(path).GetProjectsAsync());
      }
      finally {
        File.Delete(path);
      }
    }

    [Fact]
    public async Task DeleteProject_MissingId_DoesNotChangeStoredProjects() {
      string path = TempPath();
      try {
        TodoProject kept = new(Guid.NewGuid(), "Keep", []);
        await SeedAsync(path, kept);
        string before = await File.ReadAllTextAsync(path);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
          new TodoStore(path).DeleteProjectAsync(Guid.NewGuid())
        );

        Assert.Equal(before, await File.ReadAllTextAsync(path));
      }
      finally {
        File.Delete(path);
      }
    }

    private static TodoTask TaskItem(string title) =>
      new(Guid.NewGuid(), TodoStatus.Pending, title, null, null);

    private static string TempPath() => Path.Combine(Path.GetTempPath(),
      $"todo-delete-{Guid.NewGuid():N}.json"
    );

    private static Task SeedAsync(string path, params TodoProject[] projects) =>
      File.WriteAllTextAsync(path, JsonSerializer.Serialize(new {
            schemaVersion = TodoStore.SchemaVersion,
            data = projects
          }
        )
      );

    private static async Task<string> RunAppAsync(string path, string input) {
      ProcessStartInfo start = new("dotnet") {
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
      };
      start.ArgumentList.Add(typeof(TodoHost.Program).Assembly.Location);
      start.Environment["APP_DATA_PATH"] = path;

      using Process process = Process.Start(start)
                              ?? throw new InvalidOperationException(
                                "Could not start the app."
                              );
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
          $"App exited with {process.ExitCode}: {stderr}"
        );
        return stdout;
      }
      finally {
        if (!process.HasExited) {
          process.Kill(entireProcessTree: true);
        }
      }
    }
  }
}
