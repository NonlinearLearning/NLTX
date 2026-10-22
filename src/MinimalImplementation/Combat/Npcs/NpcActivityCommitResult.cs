using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Despawned snapshots require packet 23, with no death/drop contribution.</summary>
public sealed record NpcActivityCommitResult(
    OwnerCommitResult Result, NpcView? Npc, bool Despawned,
    IReadOnlyList<byte> NearbyPlayerSlots);
