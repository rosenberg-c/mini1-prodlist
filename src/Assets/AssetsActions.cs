using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using App.src.ConsoleCore;
using App.src.Validation;

namespace App.src.Assets {
  internal sealed class AssetsActions {
    private static string waitChar = "> ";
    private readonly AssetsManager _m;
    private readonly string _reportPath;

    internal AssetsActions(
      AssetsManager manager, string reportPath
    ) {
      _m = manager;
      _reportPath = reportPath;
    }

    private async Task<List<Asset>> GetAllAsync() {
      return await _m.GetAllAsync();
    }

    internal async Task<List<Asset>> GetAssetsAsync() {
      return await GetAllAsync();
    }

    internal async Task ExportAsync() {
      try {
        await AssetsReport.ExportAsync(await GetAllAsync(), _reportPath,
          DateOnly.FromDateTime(DateTime.Today)
        );
        Console.WriteLine($"JSON report exported to {_reportPath}.");
      }
      catch (Exception e) when (
        e is IOException or UnauthorizedAccessException
          or JsonException or InvalidDataException
      ) {
        ConsoleSupport.WriteLineRed($"Could not export assets: {e.Message}");
      }
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
        Console.WriteLine();
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

    internal async Task SearchAsync() {
      if (!ConsoleSupport.TryReadInput("Search brand or model (q to cancel): ",
            out string? query
          )) {
        return;
      }

      if (query.Length == 0) {
        ConsoleSupport.WriteLineYellow("Enter a brand or model to search.");
        return;
      }

      try {
        List<Asset> matches = (await GetAllAsync())
          .Where(asset => asset.Brand.Contains(query,
                            StringComparison.OrdinalIgnoreCase
                          ) ||
                          asset.Model.Contains(query,
                            StringComparison.OrdinalIgnoreCase
                          )
          )
          .ToList();

        if (matches.Count == 0) {
          Console.WriteLine("No matching assets found.");
          return;
        }

        AssetsDisplay.DisplayByOffice(matches);
      }

      catch (Exception e) when (
        e is IOException or UnauthorizedAccessException
          or JsonException or InvalidDataException
      ) {
        ConsoleSupport.WriteLineRed($"Could not search assets: {e.Message}");
      }
    }

    internal async Task AddAsync() {
      if (!TryReadAssetType(out AssetType type)
          || !TryReadText("Brand", out string brand)
          || !TryReadText("Model", out string model)
          || !TryReadDate(out DateOnly purchaseDate)
          || !TryReadPrice("Price (USD)", out decimal priceUsd)
          || !TryReadOffice(out Office office)) {
        return;
      }

      try {
        await _m.CreateAsync(id => CreateAsset(
            id, type, brand, model, purchaseDate, priceUsd, office
          )
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

    internal async Task EditAsync() {
      try {
        List<Asset> assets = await GetAllAsync();
        if (assets.Count == 0) {
          Console.WriteLine("No assets to edit.");
          return;
        }

        ShowSelectionTable(assets);
        NumberedSelectionResult choice;
        do {
          choice = NumberedSelection.ReadIndex(assets.Count,
            "Select asset to edit (q to cancel): "
          );
          if (choice.Status == SelectionStatus.Invalid) {
            ConsoleSupport.WriteLineYellow($"Choose 1-{assets.Count}.");
          }
        } while (choice.Status == SelectionStatus.Invalid);

        if (choice.Status == SelectionStatus.Cancelled) {
          return;
        }

        Asset selected = assets[choice.Index];
        Console.WriteLine();
        Console.WriteLine("Press Enter to keep the previous value.");
        Console.WriteLine();

        if (!TryReadAssetType(out AssetType type, selected.Type)
            || !TryReadText("Brand", out string brand, selected.Brand)
            || !TryReadText("Model", out string model, selected.Model)
            || !TryReadDate(out DateOnly purchaseDate, selected.PurchaseDate)
            || !TryReadPrice("Price (USD)", out decimal priceUsd,
              selected.PriceUsd
            )
            || !TryReadOffice(out Office office, selected.Office)) {
          return;
        }

        Console.WriteLine(await _m.UpdateAsync(selected.Id, existing =>
            CreateAsset(
              existing.Id, type, brand, model, purchaseDate, priceUsd, office
            )
          )
            ? "Asset updated."
            : "Asset no longer exists."
        );
      }

      catch (Exception e) when (
        e is IOException or UnauthorizedAccessException
          or JsonException or InvalidDataException
      ) {
        ConsoleSupport.WriteLineRed($"Could not edit asset: {e.Message}");
      }
    }

    internal async Task RemoveAsync() {
      try {
        List<Asset> assets = await GetAllAsync();
        if (assets.Count == 0) {
          Console.WriteLine("No assets to remove.");
          return;
        }

        ShowSelectionTable(assets);

        NumberedSelectionResult choice;
        do {
          choice = NumberedSelection.ReadIndex(assets.Count,
            "Select asset to remove (q to cancel): "
          );
          if (choice.Status == SelectionStatus.Invalid) {
            ConsoleSupport.WriteLineYellow($"Choose 1-{assets.Count}.");
          }
        } while (choice.Status == SelectionStatus.Invalid);

        if (choice.Status == SelectionStatus.Cancelled) {
          return;
        }

        Asset selected = assets[choice.Index];
        if (!ConsoleSupport.TryReadInput(
              $"Remove {selected.Brand} {selected.Model}? (y/N): ",
              out string? confirmation
            ) || !string.Equals(confirmation, "y",
              StringComparison.OrdinalIgnoreCase
            )) {
          Console.WriteLine("Removal cancelled.");
          return;
        }

        Console.WriteLine(await _m.DeleteAsync(selected.Id)
          ? "Asset removed."
          : "Asset no longer exists."
        );
      }

      catch (Exception e) when (
        e is IOException or UnauthorizedAccessException
          or JsonException or InvalidDataException
      ) {
        ConsoleSupport.WriteLineRed($"Could not remove asset: {e.Message}");
      }
    }

    private static void ShowSelectionTable(List<Asset> assets) {
      ConsoleTable.Write(
        ["Type", "Brand", "Model", "Purchase Date", "Office"],
        assets.Select(asset => (IReadOnlyList<string>) [
            asset.Type.ToString(), asset.Brand, asset.Model,
            asset.PurchaseDate.ToString("yyyy-MM-dd",
              CultureInfo.InvariantCulture
            ),
            asset.Office.ToString()
          ]
        )
      );
    }

    private static Asset CreateAsset(
      Guid id, AssetType type, string brand, string model,
      DateOnly purchaseDate, decimal priceUsd, Office office
    ) {
      return type switch {
        AssetType.Computer => new Computer(
          id, brand, model, purchaseDate, priceUsd, office
        ),
        AssetType.MobilePhone => new MobilePhone(
          id, brand, model, purchaseDate, priceUsd, office
        ),
        _ => throw new InvalidOperationException("Unsupported asset type.")
      };
    }

    private static bool TryReadAssetType(
      out AssetType type, AssetType? current = null
    ) {
      AssetType[] types = Enum.GetValues<AssetType>();

      string label = current is AssetType saved
        ? $" [{GetAssetTypeLabel(saved)}]"
        : "";
      Console.WriteLine($"Select type (1-{types.Length}){label}: ");
      for (int index = 0; index < types.Length; index++) {
        Console.WriteLine($"  {index + 1}. {GetAssetTypeLabel(types[index])}");
      }

      while (true) {
        if (!ConsoleSupport.TryReadMenuChoice(out string? input, waitChar)) {
          type = default;
          return false;
        }

        if (input.Length == 0 && current is AssetType existing) {
          type = existing;
          return true;
        }

        if (int.TryParse(input, out int selection)
            && selection >= 1 && selection <= types.Length) {
          type = types[selection - 1];
          return true;
        }

        ConsoleSupport.WriteLineYellow($"Choose 1-{types.Length}.");
      }
    }

    private static string GetAssetTypeLabel(AssetType type) {
      return typeof(AssetType)
               .GetField(type.ToString())?
               .GetCustomAttribute<DescriptionAttribute>()?.Description
             ?? type.ToString();
    }

    private static bool TryReadOffice(
      out Office office, Office? current = null
    ) {
      Office[] offices = Enum.GetValues<Office>();

      string label = current is Office saved ? $" [{saved}]" : "";
      Console.WriteLine($"Select office (1-{offices.Length}){label}: ");

      for (int index = 0; index < offices.Length; index++) {
        Console.WriteLine($"  {index + 1}. {offices[index]}");
      }

      while (true) {
        if (!ConsoleSupport.TryReadMenuChoice(out string? input, waitChar)) {
          office = default;
          return false;
        }

        if (input.Length == 0 && current is Office existing) {
          office = existing;
          return true;
        }

        if (int.TryParse(input, out int selection)
            && selection >= 1 && selection <= offices.Length) {
          office = offices[selection - 1];
          return true;
        }

        ConsoleSupport.WriteLineYellow($"Choose 1-{offices.Length}.");
      }
    }

    private static bool TryReadText(
      string prompt, out string value, string? current = null
    ) {
      while (true) {
        string label = current is null
          ? $"{prompt}: "
          : $"{prompt} [{current}]: ";
        if (!ConsoleSupport.TryReadInput(label, out string? input)) {
          value = string.Empty;
          return false;
        }

        if (input.Length == 0 && current is not null) {
          value = current;
          return true;
        }

        if (TextFieldValidation.TryField(input, out value)) {
          return true;
        }

        ConsoleSupport.WriteLineYellow("This field cannot be empty.");
      }
    }

    private static bool TryReadDate(
      out DateOnly date, DateOnly? current = null
    ) {
      while (true) {
        string prompt = current is DateOnly saved
          ? $"Purchase date (yyyy-MM-dd) [{saved.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}]: "
          : "Purchase date (yyyy-MM-dd): ";

        if (!ConsoleSupport.TryReadInput(prompt, out string? input)) {
          date = default;
          return false;
        }

        if (input.Length == 0 && current is DateOnly existing) {
          date = existing;
          return true;
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

    private static bool TryReadPrice(
      string prompt, out decimal price, decimal? current = null
    ) {
      while (true) {
        string label = current is decimal saved
          ? $"{prompt} [{saved.ToString("F2", CultureInfo.CurrentCulture)}]: "
          : $"{prompt}: ";
        if (!ConsoleSupport.TryReadInput(label, out string? input)) {
          price = default;
          return false;
        }

        if (input.Length == 0 && current is decimal existing) {
          price = existing;
          return true;
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
  }
}
