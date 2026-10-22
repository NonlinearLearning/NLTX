using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.Crafting;

/// <summary>Applies the current world lock and chest-user rules to one crafting request.</summary>
public interface ICraftingChestAccessPolicy
{
  /// <summary>Uses the same commit boundary as the chest owner for local access and lease state.</summary>
  ServerStateCommitGate CommitGate { get; }

  bool CanUseForCrafting(
    EntityUuid playerId,
    SessionEpoch epoch,
    WorldScopeId worldScope,
    ChestSnapshot chest,
    DateTimeOffset now);
}
