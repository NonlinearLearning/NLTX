namespace Terraria.Physics;

public struct CollisionPolicyComponent
{
  public CollisionPolicyComponent(
    bool collidesWithTiles,
    bool collidesWithEntities,
    bool ignoresLiquids)
  {
    CollidesWithTiles = collidesWithTiles;
    CollidesWithEntities = collidesWithEntities;
    IgnoresLiquids = ignoresLiquids;
  }

  public bool CollidesWithTiles;
  public bool CollidesWithEntities;
  public bool IgnoresLiquids;
}
