using System;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Simulation.Items.Components;

public readonly record struct ItemWorldStateComponent(
  bool IsActive,
  int PickupDelayTicks,
  int SpawnSource,
  long LastOwnerRevision,
  long LastMergeTick,
  long Revision,
  int ReservedPlayerId = 255,
  int ReservationAgeTicks = -1)
{
  public const int UnreservedPlayerId = 255;

  public const int NoReservationAge = -1;

  public bool IsReserved => ReservedPlayerId != UnreservedPlayerId;

  public bool CanBePickedUpBy(PlayerHandle player)
  {
    if (!player.IsValid || ReservedPlayerId < 0 || ReservedPlayerId > UnreservedPlayerId)
    {
      return false;
    }

    return !IsReserved || ReservedPlayerId == player.Value;
  }

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

    return new ItemWorldStateComponent(
      isActive,
      0,
      0,
      0,
      -1,
      revision,
      UnreservedPlayerId,
      NoReservationAge);
  }
}
