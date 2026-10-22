using System;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Combat.Projectiles;
using MinimalImplementation.Contracts;
using MinimalImplementation.Persistence;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Adapts the migrated projectile AI and lifecycle rules to the production server tick contract.
/// </summary>
public sealed class ProjectileServerTickPhaseOwner : IServerTickPhaseOwner
{
  private readonly ProjectileManager _projectiles;
  private readonly ProjectileAiService _ai;

  public ProjectileServerTickPhaseOwner(ProjectileManager projectiles)
  {
    ArgumentNullException.ThrowIfNull(projectiles);
    _projectiles = projectiles;
    _ai = new ProjectileAiService(projectiles);
  }

  public ServerTickPhase Phase => ServerTickPhase.Projectile;

  public object TransactionIdentity => this;

  public void Advance(long gameTickIndex)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(gameTickIndex);

    foreach (ProjectileView current in _projectiles.Capture())
    {
      if (current.TimeLeft <= 0)
      {
        RetireExpired(current.Entity);
        continue;
      }

      ProjectileAiTickResult tick = _ai.Tick(new ProjectileAiTickRequest(current.Entity));
      EnsureApplied(tick.Result, "The projectile AI tick");
      if (!tick.Expired)
      {
        continue;
      }

      ProjectileView updated = tick.Projectile
        ?? throw new InvalidOperationException(
          "An applied projectile expiry tick returned no updated snapshot.");
      RetireExpired(updated.Entity);
    }
  }

  private void RetireExpired(CombatEntityIdentity identity)
  {
    ProjectileTerminationCommitResult result = _projectiles.RetireExpired(identity);
    EnsureApplied(result.Result, "The expired projectile retirement");
  }

  private static void EnsureApplied(OwnerCommitResult result, string operation)
  {
    if (result.Status == OwnerCommitStatus.Applied)
    {
      return;
    }

    string detail = result.Detail ?? result.ErrorCode?.ToString() ?? result.Status.ToString();
    throw new InvalidOperationException($"{operation} was rejected: {detail}");
  }
}
