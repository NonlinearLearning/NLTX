using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWorldIsFrozen
{
  private const ushort FrozenWallType = 40;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int underworldLayerY,
    int rockLayerY,
    int activeSecretSeedCount,
    bool skyblockWorld,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (underworldLayerY < 0 || underworldLayerY > snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(underworldLayerY));
    }

    if (rockLayerY < 0 || rockLayerY > snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(rockLayerY));
    }

    if (activeSecretSeedCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(activeSecretSeedCount));
    }

    if (skyblockWorld)
    {
      return;
    }

    int baseUpperBoundExclusive = activeSecretSeedCount >= 6
      ? rockLayerY
      : underworldLayerY - snapshot.Metadata.Height / 10;
    baseUpperBoundExclusive = Math.Max(baseUpperBoundExclusive, 0);
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      int upperBoundExclusive = Math.Min(
        snapshot.Metadata.Height,
        baseUpperBoundExclusive + random.Next(3));
      for (int y = 0; y < upperBoundExclusive; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (TryGetFrozenTileType(tile.Type, out ushort frozenTileType))
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.UpdateTileType,
            frozenTileType,
            Source: "worldgen.biome.WorldIsFrozen"));
        }

        if (tile.WallType is 2 or 59)
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.SetWall,
            0,
            WallType: FrozenWallType,
            Source: "worldgen.biome.WorldIsFrozen"));
        }
      }
    }
  }

  private static bool TryGetFrozenTileType(ushort tileType, out ushort frozenTileType)
  {
    frozenTileType = tileType switch
    {
      0 or 2 or 23 or 109 or 199 => 147,
      1 => 161,
      25 => 163,
      117 => 164,
      123 => 224,
      196 => 460,
      203 => 200,
      _ => tileType
    };
    return frozenTileType != tileType;
  }
}
