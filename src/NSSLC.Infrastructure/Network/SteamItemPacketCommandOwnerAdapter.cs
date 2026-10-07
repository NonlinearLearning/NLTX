using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

/// <summary>Maps Steam item ingress packets into Application commands and wire projections.</summary>
public sealed class SteamItemPacketCommandOwnerAdapter :
  ISteamItemNetworkCommandOwner,
  ISteamItemDespawnCommandOwner
{
  private const short MaximumSteamItemSlots = 400;

  private readonly NetworkWorldItemOwner _owner;

  public SteamItemPacketCommandOwnerAdapter(NetworkWorldItemOwner owner)
  {
    _owner = owner ?? throw new ArgumentNullException(nameof(owner));
  }

  /// <summary>Creates a targeted Steam 160 dispatch for one activated section.</summary>
  public static OutboundDispatch ToSteamItemPositionDispatch(
    NetworkWorldItemPositionProjection projection,
    ConnectionIdentity target)
  {
    if (projection.ItemIndex < 0 || projection.ItemIndex >= MaximumSteamItemSlots ||
      !float.IsFinite(projection.PositionX) || !float.IsFinite(projection.PositionY))
    {
      throw new ArgumentException(
        "Steam item position projection contains an invalid slot or position.",
        nameof(projection));
    }

    return new OutboundDispatch(
      new ItemPositionPacket
      {
        ItemIndex = projection.ItemIndex,
        Position = new PacketVector2(projection.PositionX, projection.PositionY)
      },
      PacketDispatchKind.Single,
      [target]);
  }

  /// <summary>Creates a targeted Steam 22 dispatch from committed world-item state.</summary>
  public static OutboundDispatch ToSteamItemOwnerDispatch(
    NetworkWorldItemOwnershipProjection projection,
    ConnectionIdentity target)
  {
    if (projection.ItemIndex < 0 || projection.ItemIndex >= MaximumSteamItemSlots ||
      projection.TimeToKeepReservation < 0 || projection.GrabDelayTime < 0 ||
      !float.IsFinite(projection.PositionX) || !float.IsFinite(projection.PositionY))
    {
      throw new ArgumentException(
        "Steam item ownership projection contains an invalid slot, timer, or position.",
        nameof(projection));
    }

    return new OutboundDispatch(
      new ItemOwnerPacket
      {
        ItemIndex = projection.ItemIndex,
        ReservedForPlayer = projection.ReservedForPlayer,
        TimeToKeepReservation = projection.TimeToKeepReservation,
        GrabDelayPlayer = projection.GrabDelayPlayer,
        GrabDelayTime = projection.GrabDelayTime,
        PositionX = projection.PositionX,
        PositionY = projection.PositionY
      },
      PacketDispatchKind.Single,
      [target]);
  }

  public async ValueTask<PacketHandlingResult> ApplySyncItemAsync(
    NetworkSessionContext context,
    SyncItemPacket packet,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(packet);
    cancellationToken.ThrowIfCancellationRequested();

    var command = new NetworkWorldItemSyncCommand(
      packet.ItemIndex,
      packet.ItemType,
      packet.Stack,
      packet.Prefix,
      packet.StateFlags,
      packet.PositionX,
      packet.PositionY,
      packet.VelocityX,
      packet.VelocityY,
      packet.Shimmered,
      packet.ShimmerTime,
      packet.EnemyGrabDelayTime);
    PacketHandlingResult result = await _owner.ApplySyncItemAsync(
      context,
      command,
      cancellationToken).ConfigureAwait(false);
    if (!result.Accepted)
    {
      return new PacketHandlingResult(
        accepted: false,
        rejectionCode: result.RejectionCode);
    }

    OutboundDispatch[] dispatches = result.Outbound
      .Select(ToSteamDispatch)
      .ToArray();
    return new PacketHandlingResult(accepted: true, dispatches);
  }

  public async ValueTask<PacketHandlingResult> ApplyDespawnItemAsync(
    NetworkSessionContext context,
    SyncItemDespawnPacket packet,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(packet);
    cancellationToken.ThrowIfCancellationRequested();

    PacketHandlingResult result = await _owner.ApplyDespawnItemAsync(
      context,
      new NetworkWorldItemDespawnCommand(packet.ItemIndex),
      cancellationToken).ConfigureAwait(false);
    if (!result.Accepted)
    {
      return new PacketHandlingResult(
        accepted: false,
        rejectionCode: result.RejectionCode);
    }

    OutboundDispatch[] dispatches = result.Outbound
      .Select(ToSteamDespawnDispatch)
      .ToArray();
    return new PacketHandlingResult(accepted: true, dispatches);
  }

  private static OutboundDispatch ToSteamDispatch(OutboundDispatch dispatch)
  {
    if (dispatch.Packet is not NetworkWorldItemSyncProjection projection)
    {
      throw new InvalidOperationException(
        "The Application world-item owner returned an unsupported packet projection.");
    }

    byte stateFlags = 0;
    bool? shimmered = null;
    float? shimmerTime = null;
    byte? enemyGrabDelayTime = null;
    if (projection.IsShimmered || projection.ShimmerTime > 0.0f)
    {
      stateFlags |= 1 << 2;
      shimmered = projection.IsShimmered;
      shimmerTime = projection.ShimmerTime;
    }

    if (projection.EnemyGrabDelayTime > 0)
    {
      stateFlags |= 1 << 3;
      enemyGrabDelayTime = projection.EnemyGrabDelayTime;
    }

    var packet = new SyncItemPacket
    {
      ItemIndex = projection.ItemIndex,
      PositionX = projection.PositionX,
      PositionY = projection.PositionY,
      VelocityX = projection.VelocityX,
      VelocityY = projection.VelocityY,
      Stack = projection.Stack,
      Prefix = projection.Prefix,
      StateFlags = stateFlags,
      ItemType = projection.ItemType,
      Shimmered = shimmered,
      ShimmerTime = shimmerTime,
      EnemyGrabDelayTime = enemyGrabDelayTime
    };
    return new OutboundDispatch(
      packet,
      dispatch.Kind,
      dispatch.Targets,
      dispatch.AllowedStages,
      dispatch.WorldKey,
      dispatch.WorldGeneration,
      dispatch.Section,
      dispatch.SnapshotRevision,
      dispatch.SnapshotVariant);
  }

  private static OutboundDispatch ToSteamDespawnDispatch(OutboundDispatch dispatch)
  {
    if (dispatch.Packet is not NetworkWorldItemDespawnProjection projection)
    {
      throw new InvalidOperationException(
        "The Application world-item owner returned an unsupported despawn projection.");
    }

    return new OutboundDispatch(
      new SyncItemDespawnPacket { ItemIndex = projection.ItemIndex },
      dispatch.Kind,
      dispatch.Targets,
      dispatch.AllowedStages,
      dispatch.WorldKey,
      dispatch.WorldGeneration,
      dispatch.Section,
      dispatch.SnapshotRevision,
      dispatch.SnapshotVariant);
  }
}
