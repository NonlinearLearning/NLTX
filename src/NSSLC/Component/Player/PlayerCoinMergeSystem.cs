namespace Terraria.Player;

public sealed class PlayerCoinMergeSystem
{
  private const int CoinInventoryEnd = 54;
  private const int CopperCoinType = 71;
  private const int PlatinumCoinType = 73;

  private readonly HashSet<Guid> _acceptedCommandIds = [];
  private readonly PlayerInventorySlotsComponent _inventory;
  private readonly IPlayerInventoryItemQuery _itemQuery;
  private readonly IPlayerInventoryCoinMergePort _commitPort;

  public PlayerCoinMergeSystem(
    PlayerInventorySlotsComponent inventory,
    IPlayerInventoryItemQuery itemQuery,
    IPlayerInventoryCoinMergePort commitPort)
  {
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(itemQuery);
    ArgumentNullException.ThrowIfNull(commitPort);

    _inventory = inventory;
    _itemQuery = itemQuery;
    _commitPort = commitPort;
  }

  public PlayerCoinMergeResult Merge(in PlayerCoinMergeCommand command)
  {
    if (command.CommandId == Guid.Empty)
    {
      return PlayerCoinMergeResult.Rejected(
        PlayerCoinMergeRejectionReason.EmptyCommand);
    }

    if ((uint)command.SourceSlotIndex >= CoinInventoryEnd ||
      command.SourceSlotIndex >= _inventory.MainInventorySlots.Length)
    {
      return PlayerCoinMergeResult.Rejected(
        PlayerCoinMergeRejectionReason.InvalidSourceSlot);
    }

    if (_acceptedCommandIds.Contains(command.CommandId))
    {
      return PlayerCoinMergeResult.Rejected(
        PlayerCoinMergeRejectionReason.DuplicateCommand,
        command.SourceSlotIndex);
    }

    int currentSlotIndex = command.SourceSlotIndex;
    int appliedStepCount = 0;
    for (int step = 0; step <= CoinInventoryEnd; step++)
    {
      if (!TryCreatePlan(
        command.CommandId,
        currentSlotIndex,
        out PlayerCoinMergePlan plan,
        out PlayerCoinMergeRejectionReason rejectionReason))
      {
        if (appliedStepCount == 0)
        {
          return PlayerCoinMergeResult.Rejected(
            rejectionReason,
            currentSlotIndex);
        }

        _acceptedCommandIds.Add(command.CommandId);
        return new PlayerCoinMergeResult(
          Applied: true,
          AppliedStepCount: appliedStepCount,
          TerminalSlotIndex: currentSlotIndex,
          RejectionReason: rejectionReason);
      }

      if (!_commitPort.TryApply(plan))
      {
        if (appliedStepCount == 0)
        {
          return PlayerCoinMergeResult.Rejected(
            PlayerCoinMergeRejectionReason.CommitPortRejected,
            currentSlotIndex);
        }

        _acceptedCommandIds.Add(command.CommandId);
        return new PlayerCoinMergeResult(
          Applied: true,
          AppliedStepCount: appliedStepCount,
          TerminalSlotIndex: currentSlotIndex,
          RejectionReason: PlayerCoinMergeRejectionReason.CommitPortRejected);
      }

      appliedStepCount++;
      if (!plan.HasDestination)
      {
        _acceptedCommandIds.Add(command.CommandId);
        return new PlayerCoinMergeResult(
          Applied: true,
          AppliedStepCount: appliedStepCount,
          TerminalSlotIndex: currentSlotIndex,
          RejectionReason: PlayerCoinMergeRejectionReason.None);
      }

      currentSlotIndex = plan.DestinationSlotIndex;
    }

    _acceptedCommandIds.Add(command.CommandId);
    return new PlayerCoinMergeResult(
      Applied: true,
      AppliedStepCount: appliedStepCount,
      TerminalSlotIndex: currentSlotIndex,
      RejectionReason: PlayerCoinMergeRejectionReason.RecursionLimitExceeded);
  }

  public void ResetForLifecycle()
  {
    _acceptedCommandIds.Clear();
  }

  private bool TryCreatePlan(
    Guid commandId,
    int sourceSlotIndex,
    out PlayerCoinMergePlan plan,
    out PlayerCoinMergeRejectionReason rejectionReason)
  {
    plan = default;
    rejectionReason = PlayerCoinMergeRejectionReason.None;

    ItemEntityRef sourceEntity =
      _inventory.MainInventorySlots[sourceSlotIndex];
    if (sourceEntity.IsEmpty ||
      !_itemQuery.TryGetItem(sourceEntity, out PlayerInventoryItemSnapshot source))
    {
      rejectionReason = PlayerCoinMergeRejectionReason.ItemLookupFailed;
      return false;
    }

    if (source.TypeId < CopperCoinType ||
      source.TypeId > PlatinumCoinType ||
      source.Stack != 100)
    {
      rejectionReason = PlayerCoinMergeRejectionReason.SourceIsNotUpgradeableCoin;
      return false;
    }

    int upgradedTypeId = source.TypeId + 1;
    for (int destinationSlotIndex = 0;
      destinationSlotIndex < CoinInventoryEnd;
      destinationSlotIndex++)
    {
      if (destinationSlotIndex == sourceSlotIndex)
      {
        continue;
      }

      ItemEntityRef destinationEntity =
        _inventory.MainInventorySlots[destinationSlotIndex];
      if (destinationEntity.IsEmpty ||
        !_itemQuery.TryGetItem(
          destinationEntity,
          out PlayerInventoryItemSnapshot destination))
      {
        continue;
      }

      if (destination.TypeId != upgradedTypeId ||
        destination.Stack >= destination.MaximumStack)
      {
        continue;
      }

      plan = new PlayerCoinMergePlan(
        commandId,
        sourceSlotIndex,
        destinationSlotIndex,
        sourceEntity,
        destinationEntity,
        upgradedTypeId,
        destination.Stack,
        destination.Stack + 1);
      return true;
    }

    plan = new PlayerCoinMergePlan(
      CommandId: commandId,
      SourceSlotIndex: sourceSlotIndex,
      DestinationSlotIndex: -1,
      SourceItem: sourceEntity,
      DestinationItem: ItemEntityRef.None,
      UpgradedSourceTypeId: upgradedTypeId,
      DestinationStackBefore: 0,
      DestinationStackAfter: 0);
    return true;
  }
}
