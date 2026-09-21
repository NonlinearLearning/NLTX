using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct ChestSnapshot
{
  public ChestSnapshot(
    int chestId,
    int tileX,
    int tileY,
    PlayerHandle? opener,
    IReadOnlyList<ItemStack> slots,
    long revision,
    WorldSectionCoordinates section,
    bool isLocked)
  {
    ArgumentNullException.ThrowIfNull(slots);
    ChestId = chestId;
    TileX = tileX;
    TileY = tileY;
    Opener = opener;
    Slots = new SnapshotReadOnlyList<ItemStack>(slots);
    Revision = revision;
    Section = section;
    IsLocked = isLocked;
  }

  public int ChestId { get; }

  public int TileX { get; }

  public int TileY { get; }

  public PlayerHandle? Opener { get; }

  public IReadOnlyList<ItemStack> Slots { get; }

  public long Revision { get; }

  public WorldSectionCoordinates Section { get; }

  public bool IsLocked { get; }
}
