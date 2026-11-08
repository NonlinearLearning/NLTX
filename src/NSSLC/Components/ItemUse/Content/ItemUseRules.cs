namespace Terraria.Item;

/// <summary>Pure timing and gate rules for item use, extracted from Version4 <c>Player.ItemCheck</c>.</summary>
/// <remarks>
/// The reference keeps the gate in two fields: <c>itemAnimation</c> counts the running animation
/// down and <c>reuseDelay</c> blocks the next use of the same item. The server expresses the same
/// gate as one absolute tick — the earliest tick at which the next use may start — so no per-tick
/// countdown state has to be advanced.
/// </remarks>
public static class ItemUseRules
{
  /// <summary>Version4 <c>Item.useTime</c> is at least one tick.</summary>
  public const int MinimumUseTime = 1;

  /// <summary>Earliest tick the next use may start.</summary>
  public static long ComputeReadyTick(long tick, int useTime)
    => tick + (useTime < MinimumUseTime ? MinimumUseTime : useTime);

  public static bool IsReady(long tick, long readyTick) => tick >= readyTick;

  /// <summary>Channel items keep firing while the use button stays held.</summary>
  public static bool ContinuesChannel(bool channel, bool useHeld) => channel && useHeld;

  /// <summary>Version4 consumes exactly one ammo item per shot for the supported kinds.</summary>
  public static int AmmoCost(ItemUseDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    return definition.RequiresAmmo ? 1 : 0;
  }

  /// <summary>True when the stack can serve as ammunition for the definition.</summary>
  public static bool IsAmmoFor(ItemUseDefinition definition, in ItemStack stack)
  {
    ArgumentNullException.ThrowIfNull(definition);
    return !stack.IsEmpty && definition.RequiresAmmo && stack.ItemId == definition.UseAmmo;
  }
}
