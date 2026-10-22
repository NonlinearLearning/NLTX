using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>One successful AI or Activity commit in actual execution order.</summary>
public sealed class NpcTickCommitRecord {
  public enum CommitStage {
    Ai,
    Activity
  }

  internal NpcTickCommitRecord(
      NpcView snapshot, CommitStage stage, IReadOnlyList<byte>? nearbyPlayerSlots = null) {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.CommitVersion.IsValid) {
      throw new ArgumentException(
          "A tick commit record requires a committed snapshot.", nameof(snapshot));
    }

    Entity = snapshot.Entity;
    Slot = snapshot.Slot;
    Revision = snapshot.Revision;
    CommitVersion = snapshot.CommitVersion;
    Snapshot = snapshot;
    Stage = stage;
    NearbyPlayerSlots = stage == CommitStage.Activity
        ? Array.AsReadOnly((nearbyPlayerSlots ?? Array.Empty<byte>()).ToArray())
        : null;
  }

  public CombatEntityIdentity Entity { get; }

  public short Slot { get; }

  public long Revision { get; }

  public CommitVersion CommitVersion { get; }

  public CommitStage Stage { get; }

  public NpcView Snapshot { get; }

  /// <summary>Slots produced by Activity; null for an AI commit.</summary>
  public IReadOnlyList<byte>? NearbyPlayerSlots { get; }
}
