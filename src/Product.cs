using System.Text.RegularExpressions;

namespace ProdList.src {
  public static partial class Product {
    public record Entry(int Id, string Name);

    public static bool IsDuplicate(Entry entry, string name) {
      return string.Equals(entry.Name, name,
        StringComparison.OrdinalIgnoreCase);
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
      ArgumentNullException.ThrowIfNull(product);

      string name = product.Trim();
      if (name.Length == 0) {
        return InputError.EmptyInput;
      }

      if (!TrySplit(name, out string left, out string right)) {
        return InputError.MissingDash;
      }

      if (!LettersOnly().IsMatch(left)) {
        return InputError.InvalidLeftSide;
      }

      if (!DigitsOnly().IsMatch(right)) {
        return InputError.InvalidNumber;
      }

      if (!int.TryParse(right, out int number) || number is < 200 or > 500) {
        return InputError.NumberOutOfRange;
      }

      return null;
    }
  }
}
