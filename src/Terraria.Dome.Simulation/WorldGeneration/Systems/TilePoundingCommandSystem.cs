using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public static class TilePoundingCommandSystem
{
  public static bool TryCreatePoundCommand(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    long sequence,
    IReadOnlySet<ushort> boulderTileTypes,
    bool isGeneratingOrLoadingWorld,
    Func<int, int, bool> canKillTile,
    out TileChangeCommand command)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(sequence);
    if (!CanPound(
          snapshot,
          x,
          y,
          boulderTileTypes,
          isGeneratingOrLoadingWorld,
          canKillTile))
    {
      command = default;
      return false;
    }

    WorldTile tile = snapshot.GetTile(x, y);
    command = new TileChangeCommand(
      sequence,
      x,
      y,
      TileChangeKind.UpdateTileShape,
      tile.Type,
      IsHalfBrick: !tile.IsHalfBrick);
    return true;
  }

  public static bool TryCreateSlopeCommand(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    byte slope,
    long sequence,
    IReadOnlySet<ushort> boulderTileTypes,
    bool isGeneratingOrLoadingWorld,
    Func<int, int, bool> canKillTile,
    out TileChangeCommand command)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(sequence);
    if (!CanPound(
          snapshot,
          x,
          y,
          boulderTileTypes,
          isGeneratingOrLoadingWorld,
          canKillTile))
    {
      command = default;
      return false;
    }

    WorldTile tile = snapshot.GetTile(x, y);
    command = new TileChangeCommand(
      sequence,
      x,
      y,
      TileChangeKind.UpdateTileShape,
      tile.Type,
      IsHalfBrick: false,
      Slope: slope);
    return true;
  }

  private static bool CanPound(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlySet<ushort> boulderTileTypes,
    bool isGeneratingOrLoadingWorld,
    Func<int, int, bool> canKillTile)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(boulderTileTypes);
    ArgumentNullException.ThrowIfNull(canKillTile);
    return TilePoundingEligibilityQuery.CanPound(
      snapshot,
      x,
      y,
      boulderTileTypes,
      isGeneratingOrLoadingWorld,
      canKillTile);
  }
}
