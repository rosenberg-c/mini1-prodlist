using App.src.ConsoleCore;

namespace App.src.Products {
  internal sealed class ProductMenu {
    private readonly ProductActions _actions;

    internal ProductMenu(ProductManager manager) {
      _actions = new ProductActions(manager);
    }

    internal static void Header(string header) {
      int pad = 40;
      Console.Clear();
      Console.WriteLine("".PadRight(pad, '-'));
      Console.WriteLine(header);
      Console.WriteLine("".PadRight(pad, '-'));
    }

    internal static void Footer() {
      int pad = 40;
      Console.WriteLine("".PadRight(pad, '-'));
    }

    internal static void DisplayMenu() {
      Header("PRODUCT INVENTORY SYSTEM");
      Console.WriteLine($"Enter {ConsoleSupport.QuitCommand} to exit/back");
      Console.WriteLine();
      Console.WriteLine("1. Add");
      Console.WriteLine("2. Delete");
      Console.WriteLine("3. Edit");
      Console.WriteLine("4. View");
      Console.WriteLine("5. Search");
      Console.WriteLine("6. Statistics");
      Console.WriteLine();
      Footer();
    }

    private static void Pause() {
      Footer();
      Console.WriteLine("Press any key to return.");
      ConsoleSupport.HoldPressAnyKey();
    }

    internal async Task RunAsync() {
      while (true) {
        DisplayMenu();
        string? choice = ConsoleSupport.ReadString("Select option: ");

        if (ConsoleSupport.IsExitOrNull(choice)) {
          return;
        }

        switch (choice) {
          case "": {
            break;
          }

          case "1": {
            Header("Add");
            await _actions.AddAsync();
            break;
          }

          case "2":
            Header("Delete");
            await _actions.DeleteAsync();
            break;

          case "3": {
            Header("Edit");
            await _actions.EditAsync();
            break;
          }

          case "4": {
            Header("View");
            await _actions.ViewAsync();
            Pause();
            break;
          }

          case "5": {
            Header("Search");
            await _actions.SearchAsync();
            break;
          }

          case "6": {
            Header("Statistics");
            await _actions.StatisticsAsync();
            Pause();
            break;
          }

          default:
            ConsoleSupport.WriteLineYellow("Invalid option. Please try again.");
            break;
        }
      }
    }
  }
}
