using System;

namespace Terraria.Projectile;

/// <summary>
/// Decoded packet-29 termination request. The owner is the wire value; a
/// server-side session policy may validate or replace it before submission.
/// </summary>
public readonly record struct ProjectileNetworkTerminateCommand
{
  public ProjectileNetworkTerminateCommand(int ownerSlot, int identity)
  {
    if ((uint)ownerSlot > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(ownerSlot));
    }

    if (identity < 0 || identity > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(identity));
    }

    OwnerSlot = ownerSlot;
    Identity = identity;
  }

  public int OwnerSlot { get; }

  public int Identity { get; }
}
