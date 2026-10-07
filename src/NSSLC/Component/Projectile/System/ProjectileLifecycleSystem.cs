using System;

using EntityEcs;
using EntityEcs.Components;

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
  private readonly EntityRuntime _runtime;

  internal EntityRuntime Runtime => _runtime;

  public ProjectileLifecycleSystem(WorldStorageRoot worldStorage)
  {
    ArgumentNullException.ThrowIfNull(worldStorage);

    _projectiles = worldStorage.Projectiles;
    _identities = worldStorage.ProjectileIdentities;
    _runtime = worldStorage.ProjectileRuntime;
  }

  public ProjectileLifecycleSystem(
    EntitySlotStore<WorldEntityState, ProjectileSlot> projectiles,
    ProjectileIdentityIndex identities,
    EntityRuntime runtime)
  {
    ArgumentNullException.ThrowIfNull(projectiles);
    ArgumentNullException.ThrowIfNull(identities);
    ArgumentNullException.ThrowIfNull(runtime);

    _projectiles = projectiles;
    _identities = identities;
    _runtime = runtime;
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
    var state = new ProjectileInitialComponents(identity, lifetime, network);
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
        out ProjectileInitialComponents? state) ||
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
      if (!TryGetRuntimeHandle(existingHandle, out RuntimeEntityHandle existingRuntimeHandle))
      {
        throw new InvalidOperationException(
          "Projectile identity index and network state disagree.");
      }

      if (!TryGetIdentity(existingHandle, out ProjectileIdentityComponent existingIdentity))
      {
        if (_runtime.Has<ProjectileIdentityComponent>(existingRuntimeHandle))
        {
          handle = default;
          return false;
        }

        throw new InvalidOperationException(
          "Projectile identity index and network state disagree.");
      }

      if (!existingIdentity.MatchesOwnerAndIdentity(
        command.OwnerSlot,
        command.Identity))
      {
        throw new InvalidOperationException(
          "Projectile identity index and network state disagree.");
      }

      ProjectileLifetimeStateComponent existingLifetime =
        ReadComponent<ProjectileLifetimeStateComponent>(existingRuntimeHandle);
      ProjectileDefinitionComponent existingDefinition =
        ReadComponent<ProjectileDefinitionComponent>(existingRuntimeHandle);
      if (existingLifetime.Active &&
        existingDefinition.ProjectileType == command.ProjectileType)
      {
        ApplyNetworkFields(existingRuntimeHandle, command);
        handle = existingHandle;
        return true;
      }

      if (!ProjectileDefinitionHydrationSystem.TryHydrate(
        definition,
        ToSpawnCommand(command),
        hydrationContext,
        out ProjectileInitialComponents? replacementState) ||
        replacementState is null)
      {
        handle = default;
        return false;
      }

      replacementState.Identity = CreateNetworkIdentity(command);
      ProjectileRootBinding replacementRoot = CreateRuntimeEntity(replacementState);
      return TryReplaceAtHandle(
        existingHandle,
        replacementRoot,
        ownerIdentity,
        out handle);
    }

    if (!ProjectileDefinitionHydrationSystem.TryHydrate(
      definition,
      ToSpawnCommand(command),
      hydrationContext,
      out ProjectileInitialComponents? state) ||
      state is null)
    {
      handle = default;
      return false;
    }

    state.Identity = CreateNetworkIdentity(command);
    return TryCommitCreate(state, deriveIdentityFromSlot: false, needsUuid, out handle);
  }

  private bool TryCommitCreate(
    ProjectileInitialComponents state,
    out ProjectileHandle handle)
  {
    return TryCommitCreate(
      state,
      deriveIdentityFromSlot: false,
      needsUuid: false,
      out handle);
  }

  private bool TryCommitCreate(
    ProjectileInitialComponents state,
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

    ProjectileRootBinding entity = CreateRuntimeEntity(state);
    bool committed = false;
    try
    {
      if (_projectiles.TryAllocate(entity, out ProjectileSlot slot, out uint generation))
      {
        ProjectileIdentityComponent slotIdentity = deriveIdentityFromSlot
          ? WithSpawnSlotIdentity(identity, slot, needsUuid)
          : WithSlot(identity, slot);
        if (!_runtime.TryReplace(entity.RuntimeHandle, slotIdentity))
        {
          if (!_projectiles.TryRelease(slot, generation, out _))
          {
            throw new InvalidOperationException(
              "A projectile identity commit failure left its slot occupied.");
          }

          handle = default;
          return false;
        }

        OwnerProjectileIdentity ownerIdentity = GetOwnerIdentity(slotIdentity);
        handle = new ProjectileHandle(slot, generation);

        if (_identities.TryRegister(ownerIdentity, handle))
        {
          committed = true;
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

      if (TryReplaceOldest(entity, deriveIdentityFromSlot, needsUuid, out handle))
      {
        committed = true;
        return true;
      }

      handle = default;
      return false;
    }
    finally
    {
      if (!committed)
      {
        RemoveRuntimeEntity(entity.RuntimeHandle);
      }
    }
  }

  private bool TryReplaceOldest(
    ProjectileRootBinding replacementState,
    bool deriveIdentityFromSlot,
    bool needsUuid,
    out ProjectileHandle replacementHandle)
  {
    if (!TryFindOldest(
      out ProjectileHandle previousHandle,
      out RuntimeEntityHandle previousRuntimeHandle))
    {
      replacementHandle = default;
      return false;
    }

    if (previousHandle.Generation == uint.MaxValue)
    {
      replacementHandle = default;
      return false;
    }

    ProjectileIdentityComponent previousIdentityComponent =
      ReadComponent<ProjectileIdentityComponent>(previousRuntimeHandle);
    OwnerProjectileIdentity previousIdentity = GetOwnerIdentity(previousIdentityComponent);
    if (!_identities.TryGetIdentity(previousHandle, out OwnerProjectileIdentity indexedIdentity) ||
      indexedIdentity != previousIdentity)
    {
      throw new InvalidOperationException(
        "Projectile slot state and owner identity index disagree.");
    }

    ProjectileIdentityComponent replacementComponent =
      ReadComponent<ProjectileIdentityComponent>(replacementState.RuntimeHandle);
    ProjectileIdentityComponent replacementIdentityComponent = deriveIdentityFromSlot
      ? WithSpawnSlotIdentity(replacementComponent, previousHandle.Slot, needsUuid)
      : WithSlot(replacementComponent, previousHandle.Slot);
    if (!_runtime.TryReplace(replacementState.RuntimeHandle, replacementIdentityComponent))
    {
      replacementHandle = default;
      return false;
    }

    OwnerProjectileIdentity replacementIdentity = GetOwnerIdentity(
      replacementIdentityComponent);

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
    ProjectileRootBinding replacementState,
    OwnerProjectileIdentity replacementIdentity,
    out ProjectileHandle replacementHandle)
  {
    RuntimeEntityHandle replacementRuntimeHandle = replacementState.RuntimeHandle;
    bool committed = false;
    try
    {
      if (previousHandle.Generation == uint.MaxValue ||
        !_projectiles.TryGet(
          previousHandle.Slot,
          previousHandle.Generation,
          out WorldEntityState? previousStoredState) ||
        previousStoredState is not ProjectileRootBinding previousState)
      {
        replacementHandle = default;
        return false;
      }

      // Complete every borrowed-root and mapping read before changing the
      // identity index or the slot generation.
      OwnerProjectileIdentity previousIdentity = GetOwnerIdentity(
        ReadComponent<ProjectileIdentityComponent>(previousState.RuntimeHandle));
      if (!_identities.TryGetIdentity(previousHandle, out OwnerProjectileIdentity indexedIdentity) ||
        indexedIdentity != previousIdentity)
      {
        throw new InvalidOperationException(
          "Projectile slot state and owner identity index disagree.");
      }

      ProjectileIdentityComponent replacementIdentityComponent = WithSlot(
        ReadComponent<ProjectileIdentityComponent>(replacementRuntimeHandle),
        previousHandle.Slot);
      if (!_runtime.TryReplace(replacementRuntimeHandle, replacementIdentityComponent))
      {
        replacementHandle = default;
        return false;
      }
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
        committed = true;
        if (committedGeneration != replacementHandle.Generation)
        {
          throw new InvalidOperationException(
            "Projectile slot and identity generations diverged during replacement.");
        }

        RemoveRuntimeEntity(previousState.RuntimeHandle);
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
    finally
    {
      if (!committed)
      {
        RemoveRuntimeEntity(replacementRuntimeHandle);
      }
    }
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

  private void ApplyNetworkFields(
    RuntimeEntityHandle runtimeHandle,
    ProjectileNetworkApplyCommand command)
  {
    if (!_runtime.TryEditComponents<
      LocationComponent,
      VelocityComponent,
      ProjectileBehaviorStateComponent,
      ProjectileDamagePayloadComponent,
      ProjectileSourceMetadataComponent,
      ProjectileIdentityComponent>(
      runtimeHandle,
      (ref LocationComponent location,
        ref VelocityComponent velocity,
        ref ProjectileBehaviorStateComponent behavior,
        ref ProjectileDamagePayloadComponent damage,
        ref ProjectileSourceMetadataComponent source,
        ref ProjectileIdentityComponent identity) =>
      {
        location = new LocationComponent(command.Position.X, command.Position.Y);
        velocity = new VelocityComponent(command.Velocity.X, command.Velocity.Y);
        behavior.Ai0 = command.Ai0;
        behavior.Ai1 = command.Ai1;
        behavior.Ai2 = command.Ai2;
        damage.CurrentDamage = command.Damage;
        damage.OriginalDamage = command.OriginalDamage;
        damage.Knockback = command.Knockback;
        source.BannerIdToRespondTo = command.BannerIdToRespondTo;
        if (command.ProjectileUuid >= 0)
        {
          identity = new ProjectileIdentityComponent(
            identity.OwnerReference,
            identity.SlotIndex,
            identity.Identity,
            command.ProjectileUuid,
            identity.OwnerSlot);
        }
      }))
    {
      throw new InvalidOperationException(
        "Validated Projectile network fields could not be committed together.");
    }
  }

  public bool TryGetRuntimeHandle(
    ProjectileHandle handle,
    out RuntimeEntityHandle runtimeHandle)
  {
    if (!_projectiles.TryGet(
          handle.Slot,
          handle.Generation,
          out WorldEntityState? storedState) ||
      storedState is not ProjectileRootBinding binding ||
      !_runtime.TryGetStatus(binding.RuntimeHandle, out EntityRuntimeStatus status) ||
      status != EntityRuntimeStatus.Running)
    {
      runtimeHandle = default;
      return false;
    }

    runtimeHandle = binding.RuntimeHandle;
    return true;
  }

  public bool TryInspect<TComponent>(
    ProjectileHandle handle,
    EntityComponentInspector<TComponent> inspector)
    where TComponent : notnull
  {
    ArgumentNullException.ThrowIfNull(inspector);
    return TryGetRuntimeHandle(handle, out RuntimeEntityHandle runtimeHandle) &&
      _runtime.TryInspect(runtimeHandle, inspector);
  }

  public bool TryEdit<TComponent>(
    ProjectileHandle handle,
    EntityComponentEditor<TComponent> editor)
    where TComponent : notnull
  {
    ArgumentNullException.ThrowIfNull(editor);
    return TryGetRuntimeHandle(handle, out RuntimeEntityHandle runtimeHandle) &&
      _runtime.TryEdit(runtimeHandle, editor);
  }

  public bool TryGetIdentity(
    ProjectileHandle handle,
    out ProjectileIdentityComponent identity)
  {
    if (!TryGetRuntimeHandle(handle, out RuntimeEntityHandle runtimeHandle))
    {
      identity = default;
      return false;
    }

    bool captured = false;
    ProjectileIdentityComponent value = default;
    if (!_runtime.TryInspect<ProjectileIdentityComponent>(
      runtimeHandle,
      (in ProjectileIdentityComponent component) =>
      {
        value = component;
        captured = true;
      }))
    {
      identity = default;
      return false;
    }

    identity = captured ? value : default;
    return captured;
  }

  public bool TryGetOwnerIdentity(
    ProjectileHandle handle,
    out OwnerProjectileIdentity ownerIdentity)
  {
    if (TryGetIdentity(handle, out ProjectileIdentityComponent identity))
    {
      ownerIdentity = GetOwnerIdentity(identity);
      return true;
    }

    ownerIdentity = default;
    return false;
  }

  public bool TryGetEntityReference(
    ProjectileHandle handle,
    out EntityReference reference)
  {
    if (TryGetRuntimeHandle(handle, out RuntimeEntityHandle runtimeHandle) &&
      _runtime.TryGetReference(
        runtimeHandle,
        EntityReferenceScope.Projectile,
        out reference))
    {
      return true;
    }

    reference = EntityReference.None;
    return false;
  }

  /// <summary>
  /// Resolves the current root at a local slot without allocating or changing
  /// lifecycle state. The coordinator scans the ascending 0..999 range.
  /// </summary>
  public bool TryGetRuntimeHandleAtSlot(
    int slotIndex,
    out ProjectileHandle handle,
    out RuntimeEntityHandle runtimeHandle)
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
      runtimeHandle = default;
      return false;
    }

    if (storedState is not ProjectileRootBinding binding)
    {
      throw new InvalidOperationException(
        "The projectile slot store contains a non-projectile root binding.");
    }

    if (!_runtime.TryGetStatus(binding.RuntimeHandle, out EntityRuntimeStatus status) ||
      status != EntityRuntimeStatus.Running)
    {
      handle = default;
      runtimeHandle = default;
      return false;
    }

    handle = new ProjectileHandle(slot, generation);
    runtimeHandle = binding.RuntimeHandle;
    return true;
  }

  /// <summary>
  /// Terminates the current generation. A root whose typed access is currently
  /// borrowed is rejected without changing its slot, identity index, or lifetime.
  /// </summary>
  public bool TryTerminate(ProjectileHandle handle, ProjectileEndReason reason)
  {
    if (reason is ProjectileEndReason.None or ProjectileEndReason.WorldBoundary)
    {
      throw new ArgumentOutOfRangeException(nameof(reason));
    }

    return TryEnd(handle, reason, preserveTimeLeft: false);
  }

  /// <summary>
  /// Commits the non-Kill world-boundary deactivation while preserving its
  /// remaining time-left value until the inactive slot is released.
  /// </summary>
  public bool TryDeactivateAtWorldBoundary(ProjectileHandle handle)
  {
    return TryEnd(
      handle,
      ProjectileEndReason.WorldBoundary,
      preserveTimeLeft: true);
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

    if (!TryGetRuntimeHandle(handle, out RuntimeEntityHandle runtimeHandle) ||
      !TryGetIdentity(handle, out ProjectileIdentityComponent identity) ||
      !ReadComponent<ProjectileLifetimeStateComponent>(runtimeHandle).Active ||
      !identity.MatchesOwnerAndIdentity(
        command.OwnerSlot,
        command.Identity))
    {
      return false;
    }

    return TryTerminate(handle, ProjectileEndReason.NetworkTermination);
  }

  private bool TryEnd(
    ProjectileHandle handle,
    ProjectileEndReason reason,
    bool preserveTimeLeft)
  {
    if (reason == ProjectileEndReason.None)
    {
      throw new ArgumentOutOfRangeException(nameof(reason));
    }

    if (!TryGetRuntimeHandle(handle, out RuntimeEntityHandle runtimeHandle) ||
      !TryGetIdentity(handle, out ProjectileIdentityComponent identity))
    {
      return false;
    }

    OwnerProjectileIdentity ownerIdentity = GetOwnerIdentity(identity);
    if (!_identities.TryGetIdentity(handle, out OwnerProjectileIdentity indexedIdentity) ||
      indexedIdentity != ownerIdentity)
    {
      throw new InvalidOperationException(
        "Projectile slot state and owner identity index disagree.");
    }

    ProjectileLifetimeStateComponent previousLifetime =
      ReadComponent<ProjectileLifetimeStateComponent>(runtimeHandle);
    ProjectileLifetimeStateComponent endedLifetime = previousLifetime;
    bool ended = preserveTimeLeft
      ? ProjectileLifetimeSystem.CommitWorldBoundaryDeactivation(ref endedLifetime)
      : ProjectileLifetimeSystem.CommitTermination(ref endedLifetime, reason);
    if (!ended)
    {
      return false;
    }

    if (!_identities.TryUnregister(handle, out OwnerProjectileIdentity removedIdentity) ||
      removedIdentity != ownerIdentity)
    {
      throw new InvalidOperationException(
        "A validated projectile identity could not be unregistered.");
    }

    if (!_runtime.TryReplace(runtimeHandle, endedLifetime))
    {
      if (!_identities.TryRegister(ownerIdentity, handle))
      {
        throw new InvalidOperationException(
          "A failed projectile lifetime commit could not restore its identity mapping.");
      }

      throw new InvalidOperationException(
        "A validated projectile lifetime could not be committed.");
    }

    if (_projectiles.TryRelease(handle.Slot, handle.Generation, out _))
    {
      RemoveRuntimeEntity(runtimeHandle);
      return true;
    }

    if (!_runtime.TryReplace(runtimeHandle, previousLifetime))
    {
      throw new InvalidOperationException(
        "A failed projectile release could not restore its lifetime state.");
    }

    if (!_identities.TryRegister(ownerIdentity, handle))
    {
      throw new InvalidOperationException(
        "A failed projectile release could not restore its identity mapping.");
    }

    throw new InvalidOperationException(
      "A validated projectile slot could not be released.");
  }

  private static OwnerProjectileIdentity GetOwnerIdentity(
    ProjectileIdentityComponent identity)
  {
    return new OwnerProjectileIdentity(new PlayerSlot(identity.OwnerSlot), identity.Identity);
  }

  private bool TryFindOldest(
    out ProjectileHandle handle,
    out RuntimeEntityHandle runtimeHandle)
  {
    int oldestTimeLeft = MaximumOldestProjectileTimeLeft;
    handle = default;
    runtimeHandle = default;

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

      if (storedState is not ProjectileRootBinding candidate)
      {
        throw new InvalidOperationException(
          "The projectile slot store contains a non-projectile state.");
      }

      ProjectileNetworkStateComponent network =
        ReadComponent<ProjectileNetworkStateComponent>(candidate.RuntimeHandle);
      ProjectileLifetimeStateComponent lifetime =
        ReadComponent<ProjectileLifetimeStateComponent>(candidate.RuntimeHandle);
      if (network.NetworkImportant || lifetime.TimeLeft >= oldestTimeLeft)
      {
        continue;
      }

      oldestTimeLeft = lifetime.TimeLeft;
      handle = new ProjectileHandle(slot, generation);
      runtimeHandle = candidate.RuntimeHandle;
    }

    return runtimeHandle.IsAssigned;
  }

  private TComponent ReadComponent<TComponent>(RuntimeEntityHandle runtimeHandle)
    where TComponent : struct
  {
    TComponent value = default;
    if (!_runtime.TryInspect(
      runtimeHandle,
      (in TComponent component) => value = component))
    {
      throw new InvalidOperationException(
        $"The projectile root does not expose {typeof(TComponent).Name}.");
    }

    return value;
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

  private ProjectileRootBinding CreateRuntimeEntity(
    ProjectileInitialComponents components)
  {
    RuntimeEntityHandle runtimeHandle = _runtime.CreateEntity();
    try
    {
      Attach(runtimeHandle, components.Definition);
      Attach(runtimeHandle, components.Identity);
      Attach(runtimeHandle, components.Lifetime);
      Attach(runtimeHandle, Clone(components.Network));
      Attach(runtimeHandle, new ColliderComponent(
        components.Geometry.Width,
        components.Geometry.Height));
      Attach(runtimeHandle, new ProjectileScaleComponent(components.Geometry.Scale));
      Attach(runtimeHandle, components.Behavior);
      Attach(runtimeHandle, components.UpdateCadence);
      Attach(runtimeHandle, components.Trajectory);
      Attach(runtimeHandle, components.Direction);
      Attach(runtimeHandle, components.Disposition);
      Attach(runtimeHandle, components.Damage);
      Attach(runtimeHandle, components.DamagePolicy);
      Attach(runtimeHandle, components.Penetration);
      Attach(runtimeHandle, components.Collision);
      Attach(runtimeHandle, components.WetState);
      Attach(runtimeHandle, components.HitImmunityPolicy);
      Attach(runtimeHandle, Clone(components.HitImmunity));
      Attach(runtimeHandle, components.EffectCooldown);
      Attach(runtimeHandle, components.Reflection);
      Attach(runtimeHandle, components.Presentation);
      Attach(runtimeHandle, components.Animation);
      Attach(runtimeHandle, Clone(components.Trail));
      Attach(runtimeHandle, components.Source);
      if (components.Minion.IsMinion || components.Minion.MinionSlots != 0.0f)
      {
        Attach(runtimeHandle, components.Minion);
      }

      if (components.Sentry.IsSentry)
      {
        Attach(runtimeHandle, components.Sentry);
      }

      if (components.Bobber.IsBobber)
      {
        Attach(runtimeHandle, components.Bobber);
      }

      if (components.Counterweight.IsCounterweight)
      {
        Attach(runtimeHandle, components.Counterweight);
      }

      if (components.Trap.IsTrap)
      {
        Attach(runtimeHandle, components.Trap);
      }
      Attach(runtimeHandle, new LocationComponent(
        components.Kinematics.Position.X,
        components.Kinematics.Position.Y));
      Attach(runtimeHandle, new VelocityComponent(
        components.Kinematics.Velocity.X,
        components.Kinematics.Velocity.Y));
      Attach(runtimeHandle, components.MotionHistory);
      if (!_runtime.TryPublishEntity(runtimeHandle))
      {
        throw new InvalidOperationException("The projectile entity root could not be published.");
      }

      return new ProjectileRootBinding(runtimeHandle);
    }
    catch
    {
      RemoveRuntimeEntity(runtimeHandle);
      throw;
    }
  }

  private void Attach<TComponent>(
    RuntimeEntityHandle runtimeHandle,
    TComponent component)
    where TComponent : notnull
  {
    if (!_runtime.TryAttach(runtimeHandle, component))
    {
      throw new InvalidOperationException(
        $"The projectile {typeof(TComponent).Name} component could not be attached.");
    }
  }

  private void RemoveRuntimeEntity(RuntimeEntityHandle runtimeHandle)
  {
    if (!_runtime.TryGetStatus(runtimeHandle, out EntityRuntimeStatus status))
    {
      return;
    }

    if (status == EntityRuntimeStatus.Running &&
        !_runtime.TryBeginTermination(runtimeHandle))
    {
      throw new InvalidOperationException("A projectile root could not begin termination.");
    }

    if (!_runtime.TryRemoveEntity(runtimeHandle))
    {
      throw new InvalidOperationException("A projectile root could not be removed.");
    }
  }

  private static ProjectileNetworkStateComponent Clone(
    ProjectileNetworkStateComponent component)
  {
    return new ProjectileNetworkStateComponent
    {
      NetworkImportant = component.NetworkImportant,
      PrimaryUpdatePending = component.PrimaryUpdatePending,
      SecondaryUpdatePending = component.SecondaryUpdatePending,
      NetSpam = component.NetSpam,
      SectionSyncSkippedForPlayer = component.SectionSyncSkippedForPlayer is null
        ? null!
        : (bool[])component.SectionSyncSkippedForPlayer.Clone(),
      SendRequested = component.SendRequested,
    };
  }

  private static ProjectileHitImmunityStateComponent Clone(
    ProjectileHitImmunityStateComponent component)
  {
    return new ProjectileHitImmunityStateComponent
    {
      LocalNpcImmunityTicks = component.LocalNpcImmunityTicks is null
        ? null!
        : (int[])component.LocalNpcImmunityTicks.Clone(),
      PlayerImmunityTicks = component.PlayerImmunityTicks is null
        ? null!
        : (int[])component.PlayerImmunityTicks.Clone(),
      RestrikeDelayTicks = component.RestrikeDelayTicks,
    };
  }

  private static ProjectileTrailCacheComponent Clone(
    ProjectileTrailCacheComponent component)
  {
    return new ProjectileTrailCacheComponent
    {
      OldPositions = component.OldPositions is null
        ? null!
        : (System.Numerics.Vector2[])component.OldPositions.Clone(),
      OldRotations = component.OldRotations is null
        ? null!
        : (float[])component.OldRotations.Clone(),
      OldSpriteDirections = component.OldSpriteDirections is null
        ? null!
        : (int[])component.OldSpriteDirections.Clone(),
      WhipPoints = component.WhipPoints is null
        ? null!
        : new System.Collections.Generic.List<System.Numerics.Vector2>(component.WhipPoints),
    };
  }
}

internal sealed class ProjectileRootBinding : WorldEntityState
{
  public ProjectileRootBinding(RuntimeEntityHandle runtimeHandle)
  {
    if (!runtimeHandle.IsAssigned)
    {
      throw new ArgumentException("A projectile root binding requires an assigned runtime handle.",
        nameof(runtimeHandle));
    }

    RuntimeHandle = runtimeHandle;
  }

  public RuntimeEntityHandle RuntimeHandle { get; }
}
