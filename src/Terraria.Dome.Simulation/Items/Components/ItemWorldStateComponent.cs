using System;

namespace Terraria.Dome.Simulation.Items.Components;

public readonly record struct ItemWorldStateComponent(
  bool IsActive,
  int PickupDelayTicks,
  int SpawnSource,
  long LastOwnerRevision,
  long LastMergeTick,
  long Revision)
{
  public static ItemWorldStateComponent Active(int spawnSource, long revision = 1)
  {
    return new ItemWorldStateComponent(true, 0, spawnSource, 0, -1, revision);
  }

  public static ItemWorldStateComponent FromReplicationSnapshot(bool isActive, long revision)
  {
    if (revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    return new ItemWorldStateComponent(isActive, 0, 0, 0, -1, revision);
  }
}
