using System.Text.RegularExpressions;
using System.Text.Json.Serialization;

namespace ProdList.src {
  public static partial class Product {
    internal const int minNum = 200;
    internal const int maxNum = 500;

    public record Entry(int Id, string Left, int Right) {
      [JsonIgnore]
      public string Name => $"{Left}-{Right}";
    }

    public static bool IsDuplicate(Entry entry, string left, int right) {
      return entry.Right == right && string.Equals(entry.Left, left,
        StringComparison.OrdinalIgnoreCase
      );
    }

    public static bool TrySplit(
      string name,
      out string left,
      out string right
    ) {
      ArgumentNullException.ThrowIfNull(name);

      int dash = name.IndexOf('-');
      if (dash < 0) {
        left = string.Empty;
        right = string.Empty;
        return false;
      }

      left = name[..dash];
      right = name[(dash + 1)..];
      return true;
    }
  }
}

namespace ProdList.src {
  public static partial class Product {
    public static List<Entry> Sort(IEnumerable<Entry> entries) {
      ArgumentNullException.ThrowIfNull(entries);
      return [
        .. entries
          .OrderBy(static e => e.Name, StringComparer.OrdinalIgnoreCase)
          .ThenBy(static e => e.Name, StringComparer.Ordinal)
      ];
    }
  }
}

namespace ProdList.src {
  public static partial class Product {
    public enum InputError {
      EmptyInput,
      MissingDash,
      InvalidLeftSide,
      InvalidNumber,
      NumberOutOfRange
    }

    [GeneratedRegex("^[A-Z]+$")]
    private static partial Regex LettersOnly();

    [GeneratedRegex("^[0-9]+$")]
    private static partial Regex DigitsOnly();

    public static InputError? Validate(string product) {
      return Validate(product, out _, out _);
    }

    public static InputError? Validate(
      string product,
      out string left,
      out int right
    ) {
      ArgumentNullException.ThrowIfNull(product);

      left = string.Empty;
      right = default;

      string name = product.Trim();
      if (name.Length == 0) {
        return InputError.EmptyInput;
      }

      if (!TrySplit(name, out string parsedLeft, out string rightText)) {
        return InputError.MissingDash;
      }

      if (!LettersOnly().IsMatch(parsedLeft)) {
        return InputError.InvalidLeftSide;
      }

      if (!DigitsOnly().IsMatch(rightText)) {
        return InputError.InvalidNumber;
      }

      if (!int.TryParse(rightText, out int parsedRight)
        || parsedRight is < minNum or > maxNum) {
        return InputError.NumberOutOfRange;
      }

      left = parsedLeft;
      right = parsedRight;
      return null;
    }
  }
}
