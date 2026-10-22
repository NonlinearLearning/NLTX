using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>Owns player objects and their vitals; callers mutate them on the server loop.</summary>
public sealed class PlayerOwner
{
  private static readonly SessionStage[] UploadStages =
  [
    SessionStage.AwaitPlayerData,
    SessionStage.AwaitSectionRequest,
    SessionStage.Synchronizing,
    SessionStage.Active
  ];

  private static readonly SessionStage[] ActiveStage = [SessionStage.Active];

  private readonly Dictionary<SessionEpoch, Player> _byEpoch = [];
  private readonly Dictionary<EntityHandle, Player> _byHandle = [];
  private readonly EntityIdentityRegistry _identities;
  private CommitVersion _commitVersion = CommitVersion.Initial;
  private bool _hasCommitted;

  public PlayerOwner(EntityIdentityRegistry identities)
  {
    ArgumentNullException.ThrowIfNull(identities);
    _identities = identities;
  }

  public bool TryGet(SessionEpoch epoch, out Player? player)
  {
    if (_byEpoch.TryGetValue(epoch, out player)
      && _identities.IsActive(player.Handle))
    {
      return true;
    }

    player = null;
    return false;
  }

  public bool TryGet(EntityHandle handle, out Player? player)
  {
    if (_byHandle.TryGetValue(handle, out player)
      && _identities.IsActive(handle))
    {
      return true;
    }

    player = null;
    return false;
  }

  public bool TryCaptureJoinSnapshot(
    SessionBinding binding,
    out PlayerJoinSnapshot? snapshot)
  {
    ArgumentNullException.ThrowIfNull(binding);
    if (!TryResolve(binding, out Player? player) || player is null)
    {
      snapshot = null;
      return false;
    }

    snapshot = PlayerJoinSnapshot.Capture(binding, player);
    return true;
  }

  /// <summary>Captures the committed loadout projection without exposing inventory contents.</summary>
  public bool TryCaptureLoadoutFacts(
      SessionBinding binding,
      out PlayerLoadoutFacts? facts)
  {
    ArgumentNullException.ThrowIfNull(binding);
    if (!TryResolve(binding, out Player? player) || player is null)
    {
      facts = null;
      return false;
    }

    facts = new PlayerLoadoutFacts(
      binding.Epoch,
      player.Handle,
      binding.PlayerSlot,
      player.LoadoutIndex,
      player.AccessoryVisibilityMask,
      player.Version);
    return true;
  }

  /// <summary>Captures the authenticated identity and current session phase for another owner.</summary>
  public bool TryCaptureSessionFacts(
      SessionBinding binding,
      SessionAdmissionCoordinator sessions,
      out PlayerSessionFacts? facts)
  {
    ArgumentNullException.ThrowIfNull(binding);
    ArgumentNullException.ThrowIfNull(sessions);
    if (!TryResolve(binding, out Player? player)
        || player is null
        || !sessions.TryGetStage(binding.Epoch, out SessionStage stage))
    {
      facts = null;
      return false;
    }

    facts = new PlayerSessionFacts(
      binding.Epoch,
      player.Handle,
      binding.PlayerSlot,
      stage,
      player.Version);
    return true;
  }

  public IReadOnlyList<Player> CapturePlayers()
  {
    return Array.AsReadOnly(_byEpoch.Values
      .Where(player => _identities.IsActive(player.Handle))
      .OrderBy(player => player.Epoch.Value)
      .ToArray());
  }

  public bool TryGetBySlot(
    byte playerSlot,
    SessionAdmissionCoordinator sessions,
    out Player? player)
  {
    ArgumentNullException.ThrowIfNull(sessions);
    if (!sessions.TryGetActiveBindingBySlot(playerSlot, out SessionBinding? binding)
      || binding is null)
    {
      player = null;
      return false;
    }

    return TryResolve(binding, out player);
  }

  public bool TryGetBoundPlayer(
    SessionEpoch epoch,
    SessionAdmissionCoordinator sessions,
    out Player? player)
  {
    ArgumentNullException.ThrowIfNull(sessions);
    if (!sessions.TryGetBinding(epoch, out SessionBinding? binding)
      || binding is null)
    {
      player = null;
      return false;
    }

    return TryResolve(binding, out player);
  }

  internal bool TryGetForBinding(SessionBinding binding, out Player? player)
  {
    ArgumentNullException.ThrowIfNull(binding);
    return TryResolve(binding, out player);
  }

  /// <summary>
  /// Prepares the proposed snapshot and output before writing player state. The synchronous
  /// callback may encode or capture output; it must not mutate inputs or owners, perform I/O,
  /// or reenter owner operations. Rejection leaves the player unchanged; a version may be reserved.
  /// </summary>
  internal PlayerSpawnMutation ApplySpawn(
    SessionBinding binding,
    PlayerSpawnPacket packet,
    Func<PlayerJoinSnapshot, PlayerSpawnMutation, OwnerCommitResult>? prepareOutput = null,
    PacketVector2? fallbackSpawnPosition = null)
  {
    ArgumentNullException.ThrowIfNull(binding);
    ArgumentNullException.ThrowIfNull(packet);
    if (!TryResolve(binding, out Player? player) || player is null)
    {
      return PlayerSpawnMutation.Rejected(
        ProtocolErrorCode.StaleEntity,
        "Player identity is no longer active.");
    }

    bool hasPersonalSpawn = packet.SpawnX >= 0 && packet.SpawnY >= 0;
    bool hasNoPersonalSpawn = packet.SpawnX == -1 && packet.SpawnY == -1;
    if ((!hasPersonalSpawn && !hasNoPersonalSpawn) || packet.Team > PlayerTeam.MaximumTeam
      || packet.SpawnContext > 8
      || packet.RespawnTimer < 0 || packet.PveDeaths < 0 || packet.PvpDeaths < 0
      || fallbackSpawnPosition is PacketVector2 fallback &&
        (!float.IsFinite(fallback.X) || !float.IsFinite(fallback.Y) ||
          MathF.Abs(fallback.X) > PlayerMovementInput.MaximumCoordinateMagnitude ||
          MathF.Abs(fallback.Y) > PlayerMovementInput.MaximumCoordinateMagnitude))
    {
      return PlayerSpawnMutation.Rejected(
        ProtocolErrorCode.Forbidden,
        "The player spawn payload is outside the Version4 range.");
    }

    const int tileSize = 16;
    const byte reviveFromDeathContext = 0;
    const byte spawningIntoWorldContext = 1;
    PacketVector2 position = player.Movement.Position;
    if (hasPersonalSpawn)
    {
      int positionX = packet.SpawnX * tileSize + tileSize / 2 - player.RuntimeSpatial.Width / 2;
      int positionY = packet.SpawnY * tileSize - player.RuntimeSpatial.Height;
      position = new PacketVector2(positionX, positionY);
    } else if (fallbackSpawnPosition is PacketVector2 resolvedSpawn) {
      position = resolvedSpawn;
    }

    bool remainsDead = packet.SpawnContext == spawningIntoWorldContext &&
      (player.IsDead || packet.RespawnTimer > 0);
    PlayerVitals vitals = player.Vitals;
    PlayerImmunities immunities = player.Immunities;
    if (!remainsDead) {
      vitals = vitals.WithSpawnLife(player.Buffs, player.HasPendingPvpDeath);
      int immuneTicks = player.HasPendingPvpDeath
        ? 300 : packet.SpawnContext == reviveFromDeathContext ? 180 : 60;
      immunities = immunities with { GeneralTicks = immuneTicks };
    }

    PlayerMovementInput movement = player.Movement with {
      Position = position,
      Velocity = default,
      VelocityIncluded = false
    };
    var team = new PlayerTeam(player.Team.Current, packet.Team);
    var projection = new PlayerSpawnPacket {
      Player = binding.PlayerSlot,
      SpawnX = packet.SpawnX,
      SpawnY = packet.SpawnY,
      RespawnTimer = packet.RespawnTimer,
      PveDeaths = packet.PveDeaths,
      PvpDeaths = packet.PvpDeaths,
      Team = team.Current,
      SpawnContext = packet.SpawnContext
    };
    CommitVersion version = ReserveCommitVersion();
    PlayerSpawnMutation mutation = PlayerSpawnMutation.Applied(version, projection);
    if (prepareOutput is not null) {
      PlayerJoinSnapshot proposed = PlayerJoinSnapshot.CaptureSpawn(
        binding, player, projection, vitals, movement, team, immunities, remainsDead);
      OwnerCommitResult prepared = prepareOutput.Invoke(proposed, mutation);
      if (prepared.Status != OwnerCommitStatus.Applied) {
        return new PlayerSpawnMutation(prepared, null);
      }
    }

    player.Movement = movement;
    player.Spectating = PlayerSpectating.None;
    player.FallStart = (int)(position.Y / tileSize);
    player.Team = team;
    player.RespawnTimer = packet.RespawnTimer;
    player.PveDeaths = packet.PveDeaths;
    player.PvpDeaths = packet.PvpDeaths;
    player.SpawnContext = packet.SpawnContext;
    player.Vitals = vitals;
    player.Immunities = immunities;
    player.IsDead = remainsDead;
    if (!remainsDead) {
      player.DeadTicks = 0;
      player.HasPendingPvpDeath = false;
    }

    player.Version = version;
    return mutation;
  }

  /// <summary>
  /// Commits a server-authored teleport without exposing the mutable movement state to a
  /// mechanism adapter. The player owner remains the only writer of the player's position.
  /// </summary>
  internal OwnerCommitResult ApplyTeleport(
    SessionEpoch epoch,
    EntityUuid playerIdentity,
    Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketVector2 targetPosition)
  {
    if (!playerIdentity.IsValid || !TryGet(epoch, out Player? player) || player is null ||
        player.Handle.Id != playerIdentity)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The teleport target player identity is no longer active.");
    }

    if (!float.IsFinite(targetPosition.X) || !float.IsFinite(targetPosition.Y) ||
        MathF.Abs(targetPosition.X) > PlayerMovementInput.MaximumCoordinateMagnitude ||
        MathF.Abs(targetPosition.Y) > PlayerMovementInput.MaximumCoordinateMagnitude)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "The teleport destination is outside the supported player coordinate range.");
    }

    player.Movement = player.Movement with
    {
      Position = targetPosition,
      Velocity = default,
      VelocityIncluded = false
    };
    return OwnerCommitResult.Applied(NextCommitVersion(player));
  }

  /// <summary>
  /// Applies the modeled death state after synchronous output preparation. Already-dead players
  /// retain their state and version, while the caller may still prepare the source's death relay.
  /// This requires an authoritative death decision and respawn timer from the caller.
  /// </summary>
  internal OwnerCommitResult ApplyDeathState(
    SessionBinding binding,
    int respawnTimer,
    bool pvpDeath,
    Func<CommitVersion, OwnerCommitResult>? prepareOutput = null)
  {
    if (respawnTimer < 0)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "The respawn timer cannot be negative.");
    }

    if (!TryResolve(binding, out Player? player) || player is null)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "Player identity is no longer active.");
    }

    bool alreadyDead = player.IsDead;
    bool pendingPvpDeath = player.HasPendingPvpDeath || pvpDeath;
    short pveDeaths = player.PveDeaths;
    short pvpDeaths = player.PvpDeaths;
    if (!alreadyDead) {
      if (pendingPvpDeath) {
        pvpDeaths = checked((short)Math.Min(short.MaxValue, pvpDeaths + 1));
      } else {
        pveDeaths = checked((short)Math.Min(short.MaxValue, pveDeaths + 1));
      }
    }

    PlayerVitals vitals = player.Vitals with { Life = 0 };
    CommitVersion version = alreadyDead ? player.Version : ReserveCommitVersion();
    OwnerCommitResult? prepared = prepareOutput?.Invoke(version);
    if (prepared is not null && prepared.Status != OwnerCommitStatus.Applied) {
      return prepared;
    }

    if (alreadyDead) {
      return OwnerCommitResult.Applied(version);
    }

    player.DeadTicks = 0;
    player.Vitals = vitals;
    player.IsDead = true;
    player.RespawnTimer = respawnTimer;
    player.HasPendingPvpDeath = pendingPvpDeath;
    player.PveDeaths = pveDeaths;
    player.PvpDeaths = pvpDeaths;
    player.Version = version;
    return OwnerCommitResult.Applied(version);
  }

  internal OwnerCommitResult SetSpectating(
    SessionBinding binding,
    EntityHandle? target)
  {
    if (!TryResolve(binding, out Player? player) || player is null)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "Player identity is no longer active.");
    }

    if (target is EntityHandle targetHandle && !_identities.IsActive(targetHandle))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The spectating target is no longer active.");
    }

    player.Spectating = new PlayerSpectating(target);
    return OwnerCommitResult.Applied(NextCommitVersion(player));
  }

  internal int ClearSpectatingTarget(EntityHandle target)
  {
    int cleared = 0;
    CommitVersion? version = null;
    foreach (Player player in _byEpoch.Values)
    {
      if (player.Spectating.Target == target)
      {
        version ??= NextCommitVersion();
        player.Spectating = PlayerSpectating.None;
        player.Version = version.Value;
        cleared++;
      }
    }

    return cleared;
  }

  /// <summary>
  /// Prepares canonical life output before changing vitals, death state or the player version.
  /// The optional callback may encode and capture output, but must not mutate inputs or owners,
  /// perform I/O or reenter session handling. Rejected preparation may leave version gaps.
  /// </summary>
  public PlayerPacketMutationResult ApplyLifeUpload(
    InboundPacketEnvelope envelope,
    PlayerLifeManaPacket packet,
    SessionAdmissionCoordinator sessions,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.PlayerLifeManaMessageId,
      UploadStages,
      binding =>
      {
        if (!TryResolve(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        PlayerVitals vitals = player.Vitals.WithLifeUpload(packet.Life, packet.MaximumLife);
        int deadTicks = !vitals.IsDead || player.IsDead != vitals.IsDead ? 0 : player.DeadTicks;
        CommitVersion version = ReserveCommitVersion();
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureActiveRecipients(envelope.Epoch);
        var proposed = recipients.Count == 0
          ? new PlayerPacketMutationResult(
            OwnerCommitResult.Applied(version),
            null,
            null,
            [],
            null)
          : new PlayerPacketMutationResult(
            OwnerCommitResult.Applied(version),
            SessionAdmissionCoordinator.PlayerLifeManaMessageId,
            new PlayerLifeManaPacket
            {
              Player = binding.PlayerSlot,
              Life = vitals.Life,
              MaximumLife = vitals.MaximumLife
            },
            recipients,
            envelope.Epoch);
        IReadOnlyList<SessionPacketProjection> projections = proposed.ResponsePacket is null
          ? [] : Array.AsReadOnly(new[] {
            new SessionPacketProjection(SessionAdmissionCoordinator.PlayerLifeManaMessageId,
              proposed.ResponsePacket, recipients, envelope.Epoch, version)
          });
        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        player.DeadTicks = deadTicks;
        player.Vitals = vitals;
        player.IsDead = vitals.IsDead;
        player.Version = version;
        mutation = proposed;
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? new PlayerPacketMutationResult(result, null, null, [], null);
  }

  /// <summary>
  /// Prepares canonical heal output before accepting the packet. This effect does not heal life.
  /// The optional synchronous callback may encode and capture output, but must not mutate inputs
  /// or owners, perform I/O or reenter session handling. Rejection may leave version gaps.
  /// </summary>
  public PlayerPacketMutationResult ApplyHealEffect(
    InboundPacketEnvelope envelope,
    PlayerHealPacket packet,
    SessionAdmissionCoordinator sessions,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.PlayerHealMessageId,
      ActiveStage,
      binding =>
      {
        if (!TryResolve(binding, out _) )
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        CommitVersion version = ReserveCommitVersion();
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureActiveRecipients(envelope.Epoch);
        var canonical = new PlayerHealPacket {
          Player = binding.PlayerSlot,
          HealAmount = packet.HealAmount
        };
        PlayerPacketMutationResult proposed = recipients.Count == 0
          ? CreateNoPacketResult(version)
          : CreateMutation(
            version,
            SessionAdmissionCoordinator.PlayerHealMessageId,
            canonical,
            recipients,
            envelope.Epoch);
        IReadOnlyList<SessionPacketProjection> projections = recipients.Count == 0
          ? [] : Array.AsReadOnly(new[] {
            new SessionPacketProjection(SessionAdmissionCoordinator.PlayerHealMessageId,
              canonical, recipients, envelope.Epoch, version)
          });
        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        mutation = proposed;
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? CreateRejectedMutation(result);
  }

  /// <summary>
  /// Prepares canonical target-heal output before writing target vitals or accepting the packet.
  /// Nonpositive amounts prepare an empty output without resolving a target or writing life.
  /// The optional synchronous callback may encode and capture output, but must not mutate inputs
  /// or owners, perform I/O or reenter session handling. Rejection may leave version gaps.
  /// </summary>
  public PlayerPacketMutationResult ApplyHealToPlayer(
    InboundPacketEnvelope envelope,
    Unknown66Packet packet,
    SessionAdmissionCoordinator sessions,
    Func<Player, int> effectiveMaximumLife,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(effectiveMaximumLife);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.Unknown66MessageId,
      ActiveStage,
      binding =>
      {
        if (!TryResolve(binding, out _) )
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        short lifeAmount = packet.LifeAmount;
        if (lifeAmount <= 0)
        {
          CommitVersion noOpVersion = ReserveCommitVersion();
          OwnerCommitResult? noOpValidation = prepareOutput?.Invoke([]);
          if (noOpValidation is not null && noOpValidation.Status != OwnerCommitStatus.Applied) {
            return noOpValidation;
          }

          mutation = CreateNoPacketResult(noOpVersion);
          return mutation.Result;
        }

        if (!sessions.TryGetActiveBindingBySlot(packet.Player, out SessionBinding? targetBinding)
          || targetBinding is null
          || !TryResolve(targetBinding, out Player? target)
          || target is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "The heal target is not an active player in this world scope.");
        }

        int maximumLife = effectiveMaximumLife.Invoke(target);
        if (maximumLife < 0)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.NotReady,
            "The effective maximum life is unavailable.");
        }

        PlayerVitals vitals = target.Vitals.WithHeal(lifeAmount, maximumLife);
        CommitVersion version = ReserveCommitVersion();
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureActiveRecipients(envelope.Epoch);
        var canonical = new Unknown66Packet {
          Player = targetBinding.PlayerSlot,
          LifeAmount = lifeAmount
        };
        PlayerPacketMutationResult proposed = recipients.Count == 0
          ? CreateNoPacketResult(version)
          : CreateMutation(
            version,
            SessionAdmissionCoordinator.Unknown66MessageId,
            canonical,
            recipients,
            envelope.Epoch);
        IReadOnlyList<SessionPacketProjection> projections = recipients.Count == 0
          ? [] : Array.AsReadOnly(new[] {
            new SessionPacketProjection(SessionAdmissionCoordinator.Unknown66MessageId,
              canonical, recipients, envelope.Epoch, version)
          });
        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        target.Vitals = vitals;
        target.Version = version;
        mutation = proposed;
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? CreateRejectedMutation(result);
  }

  /// <summary>
  /// Prepares canonical profile or kick output before committing the profile or Closing stage.
  /// The synchronous callback may encode and capture output; it must not mutate inputs or
  /// owners, perform I/O or reenter session handling. Rejection may leave version gaps.
  /// </summary>
  public PlayerPacketMutationResult ApplyAppearanceUpload(
    InboundPacketEnvelope envelope,
    SyncPlayerPacket packet,
    SessionAdmissionCoordinator sessions,
    int skinVariantCount,
    int maximumNameLength,
    bool isJourneyWorld,
    bool checkDuplicateName,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.SyncPlayerMessageId,
      UploadStages,
      binding =>
      {
        if (!TryResolve(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        string[] activeNames = _byEpoch.Values
          .Where(candidate => candidate.Epoch != envelope.Epoch
            && _identities.IsActive(candidate.Handle))
          .Select(candidate => candidate.Appearance.Name)
          .Where(name => name.Length > 0)
          .ToArray();
        PlayerSyncPreparation prepared = PlayerRules.PrepareAppearance(
          packet,
          skinVariantCount,
          maximumNameLength,
          isJourneyWorld,
          activeNames,
          checkDuplicateName);
        if (!prepared.IsPrepared || prepared.Appearance is null)
        {
          KickPacket kick = new()
          {
            Text = NetworkText.Literal(prepared.RejectionMessage ?? "The player data was rejected.")
          };
          OwnerCommitResult closing = sessions.BeginClosing(
            envelope.Epoch, envelope.Sequence, closingVersion => {
              IReadOnlyList<SessionPacketProjection> kickProjections = Array.AsReadOnly(new[] {
                new SessionPacketProjection(SessionAdmissionCoordinator.KickMessageId,
                  kick, [envelope.Epoch], null, closingVersion)
              });
              return prepareOutput?.Invoke(kickProjections)
                ?? OwnerCommitResult.Applied(closingVersion);
            });
          if (closing.Status != OwnerCommitStatus.Applied) {
            return closing;
          }

          mutation = new PlayerPacketMutationResult(
            prepared.ErrorCode is ProtocolErrorCode errorCode
              ? OwnerCommitResult.Rejected(errorCode, kick.Text.Text)
              : OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, kick.Text.Text),
            SessionAdmissionCoordinator.KickMessageId,
            kick,
            [envelope.Epoch],
            ignoreEpoch: null,
            closeAfterResponse: true,
            responseVersion: closing.Version);
          return mutation.Result;
        }

        CommitVersion version = ReserveCommitVersion();
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureActiveRecipients(envelope.Epoch);
        var proposed = recipients.Count == 0
          ? new PlayerPacketMutationResult(
            OwnerCommitResult.Applied(version), null, null, [], null)
          : new PlayerPacketMutationResult(
            OwnerCommitResult.Applied(version),
            SessionAdmissionCoordinator.SyncPlayerMessageId,
            CreateAppearancePacket(binding.PlayerSlot, prepared.Appearance),
            recipients,
            envelope.Epoch);
        IReadOnlyList<SessionPacketProjection> projections = proposed.ResponsePacket is null
          ? [] : Array.AsReadOnly(new[] {
            new SessionPacketProjection(SessionAdmissionCoordinator.SyncPlayerMessageId,
              proposed.ResponsePacket, recipients, envelope.Epoch, version)
          });
        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        player.Appearance = prepared.Appearance;
        player.Version = version;
        mutation = proposed;
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? CreateRejectedMutation(result);
  }

  /// <summary>
  /// Prepares canonical controls output after movement validation and before player writes.
  /// The optional callback may encode and capture output, but must not mutate inputs or owners,
  /// perform I/O or reenter session handling. Rejected preparation may leave version gaps.
  /// </summary>
  public PlayerPacketMutationResult ApplyControls(
    InboundPacketEnvelope envelope,
    PlayerControlsPacket packet,
    SessionAdmissionCoordinator sessions,
    bool preserveServerPosition,
    Func<byte, bool> isSelectedItemValid,
    IPlayerRuntimeWorldCaptureSource? runtimeWorldCapture = null,
    PlayerRuntimeFactsCaptureService? runtimeFactsCapture = null,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(isSelectedItemValid);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.PlayerControlsMessageId,
      ActiveStage,
      binding =>
      {
        if (!TryResolve(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        if (!isSelectedItemValid(packet.SelectedItem))
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.Forbidden,
            "The selected inventory slot is not available.");
        }

        if (!PlayerMovementInput.TryCreate(
          packet,
          preserveServerPosition,
          player.Movement,
          out PlayerMovementInput movement))
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.Forbidden,
            "Player movement contains an invalid coordinate.");
        }

        PlayerRuntimeSpatialState runtimeSpatialToCommit = player.RuntimeSpatial;
        if (runtimeFactsCapture is not null)
        {
          OwnerCommitResult preparedResult = runtimeFactsCapture.TryPrepareMovement(
            binding, player, movement, packet,
            out PlayerRuntimeMovementPreparation? runtimePreparation);
          if (preparedResult.Status != OwnerCommitStatus.Applied || runtimePreparation is null)
          {
            return preparedResult;
          }
          OwnerCommitResult runtimeValidation = runtimePreparation.Validate();
          if (runtimeValidation.Status != OwnerCommitStatus.Applied)
          {
            return runtimeValidation;
          }
          movement = runtimePreparation.Movement;
          runtimeSpatialToCommit = runtimePreparation.RuntimeState;
        }

        bool mountCommand = (packet.MovementFlags & PlayerRuntimeSpatialStateCatalog.MountCommandFlag) != 0;
        bool validMountRequest = packet.MountType is ushort mountType && mountType <= 63;
        bool needsWorldFacts = validMountRequest && mountCommand
          && (!player.RuntimeSpatial.MountActive
            || player.RuntimeSpatial.ActiveMountType != packet.MountType)
          && packet.MountType == 5
          || !mountCommand && player.RuntimeSpatial.MountActive;
        IPlayerRuntimeWorldCapture? runtimeWorld = null;
        if (runtimeFactsCapture is null && needsWorldFacts)
        {
          runtimeWorld = runtimeWorldCapture?.Capture(binding, player, movement);
          if (runtimeWorld is null)
          {
            return new OwnerCommitResult(OwnerCommitStatus.Unavailable, null,
              ProtocolErrorCode.NotReady,
              "Mount wetness or dismount collision facts are unavailable.");
          }
          if (runtimeWorld.Epoch != binding.Epoch || runtimeWorld.PlayerHandle != player.Handle
            || runtimeWorld.PlayerVersion != player.Version || !runtimeWorld.Revision.IsValid)
          {
            return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
              "The mount world facts do not match the current Player version.");
          }
          OwnerCommitResult worldValidation = runtimeWorld.Validate();
          if (worldValidation.Status != OwnerCommitStatus.Applied)
          {
            return worldValidation;
          }
        }

        PlayerRuntimeSpatialState runtimeSpatial = runtimeSpatialToCommit;
        PlayerMovementInput updatedMovement = movement;
        bool movementFactsUnavailable = false;
        if (runtimeFactsCapture is null && !PlayerRuntimeSpatialStateCatalog.TryApplyMovementCommand(
          runtimeSpatialToCommit, packet, movement, runtimeWorld,
          out runtimeSpatial,
          out updatedMovement,
          out movementFactsUnavailable))
        {
          if (movementFactsUnavailable)
          {
            return new OwnerCommitResult(OwnerCommitStatus.Unavailable, null,
              ProtocolErrorCode.NotReady,
              "Mount wetness or dismount collision facts are unavailable.");
          }
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.Forbidden,
            "The explicit mount command is outside the Version4 mount range.");
        }

        if (runtimeWorld is not null && runtimeWorld.Validate().Status != OwnerCommitStatus.Applied)
        {
          return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
            "The mount world facts changed during Player control preparation.");
        }

        runtimeSpatialToCommit = runtimeFactsCapture is null ? runtimeSpatial : runtimeSpatialToCommit;
        CommitVersion version = ReserveCommitVersion();
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureActiveRecipients(envelope.Epoch);
        PlayerControlsPacket canonical = CreateControlsPacket(binding.PlayerSlot, updatedMovement);
        PlayerPacketMutationResult proposed = recipients.Count == 0
          ? CreateNoPacketResult(version)
          : CreateMutation(
            version,
            SessionAdmissionCoordinator.PlayerControlsMessageId,
            canonical,
            recipients,
            envelope.Epoch);
        IReadOnlyList<SessionPacketProjection> projections = recipients.Count == 0
          ? [] : Array.AsReadOnly(new[] {
            new SessionPacketProjection(SessionAdmissionCoordinator.PlayerControlsMessageId,
              canonical, recipients, envelope.Epoch, version)
          });
        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        player.Movement = updatedMovement;
        player.RuntimeSpatial = runtimeSpatialToCommit;
        player.Version = version;
        mutation = proposed;
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? CreateRejectedMutation(result);
  }

  /// <summary>
  /// Prepares canonical PvP output before writing player state or accepting the session packet.
  /// The optional callback may encode and capture output, but must not mutate inputs or owners,
  /// perform I/O or reenter session handling. Rejected preparation may leave version gaps.
  /// </summary>
  public PlayerPacketMutationResult ApplyTogglePvp(
    InboundPacketEnvelope envelope,
    TogglePVPPacket packet,
    SessionAdmissionCoordinator sessions,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.TogglePvpMessageId,
      ActiveStage,
      binding =>
      {
        if (!TryResolve(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        var pvpStatus = new PlayerPvpStatus(packet.Hostile);
        CommitVersion version = ReserveCommitVersion();
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureActiveRecipients(envelope.Epoch);
        var canonical = new TogglePVPPacket {
          Player = binding.PlayerSlot,
          Hostile = pvpStatus.Hostile
        };
        PlayerPacketMutationResult proposed = recipients.Count == 0
          ? CreateNoPacketResult(version)
          : CreateMutation(
            version,
            SessionAdmissionCoordinator.TogglePvpMessageId,
            canonical,
            recipients,
            envelope.Epoch);
        IReadOnlyList<SessionPacketProjection> projections = recipients.Count == 0
          ? [] : Array.AsReadOnly(new[] {
            new SessionPacketProjection(SessionAdmissionCoordinator.TogglePvpMessageId,
              canonical, recipients, envelope.Epoch, version)
          });
        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        player.PvpStatus = pvpStatus;
        player.Version = version;
        mutation = proposed;
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? CreateRejectedMutation(result);
  }

  public PlayerPacketMutationResult ApplyTeamChange(
    InboundPacketEnvelope envelope,
    TeamChangePacket packet,
    byte messageId,
    SessionAdmissionCoordinator sessions)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    if (messageId == SessionAdmissionCoordinator.TeamChangeFromUiMessageId)
    {
      return CreateRejectedMutation(OwnerCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "Packet 157 requires the team-change spawn-section coordinator."));
    }

    if (messageId != SessionAdmissionCoordinator.TeamChangeMessageId)
    {
      return CreateRejectedMutation(OwnerCommitResult.Rejected(
        ProtocolErrorCode.UnknownPacket,
        "The team handler accepts packet 45 or 157 only."));
    }

    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      messageId,
      ActiveStage,
      binding =>
      {
        if (!TryResolve(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        if (!player.Team.TryChange(packet.Team, out PlayerTeam team))
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.Forbidden,
            "The requested team is outside the Version4 range.");
        }

        player.Team = team;
        CommitVersion version = NextCommitVersion(player);
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureActiveRecipients(envelope.Epoch);
        mutation = recipients.Count == 0
          ? CreateNoPacketResult(version)
          : CreateMutation(
            version,
            messageId,
            new TeamChangePacket { Player = binding.PlayerSlot, Team = player.Team.Current },
            recipients,
            envelope.Epoch);
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? CreateRejectedMutation(result);
  }

  /// <summary>
  /// Prepares the merged canonical buff types before writing the player's buff snapshot.
  /// The optional callback may encode and capture output, but must not mutate inputs or owners,
  /// perform I/O or reenter session handling. Rejected preparation may leave version gaps.
  /// </summary>
  public PlayerPacketMutationResult ApplyBuffUpload(
    InboundPacketEnvelope envelope,
    PlayerBuffsPacket packet,
    SessionAdmissionCoordinator sessions,
    Func<ushort, bool> isValidBuff,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(isValidBuff);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.PlayerBuffsMessageId,
      UploadStages,
      binding =>
      {
        if (!TryResolve(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        if (!player.Buffs.TryReplaceClientTypes(
          packet.BuffTypes,
          isValidBuff,
          out PlayerBuffs buffs))
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.Forbidden,
            "The player buff list contains an invalid or unsupported entry.");
        }

        CommitVersion version = ReserveCommitVersion();
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureActiveRecipients(envelope.Epoch);
        var canonical = new PlayerBuffsPacket {
          Player = binding.PlayerSlot,
          BuffTypes = buffs.Types.Where(type => type != 0).ToArray()
        };
        PlayerPacketMutationResult proposed = recipients.Count == 0
          ? CreateNoPacketResult(version)
          : CreateMutation(
            version,
            SessionAdmissionCoordinator.PlayerBuffsMessageId,
            canonical,
            recipients,
            envelope.Epoch);
        IReadOnlyList<SessionPacketProjection> projections = recipients.Count == 0
          ? [] : Array.AsReadOnly(new[] {
            new SessionPacketProjection(SessionAdmissionCoordinator.PlayerBuffsMessageId,
              canonical, recipients, envelope.Epoch, version)
          });
        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        player.Buffs = buffs;
        player.Version = version;
        mutation = proposed;
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? CreateRejectedMutation(result);
  }

  /// <summary>
  /// Prepares canonical zone output before writing player state or accepting the session packet.
  /// The optional callback may encode and capture output, but must not mutate inputs or owners,
  /// perform I/O or reenter session handling. Rejected preparation may leave version gaps.
  /// </summary>
  public PlayerPacketMutationResult ApplyZoneUpload(
    InboundPacketEnvelope envelope,
    SyncPlayerZonePacket packet,
    SessionAdmissionCoordinator sessions,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.SyncPlayerZoneMessageId,
      ActiveStage,
      binding =>
      {
        if (!TryResolve(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        PlayerZones previous = player.Zones;
        var updated = new PlayerZones(
          packet.Zone1,
          packet.Zone2,
          packet.Zone3,
          packet.Zone4,
          packet.Zone5,
          packet.TownNpcCount);
        CommitVersion version = ReserveCommitVersion();
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureActiveRecipients(envelope.Epoch);
        var canonical = new SyncPlayerZonePacket {
          Player = binding.PlayerSlot,
          Zone1 = updated.Zone1,
          Zone2 = updated.Zone2,
          Zone3 = updated.Zone3,
          Zone4 = updated.Zone4,
          Zone5 = updated.Zone5,
          TownNpcCount = updated.TownNpcCount
        };
        var proposed = recipients.Count == 0
          ? new PlayerPacketMutationResult(
            OwnerCommitResult.Applied(version), null, null, [], null,
            triggeredFaelingSpawn: updated.ShouldSpawnFaelings(previous))
          : new PlayerPacketMutationResult(
            OwnerCommitResult.Applied(version),
            SessionAdmissionCoordinator.SyncPlayerZoneMessageId,
            canonical,
            recipients,
            envelope.Epoch,
            triggeredFaelingSpawn: updated.ShouldSpawnFaelings(previous));
        IReadOnlyList<SessionPacketProjection> projections = recipients.Count == 0
          ? [] : Array.AsReadOnly(new[] {
            new SessionPacketProjection(SessionAdmissionCoordinator.SyncPlayerZoneMessageId,
              canonical, recipients, envelope.Epoch, version)
          });
        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        player.Zones = updated;
        player.Version = version;
        mutation = proposed;
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? CreateRejectedMutation(result);
  }

  /// <summary>
  /// Prepares canonical stealth output before writing player state or accepting the session packet.
  /// The optional callback may encode and capture output, but must not mutate inputs or owners,
  /// perform I/O or reenter session handling. Rejected preparation may leave version gaps.
  /// </summary>
  public PlayerPacketMutationResult ApplyStealthUpload(
    InboundPacketEnvelope envelope,
    PlayerStealthPacket packet,
    SessionAdmissionCoordinator sessions,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.PlayerStealthMessageId,
      ActiveStage,
      binding =>
      {
        if (!TryResolve(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        if (!float.IsFinite(packet.Stealth))
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.Forbidden,
            "Player stealth must be finite.");
        }

        float stealth = packet.Stealth;
        CommitVersion version = ReserveCommitVersion();
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureActiveRecipients(envelope.Epoch);
        var canonical = new PlayerStealthPacket { Player = binding.PlayerSlot, Stealth = stealth };
        PlayerPacketMutationResult proposed = recipients.Count == 0
          ? CreateNoPacketResult(version)
          : CreateMutation(
            version,
            SessionAdmissionCoordinator.PlayerStealthMessageId,
            canonical,
            recipients,
            envelope.Epoch);
        IReadOnlyList<SessionPacketProjection> projections = recipients.Count == 0
          ? [] : Array.AsReadOnly(new[] {
            new SessionPacketProjection(SessionAdmissionCoordinator.PlayerStealthMessageId,
              canonical, recipients, envelope.Epoch, version)
          });
        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        player.Stealth = stealth;
        player.Version = version;
        mutation = proposed;
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? CreateRejectedMutation(result);
  }

  /// <summary>
  /// Relays the authenticated player's tile-picking cursor without making the presentation
  /// payload a world-state mutation. The bound player slot is the only authoritative identity.
  /// The optional synchronous callback may encode and capture output before acceptance; it must
  /// not mutate inputs or owners, perform I/O or reenter session handling. Rejection may leave
  /// version gaps.
  /// </summary>
  public PlayerPacketMutationResult ApplyTilePickingRelay(
    InboundPacketEnvelope envelope,
    SyncTilePickingPacket packet,
    SessionAdmissionCoordinator sessions,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.SyncTilePickingMessageId,
      ActiveStage,
      binding =>
      {
        if (!TryResolve(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        CommitVersion version = ReserveCommitVersion();
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureActiveRecipients(envelope.Epoch);
        var canonical = new SyncTilePickingPacket {
          Player = binding.PlayerSlot,
          X = packet.X,
          Y = packet.Y,
          TileType = packet.TileType
        };
        PlayerPacketMutationResult proposed = recipients.Count == 0
          ? CreateNoPacketResult(version)
          : CreateMutation(
            version,
            SessionAdmissionCoordinator.SyncTilePickingMessageId,
            canonical,
            recipients,
            envelope.Epoch);
        IReadOnlyList<SessionPacketProjection> projections = recipients.Count == 0
          ? [] : Array.AsReadOnly(new[] {
            new SessionPacketProjection(SessionAdmissionCoordinator.SyncTilePickingMessageId,
              canonical, recipients, envelope.Epoch, version)
          });
        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        mutation = proposed;
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? CreateRejectedMutation(result);
  }

  public PlayerPacketMutationResult ApplyLuckFactorUpload(
    InboundPacketEnvelope envelope,
    UpdatePlayerLuckFactorsPacket packet,
    SessionAdmissionCoordinator sessions,
    PlayerLuckCalculationContext calculationContext)
  {
    return ApplyLuckFactorUpload(envelope, packet, sessions, _ => calculationContext);
  }

  public PlayerPacketMutationResult ApplyLuckFactorUpload(
    InboundPacketEnvelope envelope,
    UpdatePlayerLuckFactorsPacket packet,
    SessionAdmissionCoordinator sessions,
    Func<Player, PlayerLuckCalculationContext> captureCalculationContext)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(captureCalculationContext);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.UpdatePlayerLuckFactorsMessageId,
      ActiveStage,
      binding =>
      {
        if (!TryResolve(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        if (!float.IsFinite(packet.TorchLuck)
          || !float.IsFinite(packet.EquipmentLuckBonus)
          || !float.IsFinite(packet.CoinLuck))
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.Forbidden,
            "Player luck factors must be finite.");
        }

        PlayerLuckCalculationContext calculationContext = captureCalculationContext.Invoke(player);
        calculationContext = calculationContext with
        {
          HasStinkyDebuff = player.Buffs.HasBuff(Version4PlayerBuffCatalog.StinkyBuffType)
        };
        PlayerLuck factors = new(
          packet.LadyBugLuckTime,
          packet.TorchLuck,
          packet.LuckPotion,
          packet.HasGardenGnome,
          packet.BrokenMirrorBadLuck,
          packet.EquipmentLuckBonus,
          packet.CoinLuck,
          packet.KiteLuckLevel,
          0f);
        factors = factors.WithAuthoritativeBuffs(player.Buffs);
        player.Luck = factors.WithCurrentLuck(
          PlayerLuckCalculator.Calculate(
            factors,
            player.Appearance.HasUsedGalaxyPearl,
            calculationContext));
        CommitVersion version = NextCommitVersion(player);
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureBoundRecipients(envelope.Epoch);
        mutation = recipients.Count == 0
          ? CreateNoPacketResult(version)
          : CreateMutation(
            version,
            SessionAdmissionCoordinator.UpdatePlayerLuckFactorsMessageId,
            PlayerPacketProjection.CreateLuckPacket(binding.PlayerSlot, player.Luck),
            recipients,
            envelope.Epoch);
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? CreateRejectedMutation(result);
  }

  /// <summary>
  /// Prepares canonical minion output before writing the target relation or accepting the packet.
  /// The optional callback may encode and capture output, but must not mutate inputs or owners,
  /// perform I/O or reenter session handling. Rejected preparation may leave version gaps.
  /// </summary>
  public PlayerPacketMutationResult ApplyMinionRestTarget(
    InboundPacketEnvelope envelope,
    MinionRestTargetUpdatePacket packet,
    SessionAdmissionCoordinator sessions,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.MinionRestTargetMessageId,
      ActiveStage,
      binding =>
      {
        if (!TryResolve(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        var target = new Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketVector2(
          packet.TargetX,
          packet.TargetY);
        if (!IsValidPlayerCoordinate(target))
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.Forbidden,
            "The minion rest-target coordinates are invalid.");
        }

        PlayerTargetRelation relation = player.TargetRelation with { MinionRestTarget = target };
        CommitVersion version = ReserveCommitVersion();
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureBoundRecipients(envelope.Epoch);
        var canonical = new MinionRestTargetUpdatePacket {
          Player = binding.PlayerSlot,
          TargetX = target.X,
          TargetY = target.Y
        };
        PlayerPacketMutationResult proposed = recipients.Count == 0
          ? CreateNoPacketResult(version)
          : CreateMutation(
            version,
            SessionAdmissionCoordinator.MinionRestTargetMessageId,
            canonical,
            recipients,
            envelope.Epoch);
        IReadOnlyList<SessionPacketProjection> projections = recipients.Count == 0
          ? [] : Array.AsReadOnly(new[] {
            new SessionPacketProjection(SessionAdmissionCoordinator.MinionRestTargetMessageId,
              canonical, recipients, envelope.Epoch, version)
          });
        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        player.TargetRelation = relation;
        player.Version = version;
        mutation = proposed;
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? CreateRejectedMutation(result);
  }

  /// <inheritdoc cref="ApplyMinionRestTarget"/>
  public PlayerPacketMutationResult ApplyMinionAttackTarget(
    InboundPacketEnvelope envelope,
    MinionAttackTargetUpdatePacket packet,
    SessionAdmissionCoordinator sessions,
    Func<short, EntityHandle?> resolveActiveNpcHandle,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(resolveActiveNpcHandle);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.MinionAttackTargetMessageId,
      ActiveStage,
      binding =>
      {
        if (!TryResolve(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        short npcIndex = packet.NpcIndex;
        EntityHandle? target = npcIndex == -1 ? null : resolveActiveNpcHandle.Invoke(npcIndex);
        if (npcIndex < -1
          || npcIndex != -1 && target is not EntityHandle
          || target is EntityHandle targetHandle && !_identities.IsActive(targetHandle))
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "The minion attack target is stale or unavailable.");
        }

        PlayerTargetRelation relation = player.TargetRelation with { MinionAttackTarget = target };
        CommitVersion version = ReserveCommitVersion();
        IReadOnlyList<SessionEpoch> recipients = sessions.CaptureBoundRecipients(envelope.Epoch);
        var canonical = new MinionAttackTargetUpdatePacket {
          Player = binding.PlayerSlot,
          NpcIndex = npcIndex
        };
        PlayerPacketMutationResult proposed = recipients.Count == 0
          ? CreateNoPacketResult(version)
          : CreateMutation(
            version,
            SessionAdmissionCoordinator.MinionAttackTargetMessageId,
            canonical,
            recipients,
            envelope.Epoch);
        IReadOnlyList<SessionPacketProjection> projections = recipients.Count == 0
          ? [] : Array.AsReadOnly(new[] {
            new SessionPacketProjection(SessionAdmissionCoordinator.MinionAttackTargetMessageId,
              canonical, recipients, envelope.Epoch, version)
          });
        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        player.TargetRelation = relation;
        player.Version = version;
        mutation = proposed;
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? CreateRejectedMutation(result);
  }

  public OwnerCommitResult SetGameplayHostStatus(EntityHandle handle, bool countsAsHost)
  {
    if (!_byHandle.TryGetValue(handle, out Player? player) || !_identities.IsActive(handle))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "Player identity is no longer active.");
    }

    player.CountsAsHostForGameplay = countsAsHost;
    return OwnerCommitResult.Applied(NextCommitVersion(player));
  }

  public PlayerPacketMutationResult ApplyManaUpload(
    InboundPacketEnvelope envelope,
    Unknown42Packet packet,
    SessionAdmissionCoordinator sessions)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(sessions);
    PlayerPacketMutationResult? mutation = null;
    OwnerCommitResult result = sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.Unknown42MessageId,
      UploadStages,
      binding =>
      {
        if (!TryResolve(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        if (packet.Mana < 0 || packet.MaximumMana < 0)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.Forbidden,
            "Player mana values cannot be negative.");
        }

        player.Vitals = player.Vitals.WithManaUpload(packet.Mana, packet.MaximumMana);
        CommitVersion version = NextCommitVersion(player);
        mutation = new PlayerPacketMutationResult(
          OwnerCommitResult.Applied(version),
          null,
          null,
          [],
          null);
        return OwnerCommitResult.Applied(version);
      });

    return mutation ?? new PlayerPacketMutationResult(result, null, null, [], null);
  }

  /// <summary>
  /// Captures the Version4 coin-luck rule for a shimmer transaction. The item owner supplies the
  /// coin position and amount, while this owner remains the only writer of the player's luck.
  /// A dead or out-of-range player is a successful no-op, matching AddCoinLuck's source rule.
  /// </summary>
  public OwnerCommitResult PrepareCoinLuck(
      EntityHandle handle,
      PacketVector2 coinPosition,
      int coinAmount,
      PlayerLuckCalculationContext calculationContext,
      out PlayerCoinLuckPreparation? preparation)
  {
    preparation = null;
    if (coinAmount <= 0 || !IsValidPlayerCoordinate(coinPosition))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "The coin-luck position or amount is outside the supported range.");
    }

    if (!TryGet(handle, out Player? player) || player is null)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The coin-luck player identity is no longer active.");
    }

    float centerX = player.Movement.Position.X + player.RuntimeSpatial.Width * 0.5f;
    float centerY = player.Movement.Position.Y + player.RuntimeSpatial.Height * 0.5f;
    float deltaX = coinPosition.X - centerX;
    float deltaY = coinPosition.Y - centerY;
    bool inRange = deltaX * deltaX + deltaY * deltaY < 1_000f * 1_000f;
    if (player.IsDead || !inRange)
    {
      preparation = new PlayerCoinLuckPreparation(
        this,
        player,
        player.Luck,
        player.Version,
        changed: false);
      return OwnerCommitResult.Applied(player.Version);
    }

    if (!TryPeekNextCommitVersion(out CommitVersion version))
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Unavailable,
        null,
        ProtocolErrorCode.NotReady,
        "The Player commit version sequence is exhausted.");
    }

    PlayerLuck factors = player.Luck
      .WithCoinLuck(coinAmount)
      .WithAuthoritativeBuffs(player.Buffs);
    PlayerLuckCalculationContext context = calculationContext with {
      HasStinkyDebuff = player.Buffs.HasBuff(Version4PlayerBuffCatalog.StinkyBuffType)
    };
    PlayerLuck luck = factors.WithCurrentLuck(
      PlayerLuckCalculator.Calculate(
        factors,
        player.Appearance.HasUsedGalaxyPearl,
        context));
    preparation = new PlayerCoinLuckPreparation(this, player, luck, version, changed: true);
    return OwnerCommitResult.Applied(version);
  }

  internal bool TryEnsurePlayer(SessionEpoch epoch, out Player? player, out bool created)
  {
    created = false;
    if (!epoch.IsValid)
    {
      player = null;
      return false;
    }

    if (_byEpoch.TryGetValue(epoch, out player))
    {
      return _identities.IsActive(player.Handle);
    }

    EntityHandle handle = _identities.Create();
    player = new Player(epoch, handle);
    _byEpoch.Add(epoch, player);
    _byHandle.Add(handle, player);
    created = true;
    NextCommitVersion(player);
    return true;
  }

  internal bool TryRetire(SessionEpoch epoch)
  {
    if (!_byEpoch.Remove(epoch, out Player? player) || player is null)
    {
      return false;
    }

    _byHandle.Remove(player.Handle);
    bool retired = _identities.Retire(player.Handle);
    NextCommitVersion();
    return retired;
  }

  internal OwnerCommitResult SetHostAuthorization(EntityHandle handle, bool isHost)
  {
    if (!_byHandle.TryGetValue(handle, out Player? player) || !_identities.IsActive(handle))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "Player identity is no longer active.");
    }

    if (player.IsHost == isHost)
    {
      return OwnerCommitResult.Applied(player.Version);
    }

    player.IsHost = isHost;
    return OwnerCommitResult.Applied(NextCommitVersion(player));
  }

  internal OwnerCommitResult ApplyTick(Player player, PlayerTickState state)
  {
    ArgumentNullException.ThrowIfNull(player);
    ArgumentNullException.ThrowIfNull(state);
    if (!_byHandle.TryGetValue(player.Handle, out Player? current)
      || !ReferenceEquals(current, player)
      || !_identities.IsActive(player.Handle))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The player identity is no longer active.");
    }

    CommitVersion version = ReserveCommitVersion();
    CommitPreparedTick(player, state, version);
    return OwnerCommitResult.Applied(version);
  }

  internal void CommitPreparedTick(
      Player player, PlayerTickState state, CommitVersion version) {
    player.Buffs = state.Buffs;
    player.Immunities = state.Immunities;
    player.Vitals = state.Vitals;
    player.Luck = state.Luck;
    player.TargetRelation = state.TargetRelation;
    player.Spectating = state.Spectating;
    player.RespawnTimer = state.RespawnTimer;
    player.DeadTicks = state.DeadTicks;
    player.RuntimeSpatial = state.RuntimeSpatial;
    player.Version = version;
  }

  /// <summary>Commits an item-use animation captured from the authoritative item system.</summary>
  public OwnerCommitResult ApplyAuthoritativeItemAnimation(EntityHandle handle, int animation)
  {
    if (animation < 0 || !TryGet(handle, out Player? player) || player is null)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The runtime item-animation target or value is invalid.");
    }

    if (player.RuntimeSpatial.ItemAnimationKnown && player.RuntimeSpatial.ItemAnimation == animation)
    {
      return OwnerCommitResult.Applied(player.Version);
    }
    player.RuntimeSpatial = player.RuntimeSpatial with
    {
      ItemAnimation = animation,
      ItemAnimationKnown = true
    };
    return OwnerCommitResult.Applied(NextCommitVersion(player));
  }

  /// <summary>Commits Player gravity-control state after the responsible gameplay effect resolves.</summary>
  public OwnerCommitResult ApplyGravityControlState(EntityHandle handle, bool gravityControlled)
  {
    if (!TryGet(handle, out Player? player) || player is null)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The gravity-control target is no longer active.");
    }
    if (player.RuntimeSpatial.GravityControlled == gravityControlled)
    {
      return OwnerCommitResult.Applied(player.Version);
    }
    player.RuntimeSpatial = player.RuntimeSpatial with { GravityControlled = gravityControlled };
    return OwnerCommitResult.Applied(NextCommitVersion(player));
  }

  /// <summary>Commits the first grappling projectile relation captured by the projectile owner.</summary>
  public OwnerCommitResult ApplyFirstGrapplingProjectile(
    EntityHandle playerHandle,
    EntityHandle? projectileHandle)
  {
    if (!TryGet(playerHandle, out Player? player) || player is null)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The grappling Player is no longer active.");
    }
    if (projectileHandle is EntityHandle projectile
      && (projectile.WorldScope != playerHandle.WorldScope || !_identities.IsActive(projectile)))
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The grappling projectile is not active in the Player world.");
    }
    if (player.RuntimeSpatial.GrapplingStateKnown
      && player.RuntimeSpatial.FirstGrapplingProjectile == projectileHandle)
    {
      return OwnerCommitResult.Applied(player.Version);
    }
    player.RuntimeSpatial = player.RuntimeSpatial with
    {
      FirstGrapplingProjectile = projectileHandle,
      GrapplingStateKnown = true
    };
    return OwnerCommitResult.Applied(NextCommitVersion(player));
  }

  /// <summary>
  /// Commits portable-stool state after the movement/collision owner has captured the real
  /// clearance result. A missing clearance result cannot be treated as a free space.
  /// </summary>
  internal OwnerCommitResult ApplyRuntimePortableStoolState(
    Player player,
    bool hasStool,
    bool inUse,
    bool? canFitSpace)
  {
    ArgumentNullException.ThrowIfNull(player);
    if (!TryGet(player.Handle, out Player? current) || !ReferenceEquals(current, player))
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The runtime stool player identity is no longer active.");
    }
    if (inUse && (!hasStool || player.RuntimeSpatial.MountActive || canFitSpace != true))
    {
      return canFitSpace is null
        ? new OwnerCommitResult(OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady,
          "Portable-stool clearance has not been captured.")
        : OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
          "The portable stool cannot be enabled in the captured player state.");
    }

    int height = inUse ? 68 : player.RuntimeSpatial.MountActive
      ? player.RuntimeSpatial.Height : 42;
    PlayerRuntimeSpatialState updated = player.RuntimeSpatial with {
      PortableStoolHas = hasStool,
      Width = player.RuntimeSpatial.MountActive ? player.RuntimeSpatial.Width : 20,
      Height = height,
      PortableStoolInUse = inUse,
      PortableStoolStateKnown = true
    };
    if (updated == player.RuntimeSpatial)
    {
      return OwnerCommitResult.Applied(player.Version);
    }
    if (height != player.RuntimeSpatial.Height)
    {
      PacketVector2 position = player.Movement.Position;
      player.Movement = player.Movement with
      {
        Position = new PacketVector2(position.X,
          position.Y + player.RuntimeSpatial.Height - height)
      };
    }
    player.RuntimeSpatial = updated;
    return OwnerCommitResult.Applied(NextCommitVersion(player));
  }

  /// <summary>Called by a domain coordinator on the server commit loop after effect validation.</summary>
  public OwnerCommitResult ApplyAuthoritativeBuff(
    EntityHandle handle,
    ushort type,
    int durationTicks,
    PlayerBuffApplicationContext context)
  {
    if (!TryGet(handle, out Player? player) || player is null)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The buff target is no longer active.");
    }

    if (!player.Buffs.TryAddAuthoritative(type, durationTicks, context, out PlayerBuffs buffs))
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "The buff cannot be applied with the supplied immunity, duration or capacity facts.");
    }

    CommitVersion version = ReserveCommitVersion();
    CommitPreparedBuffs(player, buffs, version);
    return OwnerCommitResult.Applied(version);
  }

  public OwnerCommitResult RemoveAuthoritativeBuff(EntityHandle handle, ushort type)
  {
    if (!TryGet(handle, out Player? player) || player is null)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The buff target is no longer active.");
    }

    PlayerBuffs buffs = player.Buffs.Remove(type);
    CommitVersion version = ReserveCommitVersion();
    CommitPreparedBuffs(player, buffs, version);
    return OwnerCommitResult.Applied(version);
  }

  internal OwnerCommitResult ValidateLoadoutProjection(Player player, byte loadoutIndex)
  {
    ArgumentNullException.ThrowIfNull(player);
    if (loadoutIndex > 2)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "The loadout index is outside the three supported loadouts.");
    }

    if (!_byHandle.TryGetValue(player.Handle, out Player? current)
      || !ReferenceEquals(current, player) || !_identities.IsActive(player.Handle))
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
        "The loadout player identity is no longer active.");
    }

    return OwnerCommitResult.Applied(_commitVersion);
  }

  internal CommitVersion ReserveCommitVersion()
  {
    return NextCommitVersion();
  }

  internal bool TryPeekNextCommitVersion(out CommitVersion version) {
    version = default;
    if (_hasCommitted && _commitVersion.Value == long.MaxValue) {
      return false;
    }
    version = _hasCommitted ? _commitVersion.Next() : CommitVersion.Initial;
    return true;
  }

  internal void CommitPreparedPickup(Player player, PlayerVitals vitals, CommitVersion version) {
    _commitVersion = version;
    _hasCommitted = true;
    player.Vitals = vitals;
    player.Version = version;
  }

  internal void CommitPreparedPylonTeleport(
      Player player, PlayerMovementInput movement, PlayerUnbreakableWallState wallState,
      int fallStart, byte pylonStyle, CommitVersion version) {
    _commitVersion = version;
    _hasCommitted = true;
    player.Movement = movement;
    player.Spectating = PlayerSpectating.None;
    player.UnbreakableWalls = wallState;
    player.FallStart = fallStart;
    player.TeleportStyle = 9;
    player.LastTeleportPylonStyleUsed = pylonStyle;
    player.Version = version;
  }

  internal void CommitPreparedTeamChange(Player player, PlayerTeam team, CommitVersion version)
  {
    player.Team = team;
    player.Version = version;
  }

  internal void CommitPreparedSpectating(
      Player player, PlayerSpectating spectating, CommitVersion version) {
    player.Spectating = spectating;
    player.Version = version;
  }

  internal void CommitPreparedDodge(
      Player player, PlayerImmunities immunities, PlayerBuffs buffs, PlayerLuck luck,
      CommitVersion version) {
    player.Immunities = immunities;
    player.Buffs = buffs;
    player.Luck = luck;
    player.Version = version;
  }

  internal void CommitPreparedBuffs(Player player, PlayerBuffs buffs, CommitVersion version) {
    player.Luck = player.Luck.WithAuthoritativeBuffs(buffs, player.Buffs);
    player.Buffs = buffs;
    player.Version = version;
  }

  internal void CommitPreparedBuffs(
      Player player, PlayerBuffs buffs, PlayerLuck luck, CommitVersion version) {
    player.Luck = luck;
    player.Buffs = buffs;
    player.Version = version;
  }

  internal void CommitPreparedLoadoutProjection(
    Player player,
    byte loadoutIndex,
    ushort accessoryVisibilityMask,
    CommitVersion version)
  {
    player.LoadoutIndex = loadoutIndex;
    player.AccessoryVisibilityMask = accessoryVisibilityMask;
    player.Version = version;
  }

  internal void CommitPreparedCoinLuck(
      Player player,
      PlayerLuck luck,
      CommitVersion version)
  {
    if (!_byHandle.TryGetValue(player.Handle, out Player? current)
        || !ReferenceEquals(current, player)
        || !_identities.IsActive(player.Handle))
    {
      throw new InvalidOperationException(
        "The prepared coin-luck Player identity is no longer active.");
    }

    _commitVersion = version;
    _hasCommitted = true;
    player.Luck = luck;
    player.Version = version;
  }

  private bool TryResolve(SessionBinding binding, out Player? player)
  {
    if (_byHandle.TryGetValue(binding.PlayerHandle, out player)
      && player.Epoch == binding.Epoch
      && _identities.IsActive(binding.PlayerHandle))
    {
      return true;
    }

    player = null;
    return false;
  }

  private CommitVersion NextCommitVersion(Player? player = null)
  {
    if (!_hasCommitted)
    {
      _hasCommitted = true;
      _commitVersion = CommitVersion.Initial;
      if (player is not null) {
        player.Version = _commitVersion;
      }
      return _commitVersion;
    }

    _commitVersion = _commitVersion.Next();
    if (player is not null) {
      player.Version = _commitVersion;
    }
    return _commitVersion;
  }

  private static PlayerPacketMutationResult CreateNoPacketResult(CommitVersion version)
  {
    return new PlayerPacketMutationResult(
      OwnerCommitResult.Applied(version),
      null,
      null,
      [],
      null);
  }

  private static PlayerPacketMutationResult CreateRejectedMutation(OwnerCommitResult result)
  {
    return new PlayerPacketMutationResult(result, null, null, [], null);
  }

  private static PlayerPacketMutationResult CreateMutation(
    CommitVersion version,
    byte messageId,
    object packet,
    IReadOnlyCollection<SessionEpoch> targets,
    SessionEpoch ignoreEpoch)
  {
    return new PlayerPacketMutationResult(
      OwnerCommitResult.Applied(version),
      messageId,
      packet,
      targets,
      ignoreEpoch);
  }

  private static SyncPlayerPacket CreateAppearancePacket(
    byte playerSlot,
    PlayerAppearance appearance)
  {
    return new SyncPlayerPacket
    {
      Player = playerSlot,
      SkinVariant = appearance.SkinVariant,
      VoiceVariant = appearance.VoiceVariant,
      VoicePitchOffset = appearance.VoicePitchOffset,
      Hair = appearance.Hair,
      Name = appearance.Name,
      HairDye = appearance.HairDye,
      HiddenAccessories = appearance.HiddenAccessories,
      HideMisc = appearance.HideMisc,
      HairColor = appearance.HairColor,
      SkinColor = appearance.SkinColor,
      EyeColor = appearance.EyeColor,
      ShirtColor = appearance.ShirtColor,
      UnderShirtColor = appearance.UnderShirtColor,
      PantsColor = appearance.PantsColor,
      ShoeColor = appearance.ShoeColor,
      DifficultyAndAccessoryFlags = appearance.DifficultyAndAccessoryFlags,
      BiomeAndCartFlags = appearance.BiomeAndCartFlags,
      PermanentUpgradeFlags = appearance.PermanentUpgradeFlags
    };
  }

  private static PlayerControlsPacket CreateControlsPacket(
    byte playerSlot,
    PlayerMovementInput movement)
  {
    return new PlayerControlsPacket
    {
      Player = playerSlot,
      ControlFlags = movement.ControlFlags,
      MovementFlags = movement.MovementFlags,
      PlayerFeatureFlags = movement.PlayerFeatureFlags,
      ActionFlags = movement.ActionFlags,
      SelectedItem = movement.SelectedItem,
      Position = movement.Position,
      Velocity = movement.VelocityIncluded ? movement.Velocity : null,
      MountType = movement.MountType,
      PotionOfReturnUsePosition = movement.PotionOfReturnUsePosition,
      PotionOfReturnHomePosition = movement.PotionOfReturnHomePosition,
      CameraTarget = movement.CameraTarget
    };
  }

  private static bool IsValidPlayerCoordinate(
    Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PacketVector2 coordinate)
  {
    return float.IsFinite(coordinate.X)
      && float.IsFinite(coordinate.Y)
      && MathF.Abs(coordinate.X) <= PlayerMovementInput.MaximumCoordinateMagnitude
      && MathF.Abs(coordinate.Y) <= PlayerMovementInput.MaximumCoordinateMagnitude;
  }
}
