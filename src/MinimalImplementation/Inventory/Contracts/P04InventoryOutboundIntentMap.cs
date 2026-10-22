namespace MinimalImplementation.Server.Inventory.Contracts;

/// <summary>Semantic reason for a P04 server-to-client projection.</summary>
public enum P04InventoryOutboundTrigger
{
  EquipmentCommitted,
  WorldItemCommitted,
  WorldItemReservationChanged,
  ChestSlotCommitted,
  ChestSessionProjection,
  ChestLifecycleCommitted,
  ChestNameProjection,
  QuickStackCompleted,
  ItemTweakerProjection,
  InstancedItemProjection,
  WiringPaymentCommitted,
  ShimmerProjection,
  EnemyPickupDelayProjection,
  WorldItemDespawned,
  ChestResized,
  WorldItemPositionProjection
}

/// <summary>
/// P04's transport-neutral mapping from a committed domain effect to its V4
/// server-to-client packet. The root adapter supplies target epochs and encoding.
/// </summary>
public sealed record P04InventoryOutboundIntentMapping
{
  public byte MessageId { get; }

  public string PacketTypeName { get; }

  public P04InventoryOutboundTrigger Trigger { get; }

  public P04InventoryOutboundIntentMapping(
    byte messageId,
    string packetTypeName,
    P04InventoryOutboundTrigger trigger)
  {
    if (string.IsNullOrWhiteSpace(packetTypeName))
    {
      throw new ArgumentException("A P04 outbound mapping requires a packet type name.", nameof(packetTypeName));
    }

    MessageId = messageId;
    PacketTypeName = packetTypeName;
    Trigger = trigger;
  }
}

/// <summary>Provides the P04 outbound producer map without owning transport registration.</summary>
public static class P04InventoryOutboundIntentMap
{
  private static readonly IReadOnlyList<P04InventoryOutboundIntentMapping> _entries =
    Array.AsReadOnly(
      new P04InventoryOutboundIntentMapping[]
      {
        new(
          5,
          "SyncEquipmentPacket",
          P04InventoryOutboundTrigger.EquipmentCommitted),
        new(
          21,
          "SyncItemPacket",
          P04InventoryOutboundTrigger.WorldItemCommitted),
        new(
          22,
          "ItemOwnerPacket",
          P04InventoryOutboundTrigger.WorldItemReservationChanged),
        new(
          32,
          "SyncChestItemPacket",
          P04InventoryOutboundTrigger.ChestSlotCommitted),
        new(
          33,
          "SyncPlayerChestPacket",
          P04InventoryOutboundTrigger.ChestSessionProjection),
        new(
          34,
          "ChestUpdatesPacket",
          P04InventoryOutboundTrigger.ChestLifecycleCommitted),
        new(
          69,
          "ChestNameResponsePacket",
          P04InventoryOutboundTrigger.ChestNameProjection),
        new(
          85,
          "QuickStackChestsResponsePacket",
          P04InventoryOutboundTrigger.QuickStackCompleted),
        new(
          88,
          "ItemTweakerPacket",
          P04InventoryOutboundTrigger.ItemTweakerProjection),
        new(
          90,
          "InstancedItemPacket",
          P04InventoryOutboundTrigger.InstancedItemProjection),
        new(
          110,
          "MassWireOperationPayPacket",
          P04InventoryOutboundTrigger.WiringPaymentCommitted),
        new(
          145,
          "SyncItemsWithShimmerPacket",
          P04InventoryOutboundTrigger.ShimmerProjection),
        new(
          148,
          "SyncItemCannotBeTakenByEnemiesPacket",
          P04InventoryOutboundTrigger.EnemyPickupDelayProjection),
        new(
          151,
          "SyncItemDespawnPacket",
          P04InventoryOutboundTrigger.WorldItemDespawned),
        new(
          155,
          "SyncChestSizePacket",
          P04InventoryOutboundTrigger.ChestResized),
        new(
          160,
          "ItemPositionPacket",
          P04InventoryOutboundTrigger.WorldItemPositionProjection)
      });

  public static IReadOnlyList<P04InventoryOutboundIntentMapping> Entries => _entries;

  public static bool TryGet(
    byte messageId,
    out P04InventoryOutboundIntentMapping? mapping)
  {
    mapping = _entries.FirstOrDefault(entry => entry.MessageId == messageId);
    return mapping is not null;
  }

  public static bool IsComplete()
  {
    byte[] expectedMessageIds = [5, 21, 22, 32, 33, 34, 69, 85, 88, 90, 110, 145, 148, 151, 155, 160];
    return _entries.Count == expectedMessageIds.Length
      && _entries
        .Select(entry => entry.MessageId)
        .Distinct()
        .OrderBy(id => id)
        .SequenceEqual(expectedMessageIds.OrderBy(id => id));
  }
}
