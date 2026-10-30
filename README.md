# Product Inventory System

A small console app for managing products.

## Get started

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
2. Open a terminal in this project folder and run:

   ```sh
   make run
   ```

   `make run` initializes the pinned `sharp-storage` Git submodule first. If you're on Windows or not using Make, run `git submodule update --init --recursive` from the project folder after cloning, then run `dotnet run --project App.csproj`. The submodule uses a GitHub SSH URL, so GitHub SSH access must be configured. Alternatively, clone with `git clone --recurse-submodules <repo-url>` to initialize the submodule during cloning.

3. Choose a menu option (1–6). Type `q` at the main menu to quit, or at a prompt to return to the main menu. The app also quits if input ends.

## Using the app

- **Add Product (1):** Enter a category, name, and price. You can add more products without returning to the menu.
- **Delete Product (2):** Select a product by its displayed list number.
- **Update Product (3):** Select a product by its displayed list number, then choose to change its category, name, or price.
- **View Products (4):** See the products and their total price.
- **Search Products (5):** Search category or name using a case-insensitive partial match. An empty search lists all products.
- **Statistics (6):** See the cheapest and most expensive products, average price, and count per category.

Categories and names cannot be empty (surrounding spaces are trimmed). Prices must be non-negative numbers in your system's number format. Products are displayed in ascending price order, then by category and name. Prices are shown in `kr`. The app saves products to `products.json` in the folder where you run it; run it from the same folder next time to keep your inventory.

## Existing product files

The current `products.json` schema is version 4. On startup, the app rejects files with an older schema version or invalid JSON instead of changing them. Keep a backup of any existing file before starting a new inventory; there is no migration script in this repository. To start with an empty inventory, move or delete the incompatible `products.json` from the folder where you run the app.

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
