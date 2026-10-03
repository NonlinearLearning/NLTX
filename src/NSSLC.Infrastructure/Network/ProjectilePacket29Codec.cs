using System.Buffers.Binary;

using Terraria.Projectile;

namespace NSSLC.Infrastructure.Network;

/// <summary>
/// Decodes the packet-29 projectile termination payload, excluding its frame header.
/// </summary>
public sealed class ProjectilePacket29Codec : IProjectilePacket29Codec
{
  private const int PayloadLength = sizeof(short) + sizeof(byte);

  public ProjectilePacket29DecodeResult Decode(ReadOnlySpan<byte> payload)
  {
    if (payload.Length < PayloadLength)
    {
      return Result(ProjectilePacket29DecodeStatus.Truncated);
    }

    if (payload.Length > PayloadLength)
    {
      return Result(ProjectilePacket29DecodeStatus.TrailingBytes);
    }

    short identity = BinaryPrimitives.ReadInt16LittleEndian(payload);
    byte ownerSlot = payload[sizeof(short)];
    try
    {
      var command = new ProjectileNetworkTerminateCommand(ownerSlot, identity);
      return new ProjectilePacket29DecodeResult(
        ProjectilePacket29DecodeStatus.Decoded,
        command);
    }
    catch (ArgumentOutOfRangeException)
    {
      return Result(ProjectilePacket29DecodeStatus.Invalid);
    }
  }

  public bool TryDecode(
    ReadOnlySpan<byte> payload,
    out ProjectileNetworkTerminateCommand command)
  {
    ProjectilePacket29DecodeResult result = Decode(payload);
    if (result.Status == ProjectilePacket29DecodeStatus.Decoded &&
      result.Command is ProjectileNetworkTerminateCommand decodedCommand)
    {
      command = decodedCommand;
      return true;
    }

    command = default;
    return false;
  }

  private static ProjectilePacket29DecodeResult Result(ProjectilePacket29DecodeStatus status)
  {
    return new ProjectilePacket29DecodeResult(status, null);
  }
}
