using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;
using Terraria.Dome.Simulation.WorldObjects.FoodPlatter.Commands;

namespace Terraria.Dome.Simulation.WorldObjects.FoodPlatter.Systems;

public static class FoodPlatterDestructionCommandSystem
{
  public static bool TryCreateBatch(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    FoodPlatterSnapshot platter,
    long sequence,
    out FoodPlatterDestructionBatch batch)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentOutOfRangeException.ThrowIfNegative(sequence);
    FoodPlatterDestructionResult result = FoodPlatterDestructionQuery.Evaluate(
      snapshot,
      tileDefinitions,
      platter);
    if (!result.ShouldDestroy)
    {
      batch = default;
      return false;
    }

    batch = new FoodPlatterDestructionBatch(
      sequence,
      result.TileX,
      result.TileY,
      result.EntityId,
      platter.Exists,
      result.DroppedItem);
    return true;
  }
}
