using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class StructurePlacementSystem
{
  public bool TryPrepare(
    WorldGridSnapshot snapshot,
    StructureDefinition definition,
    int originX,
    int originY,
    TileProtectionComponent protection,
    out StructurePlacementComponent placement,
    out string? failureReason)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    StructureFootprintComponent footprint = new(definition.Width, definition.Height);
    for (int localY = 0; localY < footprint.Height; localY++)
    {
      for (int localX = 0; localX < footprint.Width; localX++)
      {
        int x = originX + localX;
        int y = originY + localY;
        if (!snapshot.Metadata.IsInside(x, y))
        {
          placement = default;
          failureReason = "Structure footprint was outside the world.";
          return false;
        }

        if (protection.IsProtected(x, y))
        {
          placement = default;
          failureReason = "Structure footprint crossed a protected region.";
          return false;
        }

        if (!definition.AllowReplaceExisting && snapshot.GetTile(x, y).IsActive)
        {
          placement = default;
          failureReason = "Structure footprint overlapped an existing tile.";
          return false;
        }
      }
    }

    placement = new StructurePlacementComponent(definition.Id, originX, originY, footprint);
    failureReason = null;
    return true;
  }

  public bool AppendCommands(
    WorldGridSnapshot snapshot,
    StructureDefinition definition,
    StructurePlacementComponent placement,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (placement.DefinitionId != definition.Id ||
        placement.Footprint != new StructureFootprintComponent(definition.Width, definition.Height))
    {
      return false;
    }

    if (state.Stage < WorldGenerationStage.Structure &&
        !state.TryAdvance(WorldGenerationStage.Structure))
    {
      return false;
    }

    for (int localY = 0; localY < definition.Height; localY++)
    {
      for (int localX = 0; localX < definition.Width; localX++)
      {
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          placement.OriginX + localX,
          placement.OriginY + localY,
          TileChangeKind.Place,
          definition.TileType));
      }
    }

    return true;
  }
}
