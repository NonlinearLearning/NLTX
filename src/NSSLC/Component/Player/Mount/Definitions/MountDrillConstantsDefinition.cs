using System.Numerics;

namespace Terraria.Player.Mount;

public readonly record struct MountDrillConstantsDefinition(
  Vector2 DiodePointOne,
  Vector2 DiodePointTwo,
  int TextureWidth,
  float RotationChange,
  int PickPower,
  int PickTime,
  int BeamsAtOnce,
  float MaxLength)
{
  public static MountDrillConstantsDefinition Version4 => new(
    new Vector2(36f, -6f),
    new Vector2(36f, 8f),
    80,
    MathF.PI / 60f,
    210,
    1,
    2,
    48f);
}
