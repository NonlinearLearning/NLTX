using System.IO;
using System.Numerics;

using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;
using Terraria.Projectile;

namespace NSSLC.Infrastructure.Network;

/// <summary>
/// Maps projectile commands to the generated packet-27 body codec.
/// </summary>
public sealed class ProjectilePacket27Codec : IProjectilePacket27Codec
{
  public ProjectilePacket27DecodeResult Decode(ReadOnlySpan<byte> payload)
  {
    var reader = new PacketWireReader(payload.ToArray());
    SyncProjectilePacket packet;
    try
    {
      var value = SyncProjectilePacket.Read(reader);
      packet = new SyncProjectilePacket {
        Identity = value.Identity,
        Position = value.Position,
        Velocity = value.Velocity,
        Owner = value.Owner,
        ProjectileType = value.ProjectileType,
        Ai0 = value.Ai0,
        Ai1 = value.Ai1,
        BannerId = value.BannerId,
        Damage = value.Damage,
        Knockback = value.Knockback,
        OriginalDamage = value.OriginalDamage,
        ProjectileUuid = value.ProjectileUuid,
        Ai2 = value.Ai2
      };
    }
    catch (PacketWireTruncationException)
    {
      return Result(ProjectilePacket27DecodeStatus.Truncated);
    }
    catch (PacketWireFormatException)
    {
      return Result(ProjectilePacket27DecodeStatus.Invalid);
    }

    if (!reader.AtFrameEnd)
    {
      return Result(ProjectilePacket27DecodeStatus.TrailingBytes);
    }

    return Map(packet);
  }

  /// <summary>
  /// Maps an already decoded packet payload without parsing its bytes again.
  /// The caller remains responsible for validating the sender against the
  /// packet's declared owner before submitting the command.
  /// </summary>
  public ProjectilePacket27DecodeResult Map(SyncProjectilePacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (TryMap(packet, out ProjectileNetworkApplyCommand command))
    {
      return new ProjectilePacket27DecodeResult(
        ProjectilePacket27DecodeStatus.Decoded,
        command);
    }

    return Result(ProjectilePacket27DecodeStatus.Invalid);
  }

  public bool TryMap(
    SyncProjectilePacket packet,
    out ProjectileNetworkApplyCommand command)
  {
    ArgumentNullException.ThrowIfNull(packet);

    int projectileUuid = packet.ProjectileUuid is short uuid && uuid >= 0 && uuid < 1000
      ? uuid
      : -1;
    try
    {
      command = new ProjectileNetworkApplyCommand(
        packet.Owner,
        packet.Identity,
        packet.ProjectileType,
        new Vector2(packet.Position.X, packet.Position.Y),
        new Vector2(packet.Velocity.X, packet.Velocity.Y),
        packet.Damage ?? 0,
        packet.OriginalDamage ?? 0,
        packet.Knockback ?? 0.0f,
        packet.Ai0,
        packet.Ai1,
        packet.Ai2,
        projectileUuid,
        packet.BannerId ?? 0);
      return true;
    }
    catch (ArgumentOutOfRangeException)
    {
      command = default;
      return false;
    }
  }

  /// <summary>
  /// Creates the generated packet DTO for a profile-bound connection.
  /// The profile's packet binding remains responsible for encoding and
  /// validating the type-specific UUID protocol fact.
  /// </summary>
  public bool TryMapToPacket(
    ProjectileNetworkApplyCommand command,
    out SyncProjectilePacket packet)
  {
    packet = new SyncProjectilePacket();
    if (!CanEncode(command))
    {
      return false;
    }

    packet = CreatePacket(command, command.ProjectileUuid >= 0);
    return true;
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
    if (!CanEncode(command) || includeUuid != (command.ProjectileUuid >= 0))
    {
      return false;
    }

    SyncProjectilePacket packet = CreatePacket(command, includeUuid);

    using var body = new MemoryStream();
    var writer = new PacketWireWriter(body);
    try
    {
      SyncProjectilePacket.Write(writer, packet.Identity, packet.Position, packet.Velocity,
        packet.Owner, packet.ProjectileType, packet.Ai0, packet.Ai1, packet.BannerId,
        packet.Damage, packet.Knockback, packet.OriginalDamage, packet.ProjectileUuid,
        packet.Ai2, _ => includeUuid);
    }
    catch (PacketWireFormatException)
    {
      return false;
    }

    payload = body.ToArray();
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

  private static SyncProjectilePacket CreatePacket(
    ProjectileNetworkApplyCommand command,
    bool includeUuid)
  {
    return new SyncProjectilePacket {
      Identity = (short)command.Identity,
      Position = new PacketVector2(command.Position.X, command.Position.Y),
      Velocity = new PacketVector2(command.Velocity.X, command.Velocity.Y),
      Owner = (byte)command.OwnerSlot,
      ProjectileType = (short)command.ProjectileType,
      Ai0 = command.Ai0,
      Ai1 = command.Ai1,
      BannerId = command.BannerIdToRespondTo == 0 ? null : (ushort)command.BannerIdToRespondTo,
      Damage = command.Damage == 0 ? null : (short)command.Damage,
      Knockback = command.Knockback == 0.0f ? null : command.Knockback,
      OriginalDamage = command.OriginalDamage == 0 ? null : (short)command.OriginalDamage,
      ProjectileUuid = includeUuid ? (short)command.ProjectileUuid : null,
      Ai2 = command.Ai2
    };
  }

  private static bool IsFinite(Vector2 value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }

  private static ProjectilePacket27DecodeResult Result(ProjectilePacket27DecodeStatus status)
  {
    return new ProjectilePacket27DecodeResult(status, null);
  }
}
