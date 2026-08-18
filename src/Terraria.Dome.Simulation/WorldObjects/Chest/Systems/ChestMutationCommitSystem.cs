using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects.Chest.Commands;

namespace Terraria.Dome.Simulation.WorldObjects.Chest.Systems;

public sealed class ChestMutationCommitSystem
{
  public bool TryCreate(
    WorldGrid worldGrid,
    IDictionary<int, ChestComponent> chests,
    ChestIndexSystem indexSystem,
    ChestCreateCommand command,
    out ChestComponent? chest)
  {
    ArgumentNullException.ThrowIfNull(worldGrid);
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(indexSystem);
    chest = null;
    if (command.Sequence < 0 || command.ChestId <= 0 ||
        !worldGrid.Contains(command.TileX, command.TileY) || chests.ContainsKey(command.ChestId))
    {
      return false;
    }

    ChestComponent createdChest = new(command.ChestId, command.TileX, command.TileY);
    if (!indexSystem.CanAdd(createdChest))
    {
      return false;
    }

    chests.Add(createdChest.ChestId, createdChest);
    if (!indexSystem.TryAdd(createdChest))
    {
      _ = chests.Remove(createdChest.ChestId);
      return false;
    }

    chest = createdChest;
    return true;
  }

  public bool TryDestroy(
    IDictionary<int, ChestComponent> chests,
    ChestIndexSystem indexSystem,
    ChestDestroyCommand command)
  {
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(indexSystem);
    if (command.Sequence < 0 || command.ChestId <= 0 ||
        !chests.TryGetValue(command.ChestId, out ChestComponent? chest) ||
        chest.Opener is not null || !AreSlotsEmpty(chest) ||
        !indexSystem.TryGetChestId(chest.TileX, chest.TileY, out int indexedChestId) ||
        indexedChestId != chest.ChestId)
    {
      return false;
    }

    _ = chests.Remove(chest.ChestId);
    return indexSystem.TryRemove(chest);
  }

  public bool TryRestore(
    WorldGrid worldGrid,
    IDictionary<int, ChestComponent> chests,
    ChestIndexSystem indexSystem,
    ChestComponent chest,
    ChestCreateCommand command)
  {
    ArgumentNullException.ThrowIfNull(chest);
    if (chest.ChestId != command.ChestId || chest.TileX != command.TileX ||
        chest.TileY != command.TileY)
    {
      return false;
    }

    return TryAdd(worldGrid, chests, indexSystem, chest, command.Sequence);
  }

  private static bool AreSlotsEmpty(ChestComponent chest)
  {
    for (int slot = 0; slot < ChestComponent.SlotCount; slot++)
    {
      if (!chest.GetSlot(slot).IsEmpty)
      {
        return false;
      }
    }

    return true;
  }

  private static bool TryAdd(
    WorldGrid worldGrid,
    IDictionary<int, ChestComponent> chests,
    ChestIndexSystem indexSystem,
    ChestComponent chest,
    long sequence)
  {
    ArgumentNullException.ThrowIfNull(worldGrid);
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(indexSystem);
    if (sequence < 0 || chest.ChestId <= 0 || !worldGrid.Contains(chest.TileX, chest.TileY) ||
        chests.ContainsKey(chest.ChestId) || !indexSystem.CanAdd(chest))
    {
      return false;
    }

    chests.Add(chest.ChestId, chest);
    if (!indexSystem.TryAdd(chest))
    {
      _ = chests.Remove(chest.ChestId);
      return false;
    }

    return true;
  }
}
