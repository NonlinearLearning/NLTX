using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.Crafting;

/// <summary>
/// Combines world access and chest-lease checks for remote crafting under the shared state gate.
/// The unused-or-same-session lease rule is a historical compatibility choice, not verified
/// Version4 behavior.
/// </summary>
public sealed class LeaseAwareCraftingChestAccessPolicy : ICraftingChestAccessPolicy
{
  private readonly ChestLeaseStore _leases;
  private readonly IChestAccessPolicy _worldAccess;

  public LeaseAwareCraftingChestAccessPolicy(
    ChestLeaseStore leases,
    IChestAccessPolicy worldAccess,
    ServerStateCommitGate commitGate)
  {
    ArgumentNullException.ThrowIfNull(leases);
    ArgumentNullException.ThrowIfNull(worldAccess);
    ArgumentNullException.ThrowIfNull(commitGate);
    if (!ReferenceEquals(leases.CommitGate, commitGate))
    {
      throw new ArgumentException("The lease store and crafting policy must share one commit gate.", nameof(commitGate));
    }

    _leases = leases;
    _worldAccess = worldAccess;
    CommitGate = commitGate;
  }

  public ServerStateCommitGate CommitGate { get; }

  public bool CanUseForCrafting(
    EntityUuid playerId,
    SessionEpoch epoch,
    WorldScopeId worldScope,
    ChestSnapshot chest,
    DateTimeOffset now)
  {
    ArgumentNullException.ThrowIfNull(chest);
    return chest.WorldScope == worldScope
      && _worldAccess.CanOpen(playerId, worldScope, chest)
      && _leases.CanUseForCrafting(chest, playerId, epoch, now);
  }
}
