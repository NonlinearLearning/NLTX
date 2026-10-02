namespace Terraria.Player;

// status: implemented-isolated-core
// crossSubsystemOwner: item payload, effect rebuild, network, and persistence remain integration-review
public sealed class PlayerEquipmentCommitSystem
{
  private readonly HashSet<Guid> _acceptedCommandIds = [];
  private readonly PlayerEquipmentRelationComponent _equipment;

  public PlayerEquipmentCommitSystem(
    PlayerEquipmentRelationComponent equipment)
  {
    ArgumentNullException.ThrowIfNull(equipment);
    _equipment = equipment;
  }

  public PlayerEquipmentCommitResult Commit(
    in PlayerEquipmentCommitCommand command)
  {
    ItemEntityRef currentItem = ReadSlot(
      command.SlotKind,
      command.SlotIndex,
      out bool validSlotKind,
      out bool validSlotIndex);

    if (command.CommandId == Guid.Empty)
    {
      return PlayerEquipmentCommitResult.Rejected(
        command.SlotKind,
        command.SlotIndex,
        currentItem,
        _equipment.Revision,
        PlayerEquipmentCommitRejectionReason.EmptyCommand);
    }

    if (_acceptedCommandIds.Contains(command.CommandId))
    {
      return PlayerEquipmentCommitResult.Rejected(
        command.SlotKind,
        command.SlotIndex,
        currentItem,
        _equipment.Revision,
        PlayerEquipmentCommitRejectionReason.DuplicateCommand);
    }

    if (!validSlotKind)
    {
      return PlayerEquipmentCommitResult.Rejected(
        command.SlotKind,
        command.SlotIndex,
        currentItem,
        _equipment.Revision,
        PlayerEquipmentCommitRejectionReason.InvalidSlotKind);
    }

    if (!validSlotIndex)
    {
      return PlayerEquipmentCommitResult.Rejected(
        command.SlotKind,
        command.SlotIndex,
        currentItem,
        _equipment.Revision,
        PlayerEquipmentCommitRejectionReason.InvalidSlotIndex);
    }

    if (command.ExpectedRevision < -1)
    {
      return PlayerEquipmentCommitResult.Rejected(
        command.SlotKind,
        command.SlotIndex,
        currentItem,
        _equipment.Revision,
        PlayerEquipmentCommitRejectionReason.InvalidExpectedRevision);
    }

    if (command.ExpectedRevision >= 0 &&
      command.ExpectedRevision != _equipment.Revision)
    {
      return PlayerEquipmentCommitResult.Rejected(
        command.SlotKind,
        command.SlotIndex,
        currentItem,
        _equipment.Revision,
        PlayerEquipmentCommitRejectionReason.StaleExpectedRevision);
    }

    if (command.ExpectedCurrentItem.HasValue &&
      command.ExpectedCurrentItem.Value != currentItem)
    {
      return PlayerEquipmentCommitResult.Rejected(
        command.SlotKind,
        command.SlotIndex,
        currentItem,
        _equipment.Revision,
        PlayerEquipmentCommitRejectionReason.UnexpectedCurrentItem);
    }

    if (command.Item == currentItem)
    {
      return PlayerEquipmentCommitResult.Rejected(
        command.SlotKind,
        command.SlotIndex,
        currentItem,
        _equipment.Revision,
        PlayerEquipmentCommitRejectionReason.NoChange);
    }

    if (!command.Item.IsEmpty && ContainsItemExcept(
        command.Item,
        command.SlotKind,
        command.SlotIndex))
    {
      return PlayerEquipmentCommitResult.Rejected(
        command.SlotKind,
        command.SlotIndex,
        currentItem,
        _equipment.Revision,
        PlayerEquipmentCommitRejectionReason.ItemAlreadyEquipped);
    }

    long revisionBefore = _equipment.Revision;
    PlayerEquipmentCommitPlan plan = new(
      command.CommandId,
      command.SlotKind,
      command.SlotIndex,
      currentItem,
      command.Item,
      revisionBefore,
      revisionBefore + 1,
      EffectRebuildRequired: true);

    Apply(in plan);
    _acceptedCommandIds.Add(command.CommandId);
    return new PlayerEquipmentCommitResult(
      Applied: true,
      SlotKind: plan.SlotKind,
      SlotIndex: plan.SlotIndex,
      PreviousItem: plan.PreviousItem,
      CurrentItem: plan.NextItem,
      Revision: plan.RevisionAfter,
      EffectRebuildRequired: plan.EffectRebuildRequired,
      RejectionReason: PlayerEquipmentCommitRejectionReason.None);
  }

  public void ResetForLifecycle()
  {
    _acceptedCommandIds.Clear();
  }

  private void Apply(
    in PlayerEquipmentCommitPlan plan)
  {
    WriteSlot(plan.SlotKind, plan.SlotIndex, plan.NextItem);
    _equipment.Revision = plan.RevisionAfter;
  }

  private ItemEntityRef ReadSlot(
    PlayerEquipmentSlotKind slotKind,
    int slotIndex,
    out bool validSlotKind,
    out bool validSlotIndex)
  {
    validSlotKind = Enum.IsDefined(slotKind);
    int slotCount = GetSlotCount(slotKind, validSlotKind);
    validSlotIndex = slotIndex >= 0 && slotIndex < slotCount;
    if (!validSlotKind || !validSlotIndex)
    {
      return ItemEntityRef.None;
    }

    return GetSlots(slotKind)[slotIndex];
  }

  private bool ContainsItemExcept(
    ItemEntityRef item,
    PlayerEquipmentSlotKind slotKind,
    int slotIndex)
  {
    foreach (PlayerEquipmentSlotKind candidateKind in Enum.GetValues<PlayerEquipmentSlotKind>())
    {
      ItemEntityRef[] slots = GetSlots(candidateKind);
      for (int index = 0; index < slots.Length; index++)
      {
        if (candidateKind == slotKind && index == slotIndex)
        {
          continue;
        }

        if (slots[index] == item)
        {
          return true;
        }
      }
    }

    return false;
  }

  private ItemEntityRef[] GetSlots(
    PlayerEquipmentSlotKind slotKind)
  {
    return slotKind switch
    {
      PlayerEquipmentSlotKind.Armor => _equipment.ArmorSlots,
      PlayerEquipmentSlotKind.Dye => _equipment.DyeSlots,
      PlayerEquipmentSlotKind.MiscEquipment => _equipment.MiscEquipmentSlots,
      PlayerEquipmentSlotKind.MiscDye => _equipment.MiscDyeSlots,
      _ => throw new ArgumentOutOfRangeException(nameof(slotKind)),
    };
  }

  private static int GetSlotCount(
    PlayerEquipmentSlotKind slotKind,
    bool validSlotKind)
  {
    if (!validSlotKind)
    {
      return 0;
    }

    return slotKind switch
    {
      PlayerEquipmentSlotKind.Armor =>
        PlayerEquipmentRelationComponent.ArmorSlotCount,
      PlayerEquipmentSlotKind.Dye =>
        PlayerEquipmentRelationComponent.DyeSlotCount,
      PlayerEquipmentSlotKind.MiscEquipment =>
        PlayerEquipmentRelationComponent.MiscEquipmentSlotCount,
      PlayerEquipmentSlotKind.MiscDye =>
        PlayerEquipmentRelationComponent.MiscDyeSlotCount,
      _ => 0,
    };
  }

  private void WriteSlot(
    PlayerEquipmentSlotKind slotKind,
    int slotIndex,
    ItemEntityRef item)
  {
    GetSlots(slotKind)[slotIndex] = item;
  }
}
