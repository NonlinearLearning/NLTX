using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Validated local Arrow birth held between preparation and commit.</summary>
public sealed class ProjectileLocalArrowSpawnPlan
{
  internal ProjectileLocalArrowSpawnPlan(
      long managerRevision,
      ProjectileLocalArrowSpawnRequest request,
      ProjectileDefinition definition,
      int runtimeSlot,
      ProjectileArrowBirthLiquidFacts liquidFacts,
      ulong worldEpoch,
      CommitVersion worldCommitVersion,
      System.Numerics.Vector2 position,
      System.Numerics.Vector2 velocity)
  {
    ManagerRevision = managerRevision;
    Request = request;
    Definition = definition;
    RuntimeSlot = runtimeSlot;
    LiquidFacts = liquidFacts;
    WorldEpoch = worldEpoch;
    WorldCommitVersion = worldCommitVersion;
    Position = position;
    Velocity = velocity;
  }

  internal long ManagerRevision { get; }

  internal ProjectileLocalArrowSpawnRequest Request { get; }

  internal ProjectileDefinition Definition { get; }

  internal int RuntimeSlot { get; }

  internal ProjectileArrowBirthLiquidFacts LiquidFacts { get; }

  internal ulong WorldEpoch { get; }

  internal CommitVersion WorldCommitVersion { get; }

  internal System.Numerics.Vector2 Position { get; }

  internal System.Numerics.Vector2 Velocity { get; }
}
