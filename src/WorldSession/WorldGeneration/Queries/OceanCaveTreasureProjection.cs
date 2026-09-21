using System;
using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Projects committed ocean-cave treasure anchors into ordered placement commands.
/// </summary>
public static class OceanCaveTreasureProjection
{
  public static OceanCaveTreasurePlacementCommand[] CreateCommands(
    in OceanCaveTreasureSnapshot snapshot,
    int mainItemInChest)
  {
    ArgumentNullException.ThrowIfNull(snapshot.Positions);
    if (mainItemInChest < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(mainItemInChest));
    }

    if (snapshot.GenerationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    if (snapshot.Count < 0 || snapshot.Count > OceanCaveTreasureStateComponent.Capacity)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    if (snapshot.Positions.Count != snapshot.Count)
    {
      throw new ArgumentException(
        "Ocean-cave treasure positions must cover the complete used range.",
        nameof(snapshot));
    }

    OceanCaveTreasurePlacementCommand[] commands =
      new OceanCaveTreasurePlacementCommand[snapshot.Count];
    for (int index = 0; index < snapshot.Count; index++)
    {
      commands[index] = OceanCaveTreasurePlacementCommand.ForUnderwaterChest(
        snapshot.GenerationId,
        snapshot.Positions[index],
        mainItemInChest);
    }

    return commands;
  }
}
