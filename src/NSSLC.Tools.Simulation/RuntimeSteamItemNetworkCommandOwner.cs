using Terraria.Items;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using NSSLC.Infrastructure.Network;
namespace Terraria.NonAuthoritative.SimulationHost;

/// <summary>Applies Steam packet 21 to the B6 world-item store on its loaded-session owner.</summary>
internal sealed class RuntimeSteamItemNetworkCommandOwner : ISteamItemNetworkCommandOwner
{
  private const short NewItemSentinel = 400;

  private readonly NetworkWorldOwner _worldOwner;
  private readonly RuntimePlayerStore _players;
  private readonly RuntimeWorldItemStore _worldItems;
  private readonly Func<NetworkSessionContext, bool> _isCurrentSender;
  private readonly Func<long> _currentTick;
  private readonly int _itemLifetimeTicks;
  private readonly long _reservationDurationTicks;

  public RuntimeSteamItemNetworkCommandOwner(
    NetworkWorldOwner worldOwner,
    RuntimePlayerStore players,
    RuntimeWorldItemStore worldItems,
    Func<NetworkSessionContext, bool> isCurrentSender,
    Func<long> currentTick,
    int itemLifetimeTicks,
    long reservationDurationTicks)
  {
    _worldOwner = worldOwner ?? throw new ArgumentNullException(nameof(worldOwner));
    _players = players ?? throw new ArgumentNullException(nameof(players));
    _worldItems = worldItems ?? throw new ArgumentNullException(nameof(worldItems));
    _isCurrentSender = isCurrentSender ?? throw new ArgumentNullException(nameof(isCurrentSender));
    _currentTick = currentTick ?? throw new ArgumentNullException(nameof(currentTick));
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(itemLifetimeTicks);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(reservationDurationTicks);
    _itemLifetimeTicks = itemLifetimeTicks;
    _reservationDurationTicks = reservationDurationTicks;
  }

  public async ValueTask<PacketHandlingResult> ApplySyncItemAsync(
    NetworkSessionContext context,
    SyncItemPacket packet,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(packet);
    cancellationToken.ThrowIfCancellationRequested();

    if (packet.StateFlags != 0 ||
        packet.Shimmered.HasValue ||
        packet.ShimmerTime.HasValue ||
        packet.EnemyGrabDelayTime.HasValue)
    {
      return Reject("UnsupportedWorldItemStateFlags");
    }

    RuntimeWorldItemNetworkApplyResult applyResult = await _worldOwner.InvokeAsync(
      session => ApplyOnOwner(session, context, packet),
      cancellationToken).ConfigureAwait(false);
    if (!applyResult.Applied)
    {
      return Reject(applyResult.RejectionCode);
    }

    SyncItemPacket authoritativeSync = new()
    {
      ItemIndex = checked((short)applyResult.Projection.ItemSlot),
      PositionX = applyResult.Projection.PositionX,
      PositionY = applyResult.Projection.PositionY,
      VelocityX = applyResult.Projection.VelocityX,
      VelocityY = applyResult.Projection.VelocityY,
      Stack = checked((short)applyResult.Projection.Stack),
      Prefix = applyResult.Projection.Prefix,
      StateFlags = 0,
      ItemType = checked((short)applyResult.Projection.TypeId)
    };

    if (!applyResult.Created)
    {
      return new PacketHandlingResult(
        accepted: true,
        [new OutboundDispatch(
          authoritativeSync,
          PacketDispatchKind.AllActiveExceptSender)]);
    }

    var ownerProjection = new ItemOwnerPacket
    {
      ItemIndex = checked((short)applyResult.Projection.ItemSlot),
      ReservedForPlayer = applyResult.Projection.ReservedForPlayer,
      TimeToKeepReservation = applyResult.Projection.ReservationTicks,
      GrabDelayPlayer = applyResult.Projection.GrabDelayPlayer,
      GrabDelayTime = applyResult.Projection.IgnoreOwnerDelayTicks,
      PositionX = applyResult.Projection.PositionX,
      PositionY = applyResult.Projection.PositionY
    };

    return new PacketHandlingResult(
      accepted: true,
      [
        new OutboundDispatch(
          authoritativeSync,
          PacketDispatchKind.Single,
          [context.Connection]),
        new OutboundDispatch(
          authoritativeSync,
          PacketDispatchKind.AllActiveExceptSender),
        new OutboundDispatch(
          ownerProjection,
          PacketDispatchKind.Single,
          [context.Connection]),
        new OutboundDispatch(
          ownerProjection,
          PacketDispatchKind.AllActiveExceptSender)
      ]);
  }

  private RuntimeWorldItemNetworkApplyResult ApplyOnOwner(
    Terraria.NonAuthoritative.Persistence.LoadedWorldSession session,
    NetworkSessionContext context,
    SyncItemPacket packet)
  {
    if (context.Stage != NetworkSessionStage.Active ||
        context.Actor.GameSessionKey == Guid.Empty ||
        !_isCurrentSender(context))
    {
      return RuntimeWorldItemNetworkApplyResult.Rejected("StaleSenderBinding");
    }

    if (!_worldItems.IsBoundTo(session) || !_players.IsBoundTo(session))
    {
      return RuntimeWorldItemNetworkApplyResult.Rejected("WorldRuntimeChanged");
    }

    if (context.WorldRuntimeId is not { } expectedRuntimeId ||
        expectedRuntimeId != session.EntityRuntime.RuntimeId)
    {
      return RuntimeWorldItemNetworkApplyResult.Rejected("WorldRuntimeChanged");
    }

    int playerSlot = context.Actor.PlayerSlot;
    if (!_players.TryGetEntityReference(playerSlot, out Terraria.Relationships.EntityReference playerReference))
    {
      return RuntimeWorldItemNetworkApplyResult.Rejected("InactivePlayerSlot");
    }

    var command = new RuntimeWorldItemNetworkCommand(
      packet.ItemIndex,
      packet.ItemType,
      packet.Stack,
      packet.Prefix,
      packet.PositionX,
      packet.PositionY,
      packet.VelocityX,
      packet.VelocityY);
    long currentTick = _currentTick();
    bool created = packet.ItemIndex == NewItemSentinel;
    bool applied = _worldItems.TryApplyNetworkSync(
      command,
      _players,
      RuntimeEntityId.FromEntityReference(playerReference),
      checked((byte)playerSlot),
      currentTick,
      _itemLifetimeTicks,
      _reservationDurationTicks,
      out RuntimeWorldItemNetworkProjection projection,
      out string rejectionCode);
    return applied
      ? RuntimeWorldItemNetworkApplyResult.Accepted(projection, created)
      : RuntimeWorldItemNetworkApplyResult.Rejected(rejectionCode);
  }

  private static PacketHandlingResult Reject(string rejectionCode) =>
    new(accepted: false, rejectionCode: rejectionCode);
}

internal readonly record struct RuntimeWorldItemNetworkApplyResult(
  bool Applied,
  bool Created,
  RuntimeWorldItemNetworkProjection Projection,
  string RejectionCode)
{
  public static RuntimeWorldItemNetworkApplyResult Accepted(
    RuntimeWorldItemNetworkProjection projection,
    bool created) =>
    new(true, created, projection, string.Empty);

  public static RuntimeWorldItemNetworkApplyResult Rejected(string rejectionCode) =>
    new(false, false, default, rejectionCode);
}
