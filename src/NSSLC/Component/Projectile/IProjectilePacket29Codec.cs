using System;

namespace Terraria.Projectile;

/// <summary>
/// Boundary for Version4 packet-29 projectile termination payloads.
/// Infrastructure codecs handle payload decoding; frame handling, session
/// authorization, owner policy, and lifecycle submission remain caller responsibilities.
/// </summary>
public interface IProjectilePacket29Codec
{
  ProjectilePacket29DecodeResult Decode(ReadOnlySpan<byte> payload);

  bool TryDecode(
    ReadOnlySpan<byte> payload,
    out ProjectileNetworkTerminateCommand command);
}
