using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class UndergroundTreeTrunkCommandSystem
{
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
    UndergroundTreeGrowthEligibilityResult eligibility =
      UndergroundTreeGrowthEligibilityQuery.Evaluate(snapshot, originX, groundY, height);
    if (!eligibility.IsEligible)
    {
      return false;
    }

    for (int offset = 1; offset <= height; offset++)
    {
      int y = groundY - offset;
      if (protection.IsProtected(originX, y))
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
        TreeTileType));
    }

    return true;
  }
}
