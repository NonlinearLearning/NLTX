using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items.Components;

namespace Terraria.Dome.Simulation.Items.Snapshots;

public sealed class EquipmentSnapshot
{
  public EquipmentSnapshot(
    PlayerHandle player,
    IReadOnlyList<ItemEquipmentStateComponent> slots,
    long revision)
  {
    ArgumentNullException.ThrowIfNull(slots);
    if (revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    Player = player;
    Slots = Array.AsReadOnly([.. slots]);
    Revision = revision;
  }

  public PlayerHandle Player { get; }

  public IReadOnlyList<ItemEquipmentStateComponent> Slots { get; }

  public long Revision { get; }
}
