using Terraria.Share.Liquid;

namespace Terraria.Share.Physics;

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
