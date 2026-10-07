using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;
using Terraria.Projectile;

namespace NSSLC.Infrastructure.Network;

/// <summary>
/// Maps packet-29 payloads and termination commands to generated packet DTOs.
/// </summary>
public sealed class ProjectilePacket29Codec : IProjectilePacket29Codec
{
  public ProjectilePacket29DecodeResult Decode(ReadOnlySpan<byte> payload)
  {
    var reader = new PacketWireReader(payload.ToArray());
    KillProjectilePacket packet;
    try
    {
      var value = KillProjectilePacket.Read(reader);
      packet = new KillProjectilePacket {
        ProjectileIdentity = value.ProjectileIdentity, Owner = value.Owner
      };
    }
    catch (PacketWireTruncationException)
    {
      return Result(ProjectilePacket29DecodeStatus.Truncated);
    }
    catch (PacketWireFormatException)
    {
      return Result(ProjectilePacket29DecodeStatus.Invalid);
    }

    if (!reader.AtFrameEnd)
    {
      return Result(ProjectilePacket29DecodeStatus.TrailingBytes);
    }

    return Map(packet);
  }

  /// <summary>
  /// Maps an already decoded packet payload without parsing its bytes again.
  /// The caller remains responsible for validating the sender against the
  /// packet's declared owner before submitting the command.
  /// </summary>
  public ProjectilePacket29DecodeResult Map(KillProjectilePacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (TryMap(packet, out ProjectileNetworkTerminateCommand command))
    {
      return new ProjectilePacket29DecodeResult(
        ProjectilePacket29DecodeStatus.Decoded,
        command);
    }

    return Result(ProjectilePacket29DecodeStatus.Invalid);
  }

  public bool TryMap(
    KillProjectilePacket packet,
    out ProjectileNetworkTerminateCommand command)
  {
    ArgumentNullException.ThrowIfNull(packet);
    try
    {
      command = new ProjectileNetworkTerminateCommand(
        packet.Owner,
        packet.ProjectileIdentity);
      return true;
    }
    catch (ArgumentOutOfRangeException)
    {
      command = default;
      return false;
    }
  }

  public bool TryMapToPacket(
    ProjectileNetworkTerminateCommand command,
    out KillProjectilePacket packet)
  {
    packet = new KillProjectilePacket();
    if ((uint)command.OwnerSlot > byte.MaxValue ||
      command.Identity < 0 || command.Identity > short.MaxValue)
    {
      return false;
    }

    packet.ProjectileIdentity = (short)command.Identity;
    packet.Owner = (byte)command.OwnerSlot;
    return true;
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
