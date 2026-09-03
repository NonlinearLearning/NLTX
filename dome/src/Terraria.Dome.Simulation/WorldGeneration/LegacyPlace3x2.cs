using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPlace3x2
{
  private const int BoundaryPadding = 5;
  private const short FrameHeight = 18;
  private const short StyleWidth = 54;
  private const ushort ChasmPotTileType = 26;

  public static bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    ushort tileType,
    int style,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentNullException.ThrowIfNull(commands);
    if (tileType != ChasmPotTileType || style < 0 ||
        x < BoundaryPadding || x > snapshot.Metadata.Width - BoundaryPadding ||
        y < BoundaryPadding || y > snapshot.Metadata.Height - BoundaryPadding)
    {
      return false;
    }

    int originX = x - 1;
    int originY = y - 1;
    for (int localX = 0; localX < 3; localX++)
    {
      if (!TileStateQuery.IsSolidWithoutPlatformTop(
            snapshot,
            tileDefinitions,
            originX + localX,
            y + 1) ||
          BoulderTileRegistry.RegisterDefaults().Contains(snapshot.GetTile(originX + localX, y + 1).Type))
      {
        return false;
      }

      for (int localY = 0; localY < 2; localY++)
      {
        if (snapshot.GetTile(originX + localX, originY + localY).IsActive)
        {
          return false;
        }
      }
    }

    int styleFrameX = style * StyleWidth;
    for (int localY = 0; localY < 2; localY++)
    {
      for (int localX = 0; localX < 3; localX++)
      {
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          originX + localX,
          originY + localY,
          TileChangeKind.Place,
          tileType,
          FrameX: checked((short)(styleFrameX + localX * FrameHeight)),
          FrameY: (short)(localY * FrameHeight),
          Source: "worldgen.structure.Place3x2"));
      }
    }

    return true;
  }
}
