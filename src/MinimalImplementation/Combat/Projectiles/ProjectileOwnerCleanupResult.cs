using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed class ProjectileOwnerCleanupResult
{
  public ProjectileOwnerCleanupResult(
    OwnerCommitResult result,
    IReadOnlyCollection<ProjectileView> removed,
    OwnerCommitResult? trackerCleanup = null,
    OwnerCommitResult? trackerSessionCleanup = null)
  {
    ArgumentNullException.ThrowIfNull(removed);
    Result = result;
    Removed = Array.AsReadOnly(new List<ProjectileView>(removed).ToArray());
    TrackerCleanup = trackerCleanup;
    TrackerSessionCleanup = trackerSessionCleanup;
  }

  public OwnerCommitResult Result { get; }

  public IReadOnlyList<ProjectileView> Removed { get; }

  public OwnerCommitResult? TrackerCleanup { get; }

  public OwnerCommitResult? TrackerSessionCleanup { get; }
}
