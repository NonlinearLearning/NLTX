using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyGrassPass
{
  private const double Density = 0.002;
  private const ushort GrassTileType = 2;

  public static int CalculateAttemptCount(int width, int height)
  {
    if (width <= 0 || height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return checked((int)(width * (double)height * Density));
  }

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    bool isSkyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (isSkyblockWorld)
    {
      return;
    }

    profile.Validate(snapshot.Metadata);
    int attempts = CalculateAttemptCount(snapshot.Metadata.Width, snapshot.Metadata.Height);
    for (int attempt = 0; attempt < attempts; attempt++)
    {
      TryAppendGrass(snapshot, profile, random, ref state, commands, useLowRange: false);
      TryAppendGrass(snapshot, profile, random, ref state, commands, useLowRange: true);
    }
  }

  public static bool IsEligible(WorldGridSnapshot snapshot, int x, int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y) || x == 0 || y == 0 ||
        x == snapshot.Metadata.Width - 1 || y == snapshot.Metadata.Height - 1)
    {
      return false;
    }

    return IsEmptyCenterWithGrassNeighbors(snapshot, x, y);
  }

  public static (int MinimumInclusive, int MaximumExclusive) GetYRange(
    LegacyTerrainRuntimeProfile profile,
    int height,
    bool useLowRange)
  {
    ArgumentNullException.ThrowIfNull(profile);
    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    int minimumY = useLowRange ? 5 : (int)profile.WorldSurfaceLow;
    int maximumYExclusive = useLowRange
      ? (int)profile.WorldSurfaceLow
      : (int)profile.WorldSurfaceHigh;
    maximumYExclusive = Math.Min(maximumYExclusive, height - 1);
    return (minimumY, maximumYExclusive);
  }

  private static void TryAppendGrass(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    bool useLowRange)
  {
    int x = random.Next(1, snapshot.Metadata.Width - 1);
    (int minimumY, int maximumYExclusive) = GetYRange(
      profile,
      snapshot.Metadata.Height,
      useLowRange);
    if (maximumYExclusive <= minimumY)
    {
      return;
    }

    int y = random.Next(minimumY, maximumYExclusive);
    if (!IsEligible(snapshot, x, y))
    {
      return;
    }

    commands.Add(new TileChangeCommand(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.PlaceTile,
      GrassTileType,
      Source: "worldgen.Grass"));
  }

  private static bool IsEmptyCenterWithGrassNeighbors(
    WorldGridSnapshot snapshot,
    int x,
    int y)
  {
    WorldTile center = snapshot.GetTile(x, y);
    return !center.IsActive &&
      IsGrassSoil(snapshot.GetTile(x - 1, y)) &&
      IsGrassSoil(snapshot.GetTile(x + 1, y)) &&
      IsGrassSoil(snapshot.GetTile(x, y - 1)) &&
      IsGrassSoil(snapshot.GetTile(x, y + 1));
  }

  private static bool IsGrassSoil(WorldTile tile)
  {
    return tile.IsActive && tile.Type == 0;
  }
}
