using System;
using System.Collections.Generic;

namespace Terraria.Player;

public static class PlayerLifecycleSystem
{
  private const int RespawnTimerMaximum = 3600;
  private const int RespawnTicksPerSecond = 60;
  private const int SpectatingLingerAfterDeathTicks = 180;
  private const int LegacyPlayerSlotCount = 255;
  private const double WorldJoinRespawnElapsedSecondsMaximum = 1000.0;

  public enum ConnectionHookIntent : byte
  {
    None,
    Connected,
    Disconnected,
  }

  public enum ConnectionStateChangeSource : byte
  {
    Spawn,
    NetworkActiveState,
  }

  public readonly record struct ConnectionTransition(
    PlayerConnectionState PreviousState,
    PlayerConnectionState CurrentState,
    ConnectionHookIntent HookIntent)
  {
    public bool Changed => PreviousState != CurrentState;
  }

  public enum DeathResolutionStatus : byte
  {
    RejectedCreativeMode,
    RejectedPracticeModeReset,
    AlreadyDead,
    Committed,
  }

  public readonly record struct DeathResolutionInput(
    bool IsCreativeGodMode,
    bool PracticeModeResetReturnsTrue,
    bool IsPvpRequest,
    WorldPosition DeathPosition,
    DateTime DeathTime,
    int RespawnRemainingTicks,
    bool IsLocalPlayer,
    bool MultiplayerBroadcast,
    SpectatingNetworkMode NetworkMode);

  public readonly record struct DeathResolutionResult(
    DeathResolutionStatus Status,
    PlayerRestStopResult RestStopResult,
    SpectatingTargetResult SpectatingResult)
  {
    public bool DidCommit => Status == DeathResolutionStatus.Committed;
  }

  public readonly record struct SpawnCommitInput(
    bool IsSpawningIntoWorld,
    long LastTimePlayerWasSavedBinary,
    DateTime CurrentUtcTime,
    bool IsLocalPlayer,
    bool MultiplayerBroadcast);

  public readonly record struct SpawnCommitResult(
    ConnectionTransition ConnectionTransition,
    PlayerRestStopResult RestStopResult,
    bool PreservedDeathState,
    bool ShouldApplyPvpDeathRecovery);

  public readonly record struct DeadTickInput(
    bool IsHardcoreWithoutRespawn,
    bool IsGhost,
    bool IsLocalPlayer,
    bool IsServer,
    bool HasCursorItem);

  public readonly record struct DeadTickResult(
    bool Advanced,
    bool ShouldBecomeGhost,
    bool ShouldRequestRespawn,
    bool ShouldOpenInventory);

  public readonly record struct SpectatingEligibilityInput(
    int TargetPlayerSlot,
    int ObserverPlayerSlot,
    int ObserverSpectatingTargetSlot,
    int TargetWhoAmI,
    bool TargetIsActive,
    bool TargetIsDead,
    int TargetDeadElapsedTicks);

  public readonly record struct SpectatingPlayerSnapshot(
    int PlayerSlot,
    int PlayerWhoAmI,
    bool IsActive,
    bool IsDead,
    int DeadElapsedTicks,
    WorldPosition CenterPosition);

  public enum SpectatingNetworkMode : byte
  {
    SinglePlayer,
    Client,
    Server,
  }

  public enum SpectatingTargetAction : byte
  {
    None,
    SendClientRequest,
    ResolveFallback,
    BroadcastServerTarget,
  }

  public readonly record struct SpectatingTargetRequestInput(
    int RequestedTargetPlayerSlot,
    int ObserverPlayerSlot,
    SpectatingNetworkMode NetworkMode,
    bool IsLocalPlayer,
    int TargetWhoAmI,
    bool TargetIsActive,
    bool TargetIsDead,
    int TargetDeadElapsedTicks);

  public readonly record struct SpectatingTargetResult(
    SpectatingTargetAction Action,
    int TargetPlayerSlot,
    bool StateChanged,
    bool ShouldCheckSection);

  public static PlayerIdentityState QueryIdentity(PlayerIdentityComponent identity)
  {
    ArgumentNullException.ThrowIfNull(identity);

    return new PlayerIdentityState(
      identity.CharacterName,
      identity.TeamId,
      identity.Difficulty,
      identity.IsHost,
      identity.ConnectionState,
      identity.LegacyPlayerSlot);
  }

  public static ConnectionTransition SetConnectionState(
    PlayerIdentityComponent identity,
    PlayerConnectionState connectionState,
    ConnectionStateChangeSource source)
  {
    ArgumentNullException.ThrowIfNull(identity);

    var previousState = identity.ConnectionState;
    if (previousState == connectionState)
    {
      return new ConnectionTransition(previousState, connectionState, ConnectionHookIntent.None);
    }

    var wasActive = previousState == PlayerConnectionState.Active;
    var isActive = connectionState == PlayerConnectionState.Active;
    identity.ConnectionState = connectionState;

    var hookIntent = source == ConnectionStateChangeSource.NetworkActiveState
      ? (wasActive, isActive) switch
      {
        (false, true) => ConnectionHookIntent.Connected,
        (true, false) => ConnectionHookIntent.Disconnected,
        _ => ConnectionHookIntent.None,
      }
      : ConnectionHookIntent.None;

    return new ConnectionTransition(previousState, connectionState, hookIntent);
  }

  public static SpawnCommitResult CommitSpawn(
    PlayerIdentityComponent identity,
    ref PlayerLifecycleComponent lifecycle,
    PlayerDeathRecordComponent deathRecord,
    PlayerRestComponent rest,
    PlayerSittingComponent sitting,
    PlayerSleepingComponent sleeping,
    SpawnCommitInput input)
  {
    ArgumentNullException.ThrowIfNull(identity);
    ArgumentNullException.ThrowIfNull(deathRecord);
    ArgumentNullException.ThrowIfNull(rest);
    ArgumentNullException.ThrowIfNull(sitting);
    ArgumentNullException.ThrowIfNull(sleeping);

    bool preservedDeathState = input.IsSpawningIntoWorld && lifecycle.IsDead;
    if (preservedDeathState && input.IsLocalPlayer &&
      input.LastTimePlayerWasSavedBinary != 0L)
    {
      if (input.CurrentUtcTime.Kind != DateTimeKind.Utc)
      {
        throw new ArgumentException(
          "The world-join adjustment requires a UTC time snapshot.",
          nameof(input));
      }

      int adjustedTicks = GetWorldJoinRespawnAdjustmentTicks(
        input.CurrentUtcTime,
        input.LastTimePlayerWasSavedBinary,
        lifecycle.RespawnRemainingTicks);
      lifecycle.RespawnRemainingTicks = unchecked(
        lifecycle.RespawnRemainingTicks - adjustedTicks);
      preservedDeathState = lifecycle.RespawnRemainingTicks != 0;
    }

    PlayerRestStopResult restStopResult = PlayerRestInteractionSystem.StopAll(
      rest,
      sitting,
      sleeping,
      input.IsLocalPlayer,
      input.MultiplayerBroadcast);

    bool shouldApplyPvpDeathRecovery = !preservedDeathState && deathRecord.WasPvpDeath;
    if (!preservedDeathState)
    {
      lifecycle.Phase = PlayerLifecyclePhase.Alive;
      lifecycle.DeadElapsedTicks = 0;
      deathRecord.WasPvpDeath = false;
    }

    ConnectionTransition connectionTransition = SetConnectionState(
      identity,
      PlayerConnectionState.Active,
      ConnectionStateChangeSource.Spawn);
    lifecycle.SpectatingTargetSlot = null;

    return new SpawnCommitResult(
      connectionTransition,
      restStopResult,
      preservedDeathState,
      shouldApplyPvpDeathRecovery);
  }

  public static PlayerLifecycleState QueryLifecycle(PlayerLifecycleComponent lifecycle)
  {
    return new PlayerLifecycleState(
      lifecycle.Phase,
      lifecycle.DeadElapsedTicks,
      lifecycle.RespawnRemainingTicks,
      lifecycle.SpectatingTargetSlot);
  }

  public static bool CanSpectate(SpectatingEligibilityInput input)
  {
    if (input.TargetPlayerSlot < 0 || input.TargetPlayerSlot == input.ObserverPlayerSlot)
    {
      return true;
    }

    if (!input.TargetIsActive)
    {
      return false;
    }

    if (!input.TargetIsDead)
    {
      return true;
    }

    return input.TargetWhoAmI == input.ObserverSpectatingTargetSlot &&
      input.TargetDeadElapsedTicks < SpectatingLingerAfterDeathTicks;
  }

  public static SpectatingTargetResult RequestSpectatingTarget(
    ref PlayerLifecycleComponent lifecycle,
    SpectatingTargetRequestInput input)
  {
    int targetPlayerSlot = input.RequestedTargetPlayerSlot == input.ObserverPlayerSlot
      ? -1
      : input.RequestedTargetPlayerSlot;
    int currentTargetSlot = lifecycle.SpectatingTargetSlot?.Value ?? -1;
    if (targetPlayerSlot == currentTargetSlot)
    {
      return new SpectatingTargetResult(
        SpectatingTargetAction.None,
        targetPlayerSlot,
        false,
        false);
    }

    if (input.NetworkMode == SpectatingNetworkMode.Client)
    {
      if (!input.IsLocalPlayer)
      {
        return new SpectatingTargetResult(
          SpectatingTargetAction.None,
          targetPlayerSlot,
          false,
          false);
      }

      bool stateChanged = false;
      if (targetPlayerSlot == -1)
      {
        lifecycle.SpectatingTargetSlot = null;
        stateChanged = true;
      }
      else if (currentTargetSlot < 0)
      {
        lifecycle.SpectatingTargetSlot = new LegacyPlayerSlot(input.ObserverPlayerSlot);
        stateChanged = true;
      }

      return new SpectatingTargetResult(
        SpectatingTargetAction.SendClientRequest,
        targetPlayerSlot,
        stateChanged,
        false);
    }

    if (input.NetworkMode != SpectatingNetworkMode.Server)
    {
      return new SpectatingTargetResult(
        SpectatingTargetAction.None,
        targetPlayerSlot,
        false,
        false);
    }

    lifecycle.SpectatingTargetSlot = targetPlayerSlot == -1
      ? null
      : new LegacyPlayerSlot(targetPlayerSlot);
    bool canSpectate = CanSpectate(
      new SpectatingEligibilityInput(
        targetPlayerSlot,
        input.ObserverPlayerSlot,
        targetPlayerSlot,
        input.TargetWhoAmI,
        input.TargetIsActive,
        input.TargetIsDead,
        input.TargetDeadElapsedTicks));
    if (!canSpectate)
    {
      return new SpectatingTargetResult(
        SpectatingTargetAction.ResolveFallback,
        targetPlayerSlot,
        true,
        false);
    }

    return new SpectatingTargetResult(
      SpectatingTargetAction.BroadcastServerTarget,
      targetPlayerSlot,
      true,
      targetPlayerSlot >= 0);
  }

  public static int FindClosestSpectatablePlayer(
    IReadOnlyList<SpectatingPlayerSnapshot> candidates,
    int observerPlayerSlot,
    int observerSpectatingTargetSlot,
    WorldPosition cameraPosition)
  {
    ArgumentNullException.ThrowIfNull(candidates);

    int closestPlayerSlot = -1;
    float closestDistanceSquared = float.MaxValue;
    foreach (SpectatingPlayerSnapshot candidate in candidates)
    {
      if (candidate.PlayerSlot < 0 || candidate.PlayerSlot >= LegacyPlayerSlotCount ||
        candidate.PlayerSlot == observerPlayerSlot)
      {
        continue;
      }

      bool canSpectate = CanSpectate(
        new SpectatingEligibilityInput(
          candidate.PlayerSlot,
          observerPlayerSlot,
          observerSpectatingTargetSlot,
          candidate.PlayerWhoAmI,
          candidate.IsActive,
          candidate.IsDead,
          candidate.DeadElapsedTicks));
      if (!canSpectate)
      {
        continue;
      }

      float deltaX = candidate.CenterPosition.X - cameraPosition.X;
      float deltaY = candidate.CenterPosition.Y - cameraPosition.Y;
      float distanceSquared = deltaX * deltaX + deltaY * deltaY;
      if (distanceSquared < closestDistanceSquared ||
        (distanceSquared == closestDistanceSquared && closestPlayerSlot >= 0 &&
          candidate.PlayerSlot < closestPlayerSlot))
      {
        closestPlayerSlot = candidate.PlayerSlot;
        closestDistanceSquared = distanceSquared;
      }
    }

    return closestPlayerSlot;
  }

  public static int? FindNextSpectatablePlayer(
    IReadOnlyList<SpectatingPlayerSnapshot> candidates,
    int observerPlayerSlot,
    int spectatingTargetSlot,
    int step,
    bool includeSelf)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    if (step is not (-1 or 1))
    {
      return null;
    }

    int startingSlot = spectatingTargetSlot < 0
      ? observerPlayerSlot
      : spectatingTargetSlot;
    int closestStepCount = LegacyPlayerSlotCount;
    int? nextSpectatablePlayerSlot = null;
    foreach (SpectatingPlayerSnapshot candidate in candidates)
    {
      if (candidate.PlayerSlot < 0 || candidate.PlayerSlot >= LegacyPlayerSlotCount ||
        (candidate.PlayerSlot == observerPlayerSlot && !includeSelf))
      {
        continue;
      }

      int distanceInScanOrder = step > 0
        ? (candidate.PlayerSlot - startingSlot + LegacyPlayerSlotCount) % LegacyPlayerSlotCount
        : (startingSlot - candidate.PlayerSlot + LegacyPlayerSlotCount) % LegacyPlayerSlotCount;
      if (distanceInScanOrder == 0 || distanceInScanOrder >= closestStepCount)
      {
        continue;
      }

      bool canSpectate = CanSpectate(
        new SpectatingEligibilityInput(
          candidate.PlayerSlot,
          observerPlayerSlot,
          spectatingTargetSlot,
          candidate.PlayerWhoAmI,
          candidate.IsActive,
          candidate.IsDead,
          candidate.DeadElapsedTicks));
      if (canSpectate)
      {
        closestStepCount = distanceInScanOrder;
        nextSpectatablePlayerSlot = candidate.PlayerSlot;
      }
    }

    return nextSpectatablePlayerSlot;
  }

  // Consume ghost and spawn intents synchronously in the same Player.Update dead branch.
  public static DeadTickResult AdvanceDeadTick(
    ref PlayerLifecycleComponent lifecycle,
    DeadTickInput input)
  {
    if (!lifecycle.IsDead || input.IsGhost)
    {
      return default;
    }

    int timerAtStart = lifecycle.RespawnRemainingTicks;
    lifecycle.DeadElapsedTicks = unchecked(lifecycle.DeadElapsedTicks + 1);

    if (input.IsHardcoreWithoutRespawn)
    {
      if (timerAtStart > 0)
      {
        lifecycle.RespawnRemainingTicks = Math.Clamp(
          unchecked(timerAtStart - 1),
          0,
          RespawnTimerMaximum);
      }

      bool shouldBecomeGhost = timerAtStart <= 0 &&
        (input.IsLocalPlayer || input.IsServer);
      return new DeadTickResult(true, shouldBecomeGhost, false, false);
    }

    lifecycle.RespawnRemainingTicks = Math.Clamp(
      unchecked(timerAtStart - 1),
      0,
      RespawnTimerMaximum);
    bool shouldRequestRespawn =
      lifecycle.RespawnRemainingTicks <= 0 && input.IsLocalPlayer;
    return new DeadTickResult(
      true,
      false,
      shouldRequestRespawn,
      shouldRequestRespawn && input.HasCursorItem);
  }

  public static DeathResolutionResult ResolveDeath(
    ref PlayerLifecycleComponent lifecycle,
    PlayerDeathRecordComponent deathRecord,
    PlayerRestComponent rest,
    PlayerSittingComponent sitting,
    PlayerSleepingComponent sleeping,
    DeathResolutionInput input)
  {
    ArgumentNullException.ThrowIfNull(deathRecord);
    ArgumentNullException.ThrowIfNull(rest);
    ArgumentNullException.ThrowIfNull(sitting);
    ArgumentNullException.ThrowIfNull(sleeping);

    if (input.IsCreativeGodMode)
    {
      return new DeathResolutionResult(
        DeathResolutionStatus.RejectedCreativeMode,
        default,
        default);
    }

    if (input.PracticeModeResetReturnsTrue)
    {
      return new DeathResolutionResult(
        DeathResolutionStatus.RejectedPracticeModeReset,
        default,
        default);
    }

    if (lifecycle.IsDead)
    {
      return new DeathResolutionResult(
        DeathResolutionStatus.AlreadyDead,
        default,
        default);
    }

    PlayerRestStopResult restStopResult = PlayerRestInteractionSystem.StopAll(
      rest,
      sitting,
      sleeping,
      input.IsLocalPlayer,
      input.MultiplayerBroadcast);

    deathRecord.WasPvpDeath |= input.IsPvpRequest;
    if (deathRecord.WasPvpDeath)
    {
      deathRecord.PvpDeathCount++;
    }
    else
    {
      deathRecord.PveDeathCount++;
    }

    deathRecord.LastDeathPosition = input.DeathPosition;
    deathRecord.LastDeathTime = input.DeathTime;
    deathRecord.ShowLastDeath = true;

    lifecycle.Phase = PlayerLifecyclePhase.Dead;
    lifecycle.DeadElapsedTicks = 0;
    lifecycle.RespawnRemainingTicks = input.RespawnRemainingTicks;
    SpectatingTargetResult spectatingResult = RequestSpectatingTarget(
      ref lifecycle,
      new SpectatingTargetRequestInput(
        RequestedTargetPlayerSlot: -1,
        ObserverPlayerSlot: -1,
        NetworkMode: input.NetworkMode,
        IsLocalPlayer: input.IsLocalPlayer,
        TargetWhoAmI: -1,
        TargetIsActive: false,
        TargetIsDead: false,
        TargetDeadElapsedTicks: 0));

    return new DeathResolutionResult(
      DeathResolutionStatus.Committed,
      restStopResult,
      spectatingResult);
  }

  private static int GetWorldJoinRespawnAdjustmentTicks(
    DateTime currentUtcTime,
    long lastTimePlayerWasSavedBinary,
    int respawnTimer)
  {
    long elapsedBinaryTicks = unchecked(
      currentUtcTime.ToBinary() - lastTimePlayerWasSavedBinary);
    double elapsedSeconds = Math.Clamp(
      new TimeSpan(elapsedBinaryTicks).TotalSeconds,
      0.0,
      WorldJoinRespawnElapsedSecondsMaximum);
    int adjustedTicks = unchecked((int)(elapsedSeconds * RespawnTicksPerSecond));

    // Preserve Utils.Clamp's upper-bound-first behavior for an invalid negative timer.
    if (adjustedTicks > respawnTimer)
    {
      return respawnTimer;
    }

    if (adjustedTicks < 0)
    {
      return 0;
    }

    return adjustedTicks;
  }
}
