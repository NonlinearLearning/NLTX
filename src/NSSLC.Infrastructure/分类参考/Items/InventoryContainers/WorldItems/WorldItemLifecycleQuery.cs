namespace Terraria.Items.InventoryContainers;

public static class WorldItemLifecycleQuery
{
  public static bool IsActive(
    WorldItemLifecycleComponent lifecycle,
    ItemStackSnapshot item)
  {
    ArgumentNullException.ThrowIfNull(lifecycle);
    ArgumentNullException.ThrowIfNull(item);
    return lifecycle.KeepTime > 0 && item.Stack > 0;
  }

  public static bool CanBePickedUp(
    WorldItemLifecycleComponent lifecycle,
    int playerIndex)
  {
    ArgumentNullException.ThrowIfNull(lifecycle);
    if (playerIndex < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(playerIndex));
    }

    if (lifecycle.Shimmered
      || lifecycle.Instanced
      || lifecycle.NoGrabDelay > 0
      || lifecycle.BeingGrabbed
      || lifecycle.OwnIgnore == playerIndex)
    {
      return false;
    }

    int reservedPlayerIndex = lifecycle.PlayerIndexTheItemIsReservedFor;
    return reservedPlayerIndex == 255 || reservedPlayerIndex == playerIndex;
  }

  public static bool CanBePickedUp(
    WorldItemLifecycleComponent lifecycle,
    ItemStackSnapshot item,
    int playerIndex)
  {
    ArgumentNullException.ThrowIfNull(item);
    return IsActive(lifecycle, item) && CanBePickedUp(lifecycle, playerIndex);
  }
}
