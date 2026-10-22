using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.Protocol;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.Crafting;

/// <summary>Validates remote chest materials and atomically commits their consumption.</summary>
public sealed class CraftingTransactionCoordinator
{
  private readonly ChestStore _chests;
  private readonly CraftingMaterialPlanner _planner;
  private readonly ICraftingChestAccessPolicy _accessPolicy;
  private readonly ServerStateCommitGate _commitGate;

  public CraftingTransactionCoordinator(
    ChestStore chests,
    CraftingMaterialPlanner planner,
    ICraftingChestAccessPolicy accessPolicy)
  {
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(planner);
    ArgumentNullException.ThrowIfNull(accessPolicy);
    if (!ReferenceEquals(chests.CommitGate, accessPolicy.CommitGate))
    {
      throw new ArgumentException("The chest and crafting access policy must share one commit gate.", nameof(accessPolicy));
    }

    _chests = chests;
    _planner = planner;
    _accessPolicy = accessPolicy;
    _commitGate = chests.CommitGate;
  }

  public CraftingTransactionResult Execute(
    CraftingModule11Request request,
    EntityUuid playerId,
    SessionEpoch epoch,
    WorldScopeId worldScope,
    DateTimeOffset now)
  {
    ArgumentNullException.ThrowIfNull(request);
    lock (_commitGate.SyncRoot)
    {
      return ExecuteLocked(request, playerId, epoch, worldScope, now);
    }
  }

  private CraftingTransactionResult ExecuteLocked(
    CraftingModule11Request request,
    EntityUuid playerId,
    SessionEpoch epoch,
    WorldScopeId worldScope,
    DateTimeOffset now)
  {
    if (!playerId.IsValid || !epoch.IsValid || !worldScope.IsValid)
    {
      return Rejected(
        ProtocolErrorCode.StaleEpoch,
        "The crafting request is not bound to a current player session and world.");
    }

    var accessibleChests = new List<ChestSnapshot>();
    var seenChestIds = new HashSet<int>();
    foreach (int requestedChestId in request.ChestIds)
    {
      if (requestedChestId < 0
        || requestedChestId >= ContainerContentFacts.ChestIndexCount
        || !seenChestIds.Add(requestedChestId))
      {
        continue;
      }

      var chestId = new ChestId((ushort)requestedChestId);
      if (!_chests.TryCapture(chestId, out ChestSnapshot? chest)
        || chest is null
        || chest.WorldScope != worldScope
        || !_accessPolicy.CanUseForCrafting(playerId, epoch, worldScope, chest, now))
      {
        continue;
      }

      accessibleChests.Add(chest);
    }

    CraftingMaterialPreparation preparation = _planner.Prepare(request, accessibleChests);
    if (!preparation.IsPrepared)
    {
      return new CraftingTransactionResult(
        false,
        OwnerCommitStatus.Rejected,
        preparation.Status == CraftingMaterialPreparationStatus.UnsupportedMaterial
          ? ProtocolErrorCode.Forbidden
          : ProtocolErrorCode.Conflict,
        preparation.Status == CraftingMaterialPreparationStatus.InsufficientMaterials
          ? "The selected chests do not contain all requested materials."
          : "The crafting material request or chest snapshot is invalid.",
        preparation.Status,
        Array.Empty<ChestSnapshot>());
    }

    if (preparation.ChestMutations.Count == 0)
    {
      return new CraftingTransactionResult(
        true,
        OwnerCommitStatus.Applied,
        null,
        null,
        preparation.Status,
        Array.Empty<ChestSnapshot>());
    }

    ChestBatchCommitResult commit = _chests.ApplyItemsBatch(preparation.ChestMutations);
    if (!commit.IsApplied)
    {
      return new CraftingTransactionResult(
        false,
        commit.Status,
        commit.ErrorCode,
        commit.Detail,
        preparation.Status,
        Array.Empty<ChestSnapshot>());
    }

    return new CraftingTransactionResult(
      true,
      OwnerCommitStatus.Applied,
      null,
      null,
      preparation.Status,
      commit.Chests);
  }

  private static CraftingTransactionResult Rejected(ProtocolErrorCode code, string detail)
  {
    return new CraftingTransactionResult(
      false,
      OwnerCommitStatus.Rejected,
      code,
      detail,
      null,
      Array.Empty<ChestSnapshot>());
  }
}
