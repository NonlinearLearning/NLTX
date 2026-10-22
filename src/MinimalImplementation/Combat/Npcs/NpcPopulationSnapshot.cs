using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// Complete detached population of one NPC manager, in slot order. Capture and
/// Validate require the caller's exclusive owner window; this value is not a lock.
/// </summary>
public sealed class NpcPopulationSnapshot {
  internal NpcPopulationSnapshot(
      NpcManager owner, long worldScope, int slotCapacity,
      long managerRevision, CommitVersion commitVersion,
      IReadOnlyList<NpcPopulationEntry> entries) {
    Owner = owner;
    WorldScope = worldScope;
    SlotCapacity = slotCapacity;
    ManagerRevision = managerRevision;
    CommitVersion = commitVersion;
    Entries = Array.AsReadOnly(entries.ToArray());
  }

  public long WorldScope { get; }

  public int SlotCapacity { get; }

  public long ManagerRevision { get; }

  public CommitVersion CommitVersion { get; }

  public IReadOnlyList<NpcPopulationEntry> Entries { get; }

  internal NpcManager Owner { get; }
}
