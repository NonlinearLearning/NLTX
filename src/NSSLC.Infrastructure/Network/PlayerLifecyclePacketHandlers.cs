using System.Numerics;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Player;

namespace NSSLC.Infrastructure.Network;

/// <summary>
/// Handles the player admission, lifecycle and control packets that mutate the
/// authenticated player entity. The packet Player field is always treated as an
/// untrusted projection; NetworkPlayerOwner resolves the authenticated sender.
/// </summary>
public sealed class PlayerLifecyclePacketHandlers :
  IPacketHandler<SyncPlayerPacket>,
  IPacketHandler<PlayerSpawnPacket>,
  IPacketHandler<PlayerControlsPacket>,
  IPacketHandler<PlayerActivePacket>,
  IPacketHandler<PlayerLifeManaPacket>,
  IPacketHandler<Unknown42Packet>,
  IPacketHandler<PlayerBuffsPacket>
{
  private const int MaximumPlayerNameLength = 20;
  private const int MaximumHair = 227;
  private const byte MaximumSkinVariant = 11;
  private const int MaximumWorldCoordinate = 16 * 2_000_000;
  private readonly NetworkPlayerOwner _players;
  private readonly int _worldWidthPixels;
  private readonly int _worldHeightPixels;

  public PlayerLifecyclePacketHandlers(
    NetworkPlayerOwner players,
    int worldWidthTiles = 0,
    int worldHeightTiles = 0)
  {
    _players = players ?? throw new ArgumentNullException(nameof(players));
    _worldWidthPixels = worldWidthTiles > 0
      ? checked(worldWidthTiles * 16)
      : MaximumWorldCoordinate;
    _worldHeightPixels = worldHeightTiles > 0
      ? checked(worldHeightTiles * 16)
      : MaximumWorldCoordinate;
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    SyncPlayerPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.Name.Trim().Length is 0 or > MaximumPlayerNameLength)
    {
      return Rejected("InvalidPlayerName");
    }

    PlayerDifficulty difficulty = DecodeDifficulty(packet.DifficultyAndAccessoryFlags);
    float voicePitch = float.IsNaN(packet.VoicePitchOffset)
      ? 0.0f
      : Math.Clamp(packet.VoicePitchOffset, -1.0f, 1.0f);
    var input = new PlayerNetworkIdentityInput(
      packet.Name.Trim(),
      difficulty,
      Math.Clamp(packet.SkinVariant, (byte)0, MaximumSkinVariant),
      (byte)Math.Clamp(packet.VoiceVariant, (byte)1, (byte)4),
      voicePitch,
      packet.Hair > MaximumHair ? (byte)0 : packet.Hair,
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

    NetworkPlayerMutationResult result = await _players.ApplyIdentityForAdmissionAsync(
      context, input, cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded)
    {
      return Rejected(StatusCode(result.Status));
    }

    return Relay(new SyncPlayerPacket {
      Player = result.Player!.Value.PlayerSlot,
      SkinVariant = Math.Clamp(packet.SkinVariant, (byte)0, MaximumSkinVariant),
      VoiceVariant = (byte)Math.Clamp(packet.VoiceVariant, (byte)1, (byte)4),
      VoicePitchOffset = voicePitch,
      Hair = packet.Hair > MaximumHair ? (byte)0 : packet.Hair,
      Name = packet.Name.Trim(),
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
      DifficultyAndAccessoryFlags = packet.DifficultyAndAccessoryFlags,
      BiomeAndCartFlags = packet.BiomeAndCartFlags,
      PermanentUpgradeFlags = packet.PermanentUpgradeFlags
    });
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    PlayerSpawnPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.SpawnContext > 3 || packet.RespawnTimer < 0 ||
        packet.PveDeaths < 0 || packet.PvpDeaths < 0 || packet.Team > 5)
    {
      return Rejected("InvalidSpawnState");
    }

    NetworkPlayerBindingResult ensured = await _players.EnsurePlayerAsync(
      context, cancellationToken).ConfigureAwait(false);
    if (!ensured.Succeeded)
    {
      return Rejected(StatusCode(ensured.Status));
    }

    NetworkPlayerMutationResult result = await _players.CommitSpawnAsync(
      context,
      new PlayerSpawnPacket12Input(
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
        LastTimePlayerWasSavedBinary: 0,
        CurrentUtcTime: DateTime.UtcNow),
      cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded)
    {
      return Rejected(StatusCode(result.Status));
    }

    var outbound = new List<OutboundDispatch> {
      new OutboundDispatch(
        new FinishedConnectingToServerPacket(),
        PacketDispatchKind.Single,
        [context.Connection],
        allowedStages: NetworkSessionStage.Active),
      new OutboundDispatch(
        new PlayerSpawnPacket {
          Player = result.Player!.Value.PlayerSlot,
          SpawnX = packet.SpawnX,
          SpawnY = packet.SpawnY,
          RespawnTimer = packet.RespawnTimer,
          PveDeaths = packet.PveDeaths,
          PvpDeaths = packet.PvpDeaths,
          Team = packet.Team,
          SpawnContext = packet.SpawnContext
        },
        PacketDispatchKind.AllActiveExceptSender)
    };
    if (context.Stage == NetworkSessionStage.Synchronizing)
    {
      outbound.Add(new OutboundDispatch(
        new PlayerActivePacket {
          Player = result.Player!.Value.PlayerSlot,
          ActiveState = 1
        },
        PacketDispatchKind.AllActiveExceptSender));
    }
    return new PacketHandlingResult(true, outbound, nextStage: NetworkSessionStage.Active);
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    PlayerControlsPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (!IsValidPosition(packet.Position) ||
        packet.Velocity is PacketVector2 velocity && !IsValidVelocity(velocity) ||
        packet.SelectedItem >= PlayerInventoryComponent.MainInventorySlotCount ||
        packet.PotionOfReturnUsePosition is PacketVector2 use && !IsFinite(use) ||
        packet.PotionOfReturnHomePosition is PacketVector2 home && !IsFinite(home) ||
        packet.CameraTarget is PacketVector2 camera && !IsFinite(camera))
    {
      return Rejected("InvalidPlayerControls");
    }

    NetworkPlayerMovementResult result = await _players.CommitMovementAsync(
      context,
      new NetworkPlayerMovementInput(
        ApplyPosition: true,
        Position: new Vector2(packet.Position.X, packet.Position.Y),
        ApplyVelocity: true,
        Velocity: packet.Velocity is PacketVector2 v ? new Vector2(v.X, v.Y) : default,
        SelectedInventorySlot: packet.SelectedItem,
        ControlFlags: packet.ControlFlags,
        MovementFlags: packet.MovementFlags,
        PlayerFeatureFlags: packet.PlayerFeatureFlags,
        ActionFlags: packet.ActionFlags,
        MountType: packet.MountType,
        PotionOfReturnUsePosition: ToVector(packet.PotionOfReturnUsePosition),
        PotionOfReturnHomePosition: ToVector(packet.PotionOfReturnHomePosition),
        CameraTarget: ToVector(packet.CameraTarget)),
      cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded)
    {
      return Rejected(StatusCode(result.Status));
    }

    NetworkPlayerSnapshot player = result.Player!.Value;
    return Relay(new PlayerControlsPacket {
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
    PlayerActivePacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    // Steam's server branch ignores client packet 14. Active state is projected
    // from the authenticated connection lifecycle, never accepted from a client.
    return new PacketHandlingResult(true);
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    PlayerLifeManaPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.Life < 0 || packet.MaximumLife < 0 ||
        packet.Life > Math.Max((int)packet.MaximumLife,
          (int)PlayerNetworkStateSystem.MinimumLifeMaximum))
    {
      return Rejected("InvalidPlayerLife");
    }

    int maximumLife = Math.Max((int)packet.MaximumLife,
      (int)PlayerNetworkStateSystem.MinimumLifeMaximum);
    NetworkPlayerMutationResult result = await _players.ApplyLifeAsync(
      context, packet.Life, maximumLife, cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded)
    {
      return Rejected(StatusCode(result.Status));
    }

    NetworkPlayerSnapshot player = result.Player!.Value;
    var outbound = new List<OutboundDispatch> {
      new OutboundDispatch(new PlayerLifeManaPacket {
        Player = player.PlayerSlot,
        Life = checked((short)player.StatLife),
        MaximumLife = checked((short)player.StatLifeMax)
      }, PacketDispatchKind.AllActiveExceptSender)
    };
    if (player.Dead)
    {
      outbound.Add(new OutboundDispatch(
        new DeadPlayerPacket { Player = player.PlayerSlot },
        PacketDispatchKind.AllActiveExceptSender));
    }

    return new PacketHandlingResult(true, outbound);
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
      ? new PacketHandlingResult(true)
      : Rejected(StatusCode(result.Status));
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(
    NetworkSessionContext context,
    PlayerBuffsPacket packet,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.BuffTypes.Count > PlayerBuffSlotsComponent.MaximumSlotCount)
    {
      return Rejected("TooManyPlayerBuffs");
    }

    NetworkPlayerMutationResult result = await _players.ApplyBuffsAsync(
      context, packet.BuffTypes, cancellationToken).ConfigureAwait(false);
    if (!result.Succeeded)
    {
      return Rejected(StatusCode(result.Status));
    }

    return Relay(new PlayerBuffsPacket {
      Player = result.Player!.Value.PlayerSlot,
      BuffTypes = packet.BuffTypes.ToArray()
    });
  }

  private bool IsValidPosition(PacketVector2 position)
  {
    return IsFinite(position) && position.X >= 0.0f && position.Y >= 0.0f &&
      position.X < _worldWidthPixels && position.Y < _worldHeightPixels;
  }

  private static bool IsValidVelocity(PacketVector2 velocity)
  {
    return IsFinite(velocity) && Math.Abs(velocity.X) <= 50.0f &&
      Math.Abs(velocity.Y) <= 50.0f;
  }

  private static bool IsFinite(PacketVector2 value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }

  private static Vector2? ToVector(PacketVector2? value)
  {
    return value is PacketVector2 vector ? new Vector2(vector.X, vector.Y) : null;
  }

  private static PlayerAppearanceColor ToAppearanceColor(PacketRgb color)
  {
    return new PlayerAppearanceColor(color.Red, color.Green, color.Blue);
  }

  private static PlayerDifficulty DecodeDifficulty(byte flags)
  {
    if ((flags & (1 << 3)) != 0)
    {
      return PlayerDifficulty.Journey;
    }

    if ((flags & (1 << 1)) != 0)
    {
      return PlayerDifficulty.Hardcore;
    }

    return (flags & 1) != 0 ? PlayerDifficulty.Mediumcore : PlayerDifficulty.Classic;
  }

  private static PacketHandlingResult Relay(object packet)
  {
    return new PacketHandlingResult(true, [
      new OutboundDispatch(packet, PacketDispatchKind.AllActiveExceptSender)
    ]);
  }

  private static PacketHandlingResult Rejected(string code)
  {
    return new PacketHandlingResult(false, rejectionCode: code);
  }

  private static string StatusCode(NetworkPlayerMutationStatus status)
  {
    return status switch {
      NetworkPlayerMutationStatus.RejectedStage => "PlayerStateRequiresActiveSession",
      NetworkPlayerMutationStatus.RejectedSenderBinding => "RejectedSenderBinding",
      NetworkPlayerMutationStatus.RejectedWorldRuntime => "RejectedWorldRuntime",
      NetworkPlayerMutationStatus.RejectedInvalidState => "InvalidPlayerState",
      _ => "PlayerNotFound"
    };
  }

  private static string StatusCode(NetworkPlayerMovementStatus status)
  {
    return status switch {
      NetworkPlayerMovementStatus.RejectedSenderBinding => "RejectedSenderBinding",
      NetworkPlayerMovementStatus.RejectedWorldRuntime => "RejectedWorldRuntime",
      NetworkPlayerMovementStatus.RejectedInvalidState => "InvalidPlayerState",
      _ => "PlayerNotFound"
    };
  }

  private static string StatusCode(NetworkPlayerBindingStatus status)
  {
    return status switch {
      NetworkPlayerBindingStatus.RejectedStage => "PlayerStateRequiresPlayerData",
      NetworkPlayerBindingStatus.RejectedSenderBinding => "RejectedSenderBinding",
      NetworkPlayerBindingStatus.RejectedWorldRuntime => "RejectedWorldRuntime",
      _ => "PlayerNotFound"
    };
  }
}
