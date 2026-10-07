using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

/// <summary>Registers Steam world-item ingress handlers with authoritative owners.</summary>
public static class SteamItemPacketGatewayRegistration
{
  public static void RegisterWorldItemIngress(
    PacketGateway gateway,
    NetworkWorldItemOwner owner,
    PacketPolicy syncItemPolicy,
    PacketPolicy despawnItemPolicy)
  {
    ArgumentNullException.ThrowIfNull(gateway);
    ArgumentNullException.ThrowIfNull(owner);
    ArgumentNullException.ThrowIfNull(syncItemPolicy);
    ArgumentNullException.ThrowIfNull(despawnItemPolicy);
    ValidateSyncItemPolicy(syncItemPolicy);
    ValidateDespawnItemPolicy(despawnItemPolicy);

    var adapter = new SteamItemPacketCommandOwnerAdapter(owner);
    Register(gateway, adapter, syncItemPolicy);
    RegisterItemDespawn(gateway, adapter, despawnItemPolicy);
  }

  public static void Register(
    PacketGateway gateway,
    NetworkWorldItemOwner owner,
    PacketPolicy syncItemPolicy)
  {
    ArgumentNullException.ThrowIfNull(gateway);
    ArgumentNullException.ThrowIfNull(owner);
    RegisterWorldItemIngress(
      gateway,
      owner,
      syncItemPolicy,
      new PacketPolicy(
        151,
        NetworkSessionStage.Active,
        MaximumPerWindow: 120,
        MaximumBytesPerWindow: 512));
  }

  public static void Register(
    PacketGateway gateway,
    ISteamItemNetworkCommandOwner owner,
    PacketPolicy syncItemPolicy)
  {
    ArgumentNullException.ThrowIfNull(gateway);
    ArgumentNullException.ThrowIfNull(owner);
    ArgumentNullException.ThrowIfNull(syncItemPolicy);
    ValidateSyncItemPolicy(syncItemPolicy);

    gateway.Register(syncItemPolicy, new SyncItemHandler(owner));
  }

  public static void RegisterItemDespawn(
    PacketGateway gateway,
    ISteamItemDespawnCommandOwner owner,
    PacketPolicy despawnItemPolicy)
  {
    ArgumentNullException.ThrowIfNull(gateway);
    ArgumentNullException.ThrowIfNull(owner);
    ArgumentNullException.ThrowIfNull(despawnItemPolicy);
    ValidateDespawnItemPolicy(despawnItemPolicy);

    gateway.Register(despawnItemPolicy, new DespawnItemHandler(owner));
  }

  private static void ValidateSyncItemPolicy(PacketPolicy syncItemPolicy)
  {
    if (syncItemPolicy.MessageId != 21 ||
      syncItemPolicy.AllowedStages != NetworkSessionStage.Active ||
      syncItemPolicy.ModuleId is not null ||
      syncItemPolicy.Action is not null ||
      syncItemPolicy.RequiresHost ||
      syncItemPolicy.MaximumPerWindow < 1 ||
      syncItemPolicy.MaximumBytesPerWindow < 1)
    {
      throw new ArgumentException(
        "Steam item sync must be admitted only as an active-session packet 21.",
        nameof(syncItemPolicy));
    }
  }

  private static void ValidateDespawnItemPolicy(PacketPolicy despawnItemPolicy)
  {
    if (despawnItemPolicy.MessageId != 151 ||
      despawnItemPolicy.AllowedStages != NetworkSessionStage.Active ||
      despawnItemPolicy.ModuleId is not null ||
      despawnItemPolicy.Action is not null ||
      despawnItemPolicy.RequiresHost ||
      despawnItemPolicy.MaximumPerWindow < 1 ||
      despawnItemPolicy.MaximumBytesPerWindow < 1)
    {
      throw new ArgumentException(
        "Steam item despawn must be admitted only as an active-session packet 151.",
        nameof(despawnItemPolicy));
    }
  }

  private sealed class SyncItemHandler : IPacketHandler<SyncItemPacket>
  {
    private readonly ISteamItemNetworkCommandOwner _owner;

    public SyncItemHandler(ISteamItemNetworkCommandOwner owner)
    {
      _owner = owner;
    }

    public ValueTask<PacketHandlingResult> HandleAsync(
      NetworkSessionContext context,
      SyncItemPacket packet,
      CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return _owner.ApplySyncItemAsync(context, packet, cancellationToken);
    }
  }

  private sealed class DespawnItemHandler : IPacketHandler<SyncItemDespawnPacket>
  {
    private readonly ISteamItemDespawnCommandOwner _owner;

    public DespawnItemHandler(ISteamItemDespawnCommandOwner owner)
    {
      _owner = owner;
    }

    public ValueTask<PacketHandlingResult> HandleAsync(
      NetworkSessionContext context,
      SyncItemDespawnPacket packet,
      CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return _owner.ApplyDespawnItemAsync(context, packet, cancellationToken);
    }
  }
}
