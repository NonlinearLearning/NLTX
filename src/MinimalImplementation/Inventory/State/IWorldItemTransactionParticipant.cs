using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>A prepared World, Player, or Combat participant in a P04 transaction.</summary>
/// <remarks>
/// Prepare and Validate are read-only. Host calls the transaction in its exclusive ServerLoop
/// window; every participant validates before the first write. Commit must be synchronous,
/// perform no I/O, and have no remaining business rejection or allocation-dependent preparation.
/// A participant may not reenter another P04 transaction or change a P04 store.
/// </remarks>
public interface IWorldItemTransactionParticipant
{
  OwnerCommitResult Validate();
  void Commit();
}
