using System;

namespace Terraria.Dome.Simulation.Projectile;

public sealed class ProjectileIdentityAllocator
{
  private int _nextIdentity;

  public ProjectileIdentityAllocator(int nextIdentity = 1)
  {
    if (nextIdentity <= 0 || nextIdentity >= int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(nextIdentity));
    }

    _nextIdentity = nextIdentity;
  }

  public int NextIdentity => _nextIdentity;

  public int Allocate()
  {
    if (_nextIdentity >= int.MaxValue)
    {
      throw new InvalidOperationException("Projectile identity space is exhausted.");
    }

    int identity = _nextIdentity;
    _nextIdentity++;
    return identity;
  }

  public void Restore(int nextIdentity)
  {
    if (nextIdentity < _nextIdentity || nextIdentity <= 0 || nextIdentity >= int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(nextIdentity));
    }

    _nextIdentity = nextIdentity;
  }
}
