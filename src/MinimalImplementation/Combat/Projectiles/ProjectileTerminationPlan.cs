using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed class ProjectileTerminationPlan
{
  internal ProjectileTerminationPlan(
    long managerRevision,
    Projectile projectile,
    long expectedEntityRevision,
    AuthenticatedCombatSender sender)
  {
    ManagerRevision = managerRevision;
    Projectile = projectile;
    ExpectedEntityRevision = expectedEntityRevision;
    Sender = sender;
  }

  internal long ManagerRevision { get; }

  internal Projectile Projectile { get; }

  internal long ExpectedEntityRevision { get; }

  internal AuthenticatedCombatSender Sender { get; }
}
