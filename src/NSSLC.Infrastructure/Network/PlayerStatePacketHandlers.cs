using System.Numerics;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Player;

namespace NSSLC.Infrastructure.Network;

/// <summary>
/// Ingress handlers for the player-state messages whose authoritative state belongs to
/// the formal Player ECS components. Every declaration of Player in an incoming packet is
/// treated as untrusted; the authenticated sender or an explicitly authorized target is
/// selected by <see cref="NetworkPlayerOwner"/>.
/// </summary>
public sealed class PlayerStatePacketHandlers :
  IPacketHandler<SyncPlayerPacket>,
  IPacketHandler<PlayerSpawnPacket>,
  IPacketHandler<PlayerControlsPacket>,
  IPacketHandler<PlayerLifeManaPacket>,
  IPacketHandler<Unknown42Packet>,
  IPacketHandler<PlayerBuffsPacket>,
  IPacketHandler<TogglePVPPacket>,
  IPacketHandler<PlayerHealPacket>,
  IPacketHandler<SyncPlayerZonePacket>,
  IPacketHandler<SyncTalkNPCPacket>,
  IPacketHandler<ItemRotationAndAnimationPacket>,
  IPacketHandler<TeamChangePacket>,
  IPacketHandler<AddPlayerBuffPvPPacket>,
  IPacketHandler<Unknown66Packet>,
  IPacketHandler<QuestsCountSyncPacket>,
  IPacketHandler<PlayerStealthPacket>,
  IPacketHandler<MinionRestTargetUpdatePacket>,
  IPacketHandler<MinionAttackTargetUpdatePacket>,
  IPacketHandler<UpdatePlayerLuckFactorsPacket>
{
  private const int MaximumNpcIndex = 199;
  private readonly NetworkPlayerOwner _players;

  public PlayerStatePacketHandlers(NetworkPlayerOwner players)
  {
    _players = players ?? throw new ArgumentNullException(nameof(players));
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    SyncPlayerPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    float voicePitch = float.IsNaN(packet.VoicePitchOffset)
      ? 0.0f
      : Math.Clamp(packet.VoicePitchOffset, -1.0f, 1.0f);
    PlayerDifficulty difficulty = DecodeDifficulty(packet.DifficultyAndAccessoryFlags);
    PlayerNetworkIdentityInput input = new(
      packet.Name.Trim(),
      difficulty,
      Math.Clamp(packet.SkinVariant, (byte)0, (byte)11),
      Math.Clamp(packet.VoiceVariant, (byte)1, (byte)4),
      voicePitch,
      packet.Hair >= 228 ? (byte)0 : packet.Hair,
      packet.HairDye,
      packet.HiddenAccessories,
      packet.HideMisc,
      ToAppearanceColor(packet.HairColor),
      ToAppearanceColor(packet.SkinColor),
      ToAppearanceColor(packet.EyeColor),
      ToAppearanceColor(packet.ShirtColor),
      ToAppearanceColor(packet.UnderShirtColor),
      ToAppearanceColor(packet.PantsColor),
      ToAppearanceColor(packet.ShoeColor),
      packet.DifficultyAndAccessoryFlags,
      packet.BiomeAndCartFlags,
      packet.PermanentUpgradeFlags);
    NetworkPlayerMutationResult result = await _players.ApplyIdentityAsync(
      context, input, cancellationToken).ConfigureAwait(false);
    SyncPlayerPacket projection = new() {
      Player = context.Actor.PlayerSlot,
      SkinVariant = input.SkinVariant,
      VoiceVariant = input.VoiceVariant,
      VoicePitchOffset = input.VoicePitchOffset,
      Hair = input.Hair,
      Name = input.CharacterName,
      HairDye = packet.HairDye,
      HiddenAccessories = packet.HiddenAccessories,
      HideMisc = packet.HideMisc,
      HairColor = packet.HairColor,
      SkinColor = packet.SkinColor,
      EyeColor = packet.EyeColor,
      ShirtColor = packet.ShirtColor,
      UnderShirtColor = packet.UnderShirtColor,
      PantsColor = packet.PantsColor,
      ShoeColor = packet.ShoeColor,
      DifficultyAndAccessoryFlags = input.DifficultyAndAccessoryFlags,
      BiomeAndCartFlags = packet.BiomeAndCartFlags,
      PermanentUpgradeFlags = packet.PermanentUpgradeFlags
    };
    return RelayMutation(result, projection);
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    PlayerSpawnPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    PlayerSpawnPacket12Input input = new(
      DeclaredPlayerSlot: packet.Player,
      AuthenticatedSenderSlot: context.Actor.PlayerSlot,
      HasAuthenticatedSender: true,
      NetworkMode: PlayerLifecycleSystem.SpectatingNetworkMode.Server,
      SpawnX: packet.SpawnX,
      SpawnY: packet.SpawnY,
      RespawnRemainingTicks: packet.RespawnTimer,
      PveDeathCount: packet.PveDeaths,
      PvpDeathCount: packet.PvpDeaths,
      TeamId: packet.Team,
      SpawnContextValue: packet.SpawnContext,
      IsLocalPlayer: false,
      MultiplayerBroadcast: true,
      LastTimePlayerWasSavedBinary: 0L,
      CurrentUtcTime: DateTime.UtcNow);
    NetworkPlayerMutationResult result = await _players.CommitSpawnAsync(
      context, input, cancellationToken).ConfigureAwait(false);
    PacketHandlingResult projection = RelayMutation(result, new PlayerSpawnPacket {
      Player = context.Actor.PlayerSlot,
      SpawnX = packet.SpawnX,
      SpawnY = packet.SpawnY,
      RespawnTimer = packet.RespawnTimer,
      PveDeaths = packet.PveDeaths,
      PvpDeaths = packet.PvpDeaths,
      Team = packet.Team,
      SpawnContext = packet.SpawnContext
    });
    return result.Succeeded && context.Stage == NetworkSessionStage.Synchronizing
      ? new PacketHandlingResult(
        projection.Accepted,
        projection.Outbound,
        nextStage: NetworkSessionStage.Active,
        rejectionCode: projection.RejectionCode)
      : projection;
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    PlayerControlsPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.PotionOfReturnUsePosition.HasValue !=
        packet.PotionOfReturnHomePosition.HasValue)
    {
      return Rejected("InvalidPotionOfReturnState");
    }

    NetworkPlayerMovementInput input = new(
      ApplyPosition: true,
      Position: ToVector(packet.Position),
      ApplyVelocity: true,
      Velocity: packet.Velocity is PacketVector2 velocity
        ? ToVector(velocity)
        : Vector2.Zero,
      SelectedInventorySlot: packet.SelectedItem,
      ControlFlags: packet.ControlFlags,
      MovementFlags: packet.MovementFlags,
      PlayerFeatureFlags: packet.PlayerFeatureFlags,
      ActionFlags: packet.ActionFlags,
      MountType: packet.MountType,
      PotionOfReturnUsePosition: packet.PotionOfReturnUsePosition is PacketVector2 use
        ? ToVector(use)
        : null,
      PotionOfReturnHomePosition: packet.PotionOfReturnHomePosition is PacketVector2 home
        ? ToVector(home)
        : null,
      CameraTarget: packet.CameraTarget is PacketVector2 camera
        ? ToVector(camera)
        : null);
    NetworkPlayerMovementResult result = await _players.CommitMovementAsync(
      context, input, cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded || result.Player is not NetworkPlayerSnapshot player)
    {
      return Rejected(MovementStatusCode(result.Status));
    }

    return Relayed(new PlayerControlsPacket {
      Player = player.PlayerSlot,
      ControlFlags = packet.ControlFlags,
      MovementFlags = packet.MovementFlags,
      PlayerFeatureFlags = packet.PlayerFeatureFlags,
      ActionFlags = packet.ActionFlags,
      SelectedItem = checked((byte)player.SelectedInventorySlot),
      Position = new PacketVector2(player.Position.X, player.Position.Y),
      Velocity = new PacketVector2(player.Velocity.X, player.Velocity.Y),
      MountType = packet.MountType,
      PotionOfReturnUsePosition = packet.PotionOfReturnUsePosition,
      PotionOfReturnHomePosition = packet.PotionOfReturnHomePosition,
      CameraTarget = packet.CameraTarget
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    PlayerLifeManaPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.Life < 0 || packet.MaximumLife < 0 ||
        packet.Life > Math.Max((int)packet.MaximumLife,
          PlayerNetworkStateSystem.MinimumLifeMaximum))
    {
      return Rejected("InvalidPlayerLife");
    }

    int maximumLife = Math.Max(
      (int)packet.MaximumLife, PlayerNetworkStateSystem.MinimumLifeMaximum);
    NetworkPlayerMutationResult result = await _players.ApplyLifeAsync(
      context, packet.Life, maximumLife, cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded || result.Player is not NetworkPlayerSnapshot player)
    {
      return Rejected(MutationStatusCode(result.Status));
    }

    return Relayed(new PlayerLifeManaPacket {
      Player = player.PlayerSlot,
      Life = checked((short)player.StatLife),
      MaximumLife = checked((short)player.StatLifeMax)
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    Unknown42Packet packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    NetworkPlayerMutationResult result = await _players.ApplyManaAsync(
      context, packet.Mana, packet.MaximumMana, cancellationToken).ConfigureAwait(false);
    return result.Succeeded
      ? Accepted()
      : Rejected(MutationStatusCode(result.Status));
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    PlayerBuffsPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    NetworkPlayerMutationResult result = await _players.ApplyBuffsAsync(
      context, packet.BuffTypes, cancellationToken).ConfigureAwait(false);
    return result.Succeeded
      ? Relayed(new PlayerBuffsPacket {
        Player = context.Actor.PlayerSlot,
        BuffTypes = packet.BuffTypes.ToArray()
      })
      : Rejected(MutationStatusCode(result.Status));
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    TogglePVPPacket packet,
    CancellationToken cancellationToken)
  {
    NetworkPlayerStateMutationResult result = await _players.ApplyNetworkStateAsync(
      context,
      new NetworkPlayerStateUpdate(Hostile: packet.Hostile),
      cancellationToken).ConfigureAwait(false);
    return RelayOrReject(result, new TogglePVPPacket {
      Player = context.Actor.PlayerSlot,
      Hostile = packet.Hostile
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    PlayerHealPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    // Packet 35 is a client-side HealEffect. It must not change server life.
    NetworkPlayerSnapshot? player = await _players.CapturePlayerAsync(
      context, cancellationToken).ConfigureAwait(false);
    if (player is null)
    {
      return Rejected("UnauthenticatedPlayer");
    }

    return Relayed(new PlayerHealPacket {
      Player = player.Value.PlayerSlot,
      HealAmount = packet.HealAmount
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    SyncPlayerZonePacket packet,
    CancellationToken cancellationToken)
  {
    NetworkPlayerStateMutationResult result = await _players.ApplyZoneAsync(
      context,
      packet.Zone1,
      packet.Zone2,
      packet.Zone3,
      packet.Zone4,
      packet.Zone5,
      packet.TownNpcCount,
      cancellationToken).ConfigureAwait(false);
    return RelayOrReject(result, new SyncPlayerZonePacket {
      Player = context.Actor.PlayerSlot,
      Zone1 = packet.Zone1,
      Zone2 = packet.Zone2,
      Zone3 = packet.Zone3,
      Zone4 = packet.Zone4,
      Zone5 = packet.Zone5,
      TownNpcCount = packet.TownNpcCount
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    SyncTalkNPCPacket packet,
    CancellationToken cancellationToken)
  {
    if (packet.NpcIndex is < -1 or > MaximumNpcIndex)
    {
      return Rejected("InvalidTalkNpcIndex");
    }

    NetworkPlayerStateMutationResult result = await _players.ApplyNetworkStateAsync(
      context,
      new NetworkPlayerStateUpdate(TalkNpc: packet.NpcIndex),
      cancellationToken).ConfigureAwait(false);
    return RelayOrReject(result, new SyncTalkNPCPacket {
      Player = context.Actor.PlayerSlot,
      NpcIndex = packet.NpcIndex
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    ItemRotationAndAnimationPacket packet,
    CancellationToken cancellationToken)
  {
    if (!float.IsFinite(packet.ItemRotation) || packet.ItemAnimation < 0)
    {
      return Rejected("InvalidItemAnimation");
    }

    NetworkPlayerStateMutationResult result = await _players.ApplyItemAnimationAsync(
      context,
      packet.ItemRotation,
      packet.ItemAnimation,
      cancellationToken).ConfigureAwait(false);
    return RelayOrReject(result, new ItemRotationAndAnimationPacket {
      Player = context.Actor.PlayerSlot,
      ItemRotation = packet.ItemRotation,
      ItemAnimation = packet.ItemAnimation
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    TeamChangePacket packet,
    CancellationToken cancellationToken)
  {
    if (packet.Team > 5)
    {
      return Rejected("InvalidPlayerTeam");
    }

    NetworkPlayerTeamChangeResult result = await _players.ApplyTeamChangeAsync(
      context, packet.Team, cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded || result.Player is not NetworkPlayerSnapshot player)
    {
      return Rejected(StatusCode(result.Status));
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

    return new PacketHandlingResult(true, outbound);
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    AddPlayerBuffPvPPacket packet,
    CancellationToken cancellationToken)
  {
    if (packet.BuffTime <= 0)
    {
      return Rejected("InvalidPvpBuffTime");
    }

    NetworkPlayerStateMutationResult result = await _players.ApplyPvpBuffToTargetAsync(
      context,
      packet.Player,
      packet.BuffType,
      packet.BuffTime,
      cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded || result.Player is not NetworkPlayerSnapshot target)
    {
      return Rejected(StatusCode(result.Status));
    }

    return ValueTaskResult(new PacketHandlingResult(true, [
      new OutboundDispatch(
        new AddPlayerBuffPvPPacket {
          Player = target.PlayerSlot,
          BuffType = packet.BuffType,
          BuffTime = packet.BuffTime
        },
        PacketDispatchKind.Single,
        [target.Connection])
    ]));
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    Unknown66Packet packet,
    CancellationToken cancellationToken)
  {
    if (packet.LifeAmount <= 0)
    {
      // The vanilla server ignores non-positive increments and emits no packet.
      return Accepted();
    }

    NetworkPlayerStateMutationResult result = await _players.ApplyNetworkStateToTargetAsync(
      context,
      packet.Player,
      new NetworkPlayerStateUpdate(HealAmount: packet.LifeAmount),
      cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded || result.Player is not NetworkPlayerSnapshot target)
    {
      return Rejected(StatusCode(result.Status));
    }

    return Relayed(new Unknown66Packet {
      Player = target.PlayerSlot,
      LifeAmount = packet.LifeAmount
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    QuestsCountSyncPacket packet,
    CancellationToken cancellationToken)
  {
    NetworkPlayerStateMutationResult result = await _players.ApplyNetworkStateAsync(
      context,
      new NetworkPlayerStateUpdate(
        AnglerQuestsFinished: packet.AnglerQuestsFinished,
        GolferScoreAccumulated: packet.GolferScore),
      cancellationToken).ConfigureAwait(false);
    return RelayOrReject(result, new QuestsCountSyncPacket {
      Player = context.Actor.PlayerSlot,
      AnglerQuestsFinished = packet.AnglerQuestsFinished,
      GolferScore = packet.GolferScore
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    PlayerStealthPacket packet,
    CancellationToken cancellationToken)
  {
    NetworkPlayerStateMutationResult result = await _players.ApplyNetworkStateAsync(
      context,
      new NetworkPlayerStateUpdate(Stealth: packet.Stealth),
      cancellationToken).ConfigureAwait(false);
    return RelayOrReject(result, new PlayerStealthPacket {
      Player = context.Actor.PlayerSlot,
      Stealth = packet.Stealth
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    MinionRestTargetUpdatePacket packet,
    CancellationToken cancellationToken)
  {
    NetworkPlayerStateMutationResult result = await _players.ApplyNetworkStateAsync(
      context,
      new NetworkPlayerStateUpdate(
        MinionRestTarget: new Vector2(packet.TargetX, packet.TargetY)),
      cancellationToken).ConfigureAwait(false);
    return RelayOrReject(result, new MinionRestTargetUpdatePacket {
      Player = context.Actor.PlayerSlot,
      TargetX = packet.TargetX,
      TargetY = packet.TargetY
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    MinionAttackTargetUpdatePacket packet,
    CancellationToken cancellationToken)
  {
    if (packet.NpcIndex < -1)
    {
      return Rejected("InvalidMinionTarget");
    }

    NetworkPlayerStateMutationResult result = await _players.ApplyNetworkStateAsync(
      context,
      new NetworkPlayerStateUpdate(MinionAttackTarget: packet.NpcIndex),
      cancellationToken).ConfigureAwait(false);
    return RelayOrReject(result, new MinionAttackTargetUpdatePacket {
      Player = context.Actor.PlayerSlot,
      NpcIndex = packet.NpcIndex
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    UpdatePlayerLuckFactorsPacket packet,
    CancellationToken cancellationToken)
  {
    NetworkPlayerStateMutationResult result = await _players.ApplyNetworkStateAsync(
      context,
      new NetworkPlayerStateUpdate(Luck: new NetworkPlayerLuckUpdate(
        packet.LadyBugLuckTime,
        packet.TorchLuck,
        packet.LuckPotion,
        packet.HasGardenGnome,
        packet.BrokenMirrorBadLuck,
        packet.EquipmentLuckBonus,
        packet.CoinLuck,
        packet.KiteLuckLevel)),
      cancellationToken).ConfigureAwait(false);
    return RelayOrReject(result, new UpdatePlayerLuckFactorsPacket {
      Player = context.Actor.PlayerSlot,
      LadyBugLuckTime = packet.LadyBugLuckTime,
      TorchLuck = packet.TorchLuck,
      LuckPotion = packet.LuckPotion,
      HasGardenGnome = packet.HasGardenGnome,
      BrokenMirrorBadLuck = packet.BrokenMirrorBadLuck,
      EquipmentLuckBonus = packet.EquipmentLuckBonus,
      CoinLuck = packet.CoinLuck,
      KiteLuckLevel = packet.KiteLuckLevel
    });
  }

  private static PlayerDifficulty DecodeDifficulty(byte flags)
  {
    if ((flags & 0x08) != 0)
    {
      return PlayerDifficulty.Journey;
    }

    if ((flags & 0x02) != 0)
    {
      return PlayerDifficulty.Hardcore;
    }

    return (flags & 0x01) != 0
      ? PlayerDifficulty.Mediumcore
      : PlayerDifficulty.Classic;
  }

  private static PlayerAppearanceColor ToAppearanceColor(PacketRgb color)
  {
    return new PlayerAppearanceColor(color.Red, color.Green, color.Blue);
  }

  private static Vector2 ToVector(PacketVector2 vector)
  {
    return new Vector2(vector.X, vector.Y);
  }

  private static PacketHandlingResult RelayMutation(
    NetworkPlayerMutationResult result,
    object packet)
  {
    return result.Succeeded
      ? new PacketHandlingResult(true, [
        new OutboundDispatch(packet, PacketDispatchKind.AllActiveExceptSender)])
      : Rejected(MutationStatusCode(result.Status));
  }

  private static string MutationStatusCode(NetworkPlayerMutationStatus status)
  {
    return status switch {
      NetworkPlayerMutationStatus.RejectedStage => "PlayerStateRequiresActiveSession",
      NetworkPlayerMutationStatus.RejectedSenderBinding => "RejectedSenderBinding",
      NetworkPlayerMutationStatus.RejectedWorldRuntime => "RejectedWorldRuntime",
      NetworkPlayerMutationStatus.RejectedInvalidState => "InvalidPlayerState",
      _ => "PlayerNotFound"
    };
  }

  private static string MovementStatusCode(NetworkPlayerMovementStatus status)
  {
    return status switch {
      NetworkPlayerMovementStatus.RejectedSenderBinding => "RejectedSenderBinding",
      NetworkPlayerMovementStatus.RejectedWorldRuntime => "RejectedWorldRuntime",
      NetworkPlayerMovementStatus.RejectedInvalidState => "InvalidPlayerControls",
      _ => "PlayerNotFound"
    };
  }

  private static PacketHandlingResult RelayOrReject(
    NetworkPlayerStateMutationResult result,
    object packet)
  {
    return result.Succeeded
      ? new PacketHandlingResult(true, [
        new OutboundDispatch(packet, PacketDispatchKind.AllActiveExceptSender)])
      : new PacketHandlingResult(false, rejectionCode: StatusCode(result.Status));
  }

  private static PacketHandlingResult Relayed(object packet)
  {
    return new PacketHandlingResult(true, [
      new OutboundDispatch(packet, PacketDispatchKind.AllActiveExceptSender)]);
  }

  private static PacketHandlingResult Accepted()
  {
    return new PacketHandlingResult(true);
  }

  private static PacketHandlingResult Rejected(string code)
  {
    return new PacketHandlingResult(false, rejectionCode: code);
  }

  private static PacketHandlingResult ValueTaskResult(PacketHandlingResult result)
  {
    return result;
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
}
