using System;

using Terraria.Content;
using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.Projectile;

/// <summary>
/// Serial owner for projectile slot allocation, identity registration, and release.
/// Callers must serialize access to the supplied world storage.
/// </summary>
public sealed class ProjectileLifecycleSystem
{
  private const int MaximumOldestProjectileTimeLeft = 9_999_999;

  private readonly EntitySlotStore<WorldEntityState, ProjectileSlot> _projectiles;
  private readonly ProjectileIdentityIndex _identities;

  public ProjectileLifecycleSystem(WorldStorageRoot worldStorage)
  {
    ArgumentNullException.ThrowIfNull(worldStorage);

    _projectiles = worldStorage.Projectiles;
    _identities = worldStorage.ProjectileIdentities;
  }

  public ProjectileLifecycleSystem(
    EntitySlotStore<WorldEntityState, ProjectileSlot> projectiles,
    ProjectileIdentityIndex identities)
  {
    ArgumentNullException.ThrowIfNull(projectiles);
    ArgumentNullException.ThrowIfNull(identities);

    _projectiles = projectiles;
    _identities = identities;
  }

  public bool TryCreate(
    ProjectileIdentityComponent identity,
    ProjectileLifetimeStateComponent lifetime,
    out ProjectileHandle handle)
  {
    return TryCreate(
      identity,
      lifetime,
      new ProjectileNetworkStateComponent(),
      out handle);
  }

  public bool TryCreate(
    ProjectileIdentityComponent identity,
    ProjectileLifetimeStateComponent lifetime,
    ProjectileNetworkStateComponent network,
    out ProjectileHandle handle)
  {
    var state = new ProjectileEntityState(identity, lifetime, network);
    return TryCommitCreate(state, out handle);
  }

  public bool TrySpawn(
    ProjectileSpawnCommand spawn,
    IProjectileDefinitionQuery definitions,
    ProjectileDefinitionHydrationContext hydrationContext,
    out ProjectileHandle handle)
  {
    ArgumentNullException.ThrowIfNull(definitions);

    if (!definitions.TryGet(spawn.ProjectileType, out ProjectileDefinition definition) ||
      definition is null ||
      definition.Identity is null ||
      definition.Identity.NeedsUuid is not bool needsUuid ||
      !ProjectileDefinitionHydrationSystem.TryHydrate(
        definition,
        spawn,
        hydrationContext,
        out ProjectileEntityState? state) ||
      state is null)
    {
      handle = default;
      return false;
    }

    return TryCommitCreate(state, true, needsUuid, out handle);
  }

  /// <summary>
  /// Applies a decoded packet-27 state through the lifecycle owner.
  /// Existing active instances of the same type keep their hydrated defaults;
  /// packet identity remains owner-scoped and is never recomputed from a local slot.
  /// </summary>
  public bool TryApplyNetwork(
    ProjectileNetworkApplyCommand command,
    IProjectileDefinitionQuery definitions,
    ProjectileDefinitionHydrationContext hydrationContext,
    out ProjectileHandle handle)
  {
    ArgumentNullException.ThrowIfNull(definitions);

    if (!definitions.TryGet(command.ProjectileType, out ProjectileDefinition definition) ||
      definition is null ||
      definition.Identity is null ||
      definition.Identity.NeedsUuid is not bool needsUuid ||
      (!needsUuid && command.ProjectileUuid >= 0))
    {
      handle = default;
      return false;
    }

    OwnerProjectileIdentity ownerIdentity = new(
      new PlayerSlot(command.OwnerSlot),
      command.Identity);
    if (_identities.TryGetHandle(ownerIdentity, out ProjectileHandle existingHandle))
    {
      if (!TryGet(existingHandle, out ProjectileEntityState? existingState) ||
        existingState is null ||
        !existingState.Identity.MatchesOwnerAndIdentity(
          command.OwnerSlot,
          command.Identity))
      {
        throw new InvalidOperationException(
          "Projectile identity index and network state disagree.");
      }

      if (existingState.Lifetime.Active &&
        existingState.Definition.ProjectileType == command.ProjectileType)
      {
        ApplyNetworkFields(existingState, command);
        handle = existingHandle;
        return true;
      }

      if (!ProjectileDefinitionHydrationSystem.TryHydrate(
        definition,
        ToSpawnCommand(command),
        hydrationContext,
        out ProjectileEntityState? replacementState) ||
        replacementState is null)
      {
        handle = default;
        return false;
      }

      replacementState.Identity = CreateNetworkIdentity(command);
      return TryReplaceAtHandle(
        existingHandle,
        replacementState,
        ownerIdentity,
        out handle);
    }

    if (!ProjectileDefinitionHydrationSystem.TryHydrate(
      definition,
      ToSpawnCommand(command),
      hydrationContext,
      out ProjectileEntityState? state) ||
      state is null)
    {
      handle = default;
      return false;
    }

    state.Identity = CreateNetworkIdentity(command);
    return TryCommitCreate(state, deriveIdentityFromSlot: false, needsUuid, out handle);
  }

  private bool TryCommitCreate(
    ProjectileEntityState state,
    out ProjectileHandle handle)
  {
    return TryCommitCreate(
      state,
      deriveIdentityFromSlot: false,
      needsUuid: false,
      out handle);
  }

  private bool TryCommitCreate(
    ProjectileEntityState state,
    bool deriveIdentityFromSlot,
    bool needsUuid,
    out ProjectileHandle handle)
  {
    ProjectileIdentityComponent identity = state.Identity;
    ValidateIdentity(identity);
    if (!state.Lifetime.Active || state.Lifetime.EndReason != ProjectileEndReason.None)
    {
      handle = default;
      return false;
    }

    if (_projectiles.TryAllocate(state, out ProjectileSlot slot, out uint generation))
    {
      state.Identity = deriveIdentityFromSlot
        ? WithSpawnSlotIdentity(identity, slot, needsUuid)
        : WithSlot(identity, slot);
      OwnerProjectileIdentity ownerIdentity = GetOwnerIdentity(state.Identity);
      handle = new ProjectileHandle(slot, generation);

      if (_identities.TryRegister(ownerIdentity, handle))
      {
        return true;
      }

      if (!_projectiles.TryRelease(slot, generation, out _))
      {
        throw new InvalidOperationException(
          "A failed identity registration left its projectile slot occupied.");
      }

      handle = default;
      return false;
    }

    return TryReplaceOldest(state, deriveIdentityFromSlot, needsUuid, out handle);
  }

  private bool TryReplaceOldest(
    ProjectileEntityState replacementState,
    bool deriveIdentityFromSlot,
    bool needsUuid,
    out ProjectileHandle replacementHandle)
  {
    if (!TryFindOldest(
      out ProjectileHandle previousHandle,
      out ProjectileEntityState? previousState) ||
      previousState is null)
    {
      replacementHandle = default;
      return false;
    }

    if (previousHandle.Generation == uint.MaxValue)
    {
      replacementHandle = default;
      return false;
    }

    OwnerProjectileIdentity previousIdentity = GetOwnerIdentity(previousState.Identity);
    if (!_identities.TryGetIdentity(previousHandle, out OwnerProjectileIdentity indexedIdentity) ||
      indexedIdentity != previousIdentity)
    {
      throw new InvalidOperationException(
        "Projectile slot state and owner identity index disagree.");
    }

    replacementState.Identity = deriveIdentityFromSlot
      ? WithSpawnSlotIdentity(replacementState.Identity, previousHandle.Slot, needsUuid)
      : WithSlot(replacementState.Identity, previousHandle.Slot);
    OwnerProjectileIdentity replacementIdentity = GetOwnerIdentity(replacementState.Identity);

    if (_identities.TryGetHandle(replacementIdentity, out ProjectileHandle currentHandle) &&
      currentHandle != previousHandle)
    {
      replacementHandle = default;
      return false;
    }

    return TryReplaceAtHandle(
      previousHandle,
      replacementState,
      replacementIdentity,
      out replacementHandle);
  }

  private bool TryReplaceAtHandle(
    ProjectileHandle previousHandle,
    ProjectileEntityState replacementState,
    OwnerProjectileIdentity replacementIdentity,
    out ProjectileHandle replacementHandle)
  {
    if (previousHandle.Generation == uint.MaxValue ||
      !_projectiles.TryGet(
        previousHandle.Slot,
        previousHandle.Generation,
        out WorldEntityState? previousStoredState) ||
      previousStoredState is not ProjectileEntityState previousState)
    {
      replacementHandle = default;
      return false;
    }

    OwnerProjectileIdentity previousIdentity = GetOwnerIdentity(previousState.Identity);
    if (!_identities.TryGetIdentity(previousHandle, out OwnerProjectileIdentity indexedIdentity) ||
      indexedIdentity != previousIdentity)
    {
      throw new InvalidOperationException(
        "Projectile slot state and owner identity index disagree.");
    }

    replacementState.Identity = WithSlot(replacementState.Identity, previousHandle.Slot);
    replacementHandle = new ProjectileHandle(
      previousHandle.Slot,
      previousHandle.Generation + 1);

    if (!_identities.TryReplace(
      previousIdentity,
      previousHandle,
      replacementIdentity,
      replacementHandle))
    {
      replacementHandle = default;
      return false;
    }

    if (_projectiles.TryReplace(
      previousHandle.Slot,
      previousHandle.Generation,
      replacementState,
      out uint committedGeneration))
    {
      if (committedGeneration != replacementHandle.Generation)
      {
        throw new InvalidOperationException(
          "Projectile slot and identity generations diverged during replacement.");
      }

      return true;
    }

    if (!_identities.TryReplace(
      replacementIdentity,
      replacementHandle,
      previousIdentity,
      previousHandle))
    {
      throw new InvalidOperationException(
        "A failed projectile slot replacement could not restore its identity mapping.");
    }

    replacementHandle = default;
    return false;
  }

  private static ProjectileSpawnCommand ToSpawnCommand(
    ProjectileNetworkApplyCommand command)
  {
    return new ProjectileSpawnCommand(
      command.ProjectileType,
      new ProjectileOwnerReference(
        EntityReference.None,
        command.OwnerSlot),
      command.Position,
      command.Velocity,
      command.Damage,
      command.OriginalDamage,
      command.Knockback,
      command.Ai0,
      command.Ai1,
      command.Ai2,
      command.BannerIdToRespondTo);
  }

  private static ProjectileIdentityComponent CreateNetworkIdentity(
    ProjectileNetworkApplyCommand command)
  {
    return new ProjectileIdentityComponent(
      EntityReference.None,
      identity: command.Identity,
      projectileUuid: command.ProjectileUuid,
      ownerSlot: command.OwnerSlot);
  }

  private static void ApplyNetworkFields(
    ProjectileEntityState state,
    ProjectileNetworkApplyCommand command)
  {
    ProjectileKinematicsStateComponent kinematics = state.Kinematics;
    kinematics.Position = command.Position;
    kinematics.Velocity = command.Velocity;
    state.Kinematics = kinematics;

    ProjectileBehaviorStateComponent behavior = state.Behavior;
    behavior.Ai0 = command.Ai0;
    behavior.Ai1 = command.Ai1;
    behavior.Ai2 = command.Ai2;
    state.Behavior = behavior;

    ProjectileDamagePayloadComponent damage = state.Damage;
    damage.CurrentDamage = command.Damage;
    damage.OriginalDamage = command.OriginalDamage;
    damage.Knockback = command.Knockback;
    state.Damage = damage;

    if (command.ProjectileUuid >= 0)
    {
      state.Identity = new ProjectileIdentityComponent(
        state.Identity.OwnerReference,
        state.Identity.SlotIndex,
        state.Identity.Identity,
        command.ProjectileUuid,
        state.Identity.OwnerSlot);
    }

    ProjectileSourceMetadataComponent source = state.Source;
    source.BannerIdToRespondTo = command.BannerIdToRespondTo;
    state.Source = source;
  }

  public bool TryGet(ProjectileHandle handle, out ProjectileEntityState? state)
  {
    if (!_projectiles.TryGet(handle.Slot, handle.Generation, out WorldEntityState? storedState) ||
      storedState is not ProjectileEntityState projectileState)
    {
      state = null;
      return false;
    }

    state = projectileState;
    return true;
  }

  /// <summary>
  /// Reads the current generation at a local slot without allocating or
  /// changing lifecycle state. The update coordinator uses this to preserve
  /// the Version4 ascending 0..999 pass.
  /// </summary>
  public bool TryGetAtSlot(
    int slotIndex,
    out ProjectileHandle handle,
    out ProjectileEntityState? state)
  {
    if (slotIndex < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(slotIndex));
    }

    if (!_projectiles.TryGetOccupiedAt(
      slotIndex,
      out ProjectileSlot slot,
      out uint generation,
      out WorldEntityState? storedState))
    {
      handle = default;
      state = null;
      return false;
    }

    if (storedState is not ProjectileEntityState projectileState)
    {
      throw new InvalidOperationException(
        "The projectile slot store contains a non-projectile state.");
    }

    handle = new ProjectileHandle(slot, generation);
    state = projectileState;
    return true;
  }

  public bool TryTerminate(ProjectileHandle handle, ProjectileEndReason reason)
  {
    if (reason == ProjectileEndReason.None)
    {
      throw new ArgumentOutOfRangeException(nameof(reason));
    }

    if (!TryGet(handle, out ProjectileEntityState? state) || state is null)
    {
      return false;
    }

    OwnerProjectileIdentity ownerIdentity = GetOwnerIdentity(state.Identity);
    if (!_identities.TryGetIdentity(handle, out OwnerProjectileIdentity indexedIdentity) ||
      indexedIdentity != ownerIdentity)
    {
      throw new InvalidOperationException(
        "Projectile slot state and owner identity index disagree.");
    }

    ProjectileLifetimeStateComponent previousLifetime = state.Lifetime;
    ProjectileLifetimeStateComponent terminatedLifetime = previousLifetime;
    if (!ProjectileLifetimeSystem.CommitTermination(ref terminatedLifetime, reason))
    {
      return false;
    }

    if (!_identities.TryUnregister(handle, out OwnerProjectileIdentity removedIdentity) ||
      removedIdentity != ownerIdentity)
    {
      throw new InvalidOperationException(
        "A validated projectile identity could not be unregistered.");
    }

    state.Lifetime = terminatedLifetime;
    if (_projectiles.TryRelease(handle.Slot, handle.Generation, out _))
    {
      return true;
    }

    state.Lifetime = previousLifetime;
    if (!_identities.TryRegister(ownerIdentity, handle))
    {
      throw new InvalidOperationException(
        "A failed projectile release could not restore its identity mapping.");
    }

    throw new InvalidOperationException(
      "A validated projectile slot could not be released.");
  }

  /// <summary>
  /// Resolves and commits a packet-29 termination request through the owner
  /// and identity index. Unknown or stale requests are no-ops.
  /// </summary>
  public bool TryTerminateNetwork(ProjectileNetworkTerminateCommand command)
  {
    OwnerProjectileIdentity ownerIdentity = new(
      new PlayerSlot(command.OwnerSlot),
      command.Identity);
    if (!_identities.TryGetHandle(ownerIdentity, out ProjectileHandle handle))
    {
      return false;
    }

    if (!TryGet(handle, out ProjectileEntityState? state) ||
      state is null ||
      !state.Lifetime.Active ||
      !state.Identity.MatchesOwnerAndIdentity(
        command.OwnerSlot,
        command.Identity))
    {
      return false;
    }

    return TryTerminate(handle, ProjectileEndReason.NetworkTermination);
  }

  private static OwnerProjectileIdentity GetOwnerIdentity(
    ProjectileIdentityComponent identity)
  {
    return new OwnerProjectileIdentity(new PlayerSlot(identity.OwnerSlot), identity.Identity);
  }

  private bool TryFindOldest(
    out ProjectileHandle handle,
    out ProjectileEntityState? state)
  {
    int oldestTimeLeft = MaximumOldestProjectileTimeLeft;
    handle = default;
    state = null;

    for (int index = 0; index < _projectiles.Capacity; index++)
    {
      if (!_projectiles.TryGetOccupiedAt(
        index,
        out ProjectileSlot slot,
        out uint generation,
        out WorldEntityState? storedState))
      {
        continue;
      }

      if (storedState is not ProjectileEntityState candidate)
      {
        throw new InvalidOperationException(
          "The projectile slot store contains a non-projectile state.");
      }

      if (candidate.Network.NetworkImportant ||
        candidate.Lifetime.TimeLeft >= oldestTimeLeft)
      {
        continue;
      }

      oldestTimeLeft = candidate.Lifetime.TimeLeft;
      handle = new ProjectileHandle(slot, generation);
      state = candidate;
    }

    return state is not null;
  }

  private static ProjectileIdentityComponent WithSlot(
    ProjectileIdentityComponent identity,
    ProjectileSlot slot)
  {
    return new ProjectileIdentityComponent(
      identity.OwnerReference,
      slotIndex: slot.Value,
      identity: identity.Identity,
      projectileUuid: identity.ProjectileUuid,
      ownerSlot: identity.OwnerSlot);
  }

  private static ProjectileIdentityComponent WithSpawnSlotIdentity(
    ProjectileIdentityComponent identity,
    ProjectileSlot slot,
    bool needsUuid)
  {
    return new ProjectileIdentityComponent(
      identity.OwnerReference,
      slotIndex: slot.Value,
      identity: slot.Value,
      projectileUuid: needsUuid ? slot.Value : -1,
      ownerSlot: identity.OwnerSlot);
  }

  private static void ValidateIdentity(ProjectileIdentityComponent identity)
  {
    if (identity.SlotIndex != -1)
    {
      throw new ArgumentException(
        "Projectile slot indices are assigned by the lifecycle owner.",
        nameof(identity));
    }

    if ((uint)identity.OwnerSlot > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(identity));
    }

    if (identity.Identity < 0 || identity.ProjectileUuid < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(identity));
    }
  }
}
