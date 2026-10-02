using System;

namespace Terraria.Projectile;

/// <summary>
/// Declaration-only boundary for Version4 packet-29 projectile termination.
/// Wire decoding, validation, authorization, and lifecycle submission remain
/// unimplemented until the target network contract is closed.
/// </summary>
public interface IProjectilePacket29Codec
{
  ProjectilePacket29DecodeResult Decode(ReadOnlySpan<byte> payload);

  bool TryDecode(
    ReadOnlySpan<byte> payload,
    out ProjectileNetworkTerminateCommand command);
}
