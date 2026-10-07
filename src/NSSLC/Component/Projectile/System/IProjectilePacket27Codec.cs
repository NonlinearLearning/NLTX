using System;

namespace Terraria.Projectile;

/// <summary>
/// Boundary for Version4 packet-27 projectile payloads.
/// Infrastructure codecs handle the payload bytes; frame handling, session
/// authorization, owner policy, and state mutation remain caller responsibilities.
/// </summary>
public interface IProjectilePacket27Codec
{
  ProjectilePacket27DecodeResult Decode(ReadOnlySpan<byte> payload);

  bool TryDecode(
    ReadOnlySpan<byte> payload,
    out ProjectileNetworkApplyCommand command);

  /// <summary>
  /// Encodes a body. The caller must pass the protocol's NeedsUUID result for
  /// this type, and it must match the command's UUID presence.
  /// </summary>
  byte[] Encode(
    ProjectileNetworkApplyCommand command,
    bool includeUuid);

  /// <summary>
  /// Encodes a body. The caller must pass the protocol's NeedsUUID result for
  /// this type, and it must match the command's UUID presence.
  /// </summary>
  bool TryEncode(
    ProjectileNetworkApplyCommand command,
    bool includeUuid,
    out byte[] payload);
}
