using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// Synchronous per-tick player update closure invoked by the Root 60Hz loop.
/// It owns no timer, queue, task, or network I/O.
/// </summary>
public sealed class PlayerUpdateService
{
  private sealed record PreparedTick(
    Player Player,
    PlayerTickState State,
    CommitVersion Version,
    PreparedPlayerSpectatingChange? Spectating);

  private const int MaximumRespawnTimer = 3_600;

  private readonly PlayerOwner _players;
  private long _lastGameTickIndex = -1;

  public PlayerUpdateService(PlayerOwner players)
  {
    ArgumentNullException.ThrowIfNull(players);
    _players = players;
  }

  public PlayerUpdateResult Tick(long gameTickIndex, PlayerUpdateContext context)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(gameTickIndex);
    ArgumentNullException.ThrowIfNull(context);
    if (gameTickIndex <= _lastGameTickIndex)
    {
      throw new InvalidOperationException("Player ticks must advance in strictly increasing order.");
    }

    Player[] players = _players.CapturePlayers().ToArray();
    // Capture all external facts before calculating or committing any player update.
    var inputs = players.Select(player => context.CaptureInput?.Invoke(player)).ToArray();
    var activeRelations = players.SelectMany(player => new EntityHandle?[] {
      player.TargetRelation.MinionAttackTarget, player.Spectating.Target
    }).Where(handle => handle.HasValue).Select(handle => handle.GetValueOrDefault()).Distinct()
      .ToDictionary(handle => handle, handle => context.IsEntityActive.Invoke(handle));
    PlayerSpectatingTickPreparation? spectatingPreparation = context.Spectating?.PrepareTick();
    var changed = new List<EntityHandle>();
    var respawned = new List<EntityHandle>();
    var clearedRelations = new List<EntityHandle>();
    var commits = new List<PlayerTickCommit>();
    var preparedTicks = new List<PreparedTick>();
    CommitVersion? lastVersion = null;

    for (int index = 0; index < players.Length; index++)
    {
      Player player = players[index];
      PlayerTickInput? input = inputs[index];
      if (context.CaptureInput is not null
        && (input is null || input.ValidateSources(player).Status != OwnerCommitStatus.Applied
          || input.Binding.Epoch != player.Epoch
          || input.Binding.PlayerHandle != player.Handle))
      {
        continue;
      }

      PlayerBuffs previousBuffs = player.Buffs;
      PlayerBuffs buffs = player.IsDead ? previousBuffs.UpdateDead() : previousBuffs.AdvanceTick();
      // UpdateDead returns before the regular immunity, cooldown, buff and luck updates.
      PlayerImmunities immunities = player.IsDead
        ? player.Immunities with { BrainDodgeAnimationTicks = 0 }
        : player.Immunities.AdvanceTick();
      int respawnTimer = player.IsDead
        ? (int)Math.Clamp((long)player.RespawnTimer - 1, 0, MaximumRespawnTimer)
        : player.RespawnTimer;
      PlayerVitals vitals = player.IsDead ? player.Vitals with { Life = 0 } : player.Vitals;
      bool respawnReady = player.RespawnTimer > 0 && respawnTimer == 0 && player.IsDead;

      PlayerTargetRelation targetRelation = player.TargetRelation;
      bool targetCleared = false;
      if (targetRelation.MinionAttackTarget is EntityHandle attackTarget
        && !activeRelations[attackTarget])
      {
        targetRelation = targetRelation with { MinionAttackTarget = null };
        targetCleared = true;
      }

      PlayerSpectating spectating = player.Spectating;
      PreparedPlayerSpectatingChange? spectatingChange = null;
      if (spectatingPreparation is not null) {
        if (spectatingPreparation.Changes.TryGetValue(player.Handle, out spectatingChange)) {
          spectating = spectatingChange.State;
        }
      } else if (spectating.Target is EntityHandle spectatingTarget
          && !activeRelations[spectatingTarget]) {
        // Compatibility path; production supplies the coordinator for closest-player fallback.
        spectating = PlayerSpectating.None;
      }

      targetCleared |= player.Spectating.Target != spectating.Target;
      int deadTicks = player.IsDead
        ? (int)Math.Min(int.MaxValue, (long)player.DeadTicks + 1) : 0;
      PlayerRuntimeSpatialState runtimeSpatial = player.RuntimeSpatial;
      if (player.IsDead) {
        runtimeSpatial = runtimeSpatial with {
          GrapplingStateKnown = true,
          FirstGrapplingProjectile = null,
          ItemAnimation = 0,
          ItemAnimationKnown = true
        };
      } else if (input is not null && runtimeSpatial.ItemAnimationKnown
          && runtimeSpatial.ItemAnimation > 0) {
        runtimeSpatial = runtimeSpatial with { ItemAnimation = runtimeSpatial.ItemAnimation - 1 };
      }

      PlayerLuck previousLuck = player.Luck;
      PlayerTargetRelation previousRelation = player.TargetRelation;
      PlayerSpectating previousSpectating = player.Spectating;
      PlayerLuck luck = previousLuck;
      if (!player.IsDead) {
        PlayerLuck factors = previousLuck.AdvanceTick(input?.DayRate ?? 0)
          .WithAuthoritativeBuffs(buffs, previousBuffs);
        luck = factors.WithCurrentLuck(
          PlayerLuckCalculator.Calculate(
            factors,
            player.Appearance.HasUsedGalaxyPearl,
            new PlayerLuckCalculationContext(
              input?.LanternsUp ?? context.LuckContext.LanternsUp,
              buffs.HasBuff(Version4PlayerBuffCatalog.StinkyBuffType))));
      }

      var state = new PlayerTickState(
        buffs,
        immunities,
        vitals,
        luck,
        targetRelation,
        spectating,
        respawnTimer,
        deadTicks,
        runtimeSpatial);

      if (state.Equals(PlayerTickState.From(player)))
      {
        continue;
      }

      // Allocate the result and all output before either the section owner or PlayerOwner writes.
      CommitVersion version = _players.ReserveCommitVersion();
      changed.Add(player.Handle);
      lastVersion = version;
      if (respawnReady)
      {
        respawned.Add(player.Handle);
      }

      if (targetCleared)
      {
        clearedRelations.Add(player.Handle);
      }

      var projections = new List<SessionPacketProjection>();
      if (input is not null && input.Recipients.Count > 0
        && !previousBuffs.Types.SequenceEqual(buffs.Types))
      {
        projections.Add(new SessionPacketProjection(
          SessionAdmissionCoordinator.PlayerBuffsMessageId,
          new PlayerBuffsPacket
          {
            Player = input.Binding.PlayerSlot,
            BuffTypes = buffs.Types.Where(type => type != 0).ToArray()
          },
          input.Recipients,
          null,
          version));
      }

      if (input is not null) {
        SessionEpoch[] everyone = input.Recipients.Append(input.Binding.Epoch).ToArray();
        if (previousRelation.MinionAttackTarget is not null
            && targetRelation.MinionAttackTarget is null) {
          projections.Add(new SessionPacketProjection(
            SessionAdmissionCoordinator.MinionAttackTargetMessageId,
            new MinionAttackTargetUpdatePacket { Player = input.Binding.PlayerSlot, NpcIndex = -1 },
            everyone, null, version));
        }

        if (spectatingChange is not null) {
          spectatingChange.AppendProjections(version, projections);
        } else if (previousSpectating.Target != spectating.Target) {
          projections.Add(new SessionPacketProjection(
            SessionAdmissionCoordinator.SpectatePlayerMessageId,
            new SpectatePlayerPacket { Player = input.Binding.PlayerSlot, TargetPlayer = -1 },
            everyone, null, version));
        }

        // Natural ladybug/coin decay is local in the source. Only an owned potion-tier
        // transition adds a server correction here, avoiding a 60Hz factor broadcast.
        if (previousLuck.LuckPotion != luck.LuckPotion) {
          projections.Add(new SessionPacketProjection(
            SessionAdmissionCoordinator.UpdatePlayerLuckFactorsMessageId,
            PlayerPacketProjection.CreateLuckPacket(input.Binding.PlayerSlot, luck),
            everyone, null, version));
        }
      }

      commits.Add(new PlayerTickCommit(player, state, version, respawnReady, projections));
      preparedTicks.Add(new PreparedTick(player, state, version, spectatingChange));
    }

    var update = new PlayerUpdateResult(
      gameTickIndex,
      players.Length,
      changed,
      respawned,
      clearedRelations,
      lastVersion,
      commits,
      spectatingPreparation?.Rejections);
    foreach (PreparedTick tick in preparedTicks) {
      tick.Spectating?.CommitSections();
      _players.CommitPreparedTick(tick.Player, tick.State, tick.Version);
    }

    _lastGameTickIndex = gameTickIndex;
    return update;
  }
}

public sealed class PlayerUpdateContext
{
  /// <summary>
  /// Returning null skips a player (for example, before its session becomes Active).
  /// The factory must capture read-only facts and must not write owners or perform network I/O.
  /// </summary>
  public PlayerUpdateContext(
    Func<Player, PlayerTickInput?> captureInput,
    Func<EntityHandle, bool> isEntityActive,
    PlayerSpectatingCoordinator? spectating = null)
  {
    ArgumentNullException.ThrowIfNull(captureInput);
    ArgumentNullException.ThrowIfNull(isEntityActive);
    CaptureInput = captureInput;
    IsEntityActive = isEntityActive;
    Spectating = spectating;
  }

  /// <summary>Compatibility constructor without world time or outbound recipient facts.</summary>
  public PlayerUpdateContext(
    PlayerLuckCalculationContext luckContext,
    Func<EntityHandle, bool> isEntityActive)
  {
    ArgumentNullException.ThrowIfNull(isEntityActive);
    LuckContext = luckContext;
    IsEntityActive = isEntityActive;
  }

  public PlayerLuckCalculationContext LuckContext { get; }

  public Func<EntityHandle, bool> IsEntityActive { get; }

  public Func<Player, PlayerTickInput?>? CaptureInput { get; }

  public PlayerSpectatingCoordinator? Spectating { get; }
}

internal sealed record PlayerTickState(
  PlayerBuffs Buffs,
  PlayerImmunities Immunities,
  PlayerVitals Vitals,
  PlayerLuck Luck,
  PlayerTargetRelation TargetRelation,
  PlayerSpectating Spectating,
  int RespawnTimer,
  int DeadTicks,
  PlayerRuntimeSpatialState RuntimeSpatial)
{
  public static PlayerTickState From(Player player)
  {
    return new PlayerTickState(
      player.Buffs,
      player.Immunities,
      player.Vitals,
      player.Luck,
      player.TargetRelation,
      player.Spectating,
      player.RespawnTimer,
      player.DeadTicks,
      player.RuntimeSpatial);
  }
}

public sealed class PlayerUpdateResult
{
  internal PlayerUpdateResult(
    long gameTickIndex,
    int examinedPlayerCount,
    IReadOnlyCollection<EntityHandle> changedPlayers,
    IReadOnlyCollection<EntityHandle> respawnedPlayers,
    IReadOnlyCollection<EntityHandle> clearedRelations,
    CommitVersion? lastCommitVersion,
    IReadOnlyCollection<PlayerTickCommit> commits,
    IReadOnlyDictionary<EntityHandle, OwnerCommitResult>? spectatingRejections = null)
  {
    GameTickIndex = gameTickIndex;
    ExaminedPlayerCount = examinedPlayerCount;
    ChangedPlayers = Array.AsReadOnly(changedPlayers.ToArray());
    RespawnReadyPlayers = Array.AsReadOnly(respawnedPlayers.ToArray());
    ClearedRelations = Array.AsReadOnly(clearedRelations.ToArray());
    LastCommitVersion = lastCommitVersion;
    Commits = Array.AsReadOnly(commits.ToArray());
    SpectatingRejections = new ReadOnlyDictionary<
      EntityHandle, OwnerCommitResult>(spectatingRejections is null
        ? new Dictionary<EntityHandle, OwnerCommitResult>()
        : spectatingRejections.ToDictionary(entry => entry.Key, entry => entry.Value));
  }

  public long GameTickIndex { get; }

  public int ExaminedPlayerCount { get; }

  public IReadOnlyList<EntityHandle> ChangedPlayers { get; }

  public IReadOnlyList<EntityHandle> RespawnReadyPlayers { get; }

  // Kept for existing consumers. A timer expiry alone no longer revives a remote player.
  public IReadOnlyList<EntityHandle> RespawnedPlayers => Array.Empty<EntityHandle>();

  public IReadOnlyList<EntityHandle> ClearedRelations { get; }

  public CommitVersion? LastCommitVersion { get; }

  public IReadOnlyList<PlayerTickCommit> Commits { get; }

  /// <summary>Failed P03 preparations retain the previous relation for an explicit later retry.</summary>
  public IReadOnlyDictionary<EntityHandle, OwnerCommitResult> SpectatingRejections { get; }
}
