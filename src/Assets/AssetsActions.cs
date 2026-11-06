using System.Globalization;
using System.Text.Json;
using App.src.ConsoleCore;
using App.src.Validation;

namespace App.src.Assets {
  internal sealed class AssetsActions {
    private readonly AssetsManager _m;

    internal AssetsActions(
      AssetsManager manager
    ) {
      _m = manager;
    }

    private async Task<List<Asset>> GetAllAsync() {
      return await _m.GetAllAsync();
    }

    internal async Task<List<Asset>> GetAssetsAsync() {
      return await GetAllAsync();
    }

    internal async Task ShowAsync() {
      List<Asset> assets = await GetAssetsAsync();
      if (assets.Count == 0) {
        Console.WriteLine("No assets to show.");
        ConsoleSupport.Pause();
        return;
      }

      while (true) {
        Console.WriteLine("1. By office");
        Console.WriteLine("2. By purchase date");
        Console.WriteLine("3. By asset type, then purchase date");
        Console.WriteLine("q to return");
        if (!ConsoleSupport.TryReadMenuChoice(out string? choice)) {
          return;
        }

        switch (choice) {
          case "1":
            AssetsDisplay.DisplayByOffice(assets);
            break;
          case "2":
            AssetsDisplay.DisplayByDate(assets);
            break;
          case "3":
            AssetsDisplay.DisplayByTypeAndDate(assets);
            break;
          default:
            ConsoleSupport.WriteLineYellow(
              "Invalid option. Please try again."
            );
            break;
        }
      }
    }

    internal async Task AddAsync() {
      Console.WriteLine("1. Computer");
      Console.WriteLine("2. Mobile phone");
      if (!TryReadAssetType(out AssetType type)
          || !TryReadText("Brand: ", out string brand)
          || !TryReadText("Model: ", out string model)
          || !TryReadDate(out DateOnly purchaseDate)
          || !TryReadPrice("Price (USD): ", out decimal priceUsd)
          || !TryReadPrice("Price (local): ", out decimal priceLocal)
          || !TryReadOffice(out Office office)) {
        return;
      }

      try {
        await _m.CreateAsync(id => type switch {
            AssetType.Computer => new Computer(
              id, brand, model, purchaseDate, priceUsd, priceLocal, office
            ),
            AssetType.MobilePhone => new MobilePhone(
              id, brand, model, purchaseDate, priceUsd, priceLocal, office
            ),
            _ => throw new InvalidOperationException("Unsupported asset type.")
          }
        );
        Console.WriteLine("Asset added.");
      }
      catch (Exception e) when (
        e is IOException or UnauthorizedAccessException
          or JsonException or InvalidDataException
      ) {
        ConsoleSupport.WriteLineRed($"Could not save asset: {e.Message}");
      }
    }

    private static bool TryReadAssetType(out AssetType type) {
      while (true) {
        if (!ConsoleSupport.TryReadMenuChoice(
              out string? input, "Select type (1 or 2): "
            )) {
          type = default;
          return false;
        }

        if (input is "1" or "2") {
          type = input == "1" ? AssetType.Computer : AssetType.MobilePhone;
          return true;
        }

        ConsoleSupport.WriteLineYellow("Choose 1 or 2.");
      }
    }

    private static bool TryReadOffice(out Office office) {
      Office[] offices = Enum.GetValues<Office>();
      Console.WriteLine($"Select office (1-{offices.Length}): ");

      for (int index = 0; index < offices.Length; index++) {
        Console.WriteLine($"  {index + 1}. {offices[index]}");
      }


      while (true) {
        if (!ConsoleSupport.TryReadMenuChoice(
              out string? input, ": "
            )) {
          office = default;
          return false;
        }

        if (int.TryParse(input, out int selection)
            && selection >= 1 && selection <= offices.Length) {
          office = offices[selection - 1];
          return true;
        }

        ConsoleSupport.WriteLineYellow($"Choose 1-{offices.Length}.");
      }
    }

    private static bool TryReadText(string prompt, out string value) {
      while (true) {
        if (!ConsoleSupport.TryReadInput(prompt, out string? input)) {
          value = string.Empty;
          return false;
        }

        if (TextFieldValidation.TryField(input, out value)) {
          return true;
        }

        ConsoleSupport.WriteLineYellow("This field cannot be empty.");
      }
    }

    private static bool TryReadDate(out DateOnly date) {
      while (true) {
        if (!ConsoleSupport.TryReadInput(
              "Purchase date (yyyy-MM-dd): ", out string? input
            )) {
          date = default;
          return false;
        }

        if (DateOnly.TryParseExact(
              input, "yyyy-MM-dd", CultureInfo.InvariantCulture,
              DateTimeStyles.None, out date
            )) {
          return true;
        }

        ConsoleSupport.WriteLineYellow(
          "Enter a valid date in yyyy-MM-dd format."
        );
      }
    }

    private static bool TryReadPrice(string prompt, out decimal price) {
      while (true) {
        if (!ConsoleSupport.TryReadInput(prompt, out string? input)) {
          price = default;
          return false;
        }

        if (decimal.TryParse(
              input, NumberStyles.Number, CultureInfo.CurrentCulture,
              out price
            ) && price >= 0) {
          return true;
        }

        ConsoleSupport.WriteLineYellow("Enter a non-negative price.");
      }
    }

    internal void Edit() {
      Console.WriteLine("Not implemented.");
    }
  }
}
