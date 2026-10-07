namespace Terraria.Player;

public static class PlayerItemReuseQuery
{
  public static bool IsUsingOrReusingItem(
    in PlayerItemReuseSnapshot snapshot)
  {
    return snapshot.ItemAnimationRemainingTicks > 0 ||
      snapshot.ReuseDelayRemainingTicks > 0 ||
      snapshot.IsChanneling ||
      snapshot.HasPendingReuse;
  }
}
