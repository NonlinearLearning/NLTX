namespace Terraria.Player.Progression;

public static class RemainingMinionCapacityQuery
{
  public static float Get(PlayerMinionCapacityComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    lock (component.SyncRoot)
    {
      return Math.Max(0, component.MaxMinions - component.SlotsMinions);
    }
  }

  public static bool HasCapacity(PlayerMinionCapacityComponent component)
  {
    return Get(component) > 0;
  }

  public static bool CanFit(
    PlayerMinionCapacityComponent component,
    float slotCost)
  {
    ArgumentNullException.ThrowIfNull(component);
    return float.IsFinite(slotCost) && slotCost >= 0 && slotCost <= Get(component);
  }
}
