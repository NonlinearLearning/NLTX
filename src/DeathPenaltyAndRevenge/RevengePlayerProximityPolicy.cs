using Terraria.Items;

namespace Terraria.DeathPenaltyAndRevenge;

public static class RevengePlayerProximityPolicy
{
  public const float PlayerInnerWidth = 1968.0f;
  public const float PlayerInnerHeight = 1200.0f;
  public const float PlayerOuterWidth = 2608.0f;
  public const float PlayerOuterHeight = 1840.0f;

  public static RevengeProximityBox CreateInnerBox(WorldPosition center)
  {
    return RevengeProximityBox.CenteredAt(center, PlayerInnerWidth, PlayerInnerHeight);
  }

  public static RevengeProximityBox CreateOuterBox(WorldPosition center)
  {
    return RevengeProximityBox.CenteredAt(center, PlayerOuterWidth, PlayerOuterHeight);
  }
}
