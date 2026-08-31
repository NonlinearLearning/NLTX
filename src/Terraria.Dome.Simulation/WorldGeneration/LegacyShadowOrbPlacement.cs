using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldGeneration.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyShadowOrbPlacement
{
  private const int BoundaryPadding = 10;
  private const ushort ShadowOrbTileType = 31;

  public static bool TryAppendIntent(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    bool crimsonHeart,
    ref WorldGenerationStateComponent state,
    List<StructurePlacementCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (x < BoundaryPadding || x > snapshot.Metadata.Width - BoundaryPadding ||
        y < BoundaryPadding || y > snapshot.Metadata.Height - BoundaryPadding)
    {
      return false;
    }

    int originX = x - 1;
    int originY = y - 1;
    for (int localX = 0; localX < 2; localX++)
    {
      for (int localY = 0; localY < 2; localY++)
      {
        WorldTile tile = snapshot.GetTile(originX + localX, originY + localY);
        if (tile.IsActive && tile.Type == ShadowOrbTileType)
        {
          return false;
        }
      }
    }

    string definitionId = crimsonHeart
      ? "worldgen.structure.ShadowOrb.CrimsonHeart"
      : "worldgen.structure.ShadowOrb";
    commands.Add(new StructurePlacementCommand(
      state.ReserveSequence(),
      definitionId,
      originX,
      originY));
    return true;
  }
}
