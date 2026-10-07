using System.Numerics;
using EntityEcs;
using EntityEcs.Components;
using Terraria.Items;
using Terraria.Npc;
using Terraria.Player;
using Terraria.Player.Movement;
using Terraria.Relationships;
using Terraria.SpatialSimulation.Components;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class RuntimePlayerEntity
{
  private const int RespawnDelayTicks = 600;
  private const int ContactImmunityTicks = 40;

  private readonly EntityRuntime _entityRuntime;

  private RuntimePlayerEntity(
    EntityRuntime entityRuntime,
    RuntimeEntityHandle runtimeHandle,
    int slot,
    WeaponComponent weapon,
    RuntimePlayerInventoryOwner inventory)
  {
    _entityRuntime = entityRuntime;
    RuntimeHandle = runtimeHandle;
    Slot = slot;
    Weapon = weapon;
    Inventory = inventory;
  }

  internal readonly record struct LifecycleSnapshot(
    PlayerLifecyclePhase Phase,
    int RespawnRemainingTicks,
    int DeadElapsedTicks)
  {
    public bool IsDead => Phase is PlayerLifecyclePhase.Dead or PlayerLifecyclePhase.Respawning;
  }

  internal readonly record struct VitalsSnapshot(
    int Life,
    int EffectiveLifeMaximum,
    int Defense);

  internal readonly record struct PhysicsSnapshot(
    float Gravity,
    float MaxFallSpeed,
    float MaxRunSpeed,
    float RunAcceleration,
    float RunSlowdown);

  public int Slot { get; }

  public RuntimeEntityHandle RuntimeHandle { get; }

  public EntityReference Reference
  {
    get
    {
      if (!_entityRuntime.TryGetReference(
            RuntimeHandle,
            EntityReferenceScope.Player,
            out EntityReference reference))
      {
        throw new InvalidOperationException("The Player entity reference is no longer active.");
      }

      return reference;
    }
  }

  public MovementStateComponent Movement
  {
    get
    {
      LocationComponent location = Capture<LocationComponent, LocationComponent>(
        static component => component,
        "location");
      VelocityComponent velocity = Capture<VelocityComponent, VelocityComponent>(
        static component => component,
        "velocity");
      MovementGroundedStateComponent grounded = Capture<
        MovementGroundedStateComponent,
        MovementGroundedStateComponent>(
          static component => component,
          "grounded state");
      return new MovementStateComponent(
        new Vector2(location.X, location.Y),
        new Vector2(velocity.X, velocity.Y),
        isGrounded: grounded.IsGrounded);
    }
    set
    {
      if (!_entityRuntime.TryEditComponents<
            LocationComponent,
            VelocityComponent,
            MovementGroundedStateComponent>(
            RuntimeHandle,
            (
              ref LocationComponent location,
              ref VelocityComponent velocity,
              ref MovementGroundedStateComponent grounded) =>
            {
              location.X = value.Position.X;
              location.Y = value.Position.Y;
              velocity.X = value.Velocity.X;
              velocity.Y = value.Velocity.Y;
              grounded = new MovementGroundedStateComponent(value.IsGrounded);
            }))
      {
        throw new InvalidOperationException(
          "The Player spatial movement output could not be committed.");
      }
    }
  }

  public LocationComponent Location => Capture<LocationComponent, LocationComponent>(
    static component => component,
    "location");

  public VelocityComponent Velocity => Capture<VelocityComponent, VelocityComponent>(
    static component => component,
    "velocity");

  public ColliderComponent Collider => Capture<ColliderComponent, ColliderComponent>(
    static component => component,
    "collider");

  internal Vector2 SpawnPosition => Capture<PlayerSpawnPointComponent, Vector2>(
    static component => component.Position,
    "spawn point");

  public Vector2 MovementDeltaThisTick { get; internal set; }

  public WeaponComponent Weapon { get; }

  public RuntimePlayerInventoryOwner Inventory { get; }

  public int InventorySlotsUsed => Inventory.CountUsedSlots();

  public LifecycleSnapshot Lifecycle => Capture<PlayerLifecycleComponent, LifecycleSnapshot>(
    static component => new LifecycleSnapshot(
      component.Phase,
      component.RespawnRemainingTicks,
      component.DeadElapsedTicks),
    "lifecycle");

  public VitalsSnapshot Vitals => Capture<PlayerVitalState, VitalsSnapshot>(
    static component => new VitalsSnapshot(
      component.Life,
      component.EffectiveLifeMaximum,
      component.Defense),
    "vitals");

  public PhysicsSnapshot Physics => Capture<PlayerMovementPhysicsStateComponent, PhysicsSnapshot>(
    static component => new PhysicsSnapshot(
      component.Gravity,
      component.MaxFallSpeed,
      component.MaxRunSpeed,
      component.RunAcceleration,
      component.RunSlowdown),
    "movement physics");

  public int PveDeathCount => Capture<PlayerDeathRecordComponent, int>(
    static component => component.PveDeathCount,
    "death record");

  public bool MagicQuiver => Capture<PlayerRangedAccessoryCapabilityComponent, bool>(
    static component => component.MagicQuiver,
    "ranged accessories");

  public int JumpCount { get; internal set; }

  public int LandingCount { get; internal set; }

  public int ShotsFired { get; set; }

  public PlayerItemUseIntentComponent ItemUseIntent => Capture<
    PlayerItemUseIntentComponent,
    PlayerItemUseIntentComponent>(static component => component, "item-use intent");

  public int ItemUseAnimationRemainingTicks => Capture<PlayerItemUseState, int>(
    static component => component.AnimationRemainingTicks,
    "item-use state");

  internal NpcPlayerTargetSnapshot CaptureNpcTargetSelectionSnapshot(int npcType)
  {
    LocationComponent location = Location;
    ColliderComponent collider = Collider;
    PlayerNpcTargetingSnapshot targeting = Capture<
      PlayerNpcTargetingStateComponent,
      PlayerNpcTargetingSnapshot>(
        component => new PlayerNpcTargetingSnapshot(
          component.Aggro,
          component.HasNoAggroFor(npcType)),
        "NPC targeting state");
    return new NpcPlayerTargetSnapshot(
      Slot,
      new NpcTargetGeometrySnapshot(
        new Vector2(location.X + collider.OffsetX, location.Y + collider.OffsetY),
        Math.Max(1, (int)collider.Width),
        Math.Max(1, (int)collider.Height)),
      IsActive: true,
      IsDead: Lifecycle.IsDead,
      IsGhost: Capture<PlayerGhostStateComponent, bool>(
        static component => component.Ghost,
        "ghost state"),
      targeting.Aggro,
      targeting.NoAggro,
      Gross: Capture<PlayerDebuffStatusAliasComponent, bool>(
        static component => component.Gross,
        "debuff aliases"),
      ItemAnimation: ItemUseAnimationRemainingTicks,
      TankPet: null);
  }

  internal int CaptureTankPetProjectileSlot()
  {
    return Capture<PlayerAccessoryStringEffectComponent, int>(
      static component => component.TankPet,
      "accessory string effects");
  }

  internal void CommitNpcTargetingState(
    int aggro,
    IEnumerable<int> noAggroNpcTypes)
  {
    ArgumentNullException.ThrowIfNull(noAggroNpcTypes);
    int[] npcTypes = noAggroNpcTypes.ToArray();
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref PlayerNpcTargetingStateComponent state) => state.Commit(aggro, npcTypes)))
    {
      throw new InvalidOperationException("The Player NPC targeting state could not be committed.");
    }
  }

  internal void CommitGhostState(bool ghost)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref PlayerGhostStateComponent state) => state.CommitGhost(ghost)))
    {
      throw new InvalidOperationException("The Player ghost state could not be committed.");
    }
  }

  internal void CommitGrossStatus(bool gross)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref PlayerDebuffStatusAliasComponent state) => state.CommitGross(gross)))
    {
      throw new InvalidOperationException("The Player debuff aliases could not be committed.");
    }
  }

  internal void CommitTankPetProjectileSlot(int projectileSlot)
  {
    if (!_entityRuntime.TryEdit(
          RuntimeHandle,
          (ref PlayerAccessoryStringEffectComponent state) => state.CommitTankPet(projectileSlot)))
    {
      throw new InvalidOperationException("The Player tank pet slot could not be committed.");
    }
  }

  public static RuntimePlayerEntity Hydrate(
    EntityRuntime entityRuntime,
    RuntimeEntityHandle runtimeHandle,
    int slot,
    Vector2 position,
    Vector2 spawnPosition,
    ColliderComponent collider,
    WeaponComponent weapon,
    RuntimeItemRegistry itemRegistry,
    bool magicQuiver)
  {
    ArgumentNullException.ThrowIfNull(entityRuntime);
    ArgumentNullException.ThrowIfNull(itemRegistry);
    ArgumentOutOfRangeException.ThrowIfNegative(slot);
    if (!collider.IsEnabled || !collider.HasArea)
    {
      throw new ArgumentOutOfRangeException(
        nameof(collider),
        "The scripted Player requires an enabled collider with positive area.");
    }
    MovementStateComponent initialMovement = new(position: position);

    var identity = new PlayerIdentityComponent
    {
      CharacterName = $"Simulation Player {slot}",
      LegacyPlayerSlot = new LegacyPlayerSlot(slot),
    };
    var deathRecord = new PlayerDeathRecordComponent();
    var rest = new PlayerRestComponent();
    var sitting = new PlayerSittingComponent();
    var sleeping = new PlayerSleepingComponent();
    var ghost = new PlayerGhostStateComponent();
    var lifecycle = new PlayerLifecycleComponent(PlayerLifecyclePhase.Alive, respawnRemainingTicks: 0);
    _ = PlayerLifecycleSystem.CommitSpawn(
      identity,
      ref lifecycle,
      deathRecord,
      rest,
      sitting,
      sleeping,
      new PlayerLifecycleSystem.SpawnCommitInput(
        IsSpawningIntoWorld: true,
        LastTimePlayerWasSavedBinary: 0,
        DateTime.UnixEpoch,
        IsLocalPlayer: true,
        MultiplayerBroadcast: false));

    Attach(entityRuntime, runtimeHandle, identity);
    Attach(entityRuntime, runtimeHandle, deathRecord);
    Attach(entityRuntime, runtimeHandle, rest);
    Attach(entityRuntime, runtimeHandle, sitting);
    Attach(entityRuntime, runtimeHandle, sleeping);
    Attach(entityRuntime, runtimeHandle, ghost);
    Attach(entityRuntime, runtimeHandle, new PlayerDebuffStatusAliasComponent());
    Attach(entityRuntime, runtimeHandle, new PlayerAccessoryStringEffectComponent());
    Attach(entityRuntime, runtimeHandle, new PlayerNpcTargetingStateComponent());
    Attach(entityRuntime, runtimeHandle, new PlayerDodgeAndImmunityStateComponent());
    Attach(entityRuntime, runtimeHandle, lifecycle);
    Attach(entityRuntime, runtimeHandle, new PlayerVitalState
    {
      Life = 100,
      BaseLifeMaximum = 100,
      EffectiveLifeMaximum = 100,
    });
    Attach(
      entityRuntime,
      runtimeHandle,
      new LocationComponent(initialMovement.Position.X, initialMovement.Position.Y));
    Attach(entityRuntime, runtimeHandle, new VelocityComponent(0.0f, 0.0f));
    Attach(entityRuntime, runtimeHandle, collider);
    Attach(entityRuntime, runtimeHandle, new MovementGroundedStateComponent(false));
    Attach(entityRuntime, runtimeHandle, new PlayerMovementPhysicsStateComponent());
    Attach(entityRuntime, runtimeHandle, new PlayerContactImmunityComponent(remainingTicks: 0));
    Attach(entityRuntime, runtimeHandle, new PlayerSpawnPointComponent(spawnPosition));
    Attach(entityRuntime, runtimeHandle, new PlayerItemUseState());
    Attach(entityRuntime, runtimeHandle, default(PlayerItemUseIntentComponent));
    Attach(
      entityRuntime,
      runtimeHandle,
      new PlayerRangedAccessoryCapabilityComponent(magicQuiver));
    Attach(entityRuntime, runtimeHandle, new PlayerInventorySlotsComponent());

    if (!entityRuntime.TryPublishEntity(runtimeHandle))
    {
      throw new InvalidOperationException("The Player entity could not be published.");
    }

    var inventory = new RuntimePlayerInventoryOwner(
      entityRuntime,
      runtimeHandle,
      itemRegistry,
      slot);
    inventory.InitializeStartingLoadout();
    return new RuntimePlayerEntity(entityRuntime, runtimeHandle, slot, weapon, inventory);
  }

  public void AdvanceLifecycle()
  {
    PlayerContactImmunityComponent immunity = Capture<PlayerContactImmunityComponent, PlayerContactImmunityComponent>(
      static component => component,
      "contact immunity");
    if (immunity.RemainingTicks > 0 &&
        !_entityRuntime.TryReplace(
          RuntimeHandle,
          new PlayerContactImmunityComponent(immunity.RemainingTicks - 1)))
    {
      throw new InvalidOperationException("The Player contact immunity could not advance.");
    }

    if (!Lifecycle.IsDead)
    {
      return;
    }

    PlayerLifecycleSystem.DeadTickResult deadTick = default;
    if (!_entityRuntime.TryEditPair<PlayerLifecycleComponent, PlayerGhostStateComponent>(
          RuntimeHandle,
          (ref PlayerLifecycleComponent lifecycle, ref PlayerGhostStateComponent ghost) =>
          {
            deadTick = PlayerDeadTickAdapter.Apply(
              ref lifecycle,
              ghost,
              new PlayerLifecycleSystem.DeadTickInput(
                IsHardcoreWithoutRespawn: false,
                IsGhost: false,
                IsLocalPlayer: true,
                IsServer: false,
                HasCursorItem: false));
          }))
    {
      throw new InvalidOperationException("The Player dead-tick components are unavailable.");
    }

    if (!deadTick.ShouldRequestRespawn)
    {
      return;
    }

    if (!_entityRuntime.TryEditComponents<
          PlayerIdentityComponent,
          PlayerLifecycleComponent,
          PlayerDeathRecordComponent,
          PlayerRestComponent,
          PlayerSittingComponent,
          PlayerSleepingComponent>(
          RuntimeHandle,
          (
            ref PlayerIdentityComponent identity,
            ref PlayerLifecycleComponent lifecycle,
            ref PlayerDeathRecordComponent deathRecord,
            ref PlayerRestComponent rest,
            ref PlayerSittingComponent sitting,
            ref PlayerSleepingComponent sleeping) =>
          {
            _ = PlayerLifecycleSystem.CommitSpawn(
              identity,
              ref lifecycle,
              deathRecord,
              rest,
              sitting,
              sleeping,
              new PlayerLifecycleSystem.SpawnCommitInput(
                IsSpawningIntoWorld: false,
                LastTimePlayerWasSavedBinary: 0,
                DateTime.UnixEpoch,
                IsLocalPlayer: true,
                MultiplayerBroadcast: false));
          }))
    {
      throw new InvalidOperationException("The Player respawn lifecycle could not be committed.");
    }

    PlayerSpawnPointComponent spawnPoint = Capture<PlayerSpawnPointComponent, PlayerSpawnPointComponent>(
      static component => component,
      "spawn point");
    Movement = new MovementStateComponent(position: spawnPoint.Position);
    if (!_entityRuntime.TryEdit<PlayerVitalState>(
          RuntimeHandle,
          (ref PlayerVitalState vitals) => vitals.Life = vitals.EffectiveLifeMaximum))
    {
      throw new InvalidOperationException("The Player respawn vitals could not be reset.");
    }

    if (!_entityRuntime.TryReplace(
          RuntimeHandle,
          new PlayerContactImmunityComponent(ContactImmunityTicks)))
    {
      throw new InvalidOperationException("The Player respawn immunity could not be reset.");
    }

    ResetItemUseState();
  }

  public bool TryTakeNpcContactDamage(RuntimeNpcContactSnapshot npc, long tickNumber)
  {
    if (Lifecycle.IsDead || !npc.IsActive || !npc.IsHostile)
    {
      return false;
    }

    PlayerContactImmunityComponent currentImmunity =
      Capture<PlayerContactImmunityComponent, PlayerContactImmunityComponent>(
        static component => component,
        "contact immunity");
    bool shadowDodgeActive = Capture<PlayerDodgeAndImmunityStateComponent, bool>(
      static component => component.ShadowDodge,
      "dodge and immunity");
    Guid sourceId = npc.Reference.EntityId.Value;
    var eligibilityInput = new PlayerDamageEligibilityInput(
      sourceId,
      npc.Damage,
      HasGeneralImmunity: currentImmunity.RemainingTicks > 0,
      SourceCooldownActive: false);
    PlayerDamageEligibilityResult eligibility = PlayerDamageEligibilityQuery.Evaluate(
      in eligibilityInput,
      shadowDodgeActive);
    if (!eligibility.IsEligible)
    {
      return false;
    }

    int lifeAfterDamage = 0;
    if (!_entityRuntime.TryEditPair<PlayerVitalState, PlayerContactImmunityComponent>(
          RuntimeHandle,
          (ref PlayerVitalState vitals, ref PlayerContactImmunityComponent immunity) =>
          {
            int damage = PlayerDamageMitigationQuery.Evaluate(
              new PlayerDamageMitigationInput(
                npc.Damage,
                vitals.Defense,
                Endurance: 0f,
                Critical: false));
            vitals.Life = Math.Max(0, vitals.Life - damage);
            immunity = new PlayerContactImmunityComponent(ContactImmunityTicks);
            lifeAfterDamage = vitals.Life;
          }))
    {
      throw new InvalidOperationException("The Player contact-damage components are unavailable.");
    }

    if (lifeAfterDamage > 0)
    {
      return true;
    }

    DateTime deathTime = new(
      checked(DateTime.UnixEpoch.Ticks + tickNumber * TimeSpan.TicksPerSecond / 60),
      DateTimeKind.Utc);
    MovementStateComponent movement = Movement;
    PlayerLifecycleSystem.DeathResolutionResult death = default;
    if (!_entityRuntime.TryEditComponents<
          PlayerLifecycleComponent,
          PlayerDeathRecordComponent,
          PlayerRestComponent,
          PlayerSittingComponent,
          PlayerSleepingComponent>(
          RuntimeHandle,
          (
            ref PlayerLifecycleComponent lifecycle,
            ref PlayerDeathRecordComponent deathRecord,
            ref PlayerRestComponent rest,
            ref PlayerSittingComponent sitting,
            ref PlayerSleepingComponent sleeping) =>
          {
            death = PlayerLifecycleSystem.ResolveDeath(
              ref lifecycle,
              deathRecord,
              rest,
              sitting,
              sleeping,
              new PlayerLifecycleSystem.DeathResolutionInput(
                IsCreativeGodMode: false,
                PracticeModeResetReturnsTrue: false,
                IsPvpRequest: false,
                new Terraria.Player.WorldPosition(movement.Position.X, movement.Position.Y),
                deathTime,
                RespawnDelayTicks,
                IsLocalPlayer: true,
                MultiplayerBroadcast: false,
                PlayerLifecycleSystem.SpectatingNetworkMode.SinglePlayer));
          }))
    {
      throw new InvalidOperationException("The Player death components are unavailable.");
    }

    if (!death.DidCommit)
    {
      throw new InvalidOperationException("A depleted scripted player could not enter the dead lifecycle.");
    }

    movement.Velocity = Vector2.Zero;
    Movement = movement;

    return true;
  }

  internal bool TryEditPhysics(EntityComponentEditor<PlayerMovementPhysicsStateComponent> editor)
  {
    return _entityRuntime.TryEdit(RuntimeHandle, editor);
  }

  internal bool TryEditItemUse(EntityComponentEditor<PlayerItemUseState> editor)
  {
    return _entityRuntime.TryEdit(RuntimeHandle, editor);
  }

  internal void AdvanceItemUseIntent(PlayerItemUseIntentInput input)
  {
    if (!_entityRuntime.TryEdit<PlayerItemUseIntentComponent>(
          RuntimeHandle,
          (ref PlayerItemUseIntentComponent intent) =>
            PlayerItemUseIntentSystem.Advance(input, reset: false, ref intent)))
    {
      throw new InvalidOperationException("The Player item-use intent could not advance.");
    }
  }

  private void ResetItemUseState()
  {
    if (!_entityRuntime.TryEdit<PlayerItemUseState>(
          RuntimeHandle,
          (ref PlayerItemUseState itemUse) =>
          {
            itemUse.AnimationRemainingTicks = 0;
            itemUse.UseRemainingTicks = 0;
            itemUse.ReuseDelayRemainingTicks = 0;
            itemUse.HasPendingReuse = false;
            itemUse.LastUseAttemptSucceeded = false;
          }) ||
        !_entityRuntime.TryReplace(RuntimeHandle, default(PlayerItemUseIntentComponent)))
    {
      throw new InvalidOperationException("The Player item-use state could not be reset.");
    }
  }

  private TProjection Capture<TComponent, TProjection>(
    Func<TComponent, TProjection> capture,
    string componentName)
    where TComponent : notnull
    where TProjection : struct
  {
    if (!_entityRuntime.TryCapture(RuntimeHandle, capture, out TProjection projection))
    {
      throw new InvalidOperationException($"The Player {componentName} component is unavailable.");
    }

    return projection;
  }

  private readonly record struct PlayerNpcTargetingSnapshot(int Aggro, bool NoAggro);

  private static void Attach<TComponent>(
    EntityRuntime entityRuntime,
    RuntimeEntityHandle handle,
    TComponent component)
    where TComponent : notnull
  {
    if (!entityRuntime.TryAttach(handle, component))
    {
      throw new InvalidOperationException(
        $"The Player entity could not attach component {typeof(TComponent).Name}.");
    }
  }
}
