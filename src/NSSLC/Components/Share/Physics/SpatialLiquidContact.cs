using Terraria.Liquid;

namespace Terraria.Physics;

/// <summary>World query output describing liquid touching an entity hitbox.</summary>
public readonly record struct SpatialLiquidContact(
    LiquidContactFlags Flags,
    byte Amount)
{
  public LiquidContactComponent ToComponent()
  {
    return new LiquidContactComponent(Flags, Amount);
  }
}
