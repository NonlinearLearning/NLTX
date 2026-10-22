using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>
/// Player-owned captured hitbox, encumbering/void facts and optional heal/mana/Nebula commit.
/// Validate must bind every fact to the same player/item and reject changed dependencies.
/// Commit applies the prepared effect and its projection exactly once, without I/O or rejection.
/// </summary>
public interface IWorldItemPickupPreparation : IWorldItemTransactionParticipant
{
  EntityHandle PlayerHandle { get; }
  SessionEpoch Epoch { get; }
  WorldItemHandle ItemHandle { get; }
  CommitVersion ItemRevision { get; }
  ulong WorldEpoch { get; }
  int Width { get; }
  int Height { get; }
  bool CanAcceptItem { get; }
  bool VoidVaultEnabled { get; }
  WorldItemPickupEffect Effect { get; }
  IReadOnlyList<SessionPacketProjection> Projections { get; }
}
