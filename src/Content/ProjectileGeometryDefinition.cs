namespace Terraria.Content;

public sealed record ProjectileGeometryDefinition(
  int Width,
  int Height,
  float Scale,
  bool TileCollide,
  bool CorrectSlopeCollision)
{
  public bool IgnoreWater { get; init; }

  public float OwnerHitCheckDistance { get; init; }
}
