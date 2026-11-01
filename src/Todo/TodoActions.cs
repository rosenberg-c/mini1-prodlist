namespace App.src.Todo {
  internal sealed class TodoActions {
    private readonly TodoManager _m;

    internal TodoActions(
      TodoManager manager
    ) {
      _m = manager;
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
