using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Adapter to the authoritative WorldOwner revision check.</summary>
public interface INpcTickWorldRevisionGuard {
  bool IsCurrentWorldRevision(ulong worldEpoch, CommitVersion version);
}
