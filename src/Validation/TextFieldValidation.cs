namespace App.src.Validation {
  internal static class TextFieldValidation {
    internal static bool TryField(string? text, out string value) {
      value = text?.Trim() ?? string.Empty;
      return !string.IsNullOrWhiteSpace(value);
    }
  }
}
