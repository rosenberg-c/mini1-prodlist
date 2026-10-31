using App.src.ConsoleCore;

namespace App.src.Products {
  internal sealed class ProductMenu {
    private readonly ProductActions _actions;

    internal ProductMenu(ProductManager manager) {
      _actions = new ProductActions(manager);
    }

    internal static void DisplayMenu() {
      ConsoleRender.Header("PRODUCT INVENTORY SYSTEM");
      Console.WriteLine($"Enter {ConsoleSupport.QuitCommand} to exit/back");
      Console.WriteLine();
      Console.WriteLine("1. Add");
      Console.WriteLine("2. Delete");
      Console.WriteLine("3. Edit");
      Console.WriteLine("4. View");
      Console.WriteLine("5. Search");
      Console.WriteLine("6. Statistics");
      Console.WriteLine();
      ConsoleRender.Footer();
    }

    private static void Pause() {
      ConsoleRender.Footer();
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
            ConsoleRender.Header("Add");
            await _actions.AddAsync();
            break;
          }

          case "2":
            ConsoleRender.Header("Delete");
            await _actions.DeleteAsync();
            break;

          case "3": {
            ConsoleRender.Header("Edit");
            await _actions.EditAsync();
            break;
          }

          case "4": {
            ConsoleRender.Header("View");
            await _actions.ViewAsync();
            Pause();
            break;
          }

          case "5": {
            ConsoleRender.Header("Search");
            await _actions.SearchAsync();
            break;
          }

          case "6": {
            ConsoleRender.Header("Statistics");
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
