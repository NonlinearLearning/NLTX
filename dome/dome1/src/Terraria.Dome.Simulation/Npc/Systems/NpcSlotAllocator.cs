using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcSlotCandidate(
  NpcHandle Handle,
  bool IsActive,
  bool CanBeReplaced,
  long Revision);

public readonly record struct NpcSlotSelection(NpcHandle Handle, bool ReusesExisting);

public sealed class NpcSlotAllocator
{
  public bool TrySelect(
    int maximumSlots,
    IReadOnlyCollection<NpcSlotCandidate> candidates,
    out NpcSlotSelection selection)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    selection = default;
    if (maximumSlots <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumSlots));
    }

    Dictionary<int, NpcSlotCandidate> candidatesBySlot = new(candidates.Count);
    foreach (NpcSlotCandidate candidate in candidates)
    {
      if (!candidate.Handle.IsValid || candidate.Handle.Value > maximumSlots ||
          candidate.Revision < 0 || !candidatesBySlot.TryAdd(candidate.Handle.Value, candidate))
      {
        return false;
      }
    }

    int slot = 1;
    while (true)
    {
      if (!candidatesBySlot.TryGetValue(slot, out NpcSlotCandidate candidate))
      {
        selection = new NpcSlotSelection(new NpcHandle(slot), ReusesExisting: false);
        return true;
      }

      if (!candidate.IsActive && candidate.Revision < long.MaxValue)
      {
        selection = new NpcSlotSelection(candidate.Handle, ReusesExisting: true);
        return true;
      }

      if (slot == maximumSlots)
      {
        break;
      }

      slot++;
    }

    slot = 1;
    while (true)
    {
      if (candidatesBySlot.TryGetValue(slot, out NpcSlotCandidate candidate) &&
          candidate.IsActive && candidate.CanBeReplaced && candidate.Revision < long.MaxValue)
      {
        selection = new NpcSlotSelection(candidate.Handle, ReusesExisting: true);
        return true;
      }

      if (slot == maximumSlots)
      {
        break;
      }

      slot++;
    }

    return false;
  }
}
