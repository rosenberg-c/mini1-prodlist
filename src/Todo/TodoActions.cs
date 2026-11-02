namespace App.src.Todo {
  internal sealed class TodoActions {
    private readonly TodoManager _m;

    internal TodoActions(
      TodoManager manager
    ) {
      _m = manager;
    }

    internal async Task<List<Project>> GetProjectsAsync() {
      return await _m.GetAllAsync();
    }

    internal async Task<(List<Project> Projects, List<ATask> Tasks)>
      GetProjectsAndTasksAsync() {
      List<Project> projects = await GetProjectsAsync();
      List<ATask> tasks = [];

      for (int i = 0; i < projects.Count; i++) {
        for (int j = 0; j < projects[i].Tasks.Count; j++) {
          tasks.Add(projects[i].Tasks[j]);
        }
      }

      return (projects, tasks);
    }

    internal void Show() {
      Console.WriteLine("Not implemented.");
    }

    internal void Add() {
      Console.WriteLine("Not implemented.");
    }

    internal void Edit() {
      Console.WriteLine("Not implemented.");
    }
  }
}
