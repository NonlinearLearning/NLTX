using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;

namespace Terraria.Dome.Simulation.Npc.Snapshots;

public readonly record struct NpcSpawnCandidate(
  SpawnNpcCommand Command,
  bool IsOccupied,
  bool IsProtectedSlot);

public sealed class NpcSpawnSnapshot
{
  public NpcSpawnSnapshot(
    IReadOnlyList<NpcSpawnCandidate> candidates,
    int activeNpcCount,
    int maximumNpcCount,
    int protectedSlotCount,
    IReadOnlySet<int> existingReplicationIds)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    ArgumentNullException.ThrowIfNull(existingReplicationIds);
    if (activeNpcCount < 0 || maximumNpcCount < 0 || protectedSlotCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(activeNpcCount));
    }

    Candidates = candidates;
    ActiveNpcCount = activeNpcCount;
    MaximumNpcCount = maximumNpcCount;
    ProtectedSlotCount = protectedSlotCount;
    ExistingReplicationIds = existingReplicationIds;
  }

  public IReadOnlyList<NpcSpawnCandidate> Candidates { get; }
  public int ActiveNpcCount { get; }
  public int MaximumNpcCount { get; }
  public int ProtectedSlotCount { get; }
  public IReadOnlySet<int> ExistingReplicationIds { get; }
}
