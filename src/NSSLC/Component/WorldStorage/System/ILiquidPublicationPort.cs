namespace Terraria.WorldStorage;

public interface ILiquidPublicationPort
{
  LiquidPublicationDeliveryResult Publish(
    LiquidChangePublicationSnapshot snapshot);
}
