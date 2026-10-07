using System.Numerics;
using EntityEcs;
using EntityEcs.Components;
using Terraria.Content;
using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Player;
using Terraria.Player.Environment;
using Terraria.Player.Luck;
using Terraria.Player.Progression;
using Terraria.Relationships;

namespace Terraria.Network;

public enum NetworkPlayerStateMutationStatus : byte
{
  Applied,
  NotFound,
  RejectedStage,
  RejectedSenderBinding,
  RejectedWorldRuntime,
  RejectedPlayerSlot,
  RejectedInvalidState,
}

public readonly record struct NetworkPlayerStateMutationResult(
  NetworkPlayerStateMutationStatus Status,
  NetworkPlayerSnapshot? Player)
{
  public bool Succeeded => Status == NetworkPlayerStateMutationStatus.Applied;
}

public readonly record struct NetworkPlayerSpectatingResult(
  NetworkPlayerStateMutationStatus Status,
  PlayerLifecycleSystem.SpectatingTargetResult Target)
{
  public bool Succeeded => Status == NetworkPlayerStateMutationStatus.Applied;
}

public readonly record struct NetworkPlayerTeamChangeResult(
  NetworkPlayerStateMutationStatus Status,
  NetworkPlayerSnapshot? Player,
  int PreviousTeam,
  IReadOnlyList<ConnectionIdentity> NotificationTargets)
{
  public bool Succeeded => Status == NetworkPlayerStateMutationStatus.Applied;
}


/// <summary>
/// A validated, partial update of one authenticated player's formal components.
/// Null fields are left unchanged. Packet declarations such as Player are deliberately
/// excluded: the owner resolves the sender or an explicitly authorized target slot.
/// </summary>
public readonly record struct NetworkPlayerStateUpdate(
  int? Life = null,
  int? MaximumLife = null,
  int? Mana = null,
  int? MaximumMana = null,
  IReadOnlyList<ushort>? BuffTypes = null,
  ushort? PvpBuffType = null,
  int? PvpBuffTime = null,
  int? HealAmount = null,
  bool? Hostile = null,
  float? Stealth = null,
  bool? ShouldNotDraw = null,
  int? TalkNpc = null,
  Vector2? MinionRestTarget = null,
  int? MinionAttackTarget = null,
  byte? TownNpcCount = null,
  byte? Zone1 = null,
  byte? Zone2 = null,
  byte? Zone3 = null,
  byte? Zone4 = null,
  byte? Zone5 = null,
  float? ItemRotation = null,
  int? ItemAnimation = null,
  byte? ItemAnimationChannel = null,
  int? TeamId = null,
  int? AnglerQuestsFinished = null,
  int? GolferScoreAccumulated = null,
  NetworkPlayerLuckUpdate? Luck = null);

public readonly record struct NetworkPlayerLuckUpdate(
  int LadyBugLuckTime,
  float TorchLuck,
  byte LuckPotion,
  bool HasGardenGnomeNearby,
  bool BrokenMirrorBadLuck,
  float EquipmentBasedLuckBonus,
  float CoinLuck,
  byte KiteLuckLevel);

public sealed partial class NetworkPlayerOwner
{
  public ValueTask<NetworkPlayerStateMutationResult> ApplyZoneAsync(
    NetworkSessionContext context,
    byte zone1,
    byte zone2,
    byte zone3,
    byte zone4,
    byte zone5,
    byte townNpcCount,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => ApplyZoneOnOwnerThread(
        session, context, zone1, zone2, zone3, zone4, zone5, townNpcCount),
      cancellationToken);
  }

  private NetworkPlayerStateMutationResult ApplyZoneOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    byte zone1,
    byte zone2,
    byte zone3,
    byte zone4,
    byte zone5,
    byte townNpcCount)
  {
    NetworkPlayerMutationStatus validation = TryGetBoundForMutation(
      session, context, allowAwaitPlayerData: false, out BoundPlayer? bound);
    if (validation != NetworkPlayerMutationStatus.Applied)
    {
      return new NetworkPlayerStateMutationResult(
        validation switch
        {
          NetworkPlayerMutationStatus.RejectedStage =>
            NetworkPlayerStateMutationStatus.RejectedStage,
          NetworkPlayerMutationStatus.RejectedSenderBinding =>
            NetworkPlayerStateMutationStatus.RejectedSenderBinding,
          NetworkPlayerMutationStatus.RejectedWorldRuntime =>
            NetworkPlayerStateMutationStatus.RejectedWorldRuntime,
          NetworkPlayerMutationStatus.RejectedInvalidState =>
            NetworkPlayerStateMutationStatus.RejectedInvalidState,
          _ => NetworkPlayerStateMutationStatus.NotFound,
        }, null);
    }

    bool edited = session.EntityRuntime.TryEditPair<
      PlayerZoneAndEnvironmentStateComponent,
      PlayerNetworkStateComponent>(
      bound!.Handle,
      (ref PlayerZoneAndEnvironmentStateComponent environment,
        ref PlayerNetworkStateComponent network) =>
      {
        if (_faelingSpawnPort is not null)
        {
          PlayerShimmerTransitionSystem.UpdateLocalTransition(
            new PlayerShimmerTransitionInput((zone5 & 0x01) != 0),
            environment,
            _faelingSpawnPort);
        }

        PlayerNetworkStateSystem.ApplyZone(
          environment, zone1, zone2, zone3, zone4, zone5, townNpcCount);
        PlayerNetworkStateSystem.ApplyTownNpcCount(network, townNpcCount);
      });
    if (!edited)
    {
      return new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.NotFound, null);
    }

    return CaptureBoundPlayer(session, bound) is NetworkPlayerSnapshot snapshot
      ? new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.Applied, snapshot)
      : new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.NotFound, null);
  }

  public ValueTask<NetworkPlayerStateMutationResult> ApplyItemAnimationAsync(
    NetworkSessionContext context,
    float itemRotation,
    int itemAnimation,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => ApplyItemAnimationOnOwnerThread(
        session, context, itemRotation, itemAnimation),
      cancellationToken);
  }

  private NetworkPlayerStateMutationResult ApplyItemAnimationOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    float itemRotation,
    int itemAnimation)
  {
    if (!float.IsFinite(itemRotation) || itemAnimation < 0)
    {
      return new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.RejectedInvalidState, null);
    }

    NetworkPlayerMutationStatus validation = TryGetBoundForMutation(
      session, context, allowAwaitPlayerData: false, out BoundPlayer? bound);
    if (validation != NetworkPlayerMutationStatus.Applied)
    {
      return new NetworkPlayerStateMutationResult(
        validation switch
        {
          NetworkPlayerMutationStatus.RejectedStage =>
            NetworkPlayerStateMutationStatus.RejectedStage,
          NetworkPlayerMutationStatus.RejectedSenderBinding =>
            NetworkPlayerStateMutationStatus.RejectedSenderBinding,
          NetworkPlayerMutationStatus.RejectedWorldRuntime =>
            NetworkPlayerStateMutationStatus.RejectedWorldRuntime,
          NetworkPlayerMutationStatus.RejectedInvalidState =>
            NetworkPlayerStateMutationStatus.RejectedInvalidState,
          _ => NetworkPlayerStateMutationStatus.NotFound,
        }, null);
    }

    if (!TryResolveSelectedItemAnimationChannel(
          session, bound!.Handle, out byte channel))
    {
      return new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.RejectedInvalidState, null);
    }

    NetworkPlayerStateUpdate update = new(
      ItemRotation: itemRotation,
      ItemAnimation: itemAnimation,
      ItemAnimationChannel: channel);
    return ApplyNetworkStateOnOwnerThread(
      session, context, context.Actor.PlayerSlot, update);
  }

  private bool TryResolveSelectedItemAnimationChannel(
    LoadedWorldSession session,
    RuntimeEntityHandle playerHandle,
    out byte channel)
  {
    channel = 0;
    if (!session.EntityRuntime.TryCapture(
          playerHandle,
          static (PlayerInventoryComponent inventory) => inventory.SelectedSlotIndex,
          out int selectedSlot) ||
        selectedSlot < 0 ||
        selectedSlot >= PlayerInventorySlotsComponent.MainInventorySlotCount ||
        !session.EntityRuntime.TryCapture(
          playerHandle,
          (PlayerInventorySlotsComponent inventory) =>
            inventory.MainInventorySlots[selectedSlot],
          out ItemEntityRef selectedItem))
    {
      return false;
    }

    if (selectedItem.IsEmpty)
    {
      return true;
    }

    if (selectedItem.RuntimeId != session.EntityRuntime.RuntimeId ||
        selectedItem.Reference.Scope != EntityReferenceScope.Item ||
        !session.EntityRuntime.TryResolve(
          selectedItem.Reference, out RuntimeEntityHandle itemHandle))
    {
      return false;
    }

    int? typeId = null;
    if (session.EntityRuntime.TryCapture(
          itemHandle,
          static (ItemInstanceComponent instance) => instance.DefinitionRef.ContentId.TypeId,
          out int instanceTypeId))
    {
      typeId = instanceTypeId;
    }
    else if (session.EntityRuntime.TryCapture(
             itemHandle,
             static (ItemDefinitionComponent definition) => definition.ContentId,
             out int definitionTypeId))
    {
      typeId = definitionTypeId;
    }

    if (typeId is not int resolvedTypeId || resolvedTypeId <= 0 ||
        _itemDefinitions is null ||
        !_itemDefinitions.TryGet(resolvedTypeId, out ItemDefinition definition))
    {
      return false;
    }

    channel = definition.Use.Channel ? (byte)1 : (byte)0;
    return true;
  }

  public ValueTask<NetworkPlayerTeamChangeResult> ApplyTeamChangeAsync(
    NetworkSessionContext context,
    int teamId,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => ApplyTeamChangeOnOwnerThread(session, context, teamId),
      cancellationToken);
  }

  private NetworkPlayerTeamChangeResult ApplyTeamChangeOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    int teamId)
  {
    NetworkPlayerBindingStatus validation = ValidateContext(
      session, context, allowAwaitPlayerData: false);
    if (validation != NetworkPlayerBindingStatus.Applied)
    {
      return new NetworkPlayerTeamChangeResult(
        MapValidationStatus(validation), null, 0, Array.Empty<ConnectionIdentity>());
    }

    if (teamId is < 0 or > 5 ||
      !_playersBySlot.TryGetValue(context.Actor.PlayerSlot, out BoundPlayer? sender) ||
      sender.Connection != context.Connection || sender.Binding != context.Actor)
    {
      return new NetworkPlayerTeamChangeResult(
        teamId is < 0 or > 5
          ? NetworkPlayerStateMutationStatus.RejectedInvalidState
          : NetworkPlayerStateMutationStatus.RejectedSenderBinding,
        null,
        0,
        Array.Empty<ConnectionIdentity>());
    }

    if (!session.EntityRuntime.TryCapture(
          sender.Handle,
          static (PlayerIdentityComponent identity) => identity.TeamId,
          out int previousTeam))
    {
      return new NetworkPlayerTeamChangeResult(
        NetworkPlayerStateMutationStatus.NotFound, null, 0,
        Array.Empty<ConnectionIdentity>());
    }

    NetworkPlayerStateMutationResult mutation = ApplyNetworkStateOnOwnerThread(
      session,
      context,
      context.Actor.PlayerSlot,
      new NetworkPlayerStateUpdate(TeamId: teamId));
    if (!mutation.Succeeded || mutation.Player is not NetworkPlayerSnapshot player)
    {
      return new NetworkPlayerTeamChangeResult(
        mutation.Status, null, previousTeam, Array.Empty<ConnectionIdentity>());
    }

    var recipients = new HashSet<ConnectionIdentity> { player.Connection };
    foreach (BoundPlayer candidate in _playersBySlot.Values)
    {
      if (candidate.Connection == player.Connection ||
          !session.EntityRuntime.TryResolve(candidate.Reference, out RuntimeEntityHandle handle) ||
          handle != candidate.Handle ||
          CaptureBoundPlayer(session, candidate) is not NetworkPlayerSnapshot snapshot ||
          !snapshot.Active)
      {
        continue;
      }

      if ((previousTeam > 0 && snapshot.TeamId == previousTeam) ||
          (teamId > 0 && snapshot.TeamId == teamId))
      {
        recipients.Add(snapshot.Connection);
      }
    }

    return new NetworkPlayerTeamChangeResult(
      NetworkPlayerStateMutationStatus.Applied,
      player,
      previousTeam,
      recipients.ToArray());
  }

  public ValueTask<NetworkPlayerSpectatingResult> ApplySpectatingAsync(
    NetworkSessionContext context,
    int targetPlayerSlot,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => ApplySpectatingOnOwnerThread(session, context, targetPlayerSlot),
      cancellationToken);
  }

  private NetworkPlayerSpectatingResult ApplySpectatingOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    int targetPlayerSlot)
  {
    NetworkPlayerBindingStatus validation = ValidateContext(
      session, context, allowAwaitPlayerData: false);
    if (validation != NetworkPlayerBindingStatus.Applied)
    {
      return new NetworkPlayerSpectatingResult(MapValidationStatus(validation), default);
    }

    if (!_playersBySlot.TryGetValue(context.Actor.PlayerSlot, out BoundPlayer? sender) ||
      sender.Connection != context.Connection || sender.Binding != context.Actor ||
      targetPlayerSlot < -1 || targetPlayerSlot > byte.MaxValue)
    {
      return new NetworkPlayerSpectatingResult(
        targetPlayerSlot is < -1 or > byte.MaxValue
          ? NetworkPlayerStateMutationStatus.RejectedInvalidState
        : NetworkPlayerStateMutationStatus.RejectedSenderBinding,
        default);
    }

    int observerSpectatingTargetSlot = -1;
    Vector2 observerPosition = Vector2.Zero;
    int observerWidth = 0;
    int observerHeight = 0;
    if (!session.EntityRuntime.TryCapture(
          sender.Handle,
          static (PlayerLifecycleComponent lifecycle) =>
            lifecycle.SpectatingTargetSlot?.Value ?? -1,
          out observerSpectatingTargetSlot) ||
        !session.EntityRuntime.TryCapture(
          sender.Handle,
          static (LocationComponent location) => new Vector2(location.X, location.Y),
          out observerPosition) ||
        !session.EntityRuntime.TryCapture(
          sender.Handle,
          static (ColliderComponent collider) =>
            (Width: (int)MathF.Round(collider.Width), Height: (int)MathF.Round(collider.Height)),
          out (int Width, int Height) observerCollider))
    {
      return new NetworkPlayerSpectatingResult(
        NetworkPlayerStateMutationStatus.NotFound, default);
    }
    observerWidth = observerCollider.Width;
    observerHeight = observerCollider.Height;

    bool targetActive = false;
    bool targetDead = false;
    int targetDeadElapsed = 0;
    if (targetPlayerSlot >= 0 &&
      _playersBySlot.TryGetValue((byte)targetPlayerSlot, out BoundPlayer? target) &&
      session.EntityRuntime.TryResolve(target.Reference, out RuntimeEntityHandle targetHandle) &&
      targetHandle == target.Handle)
    {
      session.EntityRuntime.TryCapture(
        targetHandle,
        static (PlayerIdentityComponent identity) => identity.IsActive,
        out targetActive);
      session.EntityRuntime.TryCapture(
        targetHandle,
        static (PlayerLifecycleComponent lifecycle) =>
          (lifecycle.IsDead, lifecycle.DeadElapsedTicks),
        out (bool IsDead, int DeadElapsedTicks) targetLifecycle);
      targetDead = targetLifecycle.IsDead;
      targetDeadElapsed = targetLifecycle.DeadElapsedTicks;
    }

    var spectatingCandidates = new List<PlayerLifecycleSystem.SpectatingPlayerSnapshot>();
    foreach (BoundPlayer candidate in _playersBySlot.Values)
    {
      if (!session.EntityRuntime.TryResolve(candidate.Reference, out RuntimeEntityHandle handle) ||
          handle != candidate.Handle ||
          !session.EntityRuntime.TryCapture(
            handle,
            static (PlayerIdentityComponent identity) => identity.IsActive,
            out bool candidateActive) ||
          !session.EntityRuntime.TryCapture(
            handle,
            static (PlayerLifecycleComponent lifecycle) =>
              (lifecycle.IsDead, lifecycle.DeadElapsedTicks),
            out (bool IsDead, int DeadElapsedTicks) candidateLifecycle) ||
          !session.EntityRuntime.TryCapture(
            handle,
            static (LocationComponent location) => new Vector2(location.X, location.Y),
            out Vector2 candidatePosition) ||
          !session.EntityRuntime.TryCapture(
            handle,
            static (ColliderComponent collider) =>
              (Width: (int)MathF.Round(collider.Width), Height: (int)MathF.Round(collider.Height)),
            out (int Width, int Height) candidateCollider))
      {
        continue;
      }

      spectatingCandidates.Add(new PlayerLifecycleSystem.SpectatingPlayerSnapshot(
        candidate.Binding.PlayerSlot,
        candidate.Binding.PlayerSlot,
        candidateActive,
        candidateLifecycle.IsDead,
        candidateLifecycle.DeadElapsedTicks,
        new Terraria.Player.WorldPosition(
          candidatePosition.X + candidateCollider.Width * 0.5f,
          candidatePosition.Y + candidateCollider.Height * 0.5f)));
    }

    int fallbackTargetSlot = PlayerLifecycleSystem.FindClosestSpectatablePlayer(
      spectatingCandidates,
      context.Actor.PlayerSlot,
      observerSpectatingTargetSlot,
      new Terraria.Player.WorldPosition(
        observerPosition.X + observerWidth * 0.5f,
        observerPosition.Y + observerHeight * 0.5f));
    bool fallbackTargetActive = false;
    bool fallbackTargetDead = false;
    int fallbackTargetDeadElapsed = 0;
    if (fallbackTargetSlot >= 0)
    {
      PlayerLifecycleSystem.SpectatingPlayerSnapshot fallback =
        spectatingCandidates.First(candidate => candidate.PlayerSlot == fallbackTargetSlot);
      fallbackTargetActive = fallback.IsActive;
      fallbackTargetDead = fallback.IsDead;
      fallbackTargetDeadElapsed = fallback.DeadElapsedTicks;
    }

    PlayerLifecycleSystem.SpectatingTargetResult spectatingResult = default;
    bool acceptedTarget = false;
    bool edited = session.EntityRuntime.TryEdit<PlayerLifecycleComponent>(
      sender.Handle,
      (ref PlayerLifecycleComponent lifecycle) =>
      {
        PlayerLifecycleComponent nextLifecycle = lifecycle;
        spectatingResult = PlayerLifecycleSystem.RequestSpectatingTarget(
          ref nextLifecycle,
          new PlayerLifecycleSystem.SpectatingTargetRequestInput(
            targetPlayerSlot,
            context.Actor.PlayerSlot,
            PlayerLifecycleSystem.SpectatingNetworkMode.Server,
            IsLocalPlayer: false,
            targetPlayerSlot,
            targetActive,
            targetDead,
            targetDeadElapsed));
        if (spectatingResult.Action ==
            PlayerLifecycleSystem.SpectatingTargetAction.ResolveFallback &&
            fallbackTargetSlot >= 0)
        {
          PlayerLifecycleComponent fallbackLifecycle = lifecycle;
          spectatingResult = PlayerLifecycleSystem.RequestSpectatingTarget(
            ref fallbackLifecycle,
            new PlayerLifecycleSystem.SpectatingTargetRequestInput(
              fallbackTargetSlot,
              context.Actor.PlayerSlot,
              PlayerLifecycleSystem.SpectatingNetworkMode.Server,
              IsLocalPlayer: false,
              fallbackTargetSlot,
              fallbackTargetActive,
              fallbackTargetDead,
              fallbackTargetDeadElapsed));
          if (spectatingResult.Action !=
              PlayerLifecycleSystem.SpectatingTargetAction.ResolveFallback)
          {
            nextLifecycle = fallbackLifecycle;
          }
        }
        if (spectatingResult.Action !=
          PlayerLifecycleSystem.SpectatingTargetAction.ResolveFallback)
        {
          lifecycle = nextLifecycle;
          acceptedTarget = true;
        }
      });
    if (!edited || !acceptedTarget)
    {
      return new NetworkPlayerSpectatingResult(
        edited
          ? NetworkPlayerStateMutationStatus.RejectedInvalidState
          : NetworkPlayerStateMutationStatus.NotFound,
        spectatingResult);
    }

    return new NetworkPlayerSpectatingResult(
      NetworkPlayerStateMutationStatus.Applied, spectatingResult);
  }

  /// <summary>Applies a state update to the authenticated sender on the owner thread.</summary>
  public ValueTask<NetworkPlayerStateMutationResult> ApplyNetworkStateAsync(
    NetworkSessionContext context,
    NetworkPlayerStateUpdate update,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    NetworkPlayerStateUpdate capturedUpdate = CloneUpdate(update);
    return _worldOwner.InvokeAsync(
      session => ApplyNetworkStateOnOwnerThread(session, context, context.Actor.PlayerSlot,
        capturedUpdate),
      cancellationToken);
  }

  /// <summary>
  /// Applies a state update to a target slot after authenticating the sender. The caller
  /// must enforce any gameplay-specific target policy (for example packet 55's PvP rule).
  /// </summary>
  public ValueTask<NetworkPlayerStateMutationResult> ApplyNetworkStateToTargetAsync(
    NetworkSessionContext context,
    byte targetSlot,
    NetworkPlayerStateUpdate update,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    NetworkPlayerStateUpdate capturedUpdate = CloneUpdate(update);
    return _worldOwner.InvokeAsync(
      session => ApplyNetworkStateOnOwnerThread(session, context, targetSlot, capturedUpdate),
      cancellationToken);
  }

  /// <summary>
  /// Authorizes a PvP buff effect only when the authenticated sender and target are
  /// hostile and the buff is in Terraria's PvP-capable table. The target's connection
  /// is returned so the network layer can deliver the client-side effect to that player
  /// alone; packet 55 does not mutate server buff state.
  /// </summary>
  public ValueTask<NetworkPlayerStateMutationResult> ApplyPvpBuffToTargetAsync(
    NetworkSessionContext context,
    byte targetSlot,
    ushort buffType,
    int buffTime,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => ApplyPvpBuffToTargetOnOwnerThread(
        session, context, targetSlot, buffType, buffTime),
      cancellationToken);
  }

  private NetworkPlayerStateMutationResult ApplyNetworkStateOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    byte targetSlot,
    NetworkPlayerStateUpdate update)
  {
    NetworkPlayerBindingStatus validation = ValidateContext(
      session, context, allowAwaitPlayerData: false);
    NetworkPlayerStateMutationStatus validationStatus = validation switch
    {
      NetworkPlayerBindingStatus.RejectedStage =>
        NetworkPlayerStateMutationStatus.RejectedStage,
      NetworkPlayerBindingStatus.RejectedSenderBinding =>
        NetworkPlayerStateMutationStatus.RejectedSenderBinding,
      NetworkPlayerBindingStatus.RejectedWorldRuntime =>
        NetworkPlayerStateMutationStatus.RejectedWorldRuntime,
      NetworkPlayerBindingStatus.RejectedPlayerSlot =>
        NetworkPlayerStateMutationStatus.RejectedPlayerSlot,
      _ => NetworkPlayerStateMutationStatus.NotFound,
    };
    if (validation != NetworkPlayerBindingStatus.Applied)
    {
      return new NetworkPlayerStateMutationResult(validationStatus, null);
    }

    if (!_playersBySlot.TryGetValue(context.Actor.PlayerSlot, out BoundPlayer? sender))
    {
      return new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.NotFound, null);
    }

    if (sender.Connection != context.Connection || sender.Binding != context.Actor)
    {
      return new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.RejectedSenderBinding, null);
    }

    if (!_playersBySlot.TryGetValue(targetSlot, out BoundPlayer? target) ||
      !session.EntityRuntime.TryResolve(target.Reference, out RuntimeEntityHandle targetHandle) ||
      targetHandle != target.Handle)
    {
      return new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.NotFound, null);
    }

    if (!ValidateUpdate(update))
    {
      return new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.RejectedInvalidState, null);
    }

    if (!ApplyUpdate(session, targetHandle, update))
    {
      return new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.NotFound, null);
    }

    return CaptureBoundPlayer(session, target) is NetworkPlayerSnapshot snapshot
      ? new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.Applied, snapshot)
      : new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.NotFound, null);
  }

  private NetworkPlayerStateMutationResult ApplyPvpBuffToTargetOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    byte targetSlot,
    ushort buffType,
    int buffTime)
  {
    NetworkPlayerBindingStatus validation = ValidateContext(
      session, context, allowAwaitPlayerData: false);
    if (validation != NetworkPlayerBindingStatus.Applied)
    {
      return new NetworkPlayerStateMutationResult(
        MapValidationStatus(validation), null);
    }

    if (buffTime <= 0 ||
      !IsPvpBuffType(buffType) ||
      !_playersBySlot.TryGetValue(context.Actor.PlayerSlot, out BoundPlayer? sender))
    {
      return new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.NotFound, null);
    }

    if (sender.Connection != context.Connection || sender.Binding != context.Actor)
    {
      return new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.RejectedSenderBinding, null);
    }

    if (!_playersBySlot.TryGetValue(targetSlot, out BoundPlayer? target) ||
      !session.EntityRuntime.TryResolve(sender.Reference, out RuntimeEntityHandle senderHandle) ||
      senderHandle != sender.Handle ||
      !session.EntityRuntime.TryResolve(target.Reference, out RuntimeEntityHandle targetHandle) ||
      targetHandle != target.Handle)
    {
      return new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.NotFound, null);
    }

    if (!session.EntityRuntime.TryCapture(
          senderHandle,
          static (PlayerNetworkStateComponent network) => network.Hostile,
          out bool senderHostile) ||
      !session.EntityRuntime.TryCapture(
          targetHandle,
          static (PlayerNetworkStateComponent network) => network.Hostile,
          out bool targetHostile) ||
      !senderHostile || !targetHostile)
    {
      return new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.RejectedInvalidState, null);
    }

    return CaptureBoundPlayer(session, target) is NetworkPlayerSnapshot snapshot
      ? new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.Applied, snapshot)
      : new NetworkPlayerStateMutationResult(
        NetworkPlayerStateMutationStatus.NotFound, null);
  }

  private static NetworkPlayerStateMutationStatus MapValidationStatus(
    NetworkPlayerBindingStatus validation)
  {
    return validation switch
    {
      NetworkPlayerBindingStatus.RejectedStage =>
        NetworkPlayerStateMutationStatus.RejectedStage,
      NetworkPlayerBindingStatus.RejectedSenderBinding =>
        NetworkPlayerStateMutationStatus.RejectedSenderBinding,
      NetworkPlayerBindingStatus.RejectedWorldRuntime =>
        NetworkPlayerStateMutationStatus.RejectedWorldRuntime,
      NetworkPlayerBindingStatus.RejectedPlayerSlot =>
        NetworkPlayerStateMutationStatus.RejectedPlayerSlot,
      _ => NetworkPlayerStateMutationStatus.NotFound,
    };
  }

  private static bool ValidateUpdate(NetworkPlayerStateUpdate update)
  {
    if ((update.Life.HasValue != update.MaximumLife.HasValue) &&
      (update.Life.HasValue || update.MaximumLife.HasValue))
    {
      return false;
    }

    if (update.Life is int life && update.MaximumLife is int maximumLife &&
      (maximumLife < PlayerNetworkStateSystem.MinimumLifeMaximum ||
        life < 0 || life > maximumLife))
    {
      return false;
    }

    if ((update.Mana.HasValue != update.MaximumMana.HasValue) &&
      (update.Mana.HasValue || update.MaximumMana.HasValue))
    {
      return false;
    }

    if (update.Mana is int mana && update.MaximumMana is int maximumMana &&
      (maximumMana < 0 || mana < 0 || mana > maximumMana))
    {
      return false;
    }

    if (update.BuffTypes is { Count: > PlayerBuffSlotsComponent.MaximumSlotCount })
    {
      return false;
    }

    if (update.PvpBuffType.HasValue != update.PvpBuffTime.HasValue ||
      (update.PvpBuffTime is int pvpBuffTime && pvpBuffTime <= 0))
    {
      return false;
    }

    if (update.HealAmount is int healAmount && healAmount < 0)
    {
      return false;
    }

    if (update.Stealth is float stealth &&
      (!float.IsFinite(stealth) || stealth < 0.0f || stealth > 1.0f))
    {
      return false;
    }

    if (update.MinionRestTarget is Vector2 restTarget &&
      (!float.IsFinite(restTarget.X) || !float.IsFinite(restTarget.Y)))
    {
      return false;
    }

    if (update.TalkNpc is int talkNpc && (talkNpc < -1 || talkNpc > 199))
    {
      return false;
    }

    if (update.MinionAttackTarget is int attackTarget && attackTarget < -1)
    {
      return false;
    }

    if (update.ItemRotation is float rotation && !float.IsFinite(rotation))
    {
      return false;
    }

    if (update.ItemAnimation is int animation && animation < 0)
    {
      return false;
    }

    if ((update.ItemRotation.HasValue || update.ItemAnimation.HasValue) &&
      !update.ItemAnimationChannel.HasValue)
    {
      return false;
    }

    if (update.TeamId is int team && (team < 0 || team > 5))
    {
      return false;
    }

    if ((update.AnglerQuestsFinished is int angler && angler < 0) ||
      (update.GolferScoreAccumulated is int golfer &&
        (golfer < 0 || golfer > 1_000_000_000)))
    {
      return false;
    }

    if (update.Luck is NetworkPlayerLuckUpdate luck &&
      (!float.IsFinite(luck.TorchLuck) ||
        !float.IsFinite(luck.EquipmentBasedLuckBonus) ||
        !float.IsFinite(luck.CoinLuck)))
    {
      return false;
    }

    bool anyZone = update.Zone1.HasValue || update.Zone2.HasValue ||
      update.Zone3.HasValue || update.Zone4.HasValue || update.Zone5.HasValue;
    bool allZones = update.Zone1.HasValue && update.Zone2.HasValue &&
      update.Zone3.HasValue && update.Zone4.HasValue && update.Zone5.HasValue;
    return !anyZone || allZones;
  }

  private static bool ApplyUpdate(
    LoadedWorldSession session,
    RuntimeEntityHandle handle,
    NetworkPlayerStateUpdate update)
  {
    EntityRuntime runtime = session.EntityRuntime;
    if (update.Life is int life && update.MaximumLife is int maximumLife &&
      !Edit(runtime, handle, (PlayerVitalStateComponent vitals) =>
        PlayerNetworkStateSystem.ApplyLifeMana(vitals, life, maximumLife)))
    {
      return false;
    }

    if (update.Mana is int mana && update.MaximumMana is int maximumMana &&
      !Edit(runtime, handle, (PlayerVitalStateComponent vitals) =>
        PlayerNetworkStateSystem.ApplyMana(vitals, mana, maximumMana)))
    {
      return false;
    }

    if (update.BuffTypes is IReadOnlyList<ushort> buffTypes &&
      !Edit(runtime, handle, (PlayerBuffSlotsComponent buffs) =>
        PlayerNetworkStateSystem.ApplyBuffs(buffs, buffTypes)))
    {
      return false;
    }

    if (update.PvpBuffType is ushort pvpBuffType && update.PvpBuffTime is int pvpBuffTime &&
      !EditPair(runtime, handle,
        (PlayerBuffComponent buffs, PlayerNetworkStateComponent network) =>
          PlayerNetworkStateSystem.ApplyPvpBuff(buffs, network, pvpBuffType, pvpBuffTime)))
    {
      return false;
    }

    if (update.HealAmount is int healAmount && healAmount > 0 &&
      !Edit(runtime, handle, (PlayerVitalStateComponent vitals) =>
        {
          _ = PlayerNetworkStateSystem.ApplyHealAndClamp(vitals, healAmount);
          return true;
        }))
    {
      return false;
    }

    if (update.Hostile is bool hostile &&
      !Edit(runtime, handle, (PlayerNetworkStateComponent network) =>
        PlayerNetworkStateSystem.ApplyHostile(network, hostile)))
    {
      return false;
    }

    if (update.Stealth is float stealth &&
      !Edit(runtime, handle, (PlayerNetworkStateComponent network) =>
        PlayerNetworkStateSystem.ApplyStealth(network, stealth)))
    {
      return false;
    }

    if (update.ShouldNotDraw is bool shouldNotDraw &&
      !Edit(runtime, handle, (PlayerNetworkStateComponent network) =>
        ApplyShouldNotDraw(network, shouldNotDraw)))
    {
      return false;
    }

    if (update.TalkNpc is int talkNpc &&
      !Edit(runtime, handle, (PlayerNetworkStateComponent network) =>
        PlayerNetworkStateSystem.ApplyTalkNpc(network, talkNpc)))
    {
      return false;
    }

    if (update.MinionRestTarget is Vector2 restTarget &&
      !Edit(runtime, handle, (PlayerNetworkStateComponent network) =>
        PlayerNetworkStateSystem.ApplyMinionRestTarget(network, restTarget)))
    {
      return false;
    }

    if (update.MinionAttackTarget is int attackTarget &&
      !Edit(runtime, handle, (PlayerNetworkStateComponent network) =>
        PlayerNetworkStateSystem.ApplyMinionAttackTarget(network, attackTarget)))
    {
      return false;
    }

    if (update.TownNpcCount is byte townNpcCount &&
      !Edit(runtime, handle, (PlayerNetworkStateComponent network) =>
        ApplyTownNpcCount(network, townNpcCount)))
    {
      return false;
    }

    if (update.Zone1 is byte zone1 && update.Zone2 is byte zone2 &&
      update.Zone3 is byte zone3 && update.Zone4 is byte zone4 &&
      update.Zone5 is byte zone5 &&
      !Edit(runtime, handle, (PlayerZoneAndEnvironmentStateComponent environment) =>
        ApplyZone(environment, zone1, zone2, zone3, zone4, zone5,
          update.TownNpcCount ?? 0)))
    {
      return false;
    }

    if (update.ItemRotation is float rotation && update.ItemAnimation is int animation &&
      update.ItemAnimationChannel is byte channel &&
      !Edit(runtime, handle, (PlayerNetworkStateComponent network) =>
        PlayerNetworkStateSystem.ApplyItemAnimation(network, rotation, animation, channel)))
    {
      return false;
    }

    if (update.TeamId is int team &&
      !Edit(runtime, handle, (PlayerIdentityComponent identity) =>
        ApplyTeam(identity, team)))
    {
      return false;
    }

    if ((update.AnglerQuestsFinished.HasValue || update.GolferScoreAccumulated.HasValue) &&
      !Edit(runtime, handle, (PlayerQuestEventProgressComponent progress) =>
        progress.ApplyNetworkCounts(update.AnglerQuestsFinished ?? progress.AnglerQuestsFinished,
          update.GolferScoreAccumulated ?? progress.GolferScoreAccumulated)))
    {
      return false;
    }

    if (update.Luck is NetworkPlayerLuckUpdate luck)
    {
      // Capture optional inputs before editing the luck component. EntityRuntime
      // deliberately rejects a second capture while an entity is borrowed by Edit.
      bool usedGalaxyPearl = CaptureUsedGalaxyPearl(runtime, handle);
      bool stinky = CaptureStinky(runtime, handle);
      if (!Edit(runtime, handle, (PlayerLuckAndRescanStateComponent state) =>
          state.ApplyNetworkFactors(
            luck.LadyBugLuckTime,
            luck.TorchLuck,
            luck.LuckPotion,
            luck.HasGardenGnomeNearby,
            luck.BrokenMirrorBadLuck,
            luck.EquipmentBasedLuckBonus,
            luck.CoinLuck,
            luck.KiteLuckLevel,
            usedGalaxyPearl,
            lanternsUp: false,
            stinky: stinky)))
      {
        return false;
      }
    }

    return true;
  }

  private static bool Edit<TComponent>(
    EntityRuntime runtime,
    RuntimeEntityHandle handle,
    Func<TComponent, bool> apply)
    where TComponent : notnull
  {
    bool applied = false;
    bool edited = runtime.TryEdit(handle, (ref TComponent component) =>
      applied = apply(component));
    return edited && applied;
  }

  private static bool EditPair<TFirstComponent, TSecondComponent>(
    EntityRuntime runtime,
    RuntimeEntityHandle handle,
    Func<TFirstComponent, TSecondComponent, bool> apply)
    where TFirstComponent : notnull
    where TSecondComponent : notnull
  {
    bool applied = false;
    bool edited = runtime.TryEditPair(handle,
      (ref TFirstComponent first, ref TSecondComponent second) =>
        applied = apply(first, second));
    return edited && applied;
  }

  private static bool ApplyShouldNotDraw(PlayerNetworkStateComponent network, bool value)
  {
    network.ApplyShouldNotDraw(value);
    return true;
  }

  private static bool ApplyTownNpcCount(PlayerNetworkStateComponent network, byte value)
  {
    network.ApplyTownNpcCount(value);
    return true;
  }

  private static bool ApplyZone(
    PlayerZoneAndEnvironmentStateComponent environment,
    byte zone1,
    byte zone2,
    byte zone3,
    byte zone4,
    byte zone5,
    byte townNpcCount)
  {
    PlayerNetworkStateSystem.ApplyZone(
      environment, zone1, zone2, zone3, zone4, zone5, townNpcCount);
    return true;
  }

  private static bool ApplyTeam(PlayerIdentityComponent identity, int team)
  {
    identity.TeamId = team;
    return true;
  }

  private static bool IsPvpBuffType(ushort buffType)
  {
    return buffType switch
    {
      20 or 24 or 30 or 31 or 36 or 39 or 44 or 69 or 70 or 103 or
      119 or 120 or 137 or 320 or 323 or 324 => true,
      _ => false,
    };
  }

  private static bool CaptureUsedGalaxyPearl(
    EntityRuntime runtime,
    RuntimeEntityHandle handle)
  {
    return runtime.TryCapture(
      handle,
      static (PlayerConsumedProgressionLedgerComponent progression) =>
        progression.UsedGalaxyPearl,
      out bool usedGalaxyPearl) && usedGalaxyPearl;
  }

  private static bool CaptureStinky(EntityRuntime runtime, RuntimeEntityHandle handle)
  {
    return runtime.TryCapture(
      handle,
      static (PlayerCombatDetectionStateComponent combat) => combat.Stinky,
      out bool stinky) && stinky;
  }

  private static NetworkPlayerStateUpdate CloneUpdate(NetworkPlayerStateUpdate update)
  {
    IReadOnlyList<ushort>? buffs = update.BuffTypes is null
      ? null
      : Array.AsReadOnly(update.BuffTypes.ToArray());
    return update with { BuffTypes = buffs };
  }
}
