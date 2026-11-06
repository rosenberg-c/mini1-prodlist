using System.Text.Json.Serialization;

namespace App.src.Assets {
  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum OfficeCurrency {
    Usd,
    Sek
  }

  public enum AssetType {
    Computer,
    MobilePhone
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
    decimal priceLocal,
    string office
  ) {
    public Guid Id { get; } = id;
    public string Brand { get; } = brand;
    public string Model { get; } = model;
    public DateOnly PurchaseDate { get; } = purchaseDate;
    public decimal PriceUsd { get; } = priceUsd;
    public decimal PriceLocal { get; } = priceLocal;
    public string Office { get; } = office;

    [JsonIgnore] public abstract AssetType Type { get; }
  }

  public sealed class Computer : Asset {
    public Computer(
      Guid id,
      string brand,
      string model,
      DateOnly purchaseDate,
      decimal priceUsd,
      decimal priceLocal,
      string office
    ) : base(
      id, brand, model, purchaseDate, priceUsd, priceLocal, office
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
      decimal priceLocal,
      string office
    ) : base(
      id, brand, model, purchaseDate, priceUsd, priceLocal, office
    ) {
    }

    public override AssetType Type => AssetType.MobilePhone;
  }
}
