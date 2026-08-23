using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class CaveCarvingSystem
{
  public void AppendCommands(
    WorldGridSnapshot snapshot,
    WorldGenerationRequest request,
    CaveCarvingComponent profile,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(commands);
    if (profile.AlgorithmId != "single-tunnel")
    {
      throw new NotSupportedException($"Unsupported cave profile: {profile.AlgorithmId}");
    }

    if (state.Stage < WorldGenerationStage.Cave &&
        !state.TryAdvance(WorldGenerationStage.Cave))
    {
      throw new InvalidOperationException("Cave stage could not be started.");
    }

    int protectedStartX = Math.Max(0, request.SpawnX - 4);
    int protectedEndX = Math.Min(snapshot.Metadata.Width - 1, request.SpawnX + 4);
    int protectedStartY = request.SurfaceY;
    int protectedEndY = Math.Min(snapshot.Metadata.Height - 1, request.SurfaceY + 7);
    int centerY = Math.Clamp(request.SurfaceY + 24, 0, snapshot.Metadata.Height - 1);
    for (int x = profile.Density; x < snapshot.Metadata.Width; x += profile.Density)
    {
      for (int y = centerY - profile.Radius; y <= centerY + profile.Radius; y++)
      {
        if (y < 0 || y >= snapshot.Metadata.Height ||
            x >= protectedStartX && x <= protectedEndX &&
            y >= protectedStartY && y <= protectedEndY ||
            !snapshot.GetTile(x, y).IsActive)
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Kill,
          0,
          FrameX: -1,
          FrameY: -1));
      }
    }
  }
}
