# Todo App

A C# console app for managing projects and their tasks. Create projects, add tasks with optional descriptions and due dates, update their status, and move tasks between projects. Changes are saved as JSON so they are available the next time you run the app.

## Get started

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
2. From the repository root, run:

   ```sh
   make run
   ```

   `make run` initializes the pinned [sharp-storage](https://github.com/rosenberg-c/sharp-storage) Git submodule and saves Todo data in `todo.json` in the repository root. The submodule uses a GitHub SSH URL, so you need GitHub SSH access. Without Make, run `git submodule update --init --recursive` and then `dotnet run --project App.csproj` from the repository root. A direct `dotnet run` uses `data.json` by default unless you set `APP_DATA_PATH` to another file.

3. Choose a numbered menu option. Enter `q` to quit or go back at a menu.

To start with example projects and tasks, run `make reset` before `make run`. **This overwrites your current `todo.json`.** The example data is in [`todo.example.json`](todo.example.json).

## What you can do

- View tasks grouped by project or date, or show a compact date-ordered list.
- Add, edit, delete, and move tasks between projects. Tasks have a title, an optional description and due date, and a status (Pending, In Progress, or Completed).
- Create, rename, and delete projects that hold tasks. Create a project before adding a task if your data file is empty.

## How it works

The app separates console menus and input (`src/Todo/TodoMenu.cs` and `src/Todo/TodoActions.cs`) from task operations (`src/Todo/TodoManager.cs`) and JSON persistence (`src/Todo/TodoStore.cs`). `TodoStore` uses the sharp-storage submodule to read and write projects. `src/Todo/TodoValidator.cs` checks loaded project and task data before the menu starts.

The Todo data format uses schema version 1. If the file has invalid JSON, an unsupported schema version, or invalid Todo data, the app reports the problem rather than replacing the file. Back up the file before deleting or resetting it.

## Development

```sh
make build   # Build the app and initialize its dependency
make test    # Run the test suites
make lint    # Check .NET formatting without changing files
make format  # Format C# code with JetBrains ReSharper CLI
```

`make format` requires the [JetBrains ReSharper command-line tools](https://www.jetbrains.com/help/resharper/ReSharper_Command_Line_Tools.html) (`jb`). Todo tests live in `tests/todotests`; app integration tests live in `tests/apptests`.
