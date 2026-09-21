using System.Numerics;

namespace Terraria.Tiles.Interaction;

public readonly record struct TileHitEntry(
  int X,
  int Y,
  int Damage,
  TileHitKind Kind,
  int RemainingTicks,
  int CrackStyle,
  int AnimationTicks,
  Vector2 AnimationDirection)
{
  public static TileHitEntry Empty => new(
    0,
    0,
    0,
    TileHitKind.Unused,
    0,
    -1,
    0,
    Vector2.Zero);

  public bool IsActive => Kind != TileHitKind.Unused && RemainingTicks > 0;
}
