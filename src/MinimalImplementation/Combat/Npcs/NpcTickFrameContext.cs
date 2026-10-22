using System.Numerics;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Immutable shared P02/P03 facts for one NPC AI phase.</summary>
public sealed class NpcTickFrameContext {
  public NpcTickFrameContext(
      ulong worldEpoch,
      CommitVersion worldCommitVersion,
      CommitVersion playerSnapshotRevision,
      IReadOnlyList<NpcTargetPlayer> players) {
    ArgumentNullException.ThrowIfNull(players);
    if (worldEpoch == 0) {
      throw new ArgumentOutOfRangeException(nameof(worldEpoch));
    }
    if (!worldCommitVersion.IsValid) {
      throw new ArgumentOutOfRangeException(nameof(worldCommitVersion));
    }
    if (!playerSnapshotRevision.IsValid) {
      throw new ArgumentOutOfRangeException(nameof(playerSnapshotRevision));
    }
    NpcTargetPlayer[] playerSnapshot = players.ToArray();
    var slots = new HashSet<byte>();
    foreach (NpcTargetPlayer player in playerSnapshot) {
      if (player is null || !slots.Add(player.Slot) ||
          !HasFiniteBounds(player.Position, player.Width, player.Height) ||
          (player.TankPet is { } pet && !HasFiniteBounds(pet.Position, pet.Width, pet.Height))) {
        throw new ArgumentException(
            "The player snapshot contains duplicate slots or invalid spatial facts.",
            nameof(players));
      }
    }

    WorldEpoch = worldEpoch;
    WorldCommitVersion = worldCommitVersion;
    PlayerSnapshotRevision = playerSnapshotRevision;
    Players = Array.AsReadOnly(playerSnapshot);
  }

  public ulong WorldEpoch { get; }

  public CommitVersion WorldCommitVersion { get; }

  public CommitVersion PlayerSnapshotRevision { get; }

  public IReadOnlyList<NpcTargetPlayer> Players { get; }

  private static bool HasFiniteBounds(Vector2 position, int width, int height) {
    return float.IsFinite(position.X) && float.IsFinite(position.Y) && width > 0 && height > 0;
  }
}
