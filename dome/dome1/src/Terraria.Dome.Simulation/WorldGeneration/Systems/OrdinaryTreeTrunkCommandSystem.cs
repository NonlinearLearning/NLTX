using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class OrdinaryTreeTrunkCommandSystem
{
  private const int CanopyHalfWidth = 2;
  private const int CanopyTopPadding = 4;
  private const int MaximumHeight = 16;
  private const int MinimumHeight = 5;
  private const ushort SaplingTileType = 20;
  private const ushort TreeTileType = 5;

  public bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    int originX,
    int groundY,
    int height,
    TileProtectionComponent protection,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (height < MinimumHeight || height > MaximumHeight ||
        !snapshot.Metadata.IsInside(originX, groundY) ||
        !TreeCanopyClearanceQuery.IsClear(
          snapshot,
          originX - CanopyHalfWidth,
          originX + CanopyHalfWidth,
          groundY - height - CanopyTopPadding,
          groundY - 1,
          SaplingTileType,
          CommonSaplingTileRegistry.RegisterDefaults()))
    {
      return false;
    }

    for (int offset = 1; offset <= height; offset++)
    {
      int y = groundY - offset;
      if (!snapshot.Metadata.IsInside(originX, y) || protection.IsProtected(originX, y))
      {
        return false;
      }
    }

    if (state.Stage < WorldGenerationStage.Tree &&
        !state.TryAdvance(WorldGenerationStage.Tree))
    {
      return false;
    }

    for (int offset = 1; offset <= height; offset++)
    {
      commands.Add(new TileChangeCommand(
        state.ReserveSequence(),
        originX,
        groundY - offset,
        TileChangeKind.Place,
        TreeTileType,
        Source: "worldgen.tree.ordinary"));
    }

    return true;
  }
}
