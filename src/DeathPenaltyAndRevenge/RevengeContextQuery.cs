using System;

namespace Terraria.DeathPenaltyAndRevenge;

public static class RevengeContextQuery
{
  public static bool IsNpcDiscouraged(
    in RevengeDiscouragementInput input)
  {
    switch (input.AiStyle)
    {
      case 2:
        return input.AiStyle2IsDiscouraged;
      case 3:
        return !input.AiStyle3IsNotDiscouraged;
      case 6:
        return IsStyleSixNpcDiscouraged(input);
      default:
        return input.SpawnNpcNetId.Value switch
        {
          253 => !input.IsEclipse,
          490 => input.IsDayTime,
          _ => false,
        };
    }
  }

  public static bool IntersectsOuterBox(
    RevengeTargetSnapshotComponent target,
    RevengeProximityBox playerInnerBox,
    RevengeProximityBox playerOuterBox)
  {
    ArgumentNullException.ThrowIfNull(target);
    return target.EnemyHitbox.Intersects(playerOuterBox);
  }

  private static bool IsStyleSixNpcDiscouraged(
    in RevengeDiscouragementInput input)
  {
    bool checksSurfaceHeight = input.DiscouragementNpcTypeId.Value switch
    {
      513 => !input.PlayerInUndergroundDesert,
      10 or 39 or 95 or 117 or 510 => true,
      _ => false,
    };

    if (!checksSurfaceHeight)
    {
      return false;
    }

    if (!double.IsFinite(input.WorldSurfaceTileY))
    {
      throw new ArgumentOutOfRangeException(nameof(input));
    }

    return (double)input.PlayerPosition.Y < input.WorldSurfaceTileY * 16.0;
  }
}
