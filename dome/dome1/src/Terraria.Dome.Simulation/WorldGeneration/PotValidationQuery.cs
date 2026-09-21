using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class PotValidationQuery
{
  private const ushort Type653 = 653;

  public static PotValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    ushort type = 28)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    Tile2x2ValidationResult footprint = Tile2x2ValidationQuery.Evaluate(
      snapshot,
      tileDefinitions,
      x,
      y,
      type);
    bool usedType653BottomSlope = type == Type653;
    return new PotValidationResult(
      footprint.IsValid,
      footprint.ShouldKill,
      footprint.OriginX,
      footprint.OriginY,
      footprint.StyleBand,
      usedType653BottomSlope,
      true,
      true);
  }
}
