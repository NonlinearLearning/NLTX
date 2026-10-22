namespace MinimalImplementation.Combat.Projectiles;

public sealed class ProjectilePortalTerminationPlan
{
  internal ProjectilePortalTerminationPlan(
    long managerRevision,
    Projectile projectile,
    long expectedEntityRevision,
    ushort targetOwner,
    byte portalIndex)
  {
    ManagerRevision = managerRevision;
    Projectile = projectile;
    ExpectedEntityRevision = expectedEntityRevision;
    TargetOwner = targetOwner;
    PortalIndex = portalIndex;
  }

  internal long ManagerRevision { get; }

  internal Projectile Projectile { get; }

  internal long ExpectedEntityRevision { get; }

  public ushort TargetOwner { get; }

  public byte PortalIndex { get; }
}
