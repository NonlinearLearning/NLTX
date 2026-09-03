using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldTileSwap
{
  private const string Source = "worldgen.secretseed.ErrorWorld.singleTileSwap";

  public static void AppendCommands(
    int firstX,
    int firstY,
    int secondX,
    int secondY,
    WorldTile first,
    WorldTile second,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(commands);
    AppendTileCommands(firstX, firstY, second, ref state, commands);
    AppendTileCommands(secondX, secondY, first, ref state, commands);
  }

  private static void AppendTileCommands(
    int x,
    int y,
    WorldTile sourceTile,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    commands.Add(new TileChangeCommand(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.UpdateTileType,
      sourceTile.Type,
      IsActive: sourceTile.IsActive,
      Source: Source));
    commands.Add(new TileChangeCommand(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.SetPaint,
      0,
      TileColor: sourceTile.TileColor,
      Source: Source));
    commands.Add(new TileChangeCommand(
      state.ReserveSequence(),
      x,
      y,
      TileChangeKind.SetCoating,
      0,
      IsInvisibleBlock: sourceTile.IsInvisibleBlock,
      IsFullbrightBlock: sourceTile.IsFullbrightBlock,
      Source: Source));
  }
}
