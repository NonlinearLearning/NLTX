using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public readonly record struct MinecartCustomizationValue(
  float MinecartTextureWidth,
  Vector2 WheelOffset,
  Vector2 MagnetOffset)
{
  public static MinecartCustomizationValue Default => new(
    50f,
    new Vector2(12f, 0f),
    new Vector2(25f, 26f));
}
