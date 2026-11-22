using System;

namespace Terraria.Projectile;

/// <summary>Lifecycle state for one projectile instance.</summary>
public readonly record struct ProjectileDispositionStateComponent
{
  public ProjectileDispositionStateComponent(ProjectileDisposition disposition)
  {
    if (!Enum.IsDefined(disposition))
    {
      throw new ArgumentOutOfRangeException(nameof(disposition));
    }

    Disposition = disposition;
  }

  public ProjectileDisposition Disposition { get; }

  public bool IsActive => Disposition == ProjectileDisposition.Active;
}

public enum ProjectileDisposition : byte
{
  Active = 0,
  PendingTerminate = 1,
  Terminated = 2
}
