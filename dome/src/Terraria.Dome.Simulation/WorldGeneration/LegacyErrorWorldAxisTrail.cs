using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldAxisTrail
{
  private const string Source = "worldgen.secretseed.ErrorWorld.axisTrail";

  public static int AppendCommands(
    WorldGridSnapshot snapshot,
    int originX,
    int originY,
    LegacyErrorWorldAxisDirection direction,
    int length,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (!direction.IsValid || length < 0 || !snapshot.Metadata.IsInside(originX, originY))
    {
      throw new ArgumentOutOfRangeException(nameof(direction));
    }

    WorldTile sourceTile = snapshot.GetTile(originX, originY);
    int x = originX;
    int y = originY;
    int copiedCount = 0;
    for (int step = 0; step < length; step++)
    {
      x += direction.X;
      y += direction.Y;
      if (!snapshot.Metadata.IsInside(x, y) || snapshot.GetTile(x, y).IsActive)
      {
        break;
      }

      commands.Add(new TileChangeCommand(
        state.ReserveSequence(),
        originX,
        originY,
        TileChangeKind.UpdateTileShape,
        sourceTile.Type,
        IsHalfBrick: false,
        Slope: 0,
        Source: Source));
      AppendCopiedTileCommands(x, y, sourceTile, ref state, commands);
      copiedCount++;
    }

    return copiedCount;
  }

  private static void AppendCopiedTileCommands(
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
