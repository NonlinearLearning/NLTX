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
using Terraria.Player.Mount;
using Terraria.SpatialSimulation.Components;

namespace Terraria.Network;

public enum NetworkPlayerBindingStatus : byte
{
  Applied,
  Existing,
  ReplacedStaleBinding,
  Disconnected,
  NotFound,
  RejectedStage,
  RejectedSenderBinding,
  RejectedWorldRuntime,
  RejectedPlayerSlot,
}

public readonly record struct NetworkPlayerBindingResult(
  NetworkPlayerBindingStatus Status,
  NetworkPlayerSnapshot? Player)
{
  public bool Succeeded => Status is
    NetworkPlayerBindingStatus.Applied or
    NetworkPlayerBindingStatus.Existing or
    NetworkPlayerBindingStatus.ReplacedStaleBinding;
}

public readonly record struct NetworkPlayerReferenceResult(
  NetworkPlayerBindingStatus Status,
  EntityReference Reference)
{
  public bool Succeeded => Status == NetworkPlayerBindingStatus.Existing ||
    Status == NetworkPlayerBindingStatus.Applied ||
    Status == NetworkPlayerBindingStatus.ReplacedStaleBinding;
}

public readonly record struct NetworkPlayerMovementInput(
  bool ApplyPosition,
  Vector2 Position,
  bool ApplyVelocity,
  Vector2 Velocity,
  int? SelectedInventorySlot = null,
  bool AcknowledgeTeleport = false,
  byte ControlFlags = 0,
  byte MovementFlags = 0,
  byte PlayerFeatureFlags = 0,
  byte ActionFlags = 0,
  ushort? MountType = null,
  Vector2? PotionOfReturnUsePosition = null,
  Vector2? PotionOfReturnHomePosition = null,
  Vector2? CameraTarget = null);

public enum NetworkPlayerMovementStatus : byte
{
  Applied,
  NotFound,
  RejectedSenderBinding,
  RejectedWorldRuntime,
  RejectedInvalidState,
}

public readonly record struct NetworkPlayerMovementResult(
  NetworkPlayerMovementStatus Status,
  NetworkPlayerSnapshot? Player)
{
  public bool Succeeded => Status == NetworkPlayerMovementStatus.Applied;
}

/// <summary>
/// Owns the authenticated player projection for one <see cref="NetworkWorldOwner"/> session.
/// </summary>
/// <remarks>
/// The slot and connection dictionaries are owner-thread indexes only. EntityUuid and
/// RuntimeEntityHandle remain the sole identity source; every public result is detached.
/// </remarks>
public sealed partial class NetworkPlayerOwner
{
  private const int PlayerWidth = 20;
  private const int PlayerHeight = 42;

  private readonly Func<NetworkSessionContext, bool> _isCurrentSender;
  private readonly NetworkWorldOwner _worldOwner;
  private readonly IItemDefinitionQuery? _itemDefinitions;
  private readonly IPlayerFaelingSpawnPort? _faelingSpawnPort;
  private readonly Dictionary<byte, BoundPlayer> _playersBySlot = new();
  private readonly Dictionary<ConnectionIdentity, byte> _slotsByConnection = new();

  public NetworkPlayerOwner(
    NetworkWorldOwner worldOwner,
    Func<NetworkSessionContext, bool> isCurrentSender,
    IItemDefinitionQuery? itemDefinitions = null,
    IPlayerFaelingSpawnPort? faelingSpawnPort = null)
  {
    _worldOwner = worldOwner ?? throw new ArgumentNullException(nameof(worldOwner));
    _isCurrentSender = isCurrentSender ?? throw new ArgumentNullException(nameof(isCurrentSender));
    _itemDefinitions = itemDefinitions;
    _faelingSpawnPort = faelingSpawnPort;
  }

  /// <summary>Creates or returns the authenticated player's entity on the owner thread.</summary>
  public ValueTask<NetworkPlayerBindingResult> EnsurePlayerAsync(
    NetworkSessionContext context,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => EnsurePlayerOnOwnerThread(session, context),
      cancellationToken);
  }

  /// <summary>Captures the authenticated player's detached state from the same world session.</summary>
  public ValueTask<NetworkPlayerSnapshot?> CapturePlayerAsync(
    NetworkSessionContext context,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => CapturePlayerOnOwnerThread(session, context),
      cancellationToken);
  }

  /// <summary>Resolves the authenticated player's current scoped entity reference.</summary>
  public ValueTask<NetworkPlayerReferenceResult> ResolveAuthenticatedPlayerAsync(
    NetworkSessionContext context,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => ResolveAuthenticatedPlayerOnOwnerThread(session, context),
      cancellationToken);
  }

  /// <summary>Removes a player before the session binding is released.</summary>
  public ValueTask<NetworkPlayerBindingStatus> DisconnectAsync(
    NetworkSessionContext context,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => DisconnectOnOwnerThread(session, context),
      cancellationToken);
  }

  /// <summary>
  /// Commits authenticated movement and optional selection on the same owner thread.
  /// </summary>
  public ValueTask<NetworkPlayerMovementResult> CommitMovementAsync(
    NetworkSessionContext context,
    NetworkPlayerMovementInput input,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    // Capture a local value before crossing the owner-thread callback boundary.
    NetworkPlayerMovementInput movementInput = input;
    return _worldOwner.InvokeAsync(
      session => CommitMovementOnOwnerThread(session, context, movementInput),
      cancellationToken);
  }

  public ValueTask<NetworkPlayerMutationResult> ApplyIdentityAsync(
    NetworkSessionContext context,
    PlayerNetworkIdentityInput input,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    PlayerNetworkIdentityInput identityInput = input;
    return _worldOwner.InvokeAsync(
      session => ApplyIdentityOnOwnerThread(session, context, identityInput),
      cancellationToken);
  }

  /// <summary>
  /// Creates and initializes a player in one owner operation. A newly created
  /// binding is removed if identity validation fails, so rejected admission does
  /// not leave a default player entity behind.
  /// </summary>
  public ValueTask<NetworkPlayerMutationResult> ApplyIdentityForAdmissionAsync(
    NetworkSessionContext context,
    PlayerNetworkIdentityInput input,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    PlayerNetworkIdentityInput identityInput = input;
    return _worldOwner.InvokeAsync(session => {
      NetworkPlayerMutationStatus preflight = ValidateIdentityAdmissionOnOwnerThread(
        session, context, identityInput);
      if (preflight != NetworkPlayerMutationStatus.Applied)
      {
        return new NetworkPlayerMutationResult(preflight, null);
      }

      NetworkPlayerBindingResult binding = EnsurePlayerOnOwnerThread(session, context);
      if (!binding.Succeeded)
      {
        NetworkPlayerMutationStatus status = binding.Status switch
        {
          NetworkPlayerBindingStatus.RejectedStage => NetworkPlayerMutationStatus.RejectedStage,
          NetworkPlayerBindingStatus.RejectedSenderBinding =>
            NetworkPlayerMutationStatus.RejectedSenderBinding,
          NetworkPlayerBindingStatus.RejectedWorldRuntime =>
            NetworkPlayerMutationStatus.RejectedWorldRuntime,
          NetworkPlayerBindingStatus.RejectedPlayerSlot =>
            NetworkPlayerMutationStatus.RejectedInvalidState,
          _ => NetworkPlayerMutationStatus.NotFound
        };
        return new NetworkPlayerMutationResult(status, null);
      }

      NetworkPlayerMutationResult result = ApplyIdentityOnOwnerThread(
        session, context, identityInput);
      if (!result.Succeeded &&
          (binding.Status is NetworkPlayerBindingStatus.Applied or
            NetworkPlayerBindingStatus.ReplacedStaleBinding) &&
          _playersBySlot.TryGetValue(context.Actor.PlayerSlot, out BoundPlayer? created) &&
          created.Connection == context.Connection && created.Binding == context.Actor)
      {
        RemoveBoundPlayer(session, context.Actor.PlayerSlot, created);
      }

      return result;
    }, cancellationToken);
  }

  public ValueTask<NetworkPlayerMutationResult> CommitSpawnAsync(
    NetworkSessionContext context,
    PlayerSpawnPacket12Input input,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    PlayerSpawnPacket12Input spawnInput = input;
    return _worldOwner.InvokeAsync(
      session => CommitSpawnOnOwnerThread(session, context, spawnInput),
      cancellationToken);
  }

  public ValueTask<NetworkPlayerMutationResult> ApplyConnectionAsync(
    NetworkSessionContext context,
    bool active,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => ApplyConnectionOnOwnerThread(session, context, active),
      cancellationToken);
  }

  public ValueTask<NetworkPlayerMutationResult> ApplyLifeAsync(
    NetworkSessionContext context,
    int life,
    int maximumLife,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => ApplyLifeOnOwnerThread(session, context, life, maximumLife),
      cancellationToken);
  }

  public ValueTask<NetworkPlayerMutationResult> ApplyManaAsync(
    NetworkSessionContext context,
    int mana,
    int maximumMana,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => ApplyManaOnOwnerThread(session, context, mana, maximumMana),
      cancellationToken);
  }

  public ValueTask<NetworkPlayerMutationResult> ApplyBuffsAsync(
    NetworkSessionContext context,
    IReadOnlyList<ushort> buffTypes,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(buffTypes);
    cancellationToken.ThrowIfCancellationRequested();
    ushort[] copiedBuffTypes = buffTypes.ToArray();
    return _worldOwner.InvokeAsync(
      session => ApplyBuffsOnOwnerThread(session, context, copiedBuffTypes),
      cancellationToken);
  }

  private NetworkPlayerBindingResult EnsurePlayerOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context)
  {
    NetworkPlayerBindingStatus validation = ValidateContext(session, context, allowAwaitPlayerData: true);
    if (validation != NetworkPlayerBindingStatus.Applied)
    {
      return new NetworkPlayerBindingResult(validation, null);
    }

    byte slot = context.Actor.PlayerSlot;
    bool replacedStaleBinding = false;
    if (_playersBySlot.TryGetValue(slot, out BoundPlayer? existing))
    {
      if (existing.Connection == context.Connection && existing.Binding == context.Actor)
      {
        NetworkPlayerSnapshot? current = CaptureBoundPlayer(session, existing);
        return new NetworkPlayerBindingResult(
          current.HasValue ? NetworkPlayerBindingStatus.Existing :
            NetworkPlayerBindingStatus.NotFound,
          current);
      }

      RemoveBoundPlayer(session, slot, existing);
      replacedStaleBinding = true;
    }

    RuntimeEntityHandle handle = session.EntityRuntime.CreateEntity();
    try
    {
      AttachPlayerComponents(session, handle, slot);
      if (!session.EntityRuntime.TryPublishEntity(handle) ||
          !session.EntityRuntime.TryGetReference(
            handle,
            EntityReferenceScope.Player,
            out EntityReference reference))
      {
        throw new InvalidOperationException("The network player entity could not be published.");
      }

      var bound = new BoundPlayer(context.Connection, context.Actor, handle, reference);
      _playersBySlot[slot] = bound;
      _slotsByConnection[context.Connection] = slot;
      NetworkPlayerSnapshot? snapshot = CaptureBoundPlayer(session, bound);
      if (snapshot is null)
      {
        RemoveBoundPlayer(session, slot, bound);
        return new NetworkPlayerBindingResult(NetworkPlayerBindingStatus.NotFound, null);
      }

      return new NetworkPlayerBindingResult(
        replacedStaleBinding ? NetworkPlayerBindingStatus.ReplacedStaleBinding :
          NetworkPlayerBindingStatus.Applied,
        snapshot);
    }
    catch
    {
      RemoveUnpublishedEntity(session.EntityRuntime, handle);
      throw;
    }
  }

  private NetworkPlayerSnapshot? CapturePlayerOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context)
  {
    NetworkPlayerBindingStatus validation = ValidateContext(session, context, allowAwaitPlayerData: false);
    if (validation != NetworkPlayerBindingStatus.Applied ||
        !_playersBySlot.TryGetValue(context.Actor.PlayerSlot, out BoundPlayer? bound) ||
        bound.Connection != context.Connection || bound.Binding != context.Actor)
    {
      return null;
    }

    return CaptureBoundPlayer(session, bound);
  }

  private NetworkPlayerReferenceResult ResolveAuthenticatedPlayerOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context)
  {
    NetworkPlayerBindingStatus validation = ValidateContext(session, context, allowAwaitPlayerData: false);
    if (validation != NetworkPlayerBindingStatus.Applied)
    {
      return new NetworkPlayerReferenceResult(validation, EntityReference.None);
    }

    if (!_playersBySlot.TryGetValue(context.Actor.PlayerSlot, out BoundPlayer? bound))
    {
      return new NetworkPlayerReferenceResult(
        NetworkPlayerBindingStatus.NotFound,
        EntityReference.None);
    }

    // A slot that is now occupied by another connection/binding is an
    // authorization failure for the captured sender. Keep NotFound for a
    // genuinely absent or already-invalid entity so callers can distinguish
    // stale sender binding from ordinary lifecycle cleanup.
    if (bound.Connection != context.Connection || bound.Binding != context.Actor)
    {
      return new NetworkPlayerReferenceResult(
        NetworkPlayerBindingStatus.RejectedSenderBinding,
        EntityReference.None);
    }

    if (!session.EntityRuntime.TryResolve(bound.Reference, out RuntimeEntityHandle handle) ||
        handle != bound.Handle)
    {
      return new NetworkPlayerReferenceResult(
        NetworkPlayerBindingStatus.NotFound,
        EntityReference.None);
    }

    return new NetworkPlayerReferenceResult(
      NetworkPlayerBindingStatus.Existing,
      bound.Reference);
  }

  private NetworkPlayerBindingStatus DisconnectOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context)
  {
    if (context.WorldRuntimeId is { } expectedRuntimeId &&
        expectedRuntimeId != session.EntityRuntime.RuntimeId)
    {
      return NetworkPlayerBindingStatus.RejectedWorldRuntime;
    }

    if (context.Actor.PlayerSlot == byte.MaxValue)
    {
      return NetworkPlayerBindingStatus.RejectedPlayerSlot;
    }

    if (!_playersBySlot.TryGetValue(context.Actor.PlayerSlot, out BoundPlayer? bound) ||
        bound.Connection != context.Connection || bound.Binding != context.Actor)
    {
      return NetworkPlayerBindingStatus.NotFound;
    }

    RemoveBoundPlayer(session, context.Actor.PlayerSlot, bound);
    return NetworkPlayerBindingStatus.Disconnected;
  }

  private NetworkPlayerMovementResult CommitMovementOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    NetworkPlayerMovementInput input)
  {
    NetworkPlayerMovementInput movementInput = input;
    NetworkPlayerBindingStatus validation = ValidateContext(session, context, allowAwaitPlayerData: false);
    if (validation == NetworkPlayerBindingStatus.RejectedSenderBinding)
    {
      return new NetworkPlayerMovementResult(
        NetworkPlayerMovementStatus.RejectedSenderBinding,
        null);
    }

    if (validation == NetworkPlayerBindingStatus.RejectedWorldRuntime)
    {
      return new NetworkPlayerMovementResult(
        NetworkPlayerMovementStatus.RejectedWorldRuntime,
        null);
    }

    if (validation != NetworkPlayerBindingStatus.Applied ||
        !_playersBySlot.TryGetValue(context.Actor.PlayerSlot, out BoundPlayer? bound) ||
        bound.Connection != context.Connection || bound.Binding != context.Actor)
    {
      return new NetworkPlayerMovementResult(NetworkPlayerMovementStatus.NotFound, null);
    }

    if ((movementInput.ApplyPosition && !IsFinite(movementInput.Position)) ||
        (movementInput.ApplyVelocity && !IsFinite(movementInput.Velocity)) ||
        (movementInput.PotionOfReturnUsePosition is Vector2 usePosition &&
          !IsFinite(usePosition)) ||
        (movementInput.PotionOfReturnHomePosition is Vector2 homePosition &&
          !IsFinite(homePosition)) ||
        (movementInput.CameraTarget is Vector2 cameraTarget &&
          !IsFinite(cameraTarget)) ||
        (movementInput.SelectedInventorySlot is < 0 or >= PlayerInventoryComponent.MainInventorySlotCount))
    {
      return new NetworkPlayerMovementResult(
        NetworkPlayerMovementStatus.RejectedInvalidState,
        null);
    }

    bool capturedPendingTeleport = session.EntityRuntime.TryCapture(
      bound.Handle,
      static (PlayerNetworkStateComponent network) => new PendingTeleportProjection(
        network.PendingTeleportPosition is not null,
        network.PendingTeleportPosition.GetValueOrDefault()),
      out PendingTeleportProjection pendingTeleportProjection);
    bool pendingTeleport = capturedPendingTeleport && pendingTeleportProjection.HasPending;
    Vector2? pendingTeleportPosition = pendingTeleport
      ? pendingTeleportProjection.Position
      : null;
    bool acknowledgeTeleport = movementInput.AcknowledgeTeleport;
    bool applyPosition = movementInput.ApplyPosition;
    if (pendingTeleport && pendingTeleportPosition is Vector2 pendingPosition)
    {
      bool matchesPendingPosition = movementInput.ApplyPosition &&
        movementInput.Position == pendingPosition;
      if (movementInput.AcknowledgeTeleport && !matchesPendingPosition)
      {
        return new NetworkPlayerMovementResult(
          NetworkPlayerMovementStatus.RejectedInvalidState,
          null);
      }

      applyPosition = matchesPendingPosition;
      acknowledgeTeleport = matchesPendingPosition;
    }

    bool spatialEdited = session.EntityRuntime.TryEditPair<LocationComponent, VelocityComponent>(
      bound.Handle,
      (ref LocationComponent location, ref VelocityComponent velocity) =>
      {
        if (applyPosition)
        {
          location.X = movementInput.Position.X;
          location.Y = movementInput.Position.Y;
        }

        if (movementInput.ApplyVelocity)
        {
          velocity.X = movementInput.Velocity.X;
          velocity.Y = movementInput.Velocity.Y;
        }
      });
    if (!spatialEdited)
    {
      return new NetworkPlayerMovementResult(
        NetworkPlayerMovementStatus.NotFound,
        null);
    }

    if (movementInput.SelectedInventorySlot is int selectedSlot &&
        !session.EntityRuntime.TryEdit<PlayerInventoryComponent>(
          bound.Handle,
          (ref PlayerInventoryComponent inventory) => inventory.SelectedSlotIndex = selectedSlot))
    {
      return new NetworkPlayerMovementResult(
        NetworkPlayerMovementStatus.RejectedInvalidState,
        null);
    }

    if (acknowledgeTeleport &&
        !session.EntityRuntime.TryEdit<PlayerNetworkStateComponent>(
          bound.Handle,
          (ref PlayerNetworkStateComponent network) =>
            PlayerNetworkStateSystem.AcknowledgeTeleport(network)))
    {
      return new NetworkPlayerMovementResult(
        NetworkPlayerMovementStatus.RejectedInvalidState,
        null);
    }

    if (!session.EntityRuntime.TryEdit<PlayerNetworkStateComponent>(
          bound.Handle,
          (ref PlayerNetworkStateComponent network) =>
          {
            network.ApplyControlNetworkState(
              movementInput.ControlFlags,
              movementInput.MovementFlags,
              movementInput.PlayerFeatureFlags,
              movementInput.ActionFlags);
            network.ApplyPotionOfReturnState(
              movementInput.PotionOfReturnUsePosition,
              movementInput.PotionOfReturnHomePosition);
          }))
    {
      return new NetworkPlayerMovementResult(
        NetworkPlayerMovementStatus.RejectedInvalidState,
        null);
    }

    if (!session.EntityRuntime.TryEdit<PlayerMountComponent>(
          bound.Handle,
          (ref PlayerMountComponent mount) =>
          {
            if (movementInput.MountType is ushort mountType)
            {
              mount.MountType = new ContentId<MountDefinition>(mountType);
              mount.IsActive = true;
            }
            else
            {
              mount.MountType = null;
              mount.IsActive = false;
            }
          }))
    {
      return new NetworkPlayerMovementResult(
        NetworkPlayerMovementStatus.RejectedInvalidState,
        null);
    }

    if (!session.EntityRuntime.TryEdit<PlayerNetworkCameraStateComponent>(
          bound.Handle,
          (ref PlayerNetworkCameraStateComponent camera) =>
            camera.ApplyNetworkCameraTarget(movementInput.CameraTarget)))
    {
      return new NetworkPlayerMovementResult(
        NetworkPlayerMovementStatus.RejectedInvalidState,
        null);
    }

    NetworkPlayerSnapshot? snapshot = CaptureBoundPlayer(session, bound);
    return snapshot is NetworkPlayerSnapshot captured
      ? new NetworkPlayerMovementResult(NetworkPlayerMovementStatus.Applied, captured)
      : new NetworkPlayerMovementResult(NetworkPlayerMovementStatus.NotFound, null);
  }

  private NetworkPlayerMutationResult ApplyIdentityOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    PlayerNetworkIdentityInput input)
  {
    NetworkPlayerMutationStatus status = TryGetBoundForMutation(
      session, context, allowAwaitPlayerData: true, out BoundPlayer? bound);
    if (status != NetworkPlayerMutationStatus.Applied)
    {
      return new NetworkPlayerMutationResult(status, null);
    }

    if ((input.Difficulty == PlayerDifficulty.Journey) != session.World.Rules.IsJourneyMode)
    {
      return new NetworkPlayerMutationResult(
        NetworkPlayerMutationStatus.RejectedInvalidState, null);
    }

    string characterName = input.CharacterName.Trim();
    foreach ((byte slot, BoundPlayer other) in _playersBySlot)
    {
      if (slot == context.Actor.PlayerSlot ||
          !session.EntityRuntime.TryCapture(
            other.Handle,
            static (PlayerIdentityComponent identity) =>
              (identity.CharacterName, identity.IsActive),
            out (string CharacterName, bool IsActive) otherIdentity))
      {
        continue;
      }

      if (otherIdentity.IsActive &&
          string.Equals(otherIdentity.CharacterName, characterName,
            StringComparison.Ordinal))
      {
        return new NetworkPlayerMutationResult(
          NetworkPlayerMutationStatus.RejectedInvalidState, null);
      }
    }

    bool applied = false;
    bool edited = session.EntityRuntime.TryEditComponents<
      PlayerIdentityComponent,
      PlayerNetworkStateComponent,
      PlayerAppearanceCustomizationComponent>(
      bound!.Handle,
      (ref PlayerIdentityComponent identity,
        ref PlayerNetworkStateComponent network,
        ref PlayerAppearanceCustomizationComponent appearance) =>
      {
        applied = PlayerNetworkStateSystem.ApplyIdentity(
          identity, network, appearance, input);
      });
    if (!edited)
    {
      return new NetworkPlayerMutationResult(NetworkPlayerMutationStatus.NotFound, null);
    }

    if (!applied || !session.EntityRuntime.TryEdit<PlayerAppearanceSelectionComponent>(
          bound.Handle,
          (ref PlayerAppearanceSelectionComponent selection) =>
            PlayerNetworkStateSystem.ApplyIdentitySelection(selection, input)))
    {
      return new NetworkPlayerMutationResult(
        NetworkPlayerMutationStatus.RejectedInvalidState, null);
    }

    return CaptureMutationResult(session, bound);
  }

  private NetworkPlayerMutationStatus ValidateIdentityAdmissionOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    PlayerNetworkIdentityInput input)
  {
    NetworkPlayerBindingStatus bindingStatus = ValidateContext(
      session, context, allowAwaitPlayerData: true);
    if (bindingStatus != NetworkPlayerBindingStatus.Applied)
    {
      return bindingStatus switch
      {
        NetworkPlayerBindingStatus.RejectedStage => NetworkPlayerMutationStatus.RejectedStage,
        NetworkPlayerBindingStatus.RejectedSenderBinding =>
          NetworkPlayerMutationStatus.RejectedSenderBinding,
        NetworkPlayerBindingStatus.RejectedWorldRuntime =>
          NetworkPlayerMutationStatus.RejectedWorldRuntime,
        _ => NetworkPlayerMutationStatus.NotFound
      };
    }

    string characterName = input.CharacterName.Trim();
    if (characterName.Length == 0 || characterName.Length > 20 ||
        !float.IsFinite(input.VoicePitchOffset) || !Enum.IsDefined(input.Difficulty) ||
        (input.Difficulty == PlayerDifficulty.Journey) != session.World.Rules.IsJourneyMode)
    {
      return NetworkPlayerMutationStatus.RejectedInvalidState;
    }

    foreach ((byte slot, BoundPlayer other) in _playersBySlot)
    {
      if (slot == context.Actor.PlayerSlot ||
          !session.EntityRuntime.TryCapture(
            other.Handle,
            static (PlayerIdentityComponent identity) =>
              (identity.CharacterName, identity.IsActive),
            out (string CharacterName, bool IsActive) otherIdentity))
      {
        continue;
      }

      if (otherIdentity.IsActive && string.Equals(
          otherIdentity.CharacterName, characterName, StringComparison.Ordinal))
      {
        return NetworkPlayerMutationStatus.RejectedInvalidState;
      }
    }

    return NetworkPlayerMutationStatus.Applied;
  }

  private NetworkPlayerMutationResult CommitSpawnOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    PlayerSpawnPacket12Input input)
  {
    NetworkPlayerMutationStatus status = TryGetBoundForMutation(
      session, context, allowAwaitPlayerData: true, out BoundPlayer? bound);
    if (status != NetworkPlayerMutationStatus.Applied)
    {
      return new NetworkPlayerMutationResult(status, null);
    }

    if (!input.HasAuthenticatedSender ||
        input.DeclaredPlayerSlot != context.Actor.PlayerSlot ||
        input.AuthenticatedSenderSlot != context.Actor.PlayerSlot ||
        input.SpawnContextValue > 3 || input.PveDeathCount < 0 ||
        input.PvpDeathCount < 0 || input.RespawnRemainingTicks < 0 ||
        input.TeamId is < 0 or > 5 ||
        input.CurrentUtcTime.Kind != DateTimeKind.Utc)
    {
      return new NetworkPlayerMutationResult(
        NetworkPlayerMutationStatus.RejectedInvalidState, null);
    }

    bool applied = false;
    bool edited = session.EntityRuntime.TryEditComponents<
      PlayerIdentityComponent,
      PlayerLifecycleComponent,
      PlayerDeathRecordComponent,
      PlayerRestComponent,
      PlayerSittingComponent,
      PlayerSleepingComponent>(
      bound!.Handle,
      (ref PlayerIdentityComponent identity,
        ref PlayerLifecycleComponent lifecycle,
        ref PlayerDeathRecordComponent deathRecord,
        ref PlayerRestComponent rest,
        ref PlayerSittingComponent sitting,
        ref PlayerSleepingComponent sleeping) =>
      {
        PlayerNetworkStateSystem.ApplySpawnDeathCounts(
          deathRecord, input.PveDeathCount, input.PvpDeathCount);
        identity.TeamId = input.TeamId;
        if (input.RespawnRemainingTicks > 0)
        {
          lifecycle = new PlayerLifecycleComponent(
            PlayerLifecyclePhase.Dead,
            input.RespawnRemainingTicks);
        }
        PlayerLifecycleSystem.CommitSpawn(
          identity,
          ref lifecycle,
          deathRecord,
          rest,
          sitting,
          sleeping,
          new PlayerLifecycleSystem.SpawnCommitInput(
            IsSpawningIntoWorld: input.SpawnContextValue == 1,
            LastTimePlayerWasSavedBinary: input.LastTimePlayerWasSavedBinary,
            CurrentUtcTime: input.CurrentUtcTime,
            IsLocalPlayer: input.IsLocalPlayer,
            MultiplayerBroadcast: input.MultiplayerBroadcast));
        applied = true;
      });
    if (!edited || !applied)
    {
      return new NetworkPlayerMutationResult(NetworkPlayerMutationStatus.NotFound, null);
    }

    Vector2 spawnPosition = new(input.SpawnX, input.SpawnY);
    if (!session.EntityRuntime.TryReplace(
          bound.Handle, new PlayerSpawnPointComponent(spawnPosition)) ||
        !session.EntityRuntime.TryEdit<LocationComponent>(
          bound.Handle,
          (ref LocationComponent location) =>
          {
            location.X = spawnPosition.X;
            location.Y = spawnPosition.Y;
          }))
    {
      return new NetworkPlayerMutationResult(NetworkPlayerMutationStatus.NotFound, null);
    }

    return CaptureMutationResult(session, bound);
  }

  private NetworkPlayerMutationResult ApplyConnectionOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    bool active)
  {
    NetworkPlayerMutationStatus status = TryGetBoundForMutation(
      session, context, allowAwaitPlayerData: true, out BoundPlayer? bound);
    if (status != NetworkPlayerMutationStatus.Applied)
    {
      return new NetworkPlayerMutationResult(status, null);
    }

    bool edited = session.EntityRuntime.TryEdit<PlayerIdentityComponent>(
      bound!.Handle,
      (ref PlayerIdentityComponent identity) =>
        PlayerLifecycleSystem.SetConnectionState(
          identity,
          active ? PlayerConnectionState.Active : PlayerConnectionState.Inactive,
          PlayerLifecycleSystem.ConnectionStateChangeSource.NetworkActiveState));
    return edited
      ? CaptureMutationResult(session, bound)
      : new NetworkPlayerMutationResult(NetworkPlayerMutationStatus.NotFound, null);
  }

  private NetworkPlayerMutationResult ApplyLifeOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    int life,
    int maximumLife)
  {
    return ApplyVitalOnOwnerThread(session, context, life, maximumLife, applyMana: false);
  }

  private NetworkPlayerMutationResult ApplyManaOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    int mana,
    int maximumMana)
  {
    return ApplyVitalOnOwnerThread(session, context, mana, maximumMana, applyMana: true);
  }

  private NetworkPlayerMutationResult ApplyVitalOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    int value,
    int maximum,
    bool applyMana)
  {
    NetworkPlayerMutationStatus status = TryGetBoundForMutation(
      session, context, allowAwaitPlayerData: true, out BoundPlayer? bound);
    if (status != NetworkPlayerMutationStatus.Applied)
    {
      return new NetworkPlayerMutationResult(status, null);
    }

    bool applied = false;
    bool edited = applyMana
      ? session.EntityRuntime.TryEdit<PlayerVitalStateComponent>(
      bound!.Handle,
      (ref PlayerVitalStateComponent vital) =>
        applied = PlayerNetworkStateSystem.ApplyMana(vital, value, maximum))
      : session.EntityRuntime.TryEditPair<
        PlayerVitalStateComponent,
        PlayerLifecycleComponent>(
        bound!.Handle,
        (ref PlayerVitalStateComponent vital,
          ref PlayerLifecycleComponent lifecycle) =>
        {
          applied = PlayerNetworkStateSystem.ApplyLifeMana(vital, value, maximum);
          if (applied)
          {
            PlayerLifecycleSystem.ApplyNetworkLife(ref lifecycle, value);
          }
        });
    if (!edited)
    {
      return new NetworkPlayerMutationResult(NetworkPlayerMutationStatus.NotFound, null);
    }

    return applied
      ? CaptureMutationResult(session, bound)
      : new NetworkPlayerMutationResult(NetworkPlayerMutationStatus.RejectedInvalidState, null);
  }

  private NetworkPlayerMutationResult ApplyBuffsOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    IReadOnlyList<ushort> buffTypes)
  {
    NetworkPlayerMutationStatus status = TryGetBoundForMutation(
      session, context, allowAwaitPlayerData: true, out BoundPlayer? bound);
    if (status != NetworkPlayerMutationStatus.Applied)
    {
      return new NetworkPlayerMutationResult(status, null);
    }

    bool applied = false;
    bool edited = session.EntityRuntime.TryEdit<PlayerBuffSlotsComponent>(
      bound!.Handle,
      (ref PlayerBuffSlotsComponent buffs) =>
        applied = PlayerNetworkStateSystem.ApplyBuffs(buffs, buffTypes));
    if (!edited)
    {
      return new NetworkPlayerMutationResult(NetworkPlayerMutationStatus.NotFound, null);
    }

    return applied
      ? CaptureMutationResult(session, bound)
      : new NetworkPlayerMutationResult(NetworkPlayerMutationStatus.RejectedInvalidState, null);
  }

  private NetworkPlayerMutationStatus TryGetBoundForMutation(
    LoadedWorldSession session,
    NetworkSessionContext context,
    bool allowAwaitPlayerData,
    out BoundPlayer? bound)
  {
    NetworkPlayerBindingStatus validation = ValidateContext(
      session, context, allowAwaitPlayerData);
    if (validation != NetworkPlayerBindingStatus.Applied)
    {
      bound = null;
      return validation switch
      {
        NetworkPlayerBindingStatus.RejectedStage => NetworkPlayerMutationStatus.RejectedStage,
        NetworkPlayerBindingStatus.RejectedSenderBinding =>
          NetworkPlayerMutationStatus.RejectedSenderBinding,
        NetworkPlayerBindingStatus.RejectedWorldRuntime =>
          NetworkPlayerMutationStatus.RejectedWorldRuntime,
        _ => NetworkPlayerMutationStatus.NotFound
      };
    }

    if (!_playersBySlot.TryGetValue(context.Actor.PlayerSlot, out bound))
    {
      return NetworkPlayerMutationStatus.NotFound;
    }
    if (bound.Connection != context.Connection || bound.Binding != context.Actor)
    {
      bound = null;
      return NetworkPlayerMutationStatus.RejectedSenderBinding;
    }
    if (!session.EntityRuntime.TryResolve(bound.Reference, out RuntimeEntityHandle handle) ||
        handle != bound.Handle)
    {
      bound = null;
      return NetworkPlayerMutationStatus.NotFound;
    }
    return NetworkPlayerMutationStatus.Applied;
  }

  private static NetworkPlayerMutationResult CaptureMutationResult(
    LoadedWorldSession session,
    BoundPlayer bound)
  {
    NetworkPlayerSnapshot? snapshot = CaptureBoundPlayer(session, bound);
    return snapshot is NetworkPlayerSnapshot captured
      ? new NetworkPlayerMutationResult(NetworkPlayerMutationStatus.Applied, captured)
      : new NetworkPlayerMutationResult(NetworkPlayerMutationStatus.NotFound, null);
  }

  private static bool IsFinite(Vector2 value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }

  /// <summary>
  /// Captures a player while an existing NetworkWorldOwner callback is already running.
  /// Callers must invoke this only from that owner's callback; it never enqueues nested work.
  /// </summary>
  public bool TryCaptureOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    out NetworkPlayerSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(context);
    NetworkPlayerBindingStatus validation =
      ValidateContext(session, context, allowAwaitPlayerData: false);
    if (validation != NetworkPlayerBindingStatus.Applied ||
        !_playersBySlot.TryGetValue(context.Actor.PlayerSlot, out BoundPlayer? bound) ||
        bound.Connection != context.Connection || bound.Binding != context.Actor ||
        CaptureBoundPlayer(session, bound) is not NetworkPlayerSnapshot captured)
    {
      snapshot = default;
      return false;
    }

    snapshot = captured;
    return true;
  }

  /// <summary>
  /// Resolves the authenticated player while an existing owner callback is running.
  /// </summary>
  public bool TryResolveOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    out EntityReference reference)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(context);
    NetworkPlayerBindingStatus validation =
      ValidateContext(session, context, allowAwaitPlayerData: false);
    if (validation != NetworkPlayerBindingStatus.Applied ||
        !_playersBySlot.TryGetValue(context.Actor.PlayerSlot, out BoundPlayer? bound) ||
        bound.Connection != context.Connection || bound.Binding != context.Actor ||
        !session.EntityRuntime.TryResolve(bound.Reference, out RuntimeEntityHandle handle) ||
        handle != bound.Handle)
    {
      reference = EntityReference.None;
      return false;
    }

    reference = bound.Reference;
    return true;
  }

  /// <summary>
  /// Resolves an existing player entity to its current wire slot during an owner callback.
  /// </summary>
  public bool TryGetPlayerSlotOnOwnerThread(
    LoadedWorldSession session,
    RuntimeEntityId playerEntity,
    out byte playerSlot)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (playerEntity.IsEmpty ||
      playerEntity.Reference.Scope != EntityReferenceScope.Player ||
      playerEntity.Reference.RuntimeId != session.EntityRuntime.RuntimeId)
    {
      playerSlot = byte.MaxValue;
      return false;
    }

    foreach ((byte slot, BoundPlayer bound) in _playersBySlot)
    {
      if (bound.Binding.PlayerSlot != slot ||
        bound.Reference != playerEntity.Reference ||
        !session.EntityRuntime.TryResolve(bound.Reference, out RuntimeEntityHandle handle) ||
        handle != bound.Handle)
      {
        continue;
      }

      playerSlot = slot;
      return true;
    }

    playerSlot = byte.MaxValue;
    return false;
  }

  private NetworkPlayerBindingStatus ValidateContext(
    LoadedWorldSession session,
    NetworkSessionContext context,
    bool allowAwaitPlayerData)
  {
    NetworkSessionStage allowedStages = NetworkSessionStage.Active | NetworkSessionStage.Synchronizing;
    if (allowAwaitPlayerData)
    {
      allowedStages |= NetworkSessionStage.AwaitPlayerData;
    }

    if ((context.Stage & allowedStages) == 0)
    {
      return NetworkPlayerBindingStatus.RejectedStage;
    }

    if (context.Actor.PlayerSlot == byte.MaxValue || context.Actor.GameSessionKey == Guid.Empty)
    {
      return NetworkPlayerBindingStatus.RejectedPlayerSlot;
    }

    if (!_isCurrentSender(context))
    {
      return NetworkPlayerBindingStatus.RejectedSenderBinding;
    }

    if (!IsExpectedRuntime(session, context.WorldRuntimeId))
    {
      return NetworkPlayerBindingStatus.RejectedWorldRuntime;
    }

    return NetworkPlayerBindingStatus.Applied;
  }

  private static bool IsExpectedRuntime(
    LoadedWorldSession session,
    EntityRuntimeId? expectedRuntimeId)
  {
    return expectedRuntimeId is { } runtimeId &&
      runtimeId == session.EntityRuntime.RuntimeId;
  }

  private static void AttachPlayerComponents(
    LoadedWorldSession session,
    RuntimeEntityHandle handle,
    byte slot)
  {
    Vector2 spawnPosition = new(
      session.World.Descriptor.SpawnTileX * 16.0f - PlayerWidth * 0.5f,
      session.World.Descriptor.SpawnTileY * 16.0f - PlayerHeight);
    var identity = new PlayerIdentityComponent
    {
      CharacterName = string.Empty,
      LegacyPlayerSlot = new LegacyPlayerSlot(slot)
    };
    var lifecycle = new PlayerLifecycleComponent(PlayerLifecyclePhase.Alive, 0);
    _ = PlayerLifecycleSystem.SetConnectionState(
      identity,
      PlayerConnectionState.Active,
      PlayerLifecycleSystem.ConnectionStateChangeSource.Spawn);

    Attach(session.EntityRuntime, handle, identity);
    Attach(session.EntityRuntime, handle, lifecycle);
    Attach(session.EntityRuntime, handle, new PlayerDeathRecordComponent());
    Attach(session.EntityRuntime, handle, new PlayerRestComponent());
    Attach(session.EntityRuntime, handle, new PlayerSittingComponent());
    Attach(session.EntityRuntime, handle, new PlayerSleepingComponent());
    Attach(session.EntityRuntime, handle, new PlayerGhostStateComponent());
    Attach(session.EntityRuntime, handle, new PlayerVitalStateComponent());
    Attach(session.EntityRuntime, handle, new PlayerNetworkStateComponent());
    Attach(session.EntityRuntime, handle, new PlayerNetworkCameraStateComponent());
    Attach(session.EntityRuntime, handle, new PlayerSpawnPointComponent(spawnPosition));
    Attach(session.EntityRuntime, handle,
      new LocationComponent(spawnPosition.X, spawnPosition.Y));
    Attach(session.EntityRuntime, handle, new VelocityComponent(0.0f, 0.0f));
    Attach(session.EntityRuntime, handle,
      new ColliderComponent(PlayerWidth, PlayerHeight));
    Attach(session.EntityRuntime, handle, new PlayerInventoryComponent());
    Attach(session.EntityRuntime, handle, new PlayerInventorySlotsComponent());
    Attach(session.EntityRuntime, handle, new PlayerEquipmentRelationComponent());
    Attach(session.EntityRuntime, handle, new PlayerEquipmentComponent());
    Attach(session.EntityRuntime, handle, new PlayerLoadoutStateComponent());
    Attach(session.EntityRuntime, handle, new PlayerAppearanceCustomizationComponent());
    Attach(session.EntityRuntime, handle, new PlayerAppearanceSelectionComponent());
    Attach(session.EntityRuntime, handle, new PlayerBuffSlotsComponent());
    Attach(session.EntityRuntime, handle, new PlayerBuffComponent());
    Attach(session.EntityRuntime, handle, new PlayerZoneAndEnvironmentStateComponent());
    Attach(session.EntityRuntime, handle, new PlayerLuckAndRescanStateComponent());
    Attach(session.EntityRuntime, handle, new PlayerQuestEventProgressComponent());
    Attach(session.EntityRuntime, handle, new PlayerHeldItemPresentationStateComponent());
    Attach(session.EntityRuntime, handle, new PlayerMountComponent());
  }

  private static NetworkPlayerSnapshot? CaptureBoundPlayer(
    LoadedWorldSession session,
    BoundPlayer bound)
  {
    if (!session.EntityRuntime.TryResolve(bound.Reference, out RuntimeEntityHandle handle) ||
        handle != bound.Handle)
    {
      return null;
    }

    if (!session.EntityRuntime.TryCapture(
          handle,
          static (PlayerIdentityComponent component) =>
            new PlayerIdentitySnapshot(
              component.CharacterName,
              component.TeamId,
              component.ConnectionState,
              component.IsActive,
              component.LegacyPlayerSlot),
          out PlayerIdentitySnapshot identity) ||
        !session.EntityRuntime.TryCapture(
          handle,
          static (PlayerLifecycleComponent component) =>
            new PlayerLifecycleSnapshot(
              component.Phase,
              component.IsDead,
              component.DeadElapsedTicks,
              component.RespawnRemainingTicks),
          out PlayerLifecycleSnapshot lifecycle) ||
        !session.EntityRuntime.TryCapture(
          handle,
          static (LocationComponent component) => new Vector2(component.X, component.Y),
          out Vector2 position) ||
        !session.EntityRuntime.TryCapture(
          handle,
          static (VelocityComponent component) => new Vector2(component.X, component.Y),
          out Vector2 velocity) ||
        !session.EntityRuntime.TryCapture(
          handle,
          static (ColliderComponent component) =>
            new PlayerColliderSnapshot((int)component.Width, (int)component.Height),
          out PlayerColliderSnapshot collider) ||
        !session.EntityRuntime.TryCapture(
          handle,
          static (PlayerNetworkStateComponent component) =>
            new PlayerNetworkSnapshotState(
              component.ShouldNotDraw,
              component.Stealth,
              component.Hostile,
              component.TalkNpc,
              component.MinionRestTargetPoint,
              component.MinionAttackTargetNpc,
              component.HasPendingTeleport,
              component.PendingTeleportPosition),
          out PlayerNetworkSnapshotState network) ||
        !session.EntityRuntime.TryCapture(
          handle,
          static (PlayerVitalStateComponent component) =>
            new PlayerVitalSnapshot(
              component.StatLife,
              component.StatLifeMax2,
              component.StatMana,
              component.StatManaMax2),
      out PlayerVitalSnapshot vitals) ||
        !session.EntityRuntime.TryCapture(
          handle,
          static (PlayerInventoryComponent component) => component.SelectedSlotIndex,
          out int selectedSlot) ||
        !session.EntityRuntime.TryCapture(
          handle,
          static (PlayerNetworkStateComponent component) => component.ItemAnimationChannel,
          out byte itemAnimationChannel))
    {
      return null;
    }

    return new NetworkPlayerSnapshot(
      bound.Reference,
      session.EntityRuntime.RuntimeId,
      bound.Connection,
      bound.Binding,
      identity.LegacyPlayerSlot is { Value: int legacySlot }
        ? checked((byte)legacySlot)
        : bound.Binding.PlayerSlot,
      identity.CharacterName,
      identity.TeamId,
      identity.ConnectionState,
      identity.IsActive,
      lifecycle.Phase,
      lifecycle.IsDead,
      position,
      velocity,
      collider.Width,
      collider.Height,
      network.ShouldNotDraw,
      network.Stealth,
      network.Hostile,
      network.TalkNpc,
      network.MinionRestTargetPoint,
      network.MinionAttackTargetNpc,
      network.HasPendingTeleport,
      network.PendingTeleportPosition,
      vitals.StatLife,
      vitals.StatLifeMax,
      vitals.StatMana,
      vitals.StatManaMax,
      selectedSlot,
      itemAnimationChannel);
  }

  private void RemoveBoundPlayer(
    LoadedWorldSession session,
    byte slot,
    BoundPlayer bound)
  {
    if (session.EntityRuntime.TryGetStatus(bound.Handle, out EntityRuntimeStatus status))
    {
      if (status == EntityRuntimeStatus.Running &&
          !session.EntityRuntime.TryBeginTermination(bound.Handle))
      {
        throw new InvalidOperationException("The network Player entity could not begin termination.");
      }

      if (!session.EntityRuntime.TryRemoveEntity(bound.Handle))
      {
        throw new InvalidOperationException("The network Player entity could not be removed.");
      }
    }

    _playersBySlot.Remove(slot);
    if (_slotsByConnection.TryGetValue(bound.Connection, out byte mappedSlot) &&
        mappedSlot == slot)
    {
      _slotsByConnection.Remove(bound.Connection);
    }
  }

  private static void RemoveUnpublishedEntity(EntityRuntime runtime, RuntimeEntityHandle handle)
  {
    if (runtime.TryGetStatus(handle, out EntityRuntimeStatus status))
    {
      if (status == EntityRuntimeStatus.Running)
      {
        runtime.TryBeginTermination(handle);
      }

      runtime.TryRemoveEntity(handle);
    }
  }

  private static void Attach<TComponent>(
    EntityRuntime runtime,
    RuntimeEntityHandle handle,
    TComponent component)
    where TComponent : notnull
  {
    if (!runtime.TryAttach(handle, component))
    {
      throw new InvalidOperationException(
        $"The network Player entity could not attach {typeof(TComponent).Name}.");
    }
  }

  private sealed record BoundPlayer(
    ConnectionIdentity Connection,
    SenderBinding Binding,
    RuntimeEntityHandle Handle,
    EntityReference Reference);
}

public readonly record struct NetworkPlayerSnapshot(
  EntityReference Reference,
  EntityRuntimeId WorldRuntimeId,
  ConnectionIdentity Connection,
  SenderBinding Binding,
  byte PlayerSlot,
  string CharacterName,
  int TeamId,
  PlayerConnectionState ConnectionState,
  bool Active,
  PlayerLifecyclePhase LifecyclePhase,
  bool Dead,
  Vector2 Position,
  Vector2 Velocity,
  int Width,
  int Height,
  bool ShouldNotDraw,
  float Stealth,
  bool Hostile,
  int TalkNpc,
  Vector2 MinionRestTargetPoint,
  int MinionAttackTargetNpc,
  bool HasPendingTeleport,
  Vector2? PendingTeleportPosition,
  int StatLife,
  int StatLifeMax,
  int StatMana,
  int StatManaMax,
  int SelectedInventorySlot,
  byte ItemAnimationChannel);

internal readonly record struct PlayerIdentitySnapshot(
  string CharacterName,
  int TeamId,
  PlayerConnectionState ConnectionState,
  bool IsActive,
  LegacyPlayerSlot? LegacyPlayerSlot);

internal readonly record struct PlayerLifecycleSnapshot(
  PlayerLifecyclePhase Phase,
  bool IsDead,
  int DeadElapsedTicks,
  int RespawnRemainingTicks);

internal readonly record struct PlayerColliderSnapshot(int Width, int Height);

internal readonly record struct PlayerNetworkSnapshotState(
  bool ShouldNotDraw,
  float Stealth,
  bool Hostile,
  int TalkNpc,
  Vector2 MinionRestTargetPoint,
  int MinionAttackTargetNpc,
  bool HasPendingTeleport,
  Vector2? PendingTeleportPosition);

internal readonly record struct PendingTeleportProjection(bool HasPending, Vector2 Position);

internal readonly record struct PlayerVitalSnapshot(
  int StatLife,
  int StatLifeMax,
  int StatMana,
  int StatManaMax);

