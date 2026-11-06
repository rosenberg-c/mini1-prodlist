using System.ComponentModel;
using System.Text.Json.Serialization;

namespace App.src.Assets {
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum Office {
    Sweden,
    USA,
    Turkey
  }

  public enum AssetType {
    Computer,
    [Description("Mobile phone")] MobilePhone
  }

  [JsonPolymorphic(TypeDiscriminatorPropertyName = "$assetType")]
  [JsonDerivedType(typeof(Computer), "computer")]
  [JsonDerivedType(typeof(MobilePhone), "mobilePhone")]
  public abstract class Asset(
    Guid id,
    string brand,
    string model,
    DateOnly purchaseDate,
    decimal priceUsd,
    Office office
  ) {
    public Guid Id { get; } = id;
    public string Brand { get; } = brand;
    public string Model { get; } = model;
    public DateOnly PurchaseDate { get; } = purchaseDate;
    public decimal PriceUsd { get; } = priceUsd;
    public Office Office { get; } = office;

    [JsonIgnore] public abstract AssetType Type { get; }
  }

  public sealed class Computer : Asset {
    public Computer(
      Guid id,
      string brand,
      string model,
      DateOnly purchaseDate,
      decimal priceUsd,
      Office office
    ) : base(
      id, brand, model, purchaseDate, priceUsd, office
    ) {
    }

    public override AssetType Type => AssetType.Computer;
  }

  public sealed class MobilePhone : Asset {
    public MobilePhone(
      Guid id,
      string brand,
      string model,
      DateOnly purchaseDate,
      decimal priceUsd,
      Office office
    ) : base(
      id, brand, model, purchaseDate, priceUsd, office
    ) {
    }

    public override AssetType Type => AssetType.MobilePhone;
  }
}
