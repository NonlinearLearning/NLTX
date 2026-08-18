using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class OrePlacementSystem
{
  public void AppendCommands(
    WorldGridSnapshot snapshot,
    WorldGenerationRequest request,
    OreDefinition definition,
    TileProtectionComponent protection,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(commands);
    if (state.Stage < WorldGenerationStage.Ore &&
        !state.TryAdvance(WorldGenerationStage.Ore))
    {
      throw new InvalidOperationException("Ore stage could not be started.");
    }

    int depthRange = Math.Max(1, definition.MaxDepth - definition.MinDepth + 1);
    int centerX = Math.Abs(request.Metadata.Seed.Value) % snapshot.Metadata.Width;
    int centerY = definition.MinDepth + Math.Abs(request.Metadata.Seed.Value) % depthRange;
    for (int offsetX = -definition.VeinRadius; offsetX <= definition.VeinRadius; offsetX++)
    {
      for (int offsetY = -definition.VeinRadius; offsetY <= definition.VeinRadius; offsetY++)
      {
        int x = centerX + offsetX;
        int y = centerY + offsetY;
        if (!snapshot.Metadata.IsInside(x, y) || protection.IsProtected(x, y))
        {
          continue;
        }

        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Place,
          definition.TileType));
      }
    }
  }
}
