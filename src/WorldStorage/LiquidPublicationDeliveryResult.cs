namespace Terraria.WorldStorage;

public readonly record struct LiquidPublicationDeliveryResult(
  LiquidPublicationDeliveryStatus Status,
  string? FailureReason)
{
  public bool Succeeded => Status == LiquidPublicationDeliveryStatus.Delivered;

  public static LiquidPublicationDeliveryResult Delivered =>
    new(LiquidPublicationDeliveryStatus.Delivered, null);

  public static LiquidPublicationDeliveryResult Failed(string reason) =>
    new(LiquidPublicationDeliveryStatus.Failed, reason);

  public static LiquidPublicationDeliveryResult Unknown(string reason) =>
    new(LiquidPublicationDeliveryStatus.Unknown, reason);

  public static LiquidPublicationDeliveryResult Cancelled(string reason) =>
    new(LiquidPublicationDeliveryStatus.Cancelled, reason);
}
