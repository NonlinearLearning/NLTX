using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class TileFrameCommandSystem
{
  public bool TryAppendCommands(
    IReadOnlyCollection<TileFrameRequest> requests,
    ref WorldGenerationStateComponent state,
    List<TileFrameCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(requests);
    ArgumentNullException.ThrowIfNull(commands);
    if (state.Stage < WorldGenerationStage.Framing &&
        !state.TryAdvance(WorldGenerationStage.Framing))
    {
      return false;
    }

    foreach (TileFrameRequest request in requests)
    {
      TileFrameEvaluationResult evaluation = TileFrameEvaluationQuery.Evaluate(request);
      if (!evaluation.IsSupported || evaluation.ShouldKill)
      {
        continue;
      }

      commands.Add(new TileFrameCommand(
        state.ReserveSequence(),
        request.X,
        request.Y,
        evaluation.FrameX,
        evaluation.FrameY,
        evaluation.IsHalfBrick,
        evaluation.Slope,
        Source: request.Source));
    }

    return true;
  }
}
