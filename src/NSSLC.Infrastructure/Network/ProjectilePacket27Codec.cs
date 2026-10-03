using System.Buffers.Binary;
using System.Numerics;

using Terraria.Projectile;

namespace NSSLC.Infrastructure.Network;

/// <summary>
/// Encodes and decodes the packet-27 projectile payload, excluding its frame header.
/// </summary>
public sealed class ProjectilePacket27Codec : IProjectilePacket27Codec
{
  private const int FixedPayloadLength = 22;

  public ProjectilePacket27DecodeResult Decode(ReadOnlySpan<byte> payload)
  {
    var reader = new PacketReader(payload);
    if (!reader.TryReadInt16(out short identity) ||
      !reader.TryReadVector2(out Vector2 position) ||
      !reader.TryReadVector2(out Vector2 velocity) ||
      !reader.TryReadByte(out byte ownerSlot) ||
      !reader.TryReadInt16(out short projectileType) ||
      !reader.TryReadByte(out byte flags))
    {
      return Result(ProjectilePacket27DecodeStatus.Truncated);
    }

    byte extendedFlags = 0;
    if ((flags & 0b0000_0100) != 0 && !reader.TryReadByte(out extendedFlags))
    {
      return Result(ProjectilePacket27DecodeStatus.Truncated);
    }

    float ai0 = 0.0f;
    float ai1 = 0.0f;
    float ai2 = 0.0f;
    int bannerIdToRespondTo = 0;
    int damage = 0;
    float knockback = 0.0f;
    int originalDamage = 0;
    int projectileUuid = -1;

    if ((flags & 0b0000_0001) != 0 && !reader.TryReadSingle(out ai0))
    {
      return Result(ProjectilePacket27DecodeStatus.Truncated);
    }

    if ((flags & 0b0000_0010) != 0 && !reader.TryReadSingle(out ai1))
    {
      return Result(ProjectilePacket27DecodeStatus.Truncated);
    }

    if ((flags & 0b0000_1000) != 0)
    {
      if (!reader.TryReadUInt16(out ushort bannerId))
      {
        return Result(ProjectilePacket27DecodeStatus.Truncated);
      }

      bannerIdToRespondTo = bannerId;
    }

    if ((flags & 0b0001_0000) != 0)
    {
      if (!reader.TryReadInt16(out short packetDamage))
      {
        return Result(ProjectilePacket27DecodeStatus.Truncated);
      }

      damage = packetDamage;
    }

    if ((flags & 0b0010_0000) != 0 && !reader.TryReadSingle(out knockback))
    {
      return Result(ProjectilePacket27DecodeStatus.Truncated);
    }

    if ((flags & 0b0100_0000) != 0)
    {
      if (!reader.TryReadInt16(out short packetOriginalDamage))
      {
        return Result(ProjectilePacket27DecodeStatus.Truncated);
      }

      originalDamage = packetOriginalDamage;
    }

    if ((flags & 0b1000_0000) != 0)
    {
      if (!reader.TryReadInt16(out short packetUuid))
      {
        return Result(ProjectilePacket27DecodeStatus.Truncated);
      }

      if (packetUuid >= 0 && packetUuid < 1000)
      {
        projectileUuid = packetUuid;
      }
    }

    if ((extendedFlags & 0b0000_0001) != 0 && !reader.TryReadSingle(out ai2))
    {
      return Result(ProjectilePacket27DecodeStatus.Truncated);
    }

    if (reader.Remaining != 0)
    {
      return Result(ProjectilePacket27DecodeStatus.TrailingBytes);
    }

    try
    {
      var command = new ProjectileNetworkApplyCommand(
        ownerSlot,
        identity,
        projectileType,
        position,
        velocity,
        damage,
        originalDamage,
        knockback,
        ai0,
        ai1,
        ai2,
        projectileUuid,
        bannerIdToRespondTo);
      return new ProjectilePacket27DecodeResult(
        ProjectilePacket27DecodeStatus.Decoded,
        command);
    }
    catch (ArgumentOutOfRangeException)
    {
      return Result(ProjectilePacket27DecodeStatus.Invalid);
    }
  }

  public bool TryDecode(
    ReadOnlySpan<byte> payload,
    out ProjectileNetworkApplyCommand command)
  {
    ProjectilePacket27DecodeResult result = Decode(payload);
    if (result.Status == ProjectilePacket27DecodeStatus.Decoded &&
      result.Command is ProjectileNetworkApplyCommand decodedCommand)
    {
      command = decodedCommand;
      return true;
    }

    command = default;
    return false;
  }

  public byte[] Encode(ProjectileNetworkApplyCommand command, bool includeUuid)
  {
    if (!TryEncode(command, includeUuid, out byte[] payload))
    {
      throw new ArgumentOutOfRangeException(nameof(command));
    }

    return payload;
  }

  public bool TryEncode(
    ProjectileNetworkApplyCommand command,
    bool includeUuid,
    out byte[] payload)
  {
    payload = Array.Empty<byte>();
    if (!CanEncode(command))
    {
      return false;
    }

    byte flags = 0;
    byte extendedFlags = 0;
    if (command.Ai0 != 0.0f)
    {
      flags |= 0b0000_0001;
    }

    if (command.Ai1 != 0.0f)
    {
      flags |= 0b0000_0010;
    }

    if (command.Ai2 != 0.0f)
    {
      flags |= 0b0000_0100;
      extendedFlags |= 0b0000_0001;
    }

    if (command.BannerIdToRespondTo != 0)
    {
      flags |= 0b0000_1000;
    }

    if (command.Damage != 0)
    {
      flags |= 0b0001_0000;
    }

    if (command.Knockback != 0.0f)
    {
      flags |= 0b0010_0000;
    }

    if (command.OriginalDamage != 0)
    {
      flags |= 0b0100_0000;
    }

    if (includeUuid)
    {
      flags |= 0b1000_0000;
    }

    int payloadLength = FixedPayloadLength;
    payloadLength += (flags & 0b0000_0001) != 0 ? sizeof(float) : 0;
    payloadLength += (flags & 0b0000_0010) != 0 ? sizeof(float) : 0;
    payloadLength += (flags & 0b0000_0100) != 0 ? 1 : 0;
    payloadLength += (flags & 0b0000_1000) != 0 ? sizeof(ushort) : 0;
    payloadLength += (flags & 0b0001_0000) != 0 ? sizeof(short) : 0;
    payloadLength += (flags & 0b0010_0000) != 0 ? sizeof(float) : 0;
    payloadLength += (flags & 0b0100_0000) != 0 ? sizeof(short) : 0;
    payloadLength += (flags & 0b1000_0000) != 0 ? sizeof(short) : 0;
    payloadLength += (extendedFlags & 0b0000_0001) != 0 ? sizeof(float) : 0;

    payload = new byte[payloadLength];
    int offset = 0;
    WriteInt16(payload, ref offset, command.Identity);
    WriteVector2(payload, ref offset, command.Position);
    WriteVector2(payload, ref offset, command.Velocity);
    payload[offset++] = (byte)command.OwnerSlot;
    WriteInt16(payload, ref offset, command.ProjectileType);
    payload[offset++] = flags;
    if ((flags & 0b0000_0100) != 0)
    {
      payload[offset++] = extendedFlags;
    }

    if ((flags & 0b0000_0001) != 0)
    {
      WriteSingle(payload, ref offset, command.Ai0);
    }

    if ((flags & 0b0000_0010) != 0)
    {
      WriteSingle(payload, ref offset, command.Ai1);
    }

    if ((flags & 0b0000_1000) != 0)
    {
      WriteUInt16(payload, ref offset, command.BannerIdToRespondTo);
    }

    if ((flags & 0b0001_0000) != 0)
    {
      WriteInt16(payload, ref offset, command.Damage);
    }

    if ((flags & 0b0010_0000) != 0)
    {
      WriteSingle(payload, ref offset, command.Knockback);
    }

    if ((flags & 0b0100_0000) != 0)
    {
      WriteInt16(payload, ref offset, command.OriginalDamage);
    }

    if ((flags & 0b1000_0000) != 0)
    {
      WriteInt16(payload, ref offset, command.ProjectileUuid);
    }

    if ((extendedFlags & 0b0000_0001) != 0)
    {
      WriteSingle(payload, ref offset, command.Ai2);
    }

    return true;
  }

  private static bool CanEncode(ProjectileNetworkApplyCommand command)
  {
    return (uint)command.OwnerSlot <= byte.MaxValue &&
      command.Identity >= 0 && command.Identity <= short.MaxValue &&
      command.ProjectileType > 0 && command.ProjectileType <= short.MaxValue &&
      command.Damage >= short.MinValue && command.Damage <= short.MaxValue &&
      command.OriginalDamage >= short.MinValue && command.OriginalDamage <= short.MaxValue &&
      (uint)command.BannerIdToRespondTo <= ushort.MaxValue &&
      command.ProjectileUuid >= -1 && command.ProjectileUuid < 1000 &&
      IsFinite(command.Position) && IsFinite(command.Velocity) &&
      float.IsFinite(command.Knockback) && float.IsFinite(command.Ai0) &&
      float.IsFinite(command.Ai1) && float.IsFinite(command.Ai2);
  }

  private static bool IsFinite(Vector2 value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }

  private static void WriteVector2(Span<byte> payload, ref int offset, Vector2 value)
  {
    WriteSingle(payload, ref offset, value.X);
    WriteSingle(payload, ref offset, value.Y);
  }

  private static void WriteSingle(Span<byte> payload, ref int offset, float value)
  {
    BinaryPrimitives.WriteInt32LittleEndian(
      payload[offset..],
      BitConverter.SingleToInt32Bits(value));
    offset += sizeof(float);
  }

  private static void WriteInt16(Span<byte> payload, ref int offset, int value)
  {
    BinaryPrimitives.WriteInt16LittleEndian(payload[offset..], (short)value);
    offset += sizeof(short);
  }

  private static void WriteUInt16(Span<byte> payload, ref int offset, int value)
  {
    BinaryPrimitives.WriteUInt16LittleEndian(payload[offset..], (ushort)value);
    offset += sizeof(ushort);
  }

  private static ProjectilePacket27DecodeResult Result(ProjectilePacket27DecodeStatus status)
  {
    return new ProjectilePacket27DecodeResult(status, null);
  }

  private ref struct PacketReader
  {
    private readonly ReadOnlySpan<byte> _payload;
    private int _offset;

    public PacketReader(ReadOnlySpan<byte> payload)
    {
      _payload = payload;
      _offset = 0;
    }

    public int Remaining => _payload.Length - _offset;

    public bool TryReadByte(out byte value)
    {
      value = 0;
      if (Remaining < sizeof(byte))
      {
        return false;
      }

      value = _payload[_offset++];
      return true;
    }

    public bool TryReadInt16(out short value)
    {
      value = 0;
      if (Remaining < sizeof(short))
      {
        return false;
      }

      value = BinaryPrimitives.ReadInt16LittleEndian(_payload[_offset..]);
      _offset += sizeof(short);
      return true;
    }

    public bool TryReadUInt16(out ushort value)
    {
      value = 0;
      if (Remaining < sizeof(ushort))
      {
        return false;
      }

      value = BinaryPrimitives.ReadUInt16LittleEndian(_payload[_offset..]);
      _offset += sizeof(ushort);
      return true;
    }

    public bool TryReadSingle(out float value)
    {
      value = 0.0f;
      if (Remaining < sizeof(float))
      {
        return false;
      }

      int bits = BinaryPrimitives.ReadInt32LittleEndian(_payload[_offset..]);
      value = BitConverter.Int32BitsToSingle(bits);
      _offset += sizeof(float);
      return true;
    }

    public bool TryReadVector2(out Vector2 value)
    {
      value = default;
      if (!TryReadSingle(out float x) || !TryReadSingle(out float y))
      {
        return false;
      }

      value = new Vector2(x, y);
      return true;
    }
  }
}
