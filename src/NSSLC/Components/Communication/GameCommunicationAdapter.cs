using System.Buffers.Binary;
using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Movement;
using Terraria.Relationships;
using Terraria.World;

namespace Terraria.Communication;

/// <summary>
/// Converts the small set of gameplay packets needed by the standalone ECS runtime into domain
/// intents. It never writes location, velocity, tile storage or Life directly.
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
  public const byte TileManipulationMessageId = 17;
  public const byte DamageNpcMessageId = 28;

  private readonly ArchWorld _world;
  private readonly IEntityRuntimeResolver _runtimeResolver;
  private readonly CombatAttackIntentBuffer _attackIntents;
  private readonly CombatAttackDefinitionCatalog _attackDefinitions;
  private readonly GameCommunicationOptions _options;

  public GameCommunicationAdapter(
      ArchWorld world,
      IEntityRuntimeResolver runtimeResolver,
      CombatAttackIntentBuffer attackIntents,
      CombatAttackDefinitionCatalog attackDefinitions,
      GameCommunicationOptions? options = null)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _runtimeResolver = runtimeResolver ?? throw new ArgumentNullException(nameof(runtimeResolver));
    _attackIntents = attackIntents ?? throw new ArgumentNullException(nameof(attackIntents));
    _attackDefinitions = attackDefinitions ??
        throw new ArgumentNullException(nameof(attackDefinitions));
    _options = options ?? new GameCommunicationOptions();
    _options.Validate();
  }

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
      switch (packet.Envelope.MessageId)
      {
        case PlayerControlsMessageId:
          ApplyPlayerControls(player, payload, tick, packet.Envelope.Sequence.Value);
          break;
        case TileManipulationMessageId:
          ApplyTileManipulation(player, payload, tick, packet.Envelope.Sequence.Value);
          break;
        case DamageNpcMessageId:
          ApplyDamageNpc(playerHandle, payload, tick);
          break;
      }
    }
    catch (ArgumentException)
    {
      // A malformed or out-of-range gameplay packet is consumed at the bounded frame boundary.
      // It must not fault the ECS tick or bypass the owning domain's validation.
    }
    return Array.Empty<OutboundPacketIntent>();
  }

  private void ApplyPlayerControls(
      ArchEntity player,
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
    _ = ReadByte(payload, ref offset); // Selected inventory slot remains Player-owned state.
    ValidateCoordinate(ReadVector(payload, ref offset));
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
    if (!_world.Has<MovementIntent>(player))
    {
      return;
    }

    ref MovementIntent current = ref _world.Get<MovementIntent>(player);
    current = intent;
  }

  private void ApplyTileManipulation(
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
      return;
    }

    ref WorldInteractionRequestComponent current = ref _world.Get<WorldInteractionRequestComponent>(player);
    current = request;
  }

  private void ApplyDamageNpc(
      EntityRuntimeHandle source,
      ReadOnlySpan<byte> payload,
      long tick)
  {
    if (payload.Length != 10 || _options.NpcResolver is null ||
        _options.AttackDefinitionResolver is null)
    {
      return;
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
      return;
    }

    CombatAttackDefinitionId? definitionId =
        _options.AttackDefinitionResolver(npcSlot, clientDamage);
    if (definitionId is not CombatAttackDefinitionId definition ||
        !_attackDefinitions.TryGet(definition, out _))
    {
      return;
    }

    _attackIntents.Enqueue(
        source,
        target.Value,
        definition,
        tick,
        DamageCause.PlayerAttack);
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
