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

  byte[] Encode(
    ProjectileNetworkApplyCommand command,
    bool includeUuid);

  bool TryEncode(
    ProjectileNetworkApplyCommand command,
    bool includeUuid,
    out byte[] payload);
}
