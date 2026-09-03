using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class StructurePlacementTransactionSystem
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
    return new StructurePlacementSystem().TryPrepare(
      snapshot,
      definition,
      originX,
      originY,
      protection,
      out placement,
      out failureReason);
  }

  public bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    StructureDefinition definition,
    StructurePlacementComponent placement,
    TileProtectionComponent protection,
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

    for (int localY = 0; localY < definition.Height; localY++)
    {
      for (int localX = 0; localX < definition.Width; localX++)
      {
        int x = placement.OriginX + localX;
        int y = placement.OriginY + localY;
        if (!snapshot.Metadata.IsInside(x, y) || protection.IsProtected(x, y) ||
            (!definition.AllowReplaceExisting && snapshot.GetTile(x, y).IsActive))
        {
          return false;
        }
      }
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
        int x = placement.OriginX + localX;
        int y = placement.OriginY + localY;
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.Place,
          definition.TileType,
          Priority: definition.Priority,
          Source: definition.Id));
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          x,
          y,
          TileChangeKind.SetWall,
          TileType: 0,
          WallType: definition.WallType,
          Priority: definition.Priority,
          Source: definition.Id));
      }
    }

    return true;
  }
}
