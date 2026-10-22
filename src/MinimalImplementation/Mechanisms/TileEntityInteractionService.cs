using Terraria.Items;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// Applies typed TileEntity payload changes at the P06 commit boundary.
/// Inventory payment, drops, tile changes and player anchors remain external owner effects.
/// </summary>
public sealed class TileEntityInteractionService
{
  private readonly TileEntityRegistry _registry;
  private readonly IMechanismItemFacts? _itemFacts;

  public TileEntityInteractionService(
    TileEntityRegistry registry,
    IMechanismItemFacts? itemFacts = null)
  {
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    _itemFacts = itemFacts;
  }

  public TileEntityInteractionResult InsertItem(
    MechanismEntityHandle handle,
    long expectedRevision,
    ItemState item)
  {
    if (!TryGetActive(handle, out TileEntityRuntimeState? state, out TileEntityInteractionResult? failure))
    {
      return failure!;
    }

    if (state.Definition.Type is not (1 or 4 or 6 or 8))
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidEntityType,
        "Only an item frame, weapons rack, food platter or display jar accepts one item.");
    }
    if (item.IsEmpty || !IsValidItem(item))
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidItem,
        "The inserted item must contain a positive stack and valid wire fields.");
    }

    ItemState[] items = state.Items.ToArray();
    items[0] = item;
    return CommitState(state, expectedRevision, items, state.NpcIndex, state.LogicCheck,
      state.LogicOn, state.Pose, state.AnchorItemType, replacedItem: state.Items[0]);
  }

  public TileEntityInteractionResult SetDisplayDollItem(
    MechanismEntityHandle handle,
    long expectedRevision,
    bool dye,
    int slot,
    ItemState item)
  {
    if (!TryGetActive(handle, out TileEntityRuntimeState? state, out TileEntityInteractionResult? failure))
    {
      return failure!;
    }
    if (state.Definition.Type != 3)
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidEntityType,
        "The display-doll command requires TileEntity type 3.");
    }
    if (slot < 0 || slot >= 9)
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidSlot,
        "A display-doll item slot must be between 0 and 8.");
    }
    if (!IsValidItem(item))
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidItem,
        "The display-doll item is outside the Version4 item bounds.");
    }

    int itemIndex = dye ? slot + 9 : slot;
    ItemState[] items = state.Items.ToArray();
    ItemState replaced = items[itemIndex];
    items[itemIndex] = item;
    return CommitState(state, expectedRevision, items, state.NpcIndex, state.LogicCheck,
      state.LogicOn, state.Pose, state.AnchorItemType, replaced);
  }

  public TileEntityInteractionResult SetDisplayDollPose(
    MechanismEntityHandle handle,
    long expectedRevision,
    byte pose)
  {
    if (!TryGetActive(handle, out TileEntityRuntimeState? state, out TileEntityInteractionResult? failure))
    {
      return failure!;
    }
    if (state.Definition.Type != 3)
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidEntityType,
        "The display-doll pose command requires TileEntity type 3.");
    }
    return CommitState(state, expectedRevision, state.Items, state.NpcIndex, state.LogicCheck,
      state.LogicOn, pose, state.AnchorItemType, replacedItem: null);
  }

  public TileEntityInteractionResult SetHatRackItem(
    MechanismEntityHandle handle,
    long expectedRevision,
    int slot,
    ItemState item)
  {
    if (!TryGetActive(handle, out TileEntityRuntimeState? state, out TileEntityInteractionResult? failure))
    {
      return failure!;
    }
    if (state.Definition.Type != 5)
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidEntityType,
        "The hat-rack command requires TileEntity type 5.");
    }
    if (slot is < 0 or > 3)
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidSlot,
        "A hat-rack slot must be 0 or 1 for items and 2 or 3 for dyes.");
    }
    if (!IsValidItem(item))
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidItem,
        "The hat-rack item is outside the Version4 item bounds.");
    }

    ItemState[] items = state.Items.ToArray();
    ItemState replaced = items[slot];
    items[slot] = item;
    return CommitState(state, expectedRevision, items, state.NpcIndex, state.LogicCheck,
      state.LogicOn, state.Pose, state.AnchorItemType, replaced);
  }

  public TileEntityInteractionResult SetLogicSensor(
    MechanismEntityHandle handle,
    long expectedRevision,
    byte logicCheck,
    bool on)
  {
    if (!TryGetActive(handle, out TileEntityRuntimeState? state, out TileEntityInteractionResult? failure))
    {
      return failure!;
    }
    if (state.Definition.Type != 2)
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidEntityType,
        "The logic-sensor update requires TileEntity type 2.");
    }
    if (logicCheck > 7)
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidPayload,
        "The Version4 logic sensor has check types 0 through 7.");
    }
    return CommitState(state, expectedRevision, state.Items, state.NpcIndex, logicCheck,
      on, state.Pose, state.AnchorItemType, replacedItem: null);
  }

  public TileEntityInteractionResult SetTrainingDummy(
    MechanismEntityHandle handle,
    long expectedRevision,
    short npcIndex)
  {
    if (!TryGetActive(handle, out TileEntityRuntimeState? state, out TileEntityInteractionResult? failure))
    {
      return failure!;
    }
    if (state.Definition.Type != 0)
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidEntityType,
        "The training-dummy update requires TileEntity type 0.");
    }
    if (npcIndex < -1)
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidPayload,
        "A training dummy NPC link must be -1 or a nonnegative NPC index.");
    }
    return CommitState(state, expectedRevision, state.Items, npcIndex, state.LogicCheck,
      state.LogicOn, state.Pose, state.AnchorItemType, replacedItem: null);
  }

  public TileEntityInteractionResult SetAnchorItem(
    MechanismEntityHandle handle,
    long expectedRevision,
    short itemType)
  {
    if (!TryGetActive(handle, out TileEntityRuntimeState? state, out TileEntityInteractionResult? failure))
    {
      return failure!;
    }
    if (state.Definition.Type is not (9 or 10))
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidEntityType,
        "An anchor item update requires TileEntity type 9 or 10.");
    }
    if (itemType < 0 || (itemType != 0 &&
        (_itemFacts is null
          ? itemType <= 0
          : state.Definition.Type == 9
            ? !_itemFacts.FitsKiteAnchor(itemType)
            : !_itemFacts.FitsCritterAnchor(itemType))))
    {
      return Rejected(handle, TileEntityInteractionStatus.InvalidItem,
        "The anchor item is not valid for this Version4 anchor type.");
    }
    return CommitState(state, expectedRevision, state.Items, state.NpcIndex, state.LogicCheck,
      state.LogicOn, state.Pose, itemType, replacedItem: null);
  }

  private bool TryGetActive(
    MechanismEntityHandle handle,
    out TileEntityRuntimeState? state,
    out TileEntityInteractionResult? failure)
  {
    if (!_registry.TryGet(handle, out state) || state is null)
    {
      failure = Rejected(handle, TileEntityInteractionStatus.Missing,
        "The TileEntity is not active in this world.");
      return false;
    }
    failure = null;
    return true;
  }

  private TileEntityInteractionResult CommitState(
    TileEntityRuntimeState state,
    long expectedRevision,
    IReadOnlyList<ItemState> items,
    short npcIndex,
    byte logicCheck,
    bool logicOn,
    byte pose,
    short anchorItemType,
    ItemState? replacedItem)
  {
    ItemState[] previousItems = state.Items.ToArray();
    if (state.Revision != expectedRevision)
    {
      return Rejected(state.Handle, TileEntityInteractionStatus.Stale,
        "The TileEntity changed after the interaction was prepared.");
    }
    if (previousItems.SequenceEqual(items) && state.NpcIndex == npcIndex &&
        state.LogicCheck == logicCheck && state.LogicOn == logicOn && state.Pose == pose &&
        state.AnchorItemType == anchorItemType)
    {
      return new TileEntityInteractionResult(
        TileEntityInteractionStatus.NoOp,
        state.Handle,
        state.Revision,
        previousItems,
        previousItems,
        replacedItem,
        null);
    }

    if (!_registry.TryUpdateState(
        state.Handle,
        expectedRevision,
        items,
        npcIndex,
        logicCheck,
        logicOn,
        pose,
        anchorItemType,
        out TileEntityRuntimeState? updated) || updated is null)
    {
      return Rejected(state.Handle, TileEntityInteractionStatus.Stale,
        "The TileEntity changed before its interaction could commit.");
    }
    return new TileEntityInteractionResult(
      TileEntityInteractionStatus.Applied,
      updated.Handle,
      updated.Revision,
      previousItems,
      updated.Items.ToArray(),
      replacedItem,
      null);
  }

  private bool IsValidItem(ItemState item)
  {
    return _itemFacts?.IsValid(item) ??
      (item.IsEmpty || item.Type > 0 && item.Type <= short.MaxValue &&
       item.Stack > 0 && item.Stack <= short.MaxValue);
  }

  private static TileEntityInteractionResult Rejected(
    MechanismEntityHandle handle,
    TileEntityInteractionStatus status,
    string detail)
  {
    return new TileEntityInteractionResult(
      status,
      handle,
      null,
      Array.Empty<ItemState>(),
      Array.Empty<ItemState>(),
      null,
      detail);
  }
}

public enum TileEntityInteractionStatus
{
  Applied,
  NoOp,
  Missing,
  Stale,
  InvalidEntityType,
  InvalidSlot,
  InvalidItem,
  InvalidPayload
}

public sealed record TileEntityInteractionResult(
  TileEntityInteractionStatus Status,
  MechanismEntityHandle Handle,
  long? Revision,
  IReadOnlyList<ItemState> PreviousItems,
  IReadOnlyList<ItemState> CurrentItems,
  ItemState? ReplacedItem,
  string? Detail);
