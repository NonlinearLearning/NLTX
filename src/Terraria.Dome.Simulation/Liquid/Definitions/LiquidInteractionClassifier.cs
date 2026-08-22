using System;
using Terraria.Dome.Simulation.Liquid.Components;

namespace Terraria.Dome.Simulation.Liquid.Definitions;

public static class LiquidInteractionClassifier
{
  public static LiquidInteractionKind GetKind(LiquidType firstType, LiquidType secondType)
  {
    if (!Enum.IsDefined(firstType) || !Enum.IsDefined(secondType) || firstType == secondType)
    {
      return LiquidInteractionKind.None;
    }

    return (firstType, secondType) switch
    {
      (LiquidType.Water, LiquidType.Lava) or (LiquidType.Lava, LiquidType.Water) =>
        LiquidInteractionKind.LavaWater,
      (LiquidType.Water, LiquidType.Honey) or (LiquidType.Honey, LiquidType.Water) =>
        LiquidInteractionKind.HoneyWater,
      (LiquidType.Lava, LiquidType.Honey) or (LiquidType.Honey, LiquidType.Lava) =>
        LiquidInteractionKind.HoneyLava,
      (LiquidType.Water, LiquidType.Shimmer) or (LiquidType.Shimmer, LiquidType.Water) =>
        LiquidInteractionKind.ShimmerWater,
      (LiquidType.Lava, LiquidType.Shimmer) or (LiquidType.Shimmer, LiquidType.Lava) =>
        LiquidInteractionKind.ShimmerLava,
      (LiquidType.Honey, LiquidType.Shimmer) or (LiquidType.Shimmer, LiquidType.Honey) =>
        LiquidInteractionKind.ShimmerHoney,
      _ => LiquidInteractionKind.None
    };
  }
}
