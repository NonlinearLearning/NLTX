using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPaintEverythingNegative
{
  private const int BottomExclusionRows = 30;
  private const ushort DungeonDoorTileType = 19;
  private const ushort FirstCeilingTileType = 192;
  private const ushort SecondCeilingTileType = 384;
  private const ushort FirstSpecialWallType = 73;
  private const ushort SecondSpecialWallType = 60;
  private const byte NegativePaintColor = 30;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int worldSurfaceY,
    bool justUnderground,
    bool justSomeThings,
    LegacyPaintEverythingNegativeProfile profile,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(profile.DungeonTileTypes);
    ArgumentNullException.ThrowIfNull(profile.CrackedBrickTileTypes);
    ArgumentNullException.ThrowIfNull(profile.DungeonWallTypes);
    ArgumentNullException.ThrowIfNull(profile.CloudTileTypes);
    ArgumentNullException.ThrowIfNull(profile.VineTileTypes);
    ArgumentNullException.ThrowIfNull(profile.CeilingNeighborTileTypes);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (worldSurfaceY < 0 || worldSurfaceY > snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    if (justUnderground && worldSurfaceY < 2)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    int bottomBoundExclusive = Math.Max(0, snapshot.Metadata.Height - BottomExclusionRows);
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      int startY = justUnderground ? worldSurfaceY - random.Next(3) : 0;
      for (int y = startY; y < bottomBoundExclusive; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (justSomeThings)
        {
          AppendSelectiveCommands(snapshot, x, y, tile, profile, ref state, commands);
        }
        else
        {
          AppendFullPaintCommand(x, y, ref state, commands);
        }
      }
    }
  }

  private static void AppendSelectiveCommands(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    WorldTile tile,
    LegacyPaintEverythingNegativeProfile profile,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    if (profile.DungeonTileTypes.Contains(tile.Type))
    {
      AppendTilePaintCommand(x, y, ref state, commands);
    }

    if (profile.CrackedBrickTileTypes.Contains(tile.Type))
    {
      AppendTilePaintCommand(x, y, ref state, commands);
    }

    if (profile.DungeonWallTypes.Contains(tile.WallType))
    {
      AppendWallPaintCommand(x, y, ref state, commands);
      if (tile.Type == DungeonDoorTileType)
      {
        AppendTilePaintCommand(x, y, ref state, commands);
      }
    }

    if (profile.CloudTileTypes.Contains(tile.Type))
    {
      AppendTilePaintCommand(x, y, ref state, commands);
    }

    if (tile.WallType == FirstSpecialWallType)
    {
      AppendWallPaintCommand(x, y, ref state, commands);
    }

    if (tile.Type is FirstCeilingTileType or SecondCeilingTileType)
    {
      AppendTilePaintCommand(x, y, ref state, commands);
      AppendVinePaintCommands(snapshot, x, y, profile.VineTileTypes, ref state, commands);
      AppendCeilingNeighborPaintCommands(
        snapshot,
        x,
        y,
        profile.CeilingNeighborTileTypes,
        ref state,
        commands);
    }

    if (tile.WallType == SecondSpecialWallType)
    {
      AppendWallPaintCommand(x, y, ref state, commands);
    }
  }

  private static void AppendVinePaintCommands(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlySet<ushort> vineTileTypes,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    int nextY = y + 1;
    if (nextY >= snapshot.Metadata.Height ||
        !vineTileTypes.Contains(snapshot.GetTile(x, nextY).Type))
    {
      return;
    }

    for (int currentY = nextY; currentY < snapshot.Metadata.Height; currentY++)
    {
      WorldTile tile = snapshot.GetTile(x, currentY);
      if (!tile.IsActive || !vineTileTypes.Contains(tile.Type))
      {
        return;
      }

      AppendTilePaintCommand(x, currentY, ref state, commands);
    }
  }

  private static void AppendCeilingNeighborPaintCommands(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlySet<ushort> ceilingNeighborTileTypes,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    if (y < 1 || !ceilingNeighborTileTypes.Contains(snapshot.GetTile(x, y - 1).Type))
    {
      return;
    }

    AppendTilePaintCommand(x, y - 1, ref state, commands);
    if (y >= 2 && ceilingNeighborTileTypes.Contains(snapshot.GetTile(x, y - 2).Type))
    {
      AppendTilePaintCommand(x, y - 2, ref state, commands);
    }
  }

  private static void AppendFullPaintCommand(
    int x,
    int y,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    commands.Add(new TileChangeCommand(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.SetPaint,
      0,
      TileColor: NegativePaintColor,
      WallColor: NegativePaintColor,
      Source: "worldgen.secretseed.PaintEverythingNegative"));
  }

  private static void AppendTilePaintCommand(
    int x,
    int y,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    commands.Add(new TileChangeCommand(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.SetPaint,
      0,
      TileColor: NegativePaintColor,
      Source: "worldgen.secretseed.PaintEverythingNegative"));
  }

  private static void AppendWallPaintCommand(
    int x,
    int y,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    commands.Add(new TileChangeCommand(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.SetPaint,
      0,
      WallColor: NegativePaintColor,
      Source: "worldgen.secretseed.PaintEverythingNegative"));
  }
}
