using System;

namespace Terraria.Projectile;

/// <summary>
/// Declaration-only boundary for Version4 packet-27 projectile payloads.
/// Wire framing, validation, authorization, and state mutation remain
/// unimplemented until the target network contract is closed.
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
