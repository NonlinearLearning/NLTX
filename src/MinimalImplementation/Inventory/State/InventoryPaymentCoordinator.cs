using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Rules;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>
/// Prepares and commits one wiring/material payment against the authoritative inventory.
/// </summary>
public sealed class InventoryPaymentCoordinator
{
  private readonly InventoryStore _inventories;
  private readonly ItemValidationPolicy _items;

  public InventoryPaymentCoordinator(InventoryStore inventories, ItemValidationPolicy items)
  {
    ArgumentNullException.ThrowIfNull(inventories);
    ArgumentNullException.ThrowIfNull(items);
    _inventories = inventories;
    _items = items;
  }

  public InventoryPaymentPreparation PreflightPayment(
    EntityUuid playerId,
    CommitVersion expectedRevision,
    int itemType,
    int count)
  {
    if (!playerId.IsValid || itemType <= 0 || count <= 0)
    {
      return InventoryPaymentPreparation.Rejected(
        ProtocolErrorCode.Forbidden,
        "The inventory payment identity, item type, or count is invalid.");
    }

    if (!_inventories.TryCapture(playerId, out InventorySnapshot? inventory)
      || inventory is null)
    {
      return InventoryPaymentPreparation.Rejected(
        ProtocolErrorCode.NotReady,
        "The player inventory is not available.");
    }

    if (inventory.Revision != expectedRevision)
    {
      return InventoryPaymentPreparation.Rejected(
        ProtocolErrorCode.Conflict,
        "The player inventory revision changed before payment preparation.");
    }

    int remaining = count;
    var mutations = new List<InventoryMutation>();
    foreach (InventorySlotSnapshot slot in inventory.Slots)
    {
      if (slot.Item.Type != itemType || slot.Item.IsEmpty)
      {
        continue;
      }

      int consumed = Math.Min(remaining, slot.Item.Stack);
      InventoryItemState updated = slot.Item.WithStack(slot.Item.Stack - consumed);
      if (!_items.IsValid(updated))
      {
        return InventoryPaymentPreparation.Rejected(
          ProtocolErrorCode.Forbidden,
          "The payment would produce an invalid inventory item.");
      }

      mutations.Add(new InventoryMutation(slot.SlotId, slot.Generation, updated));
      remaining -= consumed;
      if (remaining == 0)
      {
        break;
      }
    }

    if (remaining != 0)
    {
      return InventoryPaymentPreparation.Rejected(
        ProtocolErrorCode.Conflict,
        "The player inventory does not contain enough payment items.");
    }

    return InventoryPaymentPreparation.Prepared(
      new InventoryPaymentPlan(playerId, expectedRevision, itemType, count, mutations));
  }

  public OwnerCommitResult CommitPayment(
    InventoryPaymentPlan plan,
    out InventorySnapshot? snapshot)
  {
    ArgumentNullException.ThrowIfNull(plan);
    return _inventories.Apply(
      plan.PlayerId,
      plan.ExpectedRevision,
      plan.Mutations,
      out snapshot);
  }
}

public sealed record InventoryPaymentPlan(
  EntityUuid PlayerId,
  CommitVersion ExpectedRevision,
  int ItemType,
  int Count,
  IReadOnlyList<InventoryMutation> Mutations);

public sealed record InventoryPaymentPreparation(
  OwnerCommitResult Result,
  InventoryPaymentPlan? Plan)
{
  public bool IsPrepared => Result.Status == OwnerCommitStatus.Applied && Plan is not null;

  public static InventoryPaymentPreparation Prepared(InventoryPaymentPlan plan) =>
    new(OwnerCommitResult.Applied(plan.ExpectedRevision), plan);

  public static InventoryPaymentPreparation Rejected(ProtocolErrorCode code, string detail) =>
    new(OwnerCommitResult.Rejected(code, detail), null);
}
