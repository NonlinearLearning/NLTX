using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldFinishCleanup
{
  private const ushort CrackedBrickFirstTileType = 481;
  private const ushort CrackedBrickLastTileType = 483;
  private const ushort CrystalBlockTileType = 501;
  private const ushort IllegalMossTileType = 381;
  private const ushort MossFirstReplacementTileType = 539;
  private const ushort MossSecondReplacementTileType = 536;
  private const ushort MossThirdReplacementTileType = 534;
  private const ushort MossFourthReplacementTileType = 625;
  private const ushort MossFifthReplacementTileType = 627;
  private const ushort MossWallType = 73;
  private const ushort ObsidianWallType = 238;
  private const ushort StatueTileType = 137;
  private const int BorderMargin = 20;
  private const int MossRerollDivisor = 5;
  private const int StatueFrameCount = 6;
  private const int FrameUnitPixels = 18;
  private const string Source = "worldgen.secretseed.ErrorWorld.finishCleanup";

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int worldSurfaceY,
    int randomAdjustment,
    IReadOnlySet<ushort> mossTileTypes,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(mossTileTypes);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (worldSurfaceY < 0 || worldSurfaceY >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(randomAdjustment);
    for (int x = BorderMargin; x < snapshot.Metadata.Width - BorderMargin; x++)
    {
      for (int y = BorderMargin; y < snapshot.Metadata.Height - BorderMargin; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        AppendCrackedBrickCommand(tile, x, y, ref state, commands);
        AppendMossCommand(tile, x, y, randomAdjustment, mossTileTypes, random, ref state, commands);
        AppendWallCommands(tile, x, y, worldSurfaceY, ref state, commands);
        AppendCrystalBlockCommand(tile, x, y, ref state, commands);
        AppendStatueFrameCommand(tile, x, y, random, ref state, commands);
      }
    }
  }

  private static void AppendCrackedBrickCommand(
    WorldTile tile,
    int x,
    int y,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    if (tile.Type is >= CrackedBrickFirstTileType and <= CrackedBrickLastTileType)
    {
      commands.Add(new TileChangeCommand(
        state.ReserveSequence(), x, y, TileChangeKind.SetCoating, 0,
        IsInvisibleBlock: true, Source: Source));
    }
  }

  private static void AppendMossCommand(
    WorldTile tile,
    int x,
    int y,
    int randomAdjustment,
    IReadOnlySet<ushort> mossTileTypes,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    if (!mossTileTypes.Contains(tile.Type) || tile.Type == IllegalMossTileType ||
        random.Next(MossRerollDivisor * randomAdjustment) != 0)
    {
      return;
    }

    ushort tileType = random.Next(5) switch
    {
      0 => MossFirstReplacementTileType,
      1 => MossSecondReplacementTileType,
      2 => MossThirdReplacementTileType,
      3 => MossFourthReplacementTileType,
      _ => MossFifthReplacementTileType
    };
    commands.Add(new TileChangeCommand(
      state.ReserveSequence(), x, y, TileChangeKind.UpdateTileType, tileType, Source: Source));
  }

  private static void AppendWallCommands(
    WorldTile tile,
    int x,
    int y,
    int worldSurfaceY,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    if ((y > worldSurfaceY && tile.WallType == MossWallType) ||
        tile.WallType == ObsidianWallType)
    {
      commands.Add(new TileChangeCommand(
        state.ReserveSequence(), x, y, TileChangeKind.SetWall, 0, WallType: 0, Source: Source));
    }
  }

  private static void AppendCrystalBlockCommand(
    WorldTile tile,
    int x,
    int y,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    if (tile.Type == CrystalBlockTileType)
    {
      commands.Add(new TileChangeCommand(
        state.ReserveSequence(), x, y, TileChangeKind.SetCoating, 0,
        IsInvisibleBlock: true, Source: Source));
    }
  }

  private static void AppendStatueFrameCommand(
    WorldTile tile,
    int x,
    int y,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    if (tile.Type != StatueTileType || tile.HasWire)
    {
      return;
    }

    short frameX = checked((short)(random.Next(StatueFrameCount) * FrameUnitPixels));
    commands.Add(new TileChangeCommand(
      state.ReserveSequence(), x, y, TileChangeKind.UpdateTileType, tile.Type,
      FrameX: frameX, Source: Source));
  }
}
