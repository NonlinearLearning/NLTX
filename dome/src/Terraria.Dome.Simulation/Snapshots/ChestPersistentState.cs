using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation;

public sealed class ChestPersistentState
{
  public ChestPersistentState(
    int chestId,
    int tileX,
    int tileY,
    IReadOnlyList<ItemStack> slots,
    long revision,
    string name = "",
    bool isLocked = false)
  {
    if (chestId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(chestId));
    }

    if (revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    ArgumentNullException.ThrowIfNull(slots);
    ArgumentNullException.ThrowIfNull(name);
    if (name.Length > WorldObjects.ChestComponent.MaximumNameLength)
    {
      throw new ArgumentException("A chest name is too long.", nameof(name));
    }

    if (slots.Count < WorldObjects.ChestComponent.SlotCount ||
        slots.Count > WorldObjects.ChestCapacityDefinition.AbsoluteMaximumItems)
    {
      throw new ArgumentException("A chest must contain between 40 and 200 slots.", nameof(slots));
    }

    ChestId = chestId;
    TileX = tileX;
    TileY = tileY;
    Slots = new SnapshotReadOnlyList<ItemStack>(slots);
    Revision = revision;
    Name = name;
    IsLocked = isLocked;
  }

  public int ChestId { get; }

  public string Name { get; }

  public long Revision { get; }

  public IReadOnlyList<ItemStack> Slots { get; }

  public int TileX { get; }

  public int TileY { get; }

  public bool IsLocked { get; }
}
