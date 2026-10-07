using System.Buffers.Binary;
using System.Numerics;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;
using Terraria.Projectile;

namespace NSSLC.Infrastructure.Network;

/// <summary>Adapts Steam's packed key while reusing the existing optional-state codec.</summary>
public sealed class SteamProjectilePacketCodec {
  private const int SyncPrefixLength = 23;
  private const int KillBodyLength = 12;
  private readonly ProjectilePacket27Codec _stateCodec = new();

  public PacketBinding CreateSyncBinding(PacketDirection direction) {
    return new PacketBinding<SteamProjectileSyncPacket>(27, direction, DecodeSync, EncodeSync);
  }

  public PacketBinding CreateKillBinding(PacketDirection direction) {
    return new PacketBinding<SteamProjectileKillPacket>(29, direction, DecodeKill, EncodeKill);
  }

  private PacketReadResult<SteamProjectileSyncPacket> DecodeSync(ReadOnlyMemory<byte> body) {
    ReadOnlySpan<byte> source = body.Span;
    if (source.Length < SyncPrefixLength) {
      return Failure<SteamProjectileSyncPacket>(PacketReadErrorCode.Truncated, source.Length);
    }
    uint key = BinaryPrimitives.ReadUInt32LittleEndian(source);
    if (((key >> 8) & 0x3ff) > 1000 || (source[22] & 0x80) != 0) {
      return Failure<SteamProjectileSyncPacket>(PacketReadErrorCode.InvalidCodecData, 22);
    }
    if ((source[22] & 4) != 0) {
      if (source.Length < SyncPrefixLength + 1) {
        return Failure<SteamProjectileSyncPacket>(PacketReadErrorCode.Truncated, source.Length);
      }
      if ((source[23] & 0xfe) != 0) {
        return Failure<SteamProjectileSyncPacket>(PacketReadErrorCode.InvalidCodecData, 23);
      }
    }

    // Only the identity prefix changed. Optional AI/damage/banner fields use the same layout.
    byte[] legacyBody = new byte[source.Length - 1];
    BinaryPrimitives.WriteInt16LittleEndian(legacyBody, (short)((key >> 8) & 0x3ff));
    source.Slice(4, 16).CopyTo(legacyBody.AsSpan(2));
    legacyBody[18] = (byte)key;
    source[20..].CopyTo(legacyBody.AsSpan(19));
    ProjectilePacket27DecodeResult decoded = _stateCodec.Decode(legacyBody);
    if (decoded.Status != ProjectilePacket27DecodeStatus.Decoded
        || decoded.Command is not ProjectileNetworkApplyCommand state) {
      PacketReadErrorCode code = decoded.Status switch {
        ProjectilePacket27DecodeStatus.Truncated => PacketReadErrorCode.Truncated,
        ProjectilePacket27DecodeStatus.TrailingBytes => PacketReadErrorCode.TrailingBytes,
        _ => PacketReadErrorCode.InvalidCodecData
      };
      return Failure<SteamProjectileSyncPacket>(code, SyncPrefixLength);
    }
    return PacketReadResult<SteamProjectileSyncPacket>.Succeeded(new(key, state), source.Length);
  }

  private MemoryStream? EncodeSync(SteamProjectileSyncPacket packet) {
    if (((packet.Key >> 8) & 0x3ff) > 1000
        || packet.State.Identity != (int)((packet.Key >> 8) & 0x3ff)
        || packet.State.OwnerSlot != (byte)packet.Key
        || !_stateCodec.TryEncode(packet.State, includeUuid: false, out byte[] legacyBody)) {
      return null;
    }
    byte[] body = new byte[legacyBody.Length + 1];
    BinaryPrimitives.WriteUInt32LittleEndian(body, packet.Key);
    legacyBody.AsSpan(2, 16).CopyTo(body.AsSpan(4));
    legacyBody.AsSpan(19).CopyTo(body.AsSpan(20));
    return new MemoryStream(body, writable: false);
  }

  private static PacketReadResult<SteamProjectileKillPacket> DecodeKill(
      ReadOnlyMemory<byte> body) {
    if (body.Length != KillBodyLength) {
      return Failure<SteamProjectileKillPacket>(body.Length < KillBodyLength
          ? PacketReadErrorCode.Truncated : PacketReadErrorCode.TrailingBytes,
          Math.Min(body.Length, KillBodyLength));
    }
    ReadOnlySpan<byte> source = body.Span;
    uint key = BinaryPrimitives.ReadUInt32LittleEndian(source);
    var position = new Vector2(BinaryPrimitives.ReadSingleLittleEndian(source[4..]),
        BinaryPrimitives.ReadSingleLittleEndian(source[8..]));
    if (((key >> 8) & 0x3ff) > 1000
        || !float.IsFinite(position.X) || !float.IsFinite(position.Y)) {
      return Failure<SteamProjectileKillPacket>(PacketReadErrorCode.InvalidCodecData, 4);
    }
    return PacketReadResult<SteamProjectileKillPacket>.Succeeded(new(key, position), body.Length);
  }

  private static MemoryStream? EncodeKill(SteamProjectileKillPacket packet) {
    if (((packet.Key >> 8) & 0x3ff) > 1000
        || !float.IsFinite(packet.Position.X) || !float.IsFinite(packet.Position.Y)) {
      return null;
    }
    byte[] body = new byte[KillBodyLength];
    BinaryPrimitives.WriteUInt32LittleEndian(body, packet.Key);
    BinaryPrimitives.WriteSingleLittleEndian(body.AsSpan(4), packet.Position.X);
    BinaryPrimitives.WriteSingleLittleEndian(body.AsSpan(8), packet.Position.Y);
    return new MemoryStream(body, writable: false);
  }

  private static PacketReadResult<TPacket> Failure<TPacket>(PacketReadErrorCode code, int offset) {
    return PacketReadResult<TPacket>.Failed(new PacketReadError(
        code == PacketReadErrorCode.Truncated ? PacketReadStatus.Truncated
            : PacketReadStatus.InvalidData,
        code, offset, null, "Invalid Steam projectile packet body."));
  }
}
