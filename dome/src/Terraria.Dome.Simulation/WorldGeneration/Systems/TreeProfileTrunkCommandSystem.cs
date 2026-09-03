using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class TreeProfileTrunkCommandSystem
{
  public bool TryAppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTreeProfileKind profileKind,
    int originX,
    int groundY,
    int height,
    TileProtectionComponent protection,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    if (!LegacyTreeProfileRegistry.TryGet(profileKind, out LegacyTreeProfileDefinition profile) ||
        height < profile.MinimumHeight || height > profile.MaximumHeight ||
        !snapshot.Metadata.IsInside(originX, groundY))
    {
      return false;
    }

    for (int offset = 1; offset <= height; offset++)
    {
      int y = groundY - offset;
      if (!snapshot.Metadata.IsInside(originX, y) ||
          snapshot.GetTile(originX, y).IsActive || protection.IsProtected(originX, y))
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
        profile.TreeTileType,
        Source: $"worldgen.tree.profile.{profile.Kind}"));
    }

    return true;
  }
}
