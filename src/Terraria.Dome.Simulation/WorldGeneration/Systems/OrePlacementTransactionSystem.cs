using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class OrePlacementTransactionSystem
{
  public bool TryPrepare(
    WorldGridSnapshot snapshot,
    WorldGenerationRequest request,
    OreDefinition definition,
    TileProtectionComponent protection,
    out OrePlacementPreparationResult preparation)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(request);
    int depthRange = Math.Max(1, definition.MaxDepth - definition.MinDepth + 1);
    uint seed = unchecked((uint)request.Metadata.Seed.Value);
    int centerX = (int)(seed % (uint)snapshot.Metadata.Width);
    int centerY = definition.MinDepth + (int)(seed % (uint)depthRange);
    return TryPrepare(snapshot, definition, centerX, centerY, protection, out preparation);
  }

  public bool TryPrepare(
    WorldGridSnapshot snapshot,
    OreDefinition definition,
    int centerX,
    int centerY,
    TileProtectionComponent protection,
    out OrePlacementPreparationResult preparation)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    List<(int X, int Y)> cells = new();
    for (int offsetX = -definition.VeinRadius; offsetX <= definition.VeinRadius; offsetX++)
    {
      for (int offsetY = -definition.VeinRadius; offsetY <= definition.VeinRadius; offsetY++)
      {
        int x = centerX + offsetX;
        int y = centerY + offsetY;
        if (!snapshot.Metadata.IsInside(x, y) || protection.IsProtected(x, y) ||
            snapshot.GetTile(x, y).IsActive)
        {
          preparation = default;
          return false;
        }

        cells.Add((x, y));
      }
    }

    preparation = new OrePlacementPreparationResult(
      definition.TileType,
      cells,
      definition.Priority,
      definition.Id);
    return true;
  }

  public bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    OrePlacementPreparationResult preparation,
    TileProtectionComponent protection,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (!preparation.IsPrepared)
    {
      return false;
    }

    foreach ((int x, int y) in preparation.Cells)
    {
      if (!snapshot.Metadata.IsInside(x, y) || protection.IsProtected(x, y) ||
          snapshot.GetTile(x, y).IsActive)
      {
        return false;
      }
    }

    if (state.Stage < WorldGenerationStage.Ore &&
        !state.TryAdvance(WorldGenerationStage.Ore))
    {
      return false;
    }

    foreach ((int x, int y) in preparation.Cells)
    {
      commands.Add(new TileChangeCommand(
        state.ReserveSequence(),
        x,
        y,
        TileChangeKind.Place,
        preparation.TileType,
        Priority: preparation.Priority,
        Source: preparation.Source));
    }

    return true;
  }
}
