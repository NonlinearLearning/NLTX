using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class StinkbugBlockerValidationQuery
{
  public static StinkbugBlockerValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    int style,
    ushort wallType)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    AnchorOrientationValidationResult orientation = AnchorOrientationValidationQuery.Evaluate(
      snapshot,
      tileDefinitions,
      x,
      y,
      style,
      wallType,
      switchToWallIfInvalid: false);
    if (!orientation.IsValid)
    {
      return new StinkbugBlockerValidationResult(
        false,
        true,
        style,
        -1,
        false,
        true);
    }

    bool swapped = style is 2 or 3;
    int suggestedStyle = orientation.SuggestedStyle;

    return new StinkbugBlockerValidationResult(
      true,
      false,
      style,
      suggestedStyle,
      swapped,
      true);
  }
}
