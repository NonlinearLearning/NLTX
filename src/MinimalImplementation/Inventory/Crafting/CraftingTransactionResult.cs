using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.Crafting;

public sealed record CraftingTransactionResult
{
  public bool Approved { get; }
  public OwnerCommitStatus Status { get; }
  public ProtocolErrorCode? ErrorCode { get; }
  public string? Detail { get; }
  public CraftingMaterialPreparationStatus? PreparationStatus { get; }
  public IReadOnlyList<ChestSnapshot> UpdatedChests { get; }

  internal CraftingTransactionResult(
    bool approved,
    OwnerCommitStatus status,
    ProtocolErrorCode? errorCode,
    string? detail,
    CraftingMaterialPreparationStatus? preparationStatus,
    IReadOnlyCollection<ChestSnapshot> updatedChests)
  {
    ArgumentNullException.ThrowIfNull(updatedChests);
    Approved = approved;
    Status = status;
    ErrorCode = errorCode;
    Detail = detail;
    PreparationStatus = preparationStatus;
    UpdatedChests = Array.AsReadOnly(updatedChests.ToArray());
  }
}
