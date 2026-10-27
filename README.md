# Product Inventory System

A small console app for managing products.

## Get started

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
2. Open a terminal in this project folder and run:

   ```sh
   make run
   ```

   `make run` initializes the pinned `sharp-storage` Git submodule first. For direct `dotnet run --project App.csproj`, run `make deps` once after cloning.

3. Choose a menu option to add, view, search, or delete products, or see statistics. Enter `6` or type `exit` to quit.

Product names use uppercase letters followed by a dash and a number from 200 to 500 (for example, `AB-300`). Search by name or number. Products are saved to `products.json` in the folder where you run the app, so run it from the same folder next time to keep your list. Each product now has a GUID ID. `m1pl` owns the rule that names matching without case are duplicates; `sharp-storage` only manages IDs and persistence.

## Existing product files

The current schema is version 3. If startup reports a legacy version-1 or version-2 `products.json`, run `python3 scripts/migrate-products.py products.json` in the directory containing that file. The script creates a `.v1.bak` or `.v2.bak` copy before replacing it, assigns a GUID to each product, and refuses to overwrite an existing backup. Keep the backup until you have checked your migrated data. The old `sharp-storage` tag `v0.0.1` is incompatible with this app; the submodule pins a newer commit.

## VS Code

For vscode users the extension "C# Dev Kit" from Microsoft may need to be installed


## Linting

```sh
Install ReSharper tools
dotnet tool install -g JetBrains.ReSharper.GlobalTools

Add tools to PATH (if needed)
export PATH="$PATH:$HOME/.dotnet/tools"
```


## Tests and code style

From the project folder, run:

```sh
make build   # Build app and storage dependency
make test    # Run tests
make lint    # Check formatting without changing files
make format  # Format app and test code
```

These commands use the .NET SDK. `make` is also required; if you don't have it, run `dotnet test tests/App.Tests.csproj` for tests or see the equivalent `dotnet format` commands in the `Makefile`.

## Make

Easy make commands

```sh
make deps
make run
make test
make lint
make format
```

## Running

[<img src="assets/run.png" width="250"/>](assets/run.png)
