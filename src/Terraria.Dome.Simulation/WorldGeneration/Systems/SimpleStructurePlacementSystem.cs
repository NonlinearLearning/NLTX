using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class SimpleStructurePlacementSystem
{
  public bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    SimpleStructurePattern pattern,
    IReadOnlyList<StructureDefinition> actions,
    int originX,
    int originY,
    TileProtectionComponent protection,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(actions);
    ArgumentNullException.ThrowIfNull(commands);
    if (!TryValidate(
      snapshot,
      pattern,
      actions,
      originX,
      originY,
      protection))
    {
      return false;
    }

    if (state.Stage < WorldGenerationStage.Structure &&
        !state.TryAdvance(WorldGenerationStage.Structure))
    {
      return false;
    }

    for (int y = 0; y < pattern.Height; y++)
    {
      for (int x = 0; x < pattern.Width; x++)
      {
        int actionIndex = pattern.GetActionIndex(x, y);
        if (actionIndex < 0)
        {
          continue;
        }

        StructureDefinition action = actions[actionIndex];
        int targetX = originX + (pattern.HorizontalMirror ? -x : x);
        int targetY = originY + (pattern.VerticalMirror ? -y : y);
        commands.Add(new TileChangeCommand(
          state.ReserveSequence(),
          targetX,
          targetY,
          TileChangeKind.Place,
          action.TileType,
          Priority: action.Priority,
          Source: action.Id));
        if (action.WallType != 0)
        {
          commands.Add(new TileChangeCommand(
            state.ReserveSequence(),
            targetX,
            targetY,
            TileChangeKind.SetWall,
            TileType: 0,
            WallType: action.WallType,
            Priority: action.Priority,
            Source: action.Id));
        }
      }
    }

    return true;
  }

  private static bool TryValidate(
    WorldGridSnapshot snapshot,
    SimpleStructurePattern pattern,
    IReadOnlyList<StructureDefinition> actions,
    int originX,
    int originY,
    TileProtectionComponent protection)
  {
    for (int y = 0; y < pattern.Height; y++)
    {
      for (int x = 0; x < pattern.Width; x++)
      {
        int actionIndex = pattern.GetActionIndex(x, y);
        if (actionIndex < 0)
        {
          continue;
        }

        if (actionIndex >= actions.Count)
        {
          return false;
        }

        StructureDefinition action = actions[actionIndex];
        if (action.Width != 1 || action.Height != 1)
        {
          return false;
        }

        int targetX = originX + (pattern.HorizontalMirror ? -x : x);
        int targetY = originY + (pattern.VerticalMirror ? -y : y);
        if (!snapshot.Metadata.IsInside(targetX, targetY) ||
            protection.IsProtected(targetX, targetY) ||
            (!action.AllowReplaceExisting && snapshot.GetTile(targetX, targetY).IsActive))
        {
          return false;
        }
      }
    }

    return true;
  }
}
