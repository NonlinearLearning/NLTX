namespace MinimalImplementation.Server.Inventory.Contracts;

[Flags]
public enum P04InventoryPacketDirections : byte
{
  None = 0,
  ClientToServer = 1,
  ServerToClient = 2,
  Bidirectional = ClientToServer | ServerToClient
}

public sealed record P04InventoryPacketRegistration
{
  public byte MessageId { get; }

  public string PacketTypeName { get; }

  public P04InventoryPacketDirections Directions { get; }

  public string HandlerSurface { get; }

  public string OutboundMapping { get; }

  public P04InventoryPacketRegistration(
    byte messageId,
    string packetTypeName,
    P04InventoryPacketDirections directions,
    string handlerSurface,
    string outboundMapping)
  {
    if (string.IsNullOrWhiteSpace(packetTypeName))
    {
      throw new ArgumentException("A P04 registration requires a packet type name.", nameof(packetTypeName));
    }

    if (directions == P04InventoryPacketDirections.None)
    {
      throw new ArgumentException("A P04 registration requires a packet direction.", nameof(directions));
    }

    if (string.IsNullOrWhiteSpace(handlerSurface))
    {
      throw new ArgumentException("A P04 registration requires a handler surface.", nameof(handlerSurface));
    }

    if (string.IsNullOrWhiteSpace(outboundMapping))
    {
      throw new ArgumentException("A P04 registration requires an outbound mapping.", nameof(outboundMapping));
    }

    MessageId = messageId;
    PacketTypeName = packetTypeName;
    Directions = directions;
    HandlerSurface = handlerSurface;
    OutboundMapping = outboundMapping;
  }
}

/// <summary>
/// Data-only registration fragment for the root composition. It does not mutate
/// a registry and does not create a second inventory or chest owner.
/// </summary>
public static class P04InventoryRegistrationFragment
{
  public const ushort CraftingModuleId = 11;

  public const string CraftingModuleName = "CraftingRequests";

  private static readonly IReadOnlyList<P04InventoryPacketRegistration> _ordinaryPackets =
    Array.AsReadOnly(
      new[]
      {
        Registration(
          5,
          "SyncEquipmentPacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandleEquipment",
          "5"),
        Registration(
          21,
          "SyncItemPacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandleWorldItemSync",
          "21"),
        Registration(
          22,
          "ItemOwnerPacket",
          P04InventoryPacketDirections.ServerToClient,
          "P04InventoryPacketCommandSurface.CreateItemOwnerProjection",
          "22"),
        Registration(
          31,
          "RequestChestOpenPacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandleChestOpen",
          "32/33"),
        Registration(
          32,
          "SyncChestItemPacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandleChestItem",
          "32"),
        Registration(
          33,
          "SyncPlayerChestPacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandlePlayerChestName",
          "33"),
        Registration(
          34,
          "ChestUpdatesPacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandleChestUpdate",
          "34"),
        Registration(
          39,
          "ReleaseItemOwnershipPacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandleReleaseItemOwnership",
          "22"),
        Registration(
          69,
          "ChestNameRequestPacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandleChestNameRequest",
          "69"),
        Registration(
          85,
          "QuickStackChestsRequestPacket",
          P04InventoryPacketDirections.Bidirectional,
          "QuickStackPacket85Handler.Handle",
          "85"),
        Registration(
          88,
          "ItemTweakerPacket",
          P04InventoryPacketDirections.ServerToClient,
          "P04InventoryPacketCommandSurface.CreateItemTweakerProjection",
          "88"),
        Registration(
          90,
          "InstancedItemPacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandleInstancedItem",
          "90"),
        Registration(
          110,
          "MassWireOperationPayPacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandleWiringPayment",
          "110"),
        Registration(
          145,
          "SyncItemsWithShimmerPacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandleShimmer",
          "145"),
        Registration(
          148,
          "SyncItemCannotBeTakenByEnemiesPacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandleEnemyPickupDelay",
          "148"),
        Registration(
          151,
          "SyncItemDespawnPacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandleItemDespawn",
          "151"),
        Registration(
          155,
          "SyncChestSizePacket",
          P04InventoryPacketDirections.Bidirectional,
          "P04InventoryPacketCommandSurface.HandleChestSize",
          "155"),
        Registration(
          160,
          "ItemPositionPacket",
          P04InventoryPacketDirections.ServerToClient,
          "P04InventoryPacketCommandSurface.CreateItemPositionProjection",
          "160")
      });

  public static IReadOnlyList<P04InventoryPacketRegistration> OrdinaryPackets => _ordinaryPackets;

  public static bool IsComplete()
  {
    byte[] expectedMessageIds = [5, 21, 22, 31, 32, 33, 34, 39, 69, 85, 88, 90, 110, 145, 148, 151, 155, 160];
    return _ordinaryPackets.Count == expectedMessageIds.Length
      && _ordinaryPackets
        .Select(entry => entry.MessageId)
        .Distinct()
        .OrderBy(id => id)
        .SequenceEqual(expectedMessageIds.OrderBy(id => id))
      && P04InventoryOutboundIntentMap.IsComplete()
      && P04InventoryErrorCodeCatalog.IsComplete();
  }

  private static P04InventoryPacketRegistration Registration(
    byte messageId,
    string packetTypeName,
    P04InventoryPacketDirections directions,
    string handlerSurface,
    string outboundMapping)
  {
    return new P04InventoryPacketRegistration(
      messageId,
      packetTypeName,
      directions,
      handlerSurface,
      outboundMapping);
  }
}
