using Terraria.Share.Entities;
using Terraria.Share.Liquid;
using Terraria.Share.Movement;

namespace Terraria.Share.Physics;

/// <summary>State that the spatial owner is allowed to write at the commit barrier.</summary>
public readonly record struct SpatialCommit(
    LocationComponent Location,
    VelocityComponent Velocity,
    GroundingComponent Grounding,
    LiquidContactComponent LiquidContact,
    TileContactGeometry TileContact = default,
    System.Numerics.Vector2 IncomingVelocity = default,
    int Substep = 0)
{
  /// <summary>本步是否撞到瓦片；接触几何随提交一起交出来，避免下游反推。</summary>
  public bool HasTileContact => TileContact.HasContact;
}
