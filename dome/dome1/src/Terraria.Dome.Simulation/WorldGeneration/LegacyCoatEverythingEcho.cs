using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyCoatEverythingEcho
{
  private const ushort FirstExcludedBoulderTileType = 665;
  private const ushort SecondExcludedBoulderTileType = 711;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    bool justSomeThings,
    bool justInnerBlocks,
    bool errorWorldEnabled,
    LegacyCoatEverythingEchoProfile profile,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(profile.SelectiveTileTypes);
    ArgumentNullException.ThrowIfNull(profile.BoulderTileTypes);
    ArgumentNullException.ThrowIfNull(profile.SolidTileTypes);
    ArgumentNullException.ThrowIfNull(commands);
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      for (int y = 0; y < snapshot.Metadata.Height; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (justSomeThings)
        {
          if (profile.SelectiveTileTypes.Contains(tile.Type) ||
              IsEligibleBoulder(tile.Type, profile.BoulderTileTypes))
          {
            AppendCoatingCommand(x, y, true, null, ref state, commands);
          }

          continue;
        }

        if (!justInnerBlocks)
        {
          AppendCoatingCommand(x, y, true, true, ref state, commands);
          continue;
        }

        bool isInnerBlock = IsInnerBlock(snapshot, x, y, profile.SolidTileTypes);
        if (isInnerBlock)
        {
          AppendCoatingCommand(x, y, true, true, ref state, commands);
        }

        if (!errorWorldEnabled)
        {
          AppendCoatingCommand(x, y, null, true, ref state, commands);
        }

      }
    }

    if (justInnerBlocks && errorWorldEnabled)
    {
      AppendErrorWorldCleanupCommands(snapshot, profile.SolidTileTypes, ref state, commands);
    }
  }

  private static bool IsEligibleBoulder(ushort tileType, IReadOnlySet<ushort> boulderTileTypes)
  {
    return boulderTileTypes.Contains(tileType) &&
      tileType is not FirstExcludedBoulderTileType and not SecondExcludedBoulderTileType;
  }

  private static bool IsInnerBlock(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlySet<ushort> solidTileTypes)
  {
    for (int neighborX = x - 1; neighborX <= x + 1; neighborX++)
    {
      for (int neighborY = y - 1; neighborY <= y + 1; neighborY++)
      {
        if (!snapshot.Metadata.IsInside(neighborX, neighborY))
        {
          return false;
        }

        WorldTile tile = snapshot.GetTile(neighborX, neighborY);
        if (!tile.IsActive || tile.IsInactive || !solidTileTypes.Contains(tile.Type))
        {
          return false;
        }
      }
    }

    return true;
  }

  private static void AppendErrorWorldCleanupCommands(
    WorldGridSnapshot snapshot,
    IReadOnlySet<ushort> solidTileTypes,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      for (int y = 0; y < snapshot.Metadata.Height; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (IsInnerBlock(snapshot, x, y, solidTileTypes))
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            x,
            y,
            TileChangeKind.UpdateTileType,
            tile.Type,
            IsActive: false,
            Source: "worldgen.secretseed.CoatEverythingEcho"));
          AppendClearWallCommand(x, y, ref state, commands);
        }

        if (tile.IsActive && !tile.IsInactive && solidTileTypes.Contains(tile.Type))
        {
          AppendClearWallCommand(x, y, ref state, commands);
        }
      }
    }
  }

  private static void AppendClearWallCommand(
    int x,
    int y,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    commands.Add(new TileChangeCommand(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.SetWall,
      0,
      WallType: 0,
      Source: "worldgen.secretseed.CoatEverythingEcho"));
  }

  private static void AppendCoatingCommand(
    int x,
    int y,
    bool? isInvisibleBlock,
    bool? isInvisibleWall,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    commands.Add(new TileChangeCommand(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.SetCoating,
      0,
      IsInvisibleBlock: isInvisibleBlock,
      IsInvisibleWall: isInvisibleWall,
      Source: "worldgen.secretseed.CoatEverythingEcho"));
  }
}
