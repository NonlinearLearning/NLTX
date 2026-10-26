using System;

namespace Terraria.Liquid;

/// <summary>Stores the liquid contact committed by the spatial simulation.</summary>
public readonly record struct LiquidContactComponent(
    LiquidContactFlags Flags,
    byte Amount)
{
  public bool IsWet => Flags != LiquidContactFlags.None;

  public bool Contains(LiquidContactFlags liquid)
  {
    if (liquid == LiquidContactFlags.None)
    {
      return false;
    }

    return (Flags & liquid) == liquid;
  }

  public LiquidContactComponent Filter(LiquidContactFlags allowed)
  {
    return new LiquidContactComponent(Flags & allowed, Amount);
  }
}
