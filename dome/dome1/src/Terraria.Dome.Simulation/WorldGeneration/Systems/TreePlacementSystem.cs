using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class TreePlacementSystem
{
  public bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    WorldGenerationRequest request,
    TreeDefinition definition,
    TreePlacementComponent placement,
    TileProtectionComponent protection,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(commands);
    if (placement.DefinitionId != definition.Id)
    {
      return false;
    }

    int height = definition.MinimumHeight +
      Math.Abs(request.Metadata.Seed.Value) %
      (definition.MaximumHeight - definition.MinimumHeight + 1);
    if (!snapshot.Metadata.IsInside(placement.OriginX, placement.OriginY) ||
        placement.OriginY - height < 0)
    {
      return false;
    }

    HashSet<(int X, int Y)> cells = new();
    for (int offset = 0; offset < height; offset++)
    {
      AddCell(placement.OriginX, placement.OriginY - offset, definition.TrunkTileType);
    }

    for (int x = placement.OriginX - definition.CanopyRadius;
         x <= placement.OriginX + definition.CanopyRadius;
         x++)
    {
      for (int y = placement.OriginY - height - definition.CanopyRadius;
           y <= placement.OriginY - height + definition.CanopyRadius;
           y++)
      {
        if (Math.Abs(x - placement.OriginX) + Math.Abs(y - (placement.OriginY - height)) <=
            definition.CanopyRadius &&
            snapshot.Metadata.IsInside(x, y))
        {
          AddCell(x, y, definition.LeafTileType);
        }
      }
    }

    if (state.Stage < WorldGenerationStage.Tree &&
        !state.TryAdvance(WorldGenerationStage.Tree))
    {
      return false;
    }

    foreach ((int x, int y) in cells)
    {
      if (protection.IsProtected(x, y))
      {
        continue;
      }

      ushort tileType = x == placement.OriginX && y >= placement.OriginY - height
        ? definition.TrunkTileType
        : definition.LeafTileType;
      commands.Add(new TileChangeCommand(
        state.ReserveSequence(),
        x,
        y,
        TileChangeKind.Place,
        tileType,
        Source: definition.Id));
    }

    return true;

    void AddCell(int x, int y, ushort tileType)
    {
      if (snapshot.Metadata.IsInside(x, y) && !protection.IsProtected(x, y))
      {
        _ = cells.Add((x, y));
      }
    }
  }
}
