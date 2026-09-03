using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTeleporterClearArea
{
  private const string Source = "worldgen.secretseed.AddTeleporters.clearArea";

  public static bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    int tileX,
    int tileY,
    IReadOnlySet<ushort> killableTileTypes,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(killableTileTypes);
    ArgumentNullException.ThrowIfNull(commands);
    if (!snapshot.Metadata.IsInside(tileX - 1, tileY) ||
        !snapshot.Metadata.IsInside(tileX + 1, tileY + 1))
    {
      return false;
    }

    ushort supportTileType = snapshot.GetTile(tileX, tileY + 1).Type;
    for (int x = tileX - 1; x <= tileX + 1; x++)
    {
      WorldTile tile = snapshot.GetTile(x, tileY);
      if (tile.IsActive && killableTileTypes.Contains(tile.Type))
      {
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(), x, tileY, TileChangeKind.Kill, 0, Source: Source));
      }

      WorldTile support = snapshot.GetTile(x, tileY + 1);
      if (!support.IsActive || support.Slope != 0 || support.IsHalfBrick)
      {
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(), x, tileY + 1, TileChangeKind.UpdateTileType,
          support.IsActive ? support.Type : supportTileType,
          IsActive: true,
          Source: Source));
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(), x, tileY + 1, TileChangeKind.UpdateTileShape, 0,
          IsHalfBrick: false,
          Slope: 0,
          Source: Source));
      }
    }

    return true;
  }
}
