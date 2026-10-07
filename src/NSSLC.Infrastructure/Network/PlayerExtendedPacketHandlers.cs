using System.Numerics;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Network;
using Terraria.Player;

namespace NSSLC.Infrastructure.Network;

/// <summary>
/// Handles player packets whose server behavior is a presentation relay or a
/// small lifecycle projection. World and Items remain owners of teleport rules
/// and inventory payloads respectively.
/// </summary>
public sealed class PlayerExtendedPacketHandlers :
  IPacketHandler<ManaEffectPacket>,
  IPacketHandler<MiscDataSyncPacket>,
  IPacketHandler<RequestTeleportationByServerPacket>,
  IPacketHandler<TeleportPlayerThroughPortalPacket>,
  IPacketHandler<NebulaLevelupRequestPacket>,
  IPacketHandler<DeadPlayerPacket>,
  IPacketHandler<SyncTilePickingPacket>,
  IPacketHandler<SyncLoadoutPacket>,
  IPacketHandler<SpectatePlayerPacket>,
  IPacketHandler<TeamChangeFromUIPacket>
{
  private readonly NetworkPlayerOwner _players;
  private readonly NetworkWorldOwner? _worldOwner;
  private readonly WorldSynchronizationPacketHandlers? _worldSections;

  public PlayerExtendedPacketHandlers(NetworkPlayerOwner players)
      : this(players, null, null)
  {
  }

  public PlayerExtendedPacketHandlers(
      NetworkPlayerOwner players,
      NetworkWorldOwner? worldOwner,
      WorldSynchronizationPacketHandlers? worldSections)
  {
    _players = players ?? throw new ArgumentNullException(nameof(players));
    if ((worldOwner is null) != (worldSections is null))
    {
      throw new ArgumentException(
          "Team-based spawn composition requires both the world owner and section projector.");
    }

    _worldOwner = worldOwner;
    _worldSections = worldSections;
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    ManaEffectPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    NetworkPlayerSnapshot? sender = await _players.CapturePlayerAsync(
      context, cancellationToken).ConfigureAwait(false);
    return sender is NetworkPlayerSnapshot player
      ? Relayed(new ManaEffectPacket {
        Player = player.PlayerSlot,
        ManaEffect = packet.ManaEffect
      })
      : RejectedResult("UnauthenticatedPlayer");
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    MiscDataSyncPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    NetworkPlayerSnapshot? sender = await _players.CapturePlayerAsync(
      context, cancellationToken).ConfigureAwait(false);
    if (sender is not NetworkPlayerSnapshot player)
    {
      return RejectedResult("UnauthenticatedPlayer");
    }

    if (packet.Action == 2)
    {
      return Relayed(new MiscDataSyncPacket {
        Player = player.PlayerSlot,
        Action = packet.Action
      });
    }

    return packet.Action switch {
      // The exact vanilla case-2 branch is a server relay excluding whoAmI.
      // Its client-side SFX is not owned by the headless server.
      1 => RejectedResult("SkeletronSpawnOwnerUnavailable"),
      3 or 6 => RejectedResult("WorldDialOwnerUnavailable"),
      4 => RejectedResult("MimicNpcEffectOwnerUnavailable"),
      5 => RejectedResult("BestiaryKillOwnerUnavailable"),
      _ => RejectedResult("UnknownPlayerMiscAction")
    };
  }

  public ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    RequestTeleportationByServerPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    // Destination selection is a World-owned rule. Do not acknowledge a
    // request until that owner supplies a validated destination and commit.
    return packet.TeleportationKind <= 4
      ? ValueTask.FromResult(RejectedResult("TeleportDestinationUnavailable"))
      : ValueTask.FromResult(RejectedResult("InvalidTeleportationKind"));
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    TeleportPlayerThroughPortalPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (!IsFinite(packet.PositionX) || !IsFinite(packet.PositionY) ||
      !IsFinite(packet.VelocityX) || !IsFinite(packet.VelocityY))
    {
      return RejectedResult("InvalidPortalTeleport");
    }

    NetworkPlayerSnapshot? current = await _players.CapturePlayerAsync(
      context, cancellationToken).ConfigureAwait(false);
    if (current is not NetworkPlayerSnapshot currentPlayer ||
      !currentPlayer.HasPendingTeleport ||
      currentPlayer.PendingTeleportPosition != new Vector2(packet.PositionX, packet.PositionY))
    {
      return RejectedResult("UnconfirmedPortalTeleport");
    }

    NetworkPlayerMovementResult result = await _players.CommitMovementAsync(
      context,
      new NetworkPlayerMovementInput(
        ApplyPosition: true,
        Position: new Vector2(packet.PositionX, packet.PositionY),
        ApplyVelocity: true,
        Velocity: new Vector2(packet.VelocityX, packet.VelocityY),
        AcknowledgeTeleport: true),
      cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded || result.Player is not NetworkPlayerSnapshot player)
    {
      return RejectedResult("InvalidPortalTeleport");
    }

    return Relayed(new TeleportPlayerThroughPortalPacket {
      Player = player.PlayerSlot,
      PortalColorIndex = packet.PortalColorIndex % 2 == 0
        ? (short)(packet.PortalColorIndex + 1)
        : (short)(packet.PortalColorIndex - 1),
      PositionX = packet.PositionX,
      PositionY = packet.PositionY,
      VelocityX = packet.VelocityX,
      VelocityY = packet.VelocityY
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    NebulaLevelupRequestPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (!IsFinite(packet.Position.X) || !IsFinite(packet.Position.Y))
    {
      return RejectedResult("InvalidNebulaLevelupPosition");
    }

    NetworkPlayerSnapshot? sender = await _players.CapturePlayerAsync(
      context, cancellationToken).ConfigureAwait(false);
    if (sender is not NetworkPlayerSnapshot player)
    {
      return RejectedResult("UnauthenticatedPlayer");
    }

    return new PacketHandlingResult(true, [
      new OutboundDispatch(new NebulaLevelupRequestPacket {
        Player = player.PlayerSlot,
        ItemType = packet.ItemType,
        Position = packet.Position
      }, PacketDispatchKind.AllActive)
    ]);
  }

  public ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    DeadPlayerPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    return ValueTask.FromResult(RejectedResult("ClientOnlyPlayerEffect"));
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    SyncTilePickingPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    NetworkPlayerSnapshot? sender = await _players.CapturePlayerAsync(
      context, cancellationToken).ConfigureAwait(false);
    if (sender is not NetworkPlayerSnapshot player)
    {
      return RejectedResult("UnauthenticatedPlayer");
    }

    return new PacketHandlingResult(true, [
      new OutboundDispatch(new SyncTilePickingPacket {
        Player = player.PlayerSlot,
        X = packet.X,
        Y = packet.Y,
        TileType = packet.TileType
      }, PacketDispatchKind.AllActiveExceptSender)
    ]);
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    SyncLoadoutPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.LoadoutIndex >= PlayerLoadoutStateComponent.LoadoutCount)
    {
      return RejectedResult("InvalidLoadoutIndex");
    }

    NetworkPlayerLoadoutResult result = await _players.ApplyLoadoutAsync(
      context,
      packet.LoadoutIndex,
      packet.AccessoryVisibilityMask,
      cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded || result.Player is not NetworkPlayerSnapshot player)
    {
      return RejectedResult(LoadoutStatusCode(result.Status));
    }

    return Relayed(new SyncLoadoutPacket {
      Player = player.PlayerSlot,
      LoadoutIndex = packet.LoadoutIndex,
      AccessoryVisibilityMask = result.AccessoryVisibilityMask
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    SpectatePlayerPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    NetworkPlayerSpectatingResult result = await _players.ApplySpectatingAsync(
      context, packet.TargetPlayer, cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded)
    {
      return RejectedResult("InvalidSpectatingTarget");
    }

    if (result.Target.Action ==
      Terraria.Player.PlayerLifecycleSystem.SpectatingTargetAction.None)
    {
      return new PacketHandlingResult(true);
    }

    if (result.Target.Action !=
      Terraria.Player.PlayerLifecycleSystem.SpectatingTargetAction.BroadcastServerTarget)
    {
      return RejectedResult("SpectatingFallbackUnavailable");
    }

    return Relayed(new SpectatePlayerPacket {
      Player = context.Actor.PlayerSlot,
      TargetPlayer = (short)result.Target.TargetPlayerSlot
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    TeamChangeFromUIPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.Team > 5)
    {
      return RejectedResult("InvalidPlayerTeam");
    }

    NetworkPlayerTeamChangeResult result = await _players.ApplyTeamChangeAsync(
      context, packet.Team, cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded || result.Player is not NetworkPlayerSnapshot player)
    {
      return RejectedResult(StatusCode(result.Status));
    }

    var outbound = new List<OutboundDispatch> {
      new(
        new TeamChangePacket {
          Player = player.PlayerSlot,
          Team = packet.Team
        },
        PacketDispatchKind.AllActiveExceptSender)
    };
    if (result.NotificationTargets.Count != 0)
    {
      outbound.Add(new OutboundDispatch(
        SocialPacketProducers.CreateTeamChangeMessage(
          player.CharacterName, packet.Team),
        PacketDispatchKind.ExplicitTargets,
        result.NotificationTargets));
    }

    SectionInterestProjection? interest = null;
    if (_worldOwner is not null && _worldSections is not null)
    {
      NetworkTeamSpawnPoint? spawnPoint = await _worldOwner.FindTeamSpawnPointAsync(
          packet.Team, cancellationToken).ConfigureAwait(false);
      if (spawnPoint is NetworkTeamSpawnPoint point)
      {
        outbound.AddRange(await _worldSections.CreateExtraSpawnSectionDispatchesAsync(
            context, point.X, point.Y, cancellationToken).ConfigureAwait(false));
        interest = _worldSections.CreateSectionInterestProjection(context);
        outbound.Add(new OutboundDispatch(
            new ExtraSpawnSectionLoadedPacket { Player = player.PlayerSlot },
            PacketDispatchKind.Single,
            [context.Connection],
            allowedStages: NetworkSessionStage.Active));
      }
    }

    return new PacketHandlingResult(true, outbound, interest: interest);
  }

  private static bool IsFinite(float value) => float.IsFinite(value);

  private static PacketHandlingResult RejectedResult(string code)
  {
    return new PacketHandlingResult(false, rejectionCode: code);
  }

  private static string LoadoutStatusCode(NetworkPlayerLoadoutStatus status)
  {
    return status switch {
      NetworkPlayerLoadoutStatus.RejectedStage => "PlayerLoadoutRequiresActiveSession",
      NetworkPlayerLoadoutStatus.RejectedSenderBinding => "RejectedSenderBinding",
      NetworkPlayerLoadoutStatus.RejectedWorldRuntime => "RejectedWorldRuntime",
      NetworkPlayerLoadoutStatus.RejectedPlayerSlot => "RejectedPlayerSlot",
      NetworkPlayerLoadoutStatus.RejectedInvalidState => "InvalidLoadoutState",
      _ => "PlayerNotFound"
    };
  }

  private static string StatusCode(NetworkPlayerStateMutationStatus status)
  {
    return status switch {
      NetworkPlayerStateMutationStatus.RejectedStage => "PlayerStateRequiresActiveSession",
      NetworkPlayerStateMutationStatus.RejectedSenderBinding => "RejectedSenderBinding",
      NetworkPlayerStateMutationStatus.RejectedWorldRuntime => "RejectedWorldRuntime",
      NetworkPlayerStateMutationStatus.RejectedPlayerSlot => "RejectedPlayerSlot",
      NetworkPlayerStateMutationStatus.RejectedInvalidState => "InvalidPlayerState",
      _ => "PlayerNotFound"
    };
  }

  private static PacketHandlingResult Relayed(object packet)
  {
    return new PacketHandlingResult(true, [
      new OutboundDispatch(packet, PacketDispatchKind.AllActiveExceptSender)
    ]);
  }
}
