using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Host;

/// <summary>Commits the prepared tile change only after its drop slots are validated.</summary>
internal sealed class TileItemTransactionParticipant : IWorldItemTransactionParticipant {
  private readonly WorldTileMutationPreparation _tiles;

  public TileItemTransactionParticipant(WorldTileMutationPreparation tiles) {
    _tiles = tiles ?? throw new ArgumentNullException(nameof(tiles));
  }

  public OwnerCommitResult Validate() {
    WorldValidationResult validation = _tiles.Validate();
    return validation.IsValid
      ? OwnerCommitResult.Applied(_tiles.BaseVersion)
      : OwnerCommitResult.Rejected(ProtocolErrorCode.Conflict,
        "The prepared terrain changed before its item transaction.");
  }

  public void Commit() {
    _tiles.Commit();
  }
}
