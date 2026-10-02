using System;

namespace Terraria.Projectile;

public struct ProjectileHitImmunityStateComponent
{
  public ProjectileHitImmunityStateComponent()
  {
    LocalNpcImmunityTicks = Array.Empty<int>();
    PlayerImmunityTicks = new int[255];
    RestrikeDelayTicks = 0;
  }

  public ProjectileHitImmunityStateComponent(
    int npcCapacity,
    int playerCapacity = 255)
  {
    if (npcCapacity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(npcCapacity));
    }

    if (playerCapacity < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(playerCapacity));
    }

    LocalNpcImmunityTicks = new int[npcCapacity];
    PlayerImmunityTicks = new int[playerCapacity];
    RestrikeDelayTicks = 0;
  }

  public int[] LocalNpcImmunityTicks;
  public int[] PlayerImmunityTicks;
  public int RestrikeDelayTicks;
}
