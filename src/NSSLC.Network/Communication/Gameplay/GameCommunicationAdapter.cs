using System.Buffers.Binary;
using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Inventory;
using Terraria.Share.Movement;
using Terraria.Share.Relationships;
using Terraria.Unshared.Chat;
using Terraria.Share.Items;
using Terraria.Unshared.Player;
using Terraria.Unshared.Session;
using Terraria.Unshared.World;

namespace Terraria.Network;

/// <summary>
/// Converts the small set of gameplay packets needed by the standalone ECS runtime into domain
/// intents. It updates the session's reported position and inventory facts, while world item and
/// tile mutations remain owned by their domain stores and systems.
/// </summary>
/// <remarks>
/// Packet 13 becomes a <see cref="MovementIntent"/> on the bound player. Packet 17 becomes a
/// <see cref="WorldInteractionRequestComponent"/>. Packet 28 becomes a
/// <see cref="CombatAttackIntent"/> after the host-provided NPC and attack-definition mappings
/// have accepted it. The owning domain systems perform the authoritative commit in later phases.
/// </remarks>
public sealed class GameCommunicationAdapter : ICommunicationPacketHandler
{
  public const byte PlayerControlsMessageId = 13;
  public const byte SyncEquipmentMessageId = 5;
  public const byte TileManipulationMessageId = 17;
  public const byte SyncItemMessageId = 21;
  public const byte ReleaseItemOwnershipMessageId = 39;
  public const byte SyncItemDespawnMessageId = 151;
  public const byte DamageNpcMessageId = 28;
  public const byte NetModulesMessageId = 82;

  private readonly ArchWorld _world;
  private readonly IEntityRuntimeResolver _runtimeResolver;
  private readonly CombatAttackCatalog _attackDefinitions;
  private readonly IItemQuery _worldItems;
  private readonly GameCommunicationOptions _options;
  private ItemRequestQueueComponent? _itemRequests;
  private PlayerInputRequestQueueComponent? _playerInputs;
  private MovementIntentRequestQueueComponent? _movementIntents;
  private ChatMessageRequestQueueComponent? _chatRequests;

  public GameCommunicationAdapter(
      ArchWorld world,
      IEntityRuntimeResolver runtimeResolver,
      CombatAttackCatalog attackDefinitions,
      IItemQuery worldItems,
      GameCommunicationOptions? options = null)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _runtimeResolver = runtimeResolver ?? throw new ArgumentNullException(nameof(runtimeResolver));
    _attackDefinitions = attackDefinitions ??
        throw new ArgumentNullException(nameof(attackDefinitions));
    _worldItems = worldItems ?? throw new ArgumentNullException(nameof(worldItems));
    _options = options ?? new GameCommunicationOptions();
    _options.Validate();
  }

  /// <summary>
  /// World-scoped item request queue, resolved on first use. Packet handling runs inside a
  /// <see cref="World.Query"/> callback, so the queue must already exist — resolving it here keeps
  /// that resolution out of the session query.
  /// </summary>
  private ItemRequestQueueComponent ItemRequests =>
      _itemRequests ??= ItemRequestQueue.Require(_world);

  /// <summary>
  /// Player-scoped input request queue, resolved on first use. Packet handling runs inside a
  /// <see cref="World.Query"/> callback, so the queue must already exist — resolving it here keeps
  /// that resolution out of the session query.
  /// </summary>
  private PlayerInputRequestQueueComponent PlayerInputs =>
      _playerInputs ??= PlayerInputRequestQueue.Require(_world);

  /// <summary>
  /// Movement intent request queue, resolved on first use. Packet handling runs inside a
  /// <see cref="World.Query"/> callback, so the queue must already exist — resolving it here keeps
  /// that resolution out of the session query.
  /// </summary>
  private MovementIntentRequestQueueComponent MovementIntents =>
      _movementIntents ??= MovementIntentRequestQueue.Require(_world);

  /// <summary>
  /// Chat request queue, resolved on first use. Packet handling runs inside a
  /// <see cref="World.Query"/> callback, so the queue must already exist — resolving it here keeps
  /// that resolution out of the session query.
  /// </summary>
  private ChatMessageRequestQueueComponent ChatRequests =>
      _chatRequests ??= ChatMessageRequestQueue.Require(_world);

  public IReadOnlyList<OutboundPacketIntent> Handle(
      InboundCommunicationPacket packet,
      CommunicationSessionComponent session,
      long tick)
  {
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(session);
    if (tick < 0 || packet.Envelope.Epoch != session.Epoch || session.Player is not
        EntityRuntimeHandle playerHandle || !_runtimeResolver.TryResolve(playerHandle, out ArchEntity player) ||
        player == ArchEntity.Null || !_world.IsAlive(player))
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    ReadOnlySpan<byte> payload = packet.Frame.Payload.Span;
    try
    {
      return packet.Envelope.MessageId switch
      {
        PlayerControlsMessageId => ApplyPlayerControls(
            player, playerHandle, payload, tick, packet.Envelope.Sequence.Value),
        SyncEquipmentMessageId => ApplyInventory(
            player, playerHandle, session, payload),
        TileManipulationMessageId => ApplyTileManipulation(
            player, payload, tick, packet.Envelope.Sequence.Value),
        SyncItemMessageId => ApplyWorldItem(
            player, session, payload),
        ReleaseItemOwnershipMessageId => ApplyWorldItemRelease(
            session, payload),
        SyncItemDespawnMessageId => ApplyWorldItemRetirement(
            player, session, payload),
        DamageNpcMessageId => ApplyDamageNpc(playerHandle, payload, tick),
        NetModulesMessageId => ApplyChatMessage(player, playerHandle, payload),
        _ => Array.Empty<OutboundPacketIntent>()
      };
    }
    catch (ArgumentException)
    {
      // A malformed or out-of-range gameplay packet is consumed at the bounded frame boundary.
      // It must not fault the ECS tick or bypass the owning domain's validation.
    }
    return Array.Empty<OutboundPacketIntent>();
  }

  private IReadOnlyList<OutboundPacketIntent> ApplyPlayerControls(
      ArchEntity player,
      EntityRuntimeHandle playerHandle,
      ReadOnlySpan<byte> payload,
      long tick,
      long sourceRevision)
  {
    if (payload.Length < 14 || payload.Length > 48)
    {
      throw new ArgumentException("Packet 13 has an invalid length.", nameof(payload));
    }

    int offset = 0;
    _ = ReadByte(payload, ref offset); // The player slot is scoped to the authenticated session.
    byte controlFlags = ReadByte(payload, ref offset);
    byte movementFlags = ReadByte(payload, ref offset);
    byte playerFeatureFlags = ReadByte(payload, ref offset);
    byte actionFlags = ReadByte(payload, ref offset);
    // 选中槽是库存自带的索引（Version4 的 Player.selectedItem 就是 inventory 的一个下标），
    // 这里照 MessageBuffer.cs:996 的做法把它交给 Player 域，而不是在适配器里直接写。
    byte selectedSlot = ReadByte(payload, ref offset);
    Vector2 position = ReadVector(payload, ref offset);
    ValidateCoordinate(position);
    Vector2 velocity = Vector2.Zero;
    bool hasVelocity = (movementFlags & (1 << 2)) != 0;
    if (hasVelocity)
    {
      velocity = ReadVector(payload, ref offset);
      ValidateVelocity(velocity);
    }

    if ((movementFlags & (1 << 7)) != 0)
    {
      _ = ReadUInt16(payload, ref offset);
    }

    if ((playerFeatureFlags & (1 << 6)) != 0)
    {
      ValidateCoordinate(ReadVector(payload, ref offset));
      ValidateCoordinate(ReadVector(payload, ref offset));
    }

    if ((actionFlags & (1 << 5)) != 0)
    {
      ValidateCoordinate(ReadVector(payload, ref offset));
    }

    if (offset != payload.Length)
    {
      throw new ArgumentException("Packet 13 contains trailing bytes.", nameof(payload));
    }

    MovementIntent intent = new(
        velocity,
        tick,
        _options.MovementPriority,
        jumpRequested: (controlFlags & (1 << 4)) != 0,
        hasDesiredVelocity: hasVelocity,
        source: MovementIntentSource.Input,
        sourceRevision: sourceRevision);
    if (_world.Has<LocationComponent>(player))
    {
      // LocationComponent 归 Player 域所有：这里只提交坐标意图。
      PlayerInputs.Enqueue(new PlayerInputRequest(
          PlayerInputRequestKind.SetPosition,
          Player: playerHandle,
          Position: position));
    }

    if (_world.Has<InventoryComponent>(player))
    {
      // 库存归 Player 域所有：这里只提交"改选哪一格"的意图，载荷仍留在原来的槽里。
      PlayerInputs.Enqueue(new PlayerInputRequest(
          PlayerInputRequestKind.SelectHeldSlot,
          Player: playerHandle,
          Slot: selectedSlot));
    }

    if (!_world.Has<MovementIntent>(player))
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    // MovementIntent 归 Movement 域所有：这里只提交意图。
    MovementIntents.Enqueue(new MovementIntentRequest(playerHandle, intent));
    return Array.Empty<OutboundPacketIntent>();
  }

  private IReadOnlyList<OutboundPacketIntent> ApplyTileManipulation(
      ArchEntity player,
      ReadOnlySpan<byte> payload,
      long tick,
      long requestId)
  {
    if (payload.Length != 8)
    {
      throw new ArgumentException("Packet 17 has an invalid length.", nameof(payload));
    }

    int offset = 0;
    byte action = ReadByte(payload, ref offset);
    int x = ReadInt16(payload, ref offset);
    int y = ReadInt16(payload, ref offset);
    int tileType = ReadInt16(payload, ref offset);
    byte style = ReadByte(payload, ref offset);
    if (x < 0 || y < 0 || x > _options.MaximumCoordinate || y > _options.MaximumCoordinate)
    {
      throw new ArgumentOutOfRangeException(nameof(payload), "Tile coordinates are outside the bounded range.");
    }

    WorldInteractionRequestComponent request = action switch
    {
      0 when tileType == 0 => WorldInteractionRequestComponent.BreakTile(
          requestId, tick, new WorldTileCoordinate(x, y)),
      1 when tileType >= 0 => WorldInteractionRequestComponent.PlaceTile(
          requestId, tick, new WorldTileCoordinate(x, y), checked((ushort)tileType), style),
      _ => throw new ArgumentException("Packet 17 contains an unsupported world operation.", nameof(payload))
    };

    if (!_world.Has<WorldInteractionRequestComponent>(player) ||
        !_world.Has<WorldInteractionResultComponent>(player))
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    ref WorldInteractionRequestComponent current = ref _world.Get<WorldInteractionRequestComponent>(player);
    current = request;
    return Array.Empty<OutboundPacketIntent>();
  }

  private IReadOnlyList<OutboundPacketIntent> ApplyDamageNpc(
      EntityRuntimeHandle source,
      ReadOnlySpan<byte> payload,
      long tick)
  {
    if (payload.Length != 10 || _options.NpcResolver is null ||
        _options.AttackDefinitionResolver is null)
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    int offset = 0;
    short npcSlot = ReadInt16(payload, ref offset);
    short clientDamage = ReadInt16(payload, ref offset);
    float knockback = ReadSingle(payload, ref offset);
    _ = ReadByte(payload, ref offset);
    _ = ReadByte(payload, ref offset);
    EntityRuntimeHandle? target = _options.NpcResolver(npcSlot);
    if (!float.IsFinite(knockback) || knockback < 0f || clientDamage < 0 ||
        !target.HasValue)
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    CombatAttackId? definitionId =
        _options.AttackDefinitionResolver(npcSlot, clientDamage);
    if (definitionId is not CombatAttackId definition ||
        !_attackDefinitions.TryGet(definition, out _))
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    CombatAttackIntentQueue.Require(_world).Enqueue(
        source,
        target.Value,
        definition,
        tick,
        DamageCause.PlayerAttack);
    return Array.Empty<OutboundPacketIntent>();
  }

  private IReadOnlyList<OutboundPacketIntent> ApplyInventory(
      ArchEntity player,
      EntityRuntimeHandle playerHandle,
      CommunicationSessionComponent session,
      ReadOnlySpan<byte> payload)
  {
    if (payload.Length != 9 || !_world.Has<InventoryComponent>(player))
    {
      throw new ArgumentException("Packet 5 has an invalid length.", nameof(payload));
    }

    int offset = 0;
    _ = ReadByte(payload, ref offset);
    short slot = ReadInt16(payload, ref offset);
    short stack = ReadInt16(payload, ref offset);
    byte prefix = ReadByte(payload, ref offset);
    short itemType = ReadInt16(payload, ref offset);
    byte flags = ReadByte(payload, ref offset);
    if (slot < 0 || stack < 0 || itemType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(payload));
    }

    // 玩家库存归 Share/玩家域所有：这里只提交槽位意图，不写入。
    PlayerInputs.Enqueue(new PlayerInputRequest(
        PlayerInputRequestKind.SetInventorySlot,
        Player: playerHandle,
        Slot: slot,
        ItemType: itemType,
        Stack: stack,
        Prefix: prefix));
    return [new OutboundPacketIntent(
        [session.Epoch],
        null,
        SyncEquipmentMessageId,
        payload.ToArray(),
        CommitVersion.Initial)];
  }

  private IReadOnlyList<OutboundPacketIntent> ApplyWorldItem(
      ArchEntity player,
      CommunicationSessionComponent session,
      ReadOnlySpan<byte> payload)
  {
    if (payload.Length < 24 || payload.Length > 30)
    {
      throw new ArgumentException("Packet 21 has an invalid length.", nameof(payload));
    }

    int offset = 0;
    short slot = ReadInt16(payload, ref offset);
    Vector2 position = ReadVector(payload, ref offset);
    Vector2 velocity = ReadVector(payload, ref offset);
    short stack = ReadInt16(payload, ref offset);
    byte prefix = ReadByte(payload, ref offset);
    byte stateFlags = ReadByte(payload, ref offset);
    short itemType = ReadInt16(payload, ref offset);
    if ((stateFlags & 4) != 0)
    {
      _ = ReadByte(payload, ref offset);
      _ = ReadSingle(payload, ref offset);
    }

    if ((stateFlags & 8) != 0)
    {
      _ = ReadByte(payload, ref offset);
    }

    if (offset != payload.Length || stack < 0 || itemType < 0 ||
        !Terraria.Share.Physics.SimulationVector.IsFinite(position) || !Terraria.Share.Physics.SimulationVector.IsFinite(velocity))
    {
      throw new ArgumentException("Packet 21 contains invalid item facts.", nameof(payload));
    }

    // 包 21 携带的是协议 itemType。未知类型无法进入权威模型，因此在这里丢弃，
    // 而不是先写进状态再投影给所有客户端。stack 为 0 表示"清除该槽位"，
    // 不携带物品身份，不参与校验。
    if (stack > 0 && !Terraria.Share.Items.ItemIdRules.TryFromNetworkType(itemType, out _))
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    if (!_world.Has<LocationComponent>(player))
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    Vector2 playerPosition = _world.Get<LocationComponent>(player).Coordinates;
    if (Vector2.Distance(playerPosition, position) > 256f)
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    if (slot == 400)
    {
      byte ownership = (byte)(stateFlags & 3);
      SessionEpoch? reservedFor = ownership is 1 or 2 ? session.Epoch : null;
      ItemRequests.Enqueue(new ItemRequest(
          ItemRequestKind.Create,
          Slot: slot,
          Actor: session.Epoch,
          ItemType: itemType,
          Stack: stack,
          Prefix: prefix,
          Position: position,
          Velocity: velocity,
          ReservedFor: reservedFor));
      return Array.Empty<OutboundPacketIntent>();
    }

    ItemRequests.Enqueue(new ItemRequest(
        ItemRequestKind.Update,
        Slot: slot,
        Actor: session.Epoch,
        ItemType: itemType,
        Stack: stack,
        Prefix: prefix,
        Position: position,
        Velocity: velocity));
    return Array.Empty<OutboundPacketIntent>();
  }

  private IReadOnlyList<OutboundPacketIntent> ApplyWorldItemRelease(
      CommunicationSessionComponent session,
      ReadOnlySpan<byte> payload)
  {
    if (payload.Length != 3)
    {
      throw new ArgumentException("Packet 39 has an invalid length.", nameof(payload));
    }

    int offset = 0;
    short slot = ReadInt16(payload, ref offset);
    _ = ReadByte(payload, ref offset);
    ItemRequests.Enqueue(new ItemRequest(
        ItemRequestKind.Release,
        Slot: slot,
        Actor: session.Epoch));
    return Array.Empty<OutboundPacketIntent>();
  }

  private IReadOnlyList<OutboundPacketIntent> ApplyWorldItemRetirement(
      ArchEntity player,
      CommunicationSessionComponent session,
      ReadOnlySpan<byte> payload)
  {
    if (payload.Length != 2)
    {
      throw new ArgumentException("Packet 151 has an invalid length.", nameof(payload));
    }

    int offset = 0;
    short slot = ReadInt16(payload, ref offset);
    if (_world.Has<LocationComponent>(player) && _worldItems.TryGet(slot, out ItemState? item) &&
        item is not null && Vector2.Distance(
            _world.Get<LocationComponent>(player).Coordinates,
            item.Position) <= 256f)
    {
      ItemRequests.Enqueue(new ItemRequest(
          ItemRequestKind.Retire,
          Slot: slot,
          Actor: session.Epoch));
    }

    return Array.Empty<OutboundPacketIntent>();
  }

  /// <summary>Packet 82 (NetModules) carrying the text module: one client chat line.</summary>
  /// <remarks>
  /// The line is accepted only when the sender declares <see cref="ChatSenderComponent"/>; the Chat
  /// domain owns sanitizing, command dispatch and delivery. The inbound frame is a text-command name
  /// plus the typed text — see <see cref="ChatNetTextFrame"/> for the provenance of that layout.
  /// </remarks>
  private IReadOnlyList<OutboundPacketIntent> ApplyChatMessage(
      ArchEntity player,
      EntityRuntimeHandle playerHandle,
      ReadOnlySpan<byte> payload)
  {
    if (!_world.Has<ChatSenderComponent>(player) ||
        !ChatNetTextFrame.TryDecodeClientMessage(payload, out string commandName, out string text) ||
        !ChatChannelRules.TryResolve(commandName, out ChatChannel channel))
    {
      return Array.Empty<OutboundPacketIntent>();
    }

    ChatRequests.Enqueue(new ChatMessageRequest(playerHandle, channel, text));
    return Array.Empty<OutboundPacketIntent>();
  }

  private void ValidateCoordinate(Vector2 value)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y) ||
        MathF.Abs(value.X) > _options.MaximumCoordinate ||
        MathF.Abs(value.Y) > _options.MaximumCoordinate)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }
  }

  private void ValidateVelocity(Vector2 value)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y) ||
        MathF.Abs(value.X) > _options.MaximumVelocity ||
        MathF.Abs(value.Y) > _options.MaximumVelocity)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }
  }

  private static byte ReadByte(ReadOnlySpan<byte> payload, ref int offset)
  {
    EnsureAvailable(payload, offset, sizeof(byte));
    return payload[offset++];
  }

  private static short ReadInt16(ReadOnlySpan<byte> payload, ref int offset)
  {
    EnsureAvailable(payload, offset, sizeof(short));
    short value = BinaryPrimitives.ReadInt16LittleEndian(payload[offset..]);
    offset += sizeof(short);
    return value;
  }

  private static ushort ReadUInt16(ReadOnlySpan<byte> payload, ref int offset)
  {
    EnsureAvailable(payload, offset, sizeof(ushort));
    ushort value = BinaryPrimitives.ReadUInt16LittleEndian(payload[offset..]);
    offset += sizeof(ushort);
    return value;
  }

  private static float ReadSingle(ReadOnlySpan<byte> payload, ref int offset)
  {
    EnsureAvailable(payload, offset, sizeof(float));
    int bits = BinaryPrimitives.ReadInt32LittleEndian(payload[offset..]);
    offset += sizeof(float);
    return BitConverter.Int32BitsToSingle(bits);
  }

  private static Vector2 ReadVector(ReadOnlySpan<byte> payload, ref int offset)
  {
    return new Vector2(ReadSingle(payload, ref offset), ReadSingle(payload, ref offset));
  }

  private static void EnsureAvailable(ReadOnlySpan<byte> payload, int offset, int length)
  {
    if (offset < 0 || length < 0 || offset > payload.Length - length)
    {
      throw new ArgumentException("The gameplay packet is truncated.", nameof(payload));
    }
  }
}
