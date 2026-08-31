using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyMountainCaveOpeningsPass
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    IReadOnlyList<LegacyCaveCoordinate> caveHistory,
    int rockLayerY,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    bool isSkyblockWorld = false,
    bool isProtectedDungeonTile = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(caveHistory);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (isSkyblockWorld)
    {
      return;
    }

    for (int index = 0; index < caveHistory.Count; index++)
    {
      LegacyCaveCoordinate cave = caveHistory[index];
      if (!snapshot.Metadata.IsInside(cave.X, cave.Y))
      {
        throw new ArgumentOutOfRangeException(nameof(caveHistory));
      }

      LegacyMountainCaveOpeningRequest openaterRequest = new(cave.X, cave.Y, 0);
      LegacyCaveOpenaterPass.AppendCommands(
        snapshot,
        openaterRequest,
        random,
        ref state,
        commands,
        isSkyblockWorld,
        isProtectedDungeonTile);
      LegacyMountainCaveOpeningRequest cavinatorRequest = new(
        cave.X,
        cave.Y,
        random.Next(40, 50));
      LegacyCavinatorPass.AppendCommands(
        snapshot,
        cavinatorRequest,
        rockLayerY,
        random,
        ref state,
        commands,
        isSkyblockWorld,
        isProtectedDungeonTile);
    }
  }
}
