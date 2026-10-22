namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Validated intent held between the read-only and commit phases.</summary>
public sealed class ProjectileSyncPlan
{
  internal ProjectileSyncPlan(
    long managerRevision,
    ProjectileSyncRequest request,
    ProjectileDefinition definition,
    byte owner,
    short? protocolUuid,
    int runtimeSlot,
    Projectile? existing,
    long? expectedEntityRevision,
    Projectile? evicted)
  {
    ManagerRevision = managerRevision;
    Request = request;
    Definition = definition;
    Owner = owner;
    ProtocolUuid = protocolUuid;
    RuntimeSlot = runtimeSlot;
    Existing = existing;
    ExpectedEntityRevision = expectedEntityRevision;
    Evicted = evicted;
  }

  internal long ManagerRevision { get; }

  internal ProjectileSyncRequest Request { get; }

  internal ProjectileDefinition Definition { get; }

  internal byte Owner { get; }

  internal short? ProtocolUuid { get; }

  internal int RuntimeSlot { get; }

  internal Projectile? Existing { get; }

  internal long? ExpectedEntityRevision { get; }

  internal Projectile? Evicted { get; }
}
