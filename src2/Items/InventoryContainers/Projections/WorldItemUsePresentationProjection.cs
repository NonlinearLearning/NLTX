namespace Terraria.Items.InventoryContainers;

public static class WorldItemUsePresentationProjection
{
  public static WorldItemUsePresentationPayload Create(
    WorldItemUsePresentationSource source)
  {
    return new WorldItemUsePresentationPayload(source);
  }
}
