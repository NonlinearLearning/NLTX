using System;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads C13 derived dungeon properties from explicit immutable inputs.
/// </summary>
public static class DungeonDerivedPropertiesQuery
{
  public static int CurrentDungeon(DungeonLayoutSnapshot snapshot)
  {
    return snapshot.CurrentDungeon;
  }

  public static DungeonRecordSnapshot CurrentDungeonGenVars(
    DungeonLayoutSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot.Records);
    return snapshot.Records[snapshot.CurrentDungeon];
  }

  public static double DualDungeonNormalizedDistanceSafeFromDither(
    IDualDungeonDistanceQuery distanceQuery)
  {
    ArgumentNullException.ThrowIfNull(distanceQuery);
    return distanceQuery.NormalizedDistanceSafeFromDither;
  }
}
