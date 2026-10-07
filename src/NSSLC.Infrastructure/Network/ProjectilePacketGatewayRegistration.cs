using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Network;
using Terraria.Projectile;

namespace NSSLC.Infrastructure.Network;

/// <summary>
/// Registers packet-27 and packet-29 handlers using the bound sender identity.
/// </summary>
public static class ProjectilePacketGatewayRegistration
{
  public static void Register(
    PacketGateway gateway,
    IProjectileNetworkCommandOwner owner,
    PacketPolicy packet27Policy,
    PacketPolicy packet29Policy)
  {
    ArgumentNullException.ThrowIfNull(gateway);
    ArgumentNullException.ThrowIfNull(owner);
    ValidatePolicy(packet27Policy, 27, nameof(packet27Policy));
    ValidatePolicy(packet29Policy, 29, nameof(packet29Policy));

    gateway.Register(
      packet27Policy,
      new Packet27Handler(owner, new ProjectilePacket27Codec()));
    gateway.Register(
      packet29Policy,
      new Packet29Handler(owner, new ProjectilePacket29Codec()));
  }

  private static void ValidatePolicy(
    PacketPolicy policy,
    byte expectedMessageId,
    string parameterName)
  {
    ArgumentNullException.ThrowIfNull(policy);
    if (policy.MessageId != expectedMessageId ||
      policy.AllowedStages != NetworkSessionStage.Active ||
      policy.ModuleId is not null ||
      policy.Action is not null ||
      policy.RequiresHost ||
      policy.MaximumPerWindow < 1 ||
      policy.MaximumBytesPerWindow < 1)
    {
      throw new ArgumentException(
        "Projectile network policies must admit only their active-session packet.",
        parameterName);
    }
  }

  private sealed class Packet27Handler : IPacketHandler<SyncProjectilePacket>
  {
    private readonly IProjectileNetworkCommandOwner _owner;
    private readonly ProjectilePacket27Codec _codec;

    public Packet27Handler(
      IProjectileNetworkCommandOwner owner,
      ProjectilePacket27Codec codec)
    {
      _owner = owner;
      _codec = codec;
    }

    public ValueTask<PacketHandlingResult> HandleAsync(
      NetworkSessionContext context,
      SyncProjectilePacket packet,
      CancellationToken cancellationToken)
    {
      ProjectilePacket27DecodeResult result = _codec.Map(packet);
      if (result.Status != ProjectilePacket27DecodeStatus.Decoded ||
        result.Command is not ProjectileNetworkApplyCommand wireCommand)
      {
        return ValueTask.FromResult(Reject("InvalidProjectilePacket27"));
      }

      int trustedOwnerSlot = wireCommand.ProjectileType == 949
        ? byte.MaxValue
        : context.Actor.PlayerSlot;
      var command = new ProjectileNetworkApplyCommand(
        trustedOwnerSlot,
        wireCommand.Identity,
        wireCommand.ProjectileType,
        wireCommand.Position,
        wireCommand.Velocity,
        wireCommand.Damage,
        wireCommand.OriginalDamage,
        wireCommand.Knockback,
        wireCommand.Ai0,
        wireCommand.Ai1,
        wireCommand.Ai2,
        wireCommand.ProjectileUuid,
        wireCommand.BannerIdToRespondTo);
      return _owner.ApplyProjectileAsync(context, command, cancellationToken);
    }
  }

  private sealed class Packet29Handler : IPacketHandler<KillProjectilePacket>
  {
    private readonly IProjectileNetworkCommandOwner _owner;
    private readonly ProjectilePacket29Codec _codec;

    public Packet29Handler(
      IProjectileNetworkCommandOwner owner,
      ProjectilePacket29Codec codec)
    {
      _owner = owner;
      _codec = codec;
    }

    public ValueTask<PacketHandlingResult> HandleAsync(
      NetworkSessionContext context,
      KillProjectilePacket packet,
      CancellationToken cancellationToken)
    {
      ProjectilePacket29DecodeResult result = _codec.Map(packet);
      if (result.Status != ProjectilePacket29DecodeStatus.Decoded ||
        result.Command is not ProjectileNetworkTerminateCommand wireCommand)
      {
        return ValueTask.FromResult(Reject("InvalidProjectilePacket29"));
      }

      var command = new ProjectileNetworkTerminateCommand(
        context.Actor.PlayerSlot,
        wireCommand.Identity);
      return _owner.TerminateProjectileAsync(context, command, cancellationToken);
    }
  }

  private static PacketHandlingResult Reject(string rejectionCode)
  {
    return new PacketHandlingResult(false, rejectionCode: rejectionCode);
  }
}
