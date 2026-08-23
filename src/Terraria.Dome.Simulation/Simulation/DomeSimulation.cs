using System;
using System.Collections.Generic;
using System.Linq;
using Arch.Buffer;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Combat;
using Terraria.Dome.Simulation.Combat.Commands;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Combat.Systems;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Movement.Systems;
using Terraria.Dome.Simulation.Movement.Components;
using Terraria.Dome.Simulation.Physics.Systems;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.Player.Commands;
using Terraria.Dome.Simulation.Player.Events;
using Terraria.Dome.Simulation.Player.Systems;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.StatusEffects.Components;
using Terraria.Dome.Simulation.StatusEffects.Systems;
using Terraria.Dome.Simulation.Tick;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Systems;
using Terraria.Dome.Simulation.World.Events;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Events;
using Terraria.Dome.Simulation.Items.Snapshots;
using Terraria.Dome.Simulation.Items.Systems;
using Terraria.Dome.Simulation.Inventory.Components;
using Terraria.Dome.Simulation.Inventory.Systems;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.Liquid.Snapshots;
using Terraria.Dome.Simulation.Liquid.Systems;
using Terraria.Dome.Simulation.Projectile.Systems;
using Terraria.Dome.Simulation.Projectile.Definitions;
using Terraria.Dome.Simulation.Projectile;
using Terraria.Dome.Simulation.Combat.Events;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.WorldObjects.Chest.Commands;
using Terraria.Dome.Simulation.WorldObjects.Chest.Systems;
using Terraria.Dome.Simulation.WorldObjects.Definitions;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Npc;
using Terraria.Dome.Simulation.Npc.Snapshots;
using Terraria.Dome.Simulation.Npc.Systems;
using Terraria.Dome.Simulation.Wiring.Commands;
using Terraria.Dome.Simulation.Wiring.Components;
using Terraria.Dome.Simulation.Wiring.Definitions;
using Terraria.Dome.Simulation.Wiring.Snapshots;
using Terraria.Dome.Simulation.Wiring.Systems;
using NpcComponents = Terraria.Dome.Simulation.Npc.Components;
using ItemUseStateComponent = Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent;
using PipelineLiquidChangeCommand = Terraria.Dome.Simulation.Commands.LiquidChangeCommand;
using SimulationLiquidMerge = Terraria.Dome.Simulation.Liquid.Components.LiquidMergeComponent;
using PipelineLiquidSourceComponent = Terraria.Dome.Simulation.Liquid.Components.LiquidSourceComponent;
using ProjectileHitImmunityComponent = global::Terraria.Dome.Simulation.Combat.Components.HitImmunityComponent;

namespace Terraria.Dome.Simulation;

public sealed partial class DomeSimulation : IDisposable
{
  private const float PlayerSpeed = 3.0f;
  private const float GravityPerTick = -1.0f;
  private const float JumpSpeed = 4.0f;
  private const float ProjectileSpeed = 4.0f;
  private const int ProjectileDamage = 10;
  private const int ProjectileLifetimeTicks = 30;
  private const int PlayerRespawnDelayTicks = 3;
  private const int NpcContactDamage = 25;
  private const int NpcHitImmunityTicks = 2;
  private const float PickupRange = 3.0f;
  private const float WorldItemStackingRange = 1.0f;
  private const float MaximumInteractionRange = 12.0f;
  private const int DomeChaserNpcType = 1;
  public const int FixtureNpcType = 2;
  private const int DomeBoltProjectileType = 1;
  private const int MaximumSignCount = 32000;
  private const int MaximumNpcCount = 200;
  private const int TrainingDummyNpcType = 488;
  private readonly WorldClock _worldClock;
  private readonly WorldClockSystem _worldClockSystem = new();
  private readonly WorldInvasionTravelSystem _worldInvasionTravelSystem = new();
  private readonly WorldProgressionSystem _worldProgressionSystem = new();
  private readonly WorldTimeRateSystem _worldTimeRateSystem = new();
  private readonly WorldInvasionStartEligibilitySystem _worldInvasionStartEligibilitySystem = new();
  private readonly WorldInvasionClearFlagSystem _worldInvasionClearFlagSystem = new();
  private readonly WorldSlimeRainEligibilitySystem _worldSlimeRainEligibilitySystem = new();
  private readonly WorldWeatherSystem _worldWeatherSystem = new();
  private readonly WorldMeteorImpactSystem _worldMeteorImpactSystem = new();
  private readonly WorldMeteorScheduleSystem _worldMeteorScheduleSystem = new();
  private readonly List<WorldEventStartCommand> _worldEventStartRequests = new();
  private readonly List<WorldLanternNightScheduleCommand> _worldLanternNightScheduleRequests = new();
  private readonly List<WorldInvasionStartCommand> _worldInvasionStartRequests = new();
  private readonly List<WorldInvasionProgressCommand> _worldInvasionProgressRequests = new();
  private readonly List<WorldRainStartCommand> _worldRainStartRequests = new();
  private readonly List<WorldWindChangeCommand> _worldWindChangeRequests = new();
  private readonly List<WorldSlimeRainStartCommand> _worldSlimeRainStartRequests = new();
  private readonly List<WorldSlimeRainStopCommand> _worldSlimeRainStopRequests = new();
  private readonly List<WorldMeteorImpactCommand> _worldMeteorImpactRequests = new();
  private readonly List<WorldMeteorScheduleCommand> _worldMeteorScheduleRequests = new();
  private readonly List<WorldMeteorImpactCommand> _scheduledMeteorImpactRequests = new();
  private readonly SimulationCommandQueue _commands = new();
  private readonly NpcStore _npcs = new();
  private readonly Dictionary<NpcHandle, NpcReplicationSnapshot> _npcReplications = new();
  private readonly PlayerStore _players = new();
  private readonly ProjectileStore _projectileIdsByEntity = new();
  private readonly Dictionary<int, ProjectileReplicationSnapshot> _projectileReplications = new();
  private readonly Dictionary<string, PlayerPersistentState> _playerAccounts = new(
    StringComparer.Ordinal);
  private readonly Dictionary<PlayerHandle, string> _playerAccountUuids = new();
  private readonly WorldItemStore _worldItems;
  private readonly Dictionary<int, DoorSnapshot> _doors = new();
  private readonly Dictionary<int, SignSnapshot> _signs = new();
  private readonly Dictionary<int, ChestComponent> _chests = new();
  private readonly ChestIndexSystem _chestIndexSystem = new();
  private readonly ChestMutationCommitSystem _chestMutationCommitSystem = new();
  private readonly ChestOpenSystem _chestOpenSystem = new();
  private readonly ChestCloseSystem _chestCloseSystem = new();
  private readonly LiquidWorldStateComponent _liquidState = new(
    maximumQueueLength: 4096,
    tickBudget: 256);
  private readonly LiquidUpdateQueueComponent _liquidQueue = new(maximumLength: 4096);
  private readonly LiquidDirtySectionComponent _liquidDirtySections = new();
  private readonly LiquidInputSystem _liquidInputSystem = new();
  private readonly LiquidPropagationSystem _liquidPropagationSystem = new();
  private readonly LiquidMergeSystem _liquidMergeSystem = new();
  private readonly LiquidCommitSystem _liquidCommitSystem = new();
  private readonly LiquidReplicationSystem _liquidReplicationSystem = new();
  private readonly LiquidTransferSystem _liquidTransferSystem = new();
  private readonly WireNetworkComponent _wireNetwork = new();
  private readonly Dictionary<(int X, int Y), TriggerComponent> _wireTriggers = new();
  private readonly Dictionary<int, MechanismComponent> _mechanisms = new();
  private readonly Dictionary<int, ActuatorComponent> _actuators = new();
  private readonly Dictionary<int, LampComponent> _lamps = new();
  private readonly Dictionary<int, int> _doorMechanisms = new();
  private readonly Dictionary<int, PumpComponent> _pumps = new();
  private readonly List<PressurePlateComponent> _pressurePlates = new();
  private readonly List<LogicGateComponent> _logicGates = new();
  private readonly Dictionary<int, bool> _logicGateOutputs = new();
  private readonly List<WiringInputCommand> _wiringInputs = new();
  private readonly WiringInputValidationSystem _wiringInputValidationSystem = new();
  private readonly WireTraversalSystem _wireTraversalSystem = new();
  private readonly PressurePlateDetectionSystem _pressurePlateDetectionSystem = new();
  private readonly LogicGateEvaluationSystem _logicGateEvaluationSystem = new();
  private readonly MechanismActivationSystem _mechanismActivationSystem = new();
  private readonly ActuatorCommandSystem _actuatorCommandSystem = new();
  private readonly LampCommandSystem _lampCommandSystem = new();
  private readonly PumpCommandSystem _pumpCommandSystem = new();
  private readonly WiringEventProjectionSystem _wiringEventProjectionSystem = new();
  private IReadOnlyList<PipelineLiquidChangeCommand> _lastLiquidCommands = [];
  private IReadOnlyList<SimulationTickPhase> _lastTickPhases = [];
  private SimulationSnapshot? _lastPublishedSnapshot;
  private readonly List<PlayerDamagedEvent> _playerDamagedEvents = new();
  private readonly List<PlayerDiedEvent> _playerDiedEvents = new();
  private readonly List<PlayerRespawnedEvent> _playerRespawnedEvents = new();
  private readonly List<ItemUsedEvent> _itemUsedEvents = new();
  private readonly List<ItemEquippedEvent> _itemEquippedEvents = new();
  private readonly List<ItemPrefixChangedEvent> _itemPrefixChangedEvents = new();
  private readonly List<InventoryChangedEvent> _inventoryChangedEvents = new();
  private readonly List<WorldItemCreatedEvent> _worldItemCreatedEvents = new();
  private readonly List<WorldItemPickedUpEvent> _worldItemPickedUpEvents = new();
  private readonly List<WorldItemDestroyedEvent> _worldItemDestroyedEvents = new();
  private readonly List<ItemDroppedEvent> _itemDroppedEvents = new();
  private readonly List<ExtractinatorResultEvent> _extractinatorResultEvents = new();
  private readonly Dictionary<(int X, int Y), long> _extractinatorCooldowns = new();
  private readonly HashSet<(int X, int Y)> _extractinatorTargetsCommittedThisTick = new();
  private readonly List<WorldSlimeRainWarningEvent> _worldSlimeRainWarningEvents = new();
  private readonly List<WorldInvasionCompletedEvent> _worldInvasionCompletedEvents = new();
  private readonly Dictionary<int, TileEntityPersistentState> _tileEntities = new();
  private readonly Dictionary<int, TrainingDummyOwnershipState> _trainingDummyOwnerships = new();
  private readonly List<int> _pendingTrainingDummyActivations = new();
  private readonly List<PlaceItemCommand> _placeItemCommands = new();
  private readonly List<EquipItemCommand> _equipItemCommands = new();
  private readonly List<UnequipItemCommand> _unequipItemCommands = new();
  private readonly List<ApplyItemPrefixCommand> _itemPrefixCommands = new();
  private readonly List<ApplyItemVariantCommand> _itemVariantCommands = new();
  private readonly ItemDefinitionRegistry _itemDefinitions = new([
    new ItemDefinition(
      1,
      99,
      HealthRestore: 25,
      UseCooldownTicks: 10,
      Prefixes: new ItemPrefixDefinition([11, 17])),
    new ItemDefinition(
      2,
      99,
      Use: new ItemUseDefinition(
        UseTime: 1,
        ShootType: 2,
        ShootSpeed: 6.0f,
        AmmoType: 3,
        ConsumesAmmo: true,
        CooldownTicks: 3)),
    new ItemDefinition(
      3,
      99,
      Prefixes: new ItemPrefixDefinition([7])),
    new ItemDefinition(
      4,
      1,
      Equipment: new ItemEquipmentDefinition(
        ItemEquipmentSlot.Head,
        Defense: 5,
        LifeRegen: 60),
      Prefixes: new ItemPrefixDefinition([])),
    new ItemDefinition(
      5,
      99,
      Placement: new ItemPlacementDefinition(TileType: 12)),
    new ItemDefinition(
      6,
      99,
      Placement: new ItemPlacementDefinition(WallType: 7)),
    new ItemDefinition(
      7,
      99,
      Recovery: new ItemRecoveryDefinition(
        BuffType: 19,
        BuffDurationTicks: 60,
        Consumable: true)),
    new ItemDefinition(
      8,
      99,
      Combat: new ItemCombatDefinition(
        Damage: 14,
        ProjectileType: 2,
        ProjectileSpeed: 5.0f)),
    new ItemDefinition(62, 999),
    new ItemDefinition(194, 999),
    new ItemDefinition(195, 999),
    new ItemDefinition(
      5395,
      999,
      Extractinator: new ItemExtractinatorDefinition(4))]);
  private readonly InventoryTransferSystem _inventoryTransfers = new();
  private readonly ItemAmmoConsumptionSystem _itemAmmoConsumptionSystem = new();
  private readonly ItemInventorySanitizationSystem _itemInventorySanitizationSystem = new();
  private readonly WorldItemStackingSystem _worldItemStackingSystem = new();
  private readonly InventoryCommandSystem _inventoryCommandSystem = new();
  private readonly ItemInputValidationSystem _itemInputValidationSystem = new();
  private readonly ItemPlacementSystem _itemPlacementSystem = new();
  private readonly ItemUseCooldownSystem _itemUseCooldownSystem = new();
  private readonly ItemUseSystem _itemUseSystem = new();
  private readonly ExtractinatorSystem _extractinatorSystem = new();
  private readonly ExtractinatorRuleRegistry _extractinatorRules =
    ExtractinatorRuleRegistry.CreateVersion4();
  private readonly ItemEquipmentSystem _itemEquipmentSystem = new();
  private readonly ItemPrefixSystem _itemPrefixSystem = new();
  private readonly ItemVariantSystem _itemVariantSystem = new();
  private readonly WorldItemPickupSystem _worldItemPickupSystem = new();
  private readonly WorldItemPickupDelaySystem _worldItemPickupDelaySystem = new();
  private readonly WorldItemSpawnSystem _worldItemSpawnSystem = new();
  private readonly WorldItemMotionSystem _worldItemMotionSystem = new();
  private readonly WorldItemDestroySystem _worldItemDestroySystem = new();
  private readonly ItemSelectionSystem _itemSelectionSystem = new();
  private readonly EquipmentStatSystem _equipmentStatSystem = new();
  private readonly DamageCalculationSystem _damageCalculationSystem = new();
  private readonly NpcDefinitionRegistry _npcDefinitions = new([
    new NpcDefinition(
      DefinitionId: 1,
      NetId: 1,
      MaximumHealth: 100,
      Defense: 0,
      ColliderWidth: 1.0f,
      ColliderHeight: 2.0f,
      BehaviorId: NpcBehaviorId.OrdinaryChase,
      LootTableId: 1),
    new NpcDefinition(
      DefinitionId: FixtureNpcType,
      NetId: 1,
      MaximumHealth: 100,
      Defense: 0,
      ColliderWidth: 1.0f,
      ColliderHeight: 2.0f,
      BehaviorId: NpcBehaviorId.TownHome,
      LootTableId: 1),
    new NpcDefinition(
      DefinitionId: TrainingDummyNpcType,
      NetId: TrainingDummyNpcType,
      MaximumHealth: 1000,
      Defense: 0,
      ColliderWidth: 18.0f,
      ColliderHeight: 40.0f,
      BehaviorId: NpcBehaviorId.TrainingDummy,
      LootTableId: 0,
      Faction: NpcFaction.Neutral,
      Category: NpcCategory.Town,
      AiStyle: 92,
      IsImmortal: true,
      AlwaysReplicate: true)]);
  private readonly NpcLootSystem _npcLootSystem = new(
    new NpcLootDefinitionRegistry([
      new NpcLootDefinition(
        LootTableId: 1,
        ItemType: 1,
        MinimumQuantity: 1,
        MaximumQuantity: 2)]),
    new WorldSeed(1));
  private readonly NpcSystemPipeline _npcSystemPipeline = new();
  private readonly PlayerLifecycleSystem _playerLifecycleSystem = new();
  private readonly NpcSpawnCommitSystem _npcSpawnCommitSystem = new();
  private readonly NpcMovementIntentSystem _npcMovementIntentSystem = new();
  private readonly NpcContactEffectSystem _npcContactEffectSystem = new();
  private readonly NpcLifecycleSystem _npcLifecycleSystem = new();
  private readonly NpcDeathSystem _npcDeathSystem = new();
  private readonly List<NpcDeathResult> _pendingNpcDeaths = new();
  private readonly HashSet<NpcHandle> _publishedNpcDeaths = new();
  private readonly WorldSeed _worldSeed = new(1);
  private readonly GroundCollisionSystem _groundCollisionSystem = new();
  private readonly TileCollisionSystem _tileCollisionSystem = new();
  private readonly TopSlopeContactSystem _topSlopeContactSystem = new();
  private readonly MovementSystem _movementSystem = new();
  private readonly PlayerControlSystem _playerControlSystem = new();
  private readonly PlayerDeathSystem _playerDeathSystem = new();
  private readonly PlayerGravitySystem _playerGravitySystem = new();
  private readonly PlayerInputApplySystem _playerInputApplySystem = new();
  private readonly PlayerRespawnSystem _playerRespawnSystem = new();
  private readonly DamageResolutionSystem _damageResolutionSystem = new();
  private readonly ImmunitySystem _immunitySystem = new();
  private readonly PlayerVitalRegenSystem _playerVitalRegenSystem = new();
  private readonly BuffDurationSystem _buffDurationSystem = new();
  private readonly BuffEffectSystem _buffEffectSystem = new();
  private readonly ProjectileCollisionSystem _projectileCollisionSystem = new();
  private readonly ProjectileDefinitionRegistry _projectileDefinitions =
    ProjectileDefinitionRegistry.CreateDefault();
  private readonly ProjectileSpawnSystem _projectileSpawnSystem = new();
  private readonly ProjectileLifetimeSystem _projectileLifetimeSystem = new();
  private readonly ProjectileBehaviorSystem _projectileBehaviorSystem =
    ProjectileBehaviorSystem.CreateDefault();
  private readonly ProjectileReplicationSystem _projectileReplicationSystem = new();
  private readonly ProjectileDamageSystem _projectileDamageSystem = new();
  private readonly ProjectileTargetEligibilitySystem _projectileTargetEligibilitySystem = new();
  private readonly ProjectileHitImmunityComponent _projectileHitImmunity = new();
  private readonly QueryDescription _npcMovementQuery = new QueryDescription()
    .WithAll<NpcTagComponent, TransformComponent, VelocityComponent>();
  private readonly QueryDescription _playerMovementQuery = new QueryDescription()
    .WithAll<PlayerTagComponent, TransformComponent, VelocityComponent>();
  private readonly QueryDescription _projectileQuery = new QueryDescription()
    .WithAll<ProjectileTagComponent, TransformComponent, VelocityComponent,
      ProjectileDamageComponent, ProjectileLifetimeComponent, ProjectileDefinitionComponent,
      ProjectileBehaviorComponent, ProjectileNetworkIdentityComponent,
      ProjectilePenetrationComponent>();
  private int _nextPlayerHandle;
  private int _nextNpcHandle;
  private int _nextProjectileReplicationId = 1;
  private int _nextWorldItemReplicationId = 1;
  private int _nextChestId = 1;
  private long _nextChestMutationSequence;
  private int _nextDoorId = 1;
  private int _nextSignId;
  private int _nextTileEntityId = 1;
  private long _nextLiquidSequence;
  private long _nextWiringSequence;
  private const ushort ClosedTallGateTileType = 388;
  private const ushort ClosedTrapdoorTileType = 386;
  private const ushort OpenTallGateTileType = 389;
  private const ushort OpenTrapdoorTileType = 387;
  private bool _disposed;
  private WorldMetadata? _worldMetadata;
  private WorldRuleState _worldRules = new();
  private WorldProgressionState _worldProgression = new();
  private WorldEventRandomState _worldEventRandomState;
  private WorldTimeRateInput? _worldTimeRateInput;
  private WorldTimeRateSnapshot _worldTimeRate = WorldTimeRateSnapshot.Unavailable;
  private IReadOnlyList<string> _lastNpcPipelineSystemNames = [];

  public DomeSimulation()
    : this(new WorldGrid(4200, 1200))
  {
  }

  public DomeSimulation(WorldGrid worldGrid)
  {
    WorldGrid = worldGrid ?? throw new ArgumentNullException(nameof(worldGrid));
    _worldClock = new WorldClock();
    _nextNpcHandle = 1;
    _nextPlayerHandle = 1;
    World = Arch.Core.World.Create();
    _worldItems = new WorldItemStore(World);
    _npcSystemPipeline.ValidateRegistration();
  }

  public DomeSimulation(DomeSimulationSnapshot snapshot)
    : this(
      WorldGrid.FromSnapshot(snapshot?.World ?? throw new ArgumentNullException(nameof(snapshot))),
      snapshot)
  {
  }

  public DomeSimulation(WorldGrid worldGrid, DomeSimulationSnapshot snapshot)
    : this(worldGrid)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    _worldClock.Restore(snapshot.Clock);
    _worldMetadata = snapshot.World.Metadata;
    _worldRules = snapshot.WorldRules;
    _worldProgression = snapshot.Progression;
    _worldEventRandomState = snapshot.WorldEventRandomState;
    _worldTimeRate = snapshot.WorldTimeRate;
    if (snapshot.NpcStates.Count > 0)
    {
      for (int index = 0; index < snapshot.NpcStates.Count; index++)
      {
        RestoreNpc(snapshot.NpcStates[index]);
      }
    }
    else
    {
      for (int index = 0; index < snapshot.Npcs.Count; index++)
      {
        RestoreNpc(snapshot.Npcs[index]);
      }
    }

    for (int index = 0; index < snapshot.WorldItems.Count; index++)
    {
      ItemReplicationSnapshot item = snapshot.WorldItems[index];
      if (item.ReplicationId == int.MaxValue)
      {
        throw new ArgumentOutOfRangeException(
          nameof(snapshot),
          "Persistence snapshot world-item replication IDs must leave room for the next ID.");
      }

      ItemWorldStateComponent worldState = item.WorldState.Revision == 0 && item.Revision > 0
        ? ItemWorldStateComponent.FromReplicationSnapshot(item.IsActive, item.Revision)
        : item.WorldState;
      _worldItems.Add(new WorldItemComponent(
        item.ReplicationId,
        item.Stack,
        item.Position,
        item.IsActive,
        item.Revision,
        item.Section,
        worldState,
        InstanceState: item.InstanceState));
      _nextWorldItemReplicationId = Math.Max(
        _nextWorldItemReplicationId,
        item.ReplicationId + 1);
    }

    for (int index = 0; index < snapshot.PlayerAccounts.Count; index++)
    {
      PlayerPersistentState account = snapshot.PlayerAccounts[index];
      if (!_playerAccounts.TryAdd(account.Uuid, account))
      {
        throw new ArgumentException("Persistence snapshot contains duplicate player UUIDs.", nameof(snapshot));
      }
    }

    for (int index = 0; index < snapshot.Chests.Count; index++)
    {
      RestoreChest(snapshot.Chests[index]);
    }

    for (int index = 0; index < snapshot.Signs.Count; index++)
    {
      RestoreSign(snapshot.Signs[index]);
    }

    for (int index = 0; index < snapshot.TileEntities.Count; index++)
    {
      TileEntityPersistentState entity = snapshot.TileEntities[index];
      if (entity.Id == int.MaxValue)
      {
        throw new ArgumentOutOfRangeException(
          nameof(snapshot),
          "Persistence snapshot tile-entity IDs must leave room for the next ID.");
      }

      if (!_tileEntities.TryAdd(entity.Id, entity))
      {
        throw new ArgumentException(
          "Persistence snapshot contains duplicate tile entity IDs.",
          nameof(snapshot));
      }

      _nextTileEntityId = Math.Max(_nextTileEntityId, entity.Id + 1);

      TryRestoreTrainingDummyOwnership(entity);
    }
  }

  public Arch.Core.World World { get; }
  public WorldGrid WorldGrid { get; }
  public int NpcCount => _npcReplications.Count;
  public long TickNumber => _worldClock.TickNumber;
  public double TimeOfDay => _worldClock.TimeOfDay;
  public bool IsDayTime => _worldClock.IsDayTime;
  public byte MoonPhase => _worldClock.MoonPhase;
  public IReadOnlyList<string> LastNpcPipelineSystemNames => _lastNpcPipelineSystemNames;
  public IReadOnlyList<string> NpcPipelineSystemNames => _npcSystemPipeline.SystemNames;
  public IReadOnlyList<SimulationTickPhase> LastTickPhases => _lastTickPhases;
  public SimulationSnapshot? LastPublishedSnapshot => _lastPublishedSnapshot;

  public WorldProgressionState CreateWorldProgressionSnapshot()
  {
    ThrowIfDisposed();
    return _worldProgression;
  }

  public WorldTimeRateSnapshot CreateWorldTimeRateSnapshot()
  {
    ThrowIfDisposed();
    return _worldTimeRate;
  }

  public bool TryQueueWorldEvent(WorldEventStartCommand command)
  {
    ThrowIfDisposed();
    if (!command.IsValid || !CanQueueWorldEvent(command.Kind))
    {
      return false;
    }

    for (int index = 0; index < _worldEventStartRequests.Count; index++)
    {
      if (_worldEventStartRequests[index].Kind == command.Kind)
      {
        return false;
      }
    }

    _worldEventStartRequests.Add(command);
    return true;
  }

  public bool TryQueueNpcGameEventFirstClear(NpcGameEventFirstClearCommand command)
  {
    ThrowIfDisposed();
    if (!command.IsValid || !command.SchedulesLanternNight)
    {
      return false;
    }

    return TryQueueWorldLanternNightSchedule(new WorldLanternNightScheduleCommand(command.Sequence));
  }

  private bool TryQueueWorldLanternNightSchedule(WorldLanternNightScheduleCommand command)
  {
    if (!command.IsValid || _worldProgression.IsNextNightLanternNight ||
        _worldLanternNightScheduleRequests.Count != 0)
    {
      return false;
    }

    _worldLanternNightScheduleRequests.Add(command);
    return true;
  }

  public bool TryQueueWorldInvasion(WorldInvasionStartCommand command)
  {
    ThrowIfDisposed();
    if (!command.IsValid || _worldProgression.InvasionType != 0 ||
        _worldProgression.InvasionSize != 0 || _worldInvasionStartRequests.Count != 0)
    {
      return false;
    }

    _worldInvasionStartRequests.Add(command);
    return true;
  }

  public bool TryQueueWorldInvasion(
    WorldInvasionStartCommand command,
    IReadOnlyList<WorldInvasionPlayerSnapshot> players)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(players);
    if (!_worldInvasionStartEligibilitySystem.CanStart(command.Type, players))
    {
      return false;
    }

    return TryQueueWorldInvasion(command);
  }

  public bool TryQueueWorldInvasion(
    int invasionType,
    long sequence,
    IReadOnlyList<WorldInvasionPlayerSnapshot> players)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(players);
    if (!_worldInvasionStartEligibilitySystem.CanStart(invasionType, players))
    {
      return false;
    }

    int qualifiedPlayers = _worldInvasionStartEligibilitySystem.CountQualifiedPlayers(players);
    int size = new WorldInvasionSizeSystem().Resolve(invasionType, qualifiedPlayers);
    return TryQueueWorldInvasion(new WorldInvasionStartCommand(invasionType, size, sequence));
  }

  public bool TryQueueWorldInvasionProgress(WorldInvasionProgressCommand command)
  {
    ThrowIfDisposed();
    if (!command.IsValid || _worldProgression.InvasionType == 0 ||
        _worldProgression.InvasionSize == 0 ||
        HasPendingWorldInvasionProgressSequence(command.Sequence))
    {
      return false;
    }

    _worldInvasionProgressRequests.Add(command);
    return true;
  }

  private bool HasPendingWorldInvasionProgressSequence(long sequence)
  {
    for (int index = 0; index < _worldInvasionProgressRequests.Count; index++)
    {
      if (_worldInvasionProgressRequests[index].Sequence == sequence)
      {
        return true;
      }
    }

    return false;
  }

  public bool TryQueueWorldRain(WorldRainStartCommand command)
  {
    ThrowIfDisposed();
    if (!command.IsValid || _worldRules.IsRaining || _worldProgression.IsLanternNight ||
        _worldRainStartRequests.Count > 0 || _worldProgression.IsSlimeRaining ||
        _worldSlimeRainStartRequests.Count > 0)
    {
      return false;
    }

    _worldRainStartRequests.Add(command);
    return true;
  }

  public bool TryQueueWorldSlimeRain(WorldSlimeRainStartCommand command)
  {
    ThrowIfDisposed();
    if (_worldMetadata is not null &&
        !_worldSlimeRainEligibilitySystem.Evaluate(_worldMetadata).IsEligible)
    {
      return false;
    }

    if (!command.IsValid || _worldRules.IsRaining || _worldRainStartRequests.Count > 0 ||
        _worldProgression.IsSlimeRaining || _worldProgression.IsSlimeRainCoolingDown ||
        _worldSlimeRainStartRequests.Count > 0)
    {
      return false;
    }

    _worldSlimeRainStartRequests.Add(command);
    return true;
  }

  public bool TryQueueWorldSlimeRainStop(WorldSlimeRainStopCommand command)
  {
    ThrowIfDisposed();
    if (!command.IsValid || !_worldProgression.IsSlimeRaining ||
        _worldSlimeRainStopRequests.Count > 0)
    {
      return false;
    }

    _worldSlimeRainStopRequests.Add(command);
    return true;
  }

  public bool TryQueueWorldWind(WorldWindChangeCommand command)
  {
    ThrowIfDisposed();
    if (!command.IsValid || _worldWindChangeRequests.Count > 0)
    {
      return false;
    }

    _worldWindChangeRequests.Add(command);
    return true;
  }

  public bool TryQueueWorldMeteorImpact(WorldMeteorImpactCommand command)
  {
    ThrowIfDisposed();
    if (!command.IsValid || _worldProgression.IsMeteorScheduled ||
        _worldMeteorImpactRequests.Count > 0 || _scheduledMeteorImpactRequests.Count > 0)
    {
      return false;
    }

    if (!_worldMeteorImpactSystem.TryCreateCommands(
          WorldGrid,
          command,
          CreateMeteorOccupants(),
          firstSequence: 0,
          out _,
          out _))
    {
      return false;
    }

    _worldMeteorImpactRequests.Add(command);
    return true;
  }

  public bool TryQueueWorldMeteorSchedule(WorldMeteorScheduleCommand command)
  {
    ThrowIfDisposed();
    if (!command.IsValid || _worldClock.IsDayTime ||
        !_worldProgression.DefeatedEaterOrBrain ||
        _worldProgression.IsMeteorScheduled || _worldMeteorScheduleRequests.Count > 0)
    {
      return false;
    }

    _worldMeteorScheduleRequests.Add(command);
    return true;
  }

  public bool TryQueueScheduledWorldMeteorImpact(WorldMeteorImpactCommand command)
  {
    ThrowIfDisposed();
    if (!command.IsValid || !_worldClock.IsDayTime ||
        _worldClock.TimeOfDay <= WorldMeteorScheduleSystem.MeteorScheduleCutoffTime ||
        !_worldProgression.IsMeteorScheduled || _scheduledMeteorImpactRequests.Count > 0 ||
        _worldMeteorImpactRequests.Count > 0)
    {
      return false;
    }

    if (!_worldMeteorImpactSystem.TryCreateCommands(
          WorldGrid,
          command,
          CreateMeteorOccupants(),
          firstSequence: 0,
          out _,
          out _))
    {
      return false;
    }

    _scheduledMeteorImpactRequests.Add(command);
    return true;
  }

  private bool CanQueueWorldEvent(WorldEventKind kind)
  {
    return kind switch
    {
      WorldEventKind.BloodMoon => !_worldClock.IsDayTime && !_worldProgression.IsBloodMoon,
      WorldEventKind.Eclipse => _worldClock.IsDayTime && _worldProgression.IsHardMode &&
        _worldProgression.DefeatedMechanicalBoss && !_worldProgression.IsEclipse,
      WorldEventKind.LanternNight => !_worldClock.IsDayTime && !_worldProgression.IsLanternNight,
      _ => false
    };
  }

  private bool HasPendingLanternNightStart()
  {
    for (int index = 0; index < _worldEventStartRequests.Count; index++)
    {
      WorldEventStartCommand request = _worldEventStartRequests[index];
      if (request.IsValid && request.Kind == WorldEventKind.LanternNight)
      {
        return true;
      }
    }

    return false;
  }

  public void SetWorldTimePaused(bool isPaused)
  {
    ThrowIfDisposed();
    _worldClock.SetPaused(isPaused);
  }

  public void ConfigureWorldTimeRate(WorldTimeRateInput input)
  {
    ThrowIfDisposed();
    _worldTimeRateInput = input;
  }

  public bool TryQueueLiquidSource(PipelineLiquidSourceComponent source)
  {
    ThrowIfDisposed();
    if (source.Sequence >= long.MaxValue - 1)
    {
      return false;
    }

    bool accepted = _liquidInputSystem.TryAccept(WorldGrid, _liquidQueue, _liquidState, source);
    if (accepted)
    {
      _nextLiquidSequence = Math.Max(_nextLiquidSequence, source.Sequence + 1);
    }

    return accepted;
  }

  public IReadOnlyList<LiquidReplicationSnapshot> CreateLiquidReplicationSnapshots()
  {
    ThrowIfDisposed();
    return _liquidReplicationSystem.CreateRevisionedSnapshots(_lastLiquidCommands, TickNumber);
  }

  public int GetLiquidRetryCount(int x, int y)
  {
    ThrowIfDisposed();
    if (!WorldGrid.Contains(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    return _liquidQueue.GetRetryCount(x, y);
  }

  public void SetWireMask(int x, int y, byte mask)
  {
    ThrowIfDisposed();
    if (!WorldGrid.Contains(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    _wireNetwork.SetMask(x, y, mask);
  }

  public bool TryRegisterWiringTrigger(int x, int y, TriggerComponent trigger)
  {
    ThrowIfDisposed();
    if (!WorldGrid.Contains(x, y) || trigger.TriggerId <= 0 ||
        !_mechanisms.ContainsKey(trigger.TargetMechanismId) ||
        _wireTriggers.ContainsKey((x, y)))
    {
      return false;
    }

    _wireTriggers.Add((x, y), trigger);
    return true;
  }

  public bool TryRegisterMechanism(
    MechanismComponent mechanism,
    ActuatorComponent? actuator = null,
    PumpComponent? pump = null,
    LampComponent? lamp = null)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(mechanism);
    if (_mechanisms.ContainsKey(mechanism.MechanismId) ||
        actuator is not null && actuator.MechanismId != mechanism.MechanismId ||
        pump.HasValue && pump.Value.MechanismId != mechanism.MechanismId ||
        lamp is not null && lamp.MechanismId != mechanism.MechanismId ||
        mechanism.Type == MechanismType.Lamp && lamp is null ||
        mechanism.Type != MechanismType.Lamp && lamp is not null)
    {
      return false;
    }

    _mechanisms.Add(mechanism.MechanismId, mechanism);
    if (actuator is not null)
    {
      _actuators.Add(mechanism.MechanismId, actuator);
    }

    if (pump is PumpComponent pumpValue)
    {
      _pumps.Add(mechanism.MechanismId, pumpValue);
    }

    if (lamp is not null)
    {
      _lamps.Add(mechanism.MechanismId, lamp);
    }

    return true;
  }

  public bool TryRegisterPressurePlate(PressurePlateComponent plate)
  {
    ThrowIfDisposed();
    if (!_mechanisms.ContainsKey(plate.MechanismId) || !WorldGrid.Contains(plate.X, plate.Y))
    {
      return false;
    }

    _pressurePlates.Add(plate);
    return true;
  }

  public bool TryRegisterLogicGate(LogicGateComponent gate)
  {
    ThrowIfDisposed();
    if (gate.MechanismId <= 0 || gate.OutputMechanismId <= 0 || gate.Inputs.Count == 0 ||
        !_mechanisms.ContainsKey(gate.OutputMechanismId) ||
        _logicGateOutputs.ContainsKey(gate.MechanismId))
    {
      return false;
    }

    _logicGates.Add(gate);
    _logicGateOutputs.Add(gate.MechanismId, false);
    return true;
  }

  public bool TryRegisterDoorMechanism(int mechanismId, int doorId)
  {
    ThrowIfDisposed();
    if (!_mechanisms.TryGetValue(mechanismId, out MechanismComponent? mechanism) ||
        mechanism.Type != MechanismType.Door || !_doors.ContainsKey(doorId) ||
        !_doorMechanisms.TryAdd(mechanismId, doorId))
    {
      return false;
    }

    return true;
  }

  public bool TryQueueWiringInput(WiringInputCommand command)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(command.Player, out Entity entity) ||
        command.Sequence < 0 || command.Sequence >= long.MaxValue - 1 ||
        !_wiringInputValidationSystem.TryValidate(
          WorldGrid,
          _wireNetwork,
          command,
          maximumRadius: 6))
    {
      return false;
    }

    TransformComponent transform = World.Get<TransformComponent>(entity);
    float deltaX = transform.X - command.X;
    float deltaY = transform.Y - command.Y;
    if (deltaX * deltaX + deltaY * deltaY > 6.0f * 6.0f)
    {
      return false;
    }

    _wiringInputs.Add(command);
    _nextWiringSequence = Math.Max(_nextWiringSequence, command.Sequence + 1);
    return true;
  }

  public IReadOnlyList<MechanismSnapshot> CreateMechanismSnapshots()
  {
    ThrowIfDisposed();
    return _wiringEventProjectionSystem.CreateSnapshots(_mechanisms);
  }

  public WorldRuleSnapshot CreateWorldRuleSnapshot()
  {
    ThrowIfDisposed();
    return new WorldRuleSnapshot(TickNumber, TimeOfDay, IsDayTime);
  }

  public WorldRuleState CreateWorldRuleState()
  {
    ThrowIfDisposed();
    return _worldRules;
  }

  public int CreateDoor(int tileX, int tileY)
  {
    ThrowIfDisposed();
    WorldSectionCoordinates section = WorldGrid.GetSectionCoordinates(tileX, tileY);
    int doorId = NextDoorId();
    _doors.Add(doorId, new DoorSnapshot(doorId, tileX, tileY, false, 1, section));
    return doorId;
  }

  public int CreateTrapdoor(int tileX, int tileY, bool opensDown)
  {
    ThrowIfDisposed();
    int frameXOffset = opensDown ? 0 : 36;
    for (int row = 0; row < 2; row++)
    {
      for (int column = 0; column < 2; column++)
      {
        if (!WorldGrid.TrySetTile(tileX + column, tileY + row, new WorldTile(
              true,
              ClosedTrapdoorTileType,
              FrameX: (short)(frameXOffset + column * 18),
              FrameY: (short)(row * 18))))
        {
          throw new ArgumentOutOfRangeException(nameof(tileX));
        }
      }
    }

    WorldSectionCoordinates section = WorldGrid.GetSectionCoordinates(tileX, tileY);
    int doorId = NextDoorId();
    _doors.Add(doorId, new DoorSnapshot(
      doorId,
      tileX,
      tileY,
      false,
      1,
      section,
      DoorObjectKind.Trapdoor));
    return doorId;
  }

  public int CreateTallGate(int tileX, int tileY)
  {
    ThrowIfDisposed();
    for (int row = 0; row < 5; row++)
    {
      short frameY = (short)(row == 0 ? 0 : row * 18 + 2);
      if (!WorldGrid.TrySetTile(tileX, tileY + row, new WorldTile(
            true,
            ClosedTallGateTileType,
            FrameY: frameY)))
      {
        throw new ArgumentOutOfRangeException(nameof(tileX));
      }
    }

    WorldSectionCoordinates section = WorldGrid.GetSectionCoordinates(tileX, tileY);
    int doorId = NextDoorId();
    _doors.Add(doorId, new DoorSnapshot(
      doorId,
      tileX,
      tileY,
      false,
      1,
      section,
      DoorObjectKind.TallGate));
    return doorId;
  }

  public IReadOnlyList<DoorSnapshot> CreateDoorSnapshots()
  {
    ThrowIfDisposed();
    List<DoorSnapshot> snapshots = new(_doors.Count);
    for (int doorId = 1; doorId < _nextDoorId; doorId++)
    {
      if (_doors.TryGetValue(doorId, out DoorSnapshot door))
      {
        snapshots.Add(door);
      }
    }

    return snapshots;
  }

  private int NextDoorId()
  {
    if (_nextDoorId == int.MaxValue)
    {
      throw new InvalidOperationException("Door ID allocator was exhausted.");
    }

    int doorId = _nextDoorId;
    _nextDoorId++;
    return doorId;
  }

  public DoorSnapshot? FindDoorAt(int tileX, int tileY)
  {
    ThrowIfDisposed();
    for (int doorId = 1; doorId < _nextDoorId; doorId++)
    {
      if (_doors.TryGetValue(doorId, out DoorSnapshot door) &&
          IsDoorAt(door, tileX, tileY))
      {
        return door;
      }
    }

    return null;
  }

  public bool TryToggleDoor(int doorId, SimulationVector playerPosition)
  {
    ThrowIfDisposed();
    if (!_doors.TryGetValue(doorId, out DoorSnapshot door) || door.Revision == long.MaxValue)
    {
      return false;
    }

    float deltaX = playerPosition.X - door.TileX;
    float deltaY = playerPosition.Y - door.TileY;
    if (!float.IsFinite(playerPosition.X) || !float.IsFinite(playerPosition.Y) ||
        deltaX * deltaX + deltaY * deltaY > 6.0f * 6.0f)
    {
      return false;
    }

    _doors[doorId] = door with { IsOpen = !door.IsOpen, Revision = door.Revision + 1 };
    return true;
  }

  public bool TrySetDoorOpen(int doorId, bool isOpen, SimulationVector playerPosition)
  {
    ThrowIfDisposed();
    if (!_doors.TryGetValue(doorId, out DoorSnapshot door) || door.Revision == long.MaxValue)
    {
      return false;
    }

    float deltaX = playerPosition.X - door.TileX;
    float deltaY = playerPosition.Y - door.TileY;
    if (!float.IsFinite(playerPosition.X) || !float.IsFinite(playerPosition.Y) ||
        deltaX * deltaX + deltaY * deltaY > 6.0f * 6.0f)
    {
      return false;
    }

    if (door.IsOpen == isOpen)
    {
      return true;
    }

    _doors[doorId] = door with { IsOpen = isOpen, Revision = door.Revision + 1 };
    return true;
  }

  public bool TryApplyDoorTransition(
    int doorId,
    DoorTransition transition,
    bool direction,
    SimulationVector playerPosition)
  {
    ThrowIfDisposed();
    if (!_doors.TryGetValue(doorId, out DoorSnapshot door) ||
        door.Revision == long.MaxValue || !IsInDoorRange(door, playerPosition))
    {
      return false;
    }

    bool applied = door.ObjectKind switch
    {
      DoorObjectKind.Door => ApplyDoorTransition(door, transition),
      DoorObjectKind.Trapdoor => ApplyTrapdoorTransition(door, transition, direction),
      DoorObjectKind.TallGate => ApplyTallGateTransition(door, transition),
      _ => false
    };
    if (!applied)
    {
      return false;
    }

    return true;
  }

  public int CreateSign(int tileX, int tileY, string text)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(text);
    if (text.Length > 100)
    {
      throw new ArgumentOutOfRangeException(nameof(text));
    }

    if (_nextSignId >= MaximumSignCount)
    {
      throw new InvalidOperationException("The V1456 sign capacity was exceeded.");
    }

    WorldSectionCoordinates section = WorldGrid.GetSectionCoordinates(tileX, tileY);
    int signId = _nextSignId++;
    _signs.Add(signId, new SignSnapshot(signId, tileX, tileY, text, 1, section));
    return signId;
  }

  public IReadOnlyList<SignSnapshot> CreateSignSnapshots()
  {
    ThrowIfDisposed();
    List<SignSnapshot> snapshots = new(_signs.Count);
    for (int signId = 0; signId < _nextSignId; signId++)
    {
      if (_signs.TryGetValue(signId, out SignSnapshot sign))
      {
        snapshots.Add(sign);
      }
    }

    return snapshots;
  }

  public IReadOnlyList<SignPersistentState> CreateSignPersistentSnapshots()
  {
    ThrowIfDisposed();
    List<SignPersistentState> snapshots = new(_signs.Count);
    foreach (SignSnapshot sign in CreateSignSnapshots())
    {
      snapshots.Add(new SignPersistentState(
        sign.SignId,
        sign.TileX,
        sign.TileY,
        sign.Text,
        sign.Revision));
    }

    return snapshots;
  }

  public bool TryUpdateSign(int signId, SimulationVector playerPosition, string text)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(text);
    if (!_signs.TryGetValue(signId, out SignSnapshot sign) || text.Length > 100 ||
        sign.Revision == long.MaxValue)
    {
      return false;
    }

    float deltaX = playerPosition.X - sign.TileX;
    float deltaY = playerPosition.Y - sign.TileY;
    if (!float.IsFinite(playerPosition.X) || !float.IsFinite(playerPosition.Y) ||
        deltaX * deltaX + deltaY * deltaY > 6.0f * 6.0f)
    {
      return false;
    }

    _signs[signId] = sign with { Text = text, Revision = sign.Revision + 1 };
    return true;
  }

  public int CreateChest(int tileX, int tileY)
  {
    ThrowIfDisposed();
    if (tileX < 0 || tileX >= WorldGrid.Width || tileY < 0 || tileY >= WorldGrid.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(tileX));
    }

    if (_nextChestId == int.MaxValue)
    {
      throw new InvalidOperationException("Chest ID allocator was exhausted.");
    }

    int chestId = _nextChestId;
    ChestCreateCommand command = new(
      NextChestMutationSequence(),
      chestId,
      tileX,
      tileY);
    if (!_chestMutationCommitSystem.TryCreate(
          WorldGrid,
          _chests,
          _chestIndexSystem,
          command,
          out _))
    {
      throw new InvalidOperationException("Chest placement was rejected by authoritative state.");
    }

    _nextChestId++;
    return chestId;
  }

  public bool TryGetChestIdAt(int tileX, int tileY, out int chestId)
  {
    ThrowIfDisposed();
    return _chestIndexSystem.TryGetChestId(tileX, tileY, out chestId);
  }

  public bool TryDestroyChest(int chestId)
  {
    ThrowIfDisposed();
    return _chestMutationCommitSystem.TryDestroy(
      _chests,
      _chestIndexSystem,
      new ChestDestroyCommand(NextChestMutationSequence(), chestId));
  }

  public ChestComponent GetChest(int chestId)
  {
    ThrowIfDisposed();
    if (!_chests.TryGetValue(chestId, out ChestComponent? chest))
    {
      throw new ArgumentOutOfRangeException(nameof(chestId));
    }

    return chest;
  }

  public void SetChestItem(int chestId, int chestSlot, ItemStack stack)
  {
    ThrowIfDisposed();
    ChestComponent chest = GetChest(chestId);
    if (!stack.IsEmpty)
    {
      _ = _itemDefinitions.Get(stack.ItemType);
    }

    _ = chest.GetSlot(chestSlot);

    if (!chest.TryIncrementRevision())
    {
      return;
    }

    chest.SetSlot(chestSlot, stack);
  }

  public IReadOnlyList<ChestSnapshot> CreateChestSnapshots()
  {
    ThrowIfDisposed();
    List<ChestSnapshot> snapshots = new(_chests.Count);
    foreach (KeyValuePair<int, ChestComponent> entry in _chests)
    {
      ChestComponent chest = entry.Value;
      ItemStack[] slots = new ItemStack[ChestComponent.SlotCount];
      for (int index = 0; index < slots.Length; index++)
      {
        slots[index] = chest.GetSlot(index);
      }

      snapshots.Add(new ChestSnapshot(
        chest.ChestId,
        chest.TileX,
        chest.TileY,
        chest.Opener,
        slots,
        chest.Revision,
        chest.Section,
        chest.IsLocked));
    }

    return snapshots;
  }

  public IReadOnlyList<ChestPersistentState> CreateChestPersistentSnapshots()
  {
    ThrowIfDisposed();
    List<ChestPersistentState> snapshots = new(_chests.Count);
    foreach (ChestSnapshot chest in CreateChestSnapshots())
    {
      snapshots.Add(new ChestPersistentState(
        chest.ChestId,
        chest.TileX,
        chest.TileY,
        chest.Slots,
        chest.Revision,
        _chests[chest.ChestId].Name,
        chest.IsLocked));
    }

    return snapshots;
  }

  public IReadOnlyList<TileEntityPersistentState> CreateTileEntitySnapshots()
  {
    ThrowIfDisposed();
    List<TileEntityPersistentState> snapshots = new(_tileEntities.Count);
    foreach (TileEntityPersistentState entity in _tileEntities.Values)
    {
      if (_trainingDummyOwnerships.TryGetValue(
            entity.Id,
            out TrainingDummyOwnershipState ownership) &&
          TrainingDummyPersistenceProjection.TryProject(
            entity,
            ownership,
            out TileEntityPersistentState projected))
      {
        snapshots.Add(projected);
      }
      else
      {
        snapshots.Add(entity);
      }
    }

    return snapshots.OrderBy(entity => entity.Id).ToArray();
  }

  public bool TryPlaceTrainingDummy(int tileX, int tileY, out int entityId)
  {
    ThrowIfDisposed();
    entityId = 0;
    if (!WorldGrid.Contains(tileX, tileY) ||
        !TileEntityTrainingDummyValidityQuery.IsValid(WorldGrid.GetTile(tileX, tileY)) ||
        _tileEntities.Values.Any(entity => entity.TileX == tileX && entity.TileY == tileY) ||
        _nextTileEntityId == int.MaxValue)
    {
      return false;
    }

    entityId = _nextTileEntityId++;
    _tileEntities.Add(entityId, new TileEntityPersistentState(
      entityId,
      0,
      tileX,
      tileY,
      [0xFF, 0xFF],
      isOpaque: false));
    return true;
  }

  public bool TryRemoveTrainingDummy(int tileX, int tileY)
  {
    ThrowIfDisposed();
    int entityId = _tileEntities.Values
      .Where(entity => entity.Type == 0 && entity.TileX == tileX && entity.TileY == tileY)
      .Select(entity => entity.Id)
      .FirstOrDefault();
    if (entityId <= 0 || !_tileEntities.Remove(entityId))
    {
      return false;
    }

    if (_trainingDummyOwnerships.Remove(entityId, out TrainingDummyOwnershipState ownership) &&
        ownership.Npc.IsValid)
    {
      _commands.Enqueue(new DespawnNpcCommand(
        ownership.Npc,
        NpcComponents.NpcDespawnReason.OutOfRange));
    }

    return true;
  }

  public bool TryLinkTrainingDummyNpc(
    int entityId,
    NpcHandle npc,
    TrainingDummyNpcLinkSnapshot npcSnapshot,
    out long revision)
  {
    ThrowIfDisposed();
    revision = 0;
    if (_trainingDummyOwnerships.TryGetValue(entityId, out TrainingDummyOwnershipState existing) &&
        existing.Npc.IsValid)
    {
      return false;
    }

    if (!_tileEntities.TryGetValue(entityId, out TileEntityPersistentState? persistent) ||
        persistent is null)
    {
      return false;
    }

    if (!TrainingDummyTileEntityState.TryRead(
          persistent,
          out TrainingDummyTileEntityState entity) ||
        !_npcReplications.TryGetValue(npc, out NpcReplicationSnapshot replication) ||
        !replication.IsActive ||
        replication.NpcType != 488)
    {
      return false;
    }

    if (!TrainingDummyOwnershipState.TryLink(
          entity,
          npc,
          npcSnapshot,
          out TrainingDummyOwnershipState ownership))
    {
      return false;
    }

    _trainingDummyOwnerships[entityId] = ownership;
    revision = ownership.Revision;
    return true;
  }

  public bool TryClearTrainingDummyNpc(
    int entityId,
    TrainingDummyNpcLinkSnapshot npcSnapshot,
    out long revision)
  {
    ThrowIfDisposed();
    revision = 0;
    if (!_tileEntities.TryGetValue(entityId, out TileEntityPersistentState? persistent) ||
        persistent is null ||
        !_trainingDummyOwnerships.TryGetValue(entityId, out TrainingDummyOwnershipState current))
    {
      return false;
    }

    if (!TrainingDummyTileEntityState.TryRead(
          persistent,
          out TrainingDummyTileEntityState entity) ||
        !TrainingDummyOwnershipState.TryClear(
          current,
          entity,
          npcSnapshot,
          out TrainingDummyOwnershipState cleared))
    {
      return false;
    }

    _trainingDummyOwnerships[entityId] = cleared;
    revision = cleared.Revision;
    return true;
  }

  private void TryRestoreTrainingDummyOwnership(TileEntityPersistentState persistent)
  {
    if (!TrainingDummyTileEntityState.TryRead(
          persistent,
          out TrainingDummyTileEntityState entity) ||
        entity.NpcId < 0)
    {
      return;
    }

    NpcHandle npcHandle = new(entity.NpcId);
    if (!_npcReplications.TryGetValue(npcHandle, out NpcReplicationSnapshot npc) ||
        npc.Position.X != entity.TileX ||
        npc.Position.Y != entity.TileY)
    {
      return;
    }

    TrainingDummyNpcLinkSnapshot link = new(
      npc.IsActive,
      npc.NpcType,
      entity.TileX,
      entity.TileY);
    if (TrainingDummyOwnershipState.TryRestore(
          entity,
          npcHandle,
          link,
          out TrainingDummyOwnershipState ownership))
    {
      _trainingDummyOwnerships[entity.EntityId] = ownership;
    }
  }

  private void EvaluateTrainingDummyLifecycle(IReadOnlyList<Entity> activePlayers)
  {
    _pendingTrainingDummyActivations.Clear();
    List<TrainingDummyPlayerHitboxSnapshot> players = new(activePlayers.Count);
    for (int index = 0; index < activePlayers.Count; index++)
    {
      Entity player = activePlayers[index];
      TransformComponent transform = World.Get<TransformComponent>(player);
      ColliderComponent collider = World.Get<ColliderComponent>(player);
      players.Add(new TrainingDummyPlayerHitboxSnapshot(
        IsActive: true,
        X: checked((int)MathF.Floor(transform.X)),
        Y: checked((int)MathF.Floor(transform.Y)),
        Width: checked((int)MathF.Ceiling(collider.Width)),
        Height: checked((int)MathF.Ceiling(collider.Height))));
    }

    int activeNpcCount = _npcReplications.Values.Count(npc => npc.IsActive);
    foreach (TileEntityPersistentState persistent in _tileEntities.Values.OrderBy(entity => entity.Id))
    {
      if (!TrainingDummyTileEntityState.TryRead(
            persistent,
            out TrainingDummyTileEntityState entity) ||
          !TileEntityTrainingDummyValidityQuery.IsValid(
            WorldGrid.GetTile(entity.TileX, entity.TileY)))
      {
        continue;
      }

      if (_trainingDummyOwnerships.TryGetValue(
            entity.EntityId,
            out TrainingDummyOwnershipState ownership) &&
          ownership.Npc.IsValid)
      {
        if (!_npcReplications.TryGetValue(ownership.Npc, out NpcReplicationSnapshot npc))
        {
          continue;
        }

        TrainingDummyNpcLinkSnapshot link = new(
          npc.IsActive,
          npc.NpcType,
          entity.TileX,
          entity.TileY);
        if (TrainingDummyDeactivationDecisionQuery.ShouldDeactivate(entity, link) &&
            TryClearTrainingDummyNpc(entity.EntityId, link, out _))
        {
          _commands.Enqueue(new DespawnNpcCommand(
            ownership.Npc,
            NpcComponents.NpcDespawnReason.OutOfRange));
        }

        continue;
      }

      TrainingDummyActivationDecisionResult decision =
        TrainingDummyActivationDecisionQuery.Evaluate(
          entity,
          players,
          activeNpcCount >= MaximumNpcCount);
      if (!decision.ShouldActivate)
      {
        continue;
      }

      _pendingTrainingDummyActivations.Add(entity.EntityId);
      _commands.Enqueue(new SpawnNpcCommand(
        DefinitionId: TrainingDummyNpcType,
        Position: new SimulationVector(entity.TileX, entity.TileY),
        Source: NpcComponents.NpcSpawnSource.TileEntity));
      activeNpcCount++;
    }
  }

  public bool TryOpenChest(int chestId, PlayerHandle player, SimulationVector playerPosition)
  {
    ThrowIfDisposed();
    if (!_chests.TryGetValue(chestId, out ChestComponent? chest) ||
        !_chestOpenSystem.TryApply(
          chest,
          new ChestOpenCommand(TickNumber, chestId, player, playerPosition),
          GetInventory(player)))
    {
      return false;
    }

    return true;
  }

  public void CloseChest(PlayerHandle player)
  {
    foreach (KeyValuePair<int, ChestComponent> entry in _chests)
    {
      if (entry.Value.Opener != player)
      {
        continue;
      }

      _ = _chestCloseSystem.TryApply(
        entry.Value,
        new ChestCloseCommand(TickNumber, entry.Key, player));
    }
  }

  public bool TryTransferChestItem(
    int chestId,
    PlayerHandle player,
    int inventorySlot,
    int chestSlot,
    bool withdraw,
    SimulationVector playerPosition,
    long expectedRevision = -1)
  {
    ThrowIfDisposed();
    if (!_chests.TryGetValue(chestId, out ChestComponent? chest) || chest.Opener != player ||
        (expectedRevision >= 0 && chest.Revision != expectedRevision) ||
        !_players.TryGetValue(player, out Entity playerEntity) ||
        !World.Has<InventoryComponent>(playerEntity) ||
        !IsChestInRange(chest, playerPosition))
    {
      return false;
    }

    InventoryComponent inventory = World.Get<InventoryComponent>(playerEntity);
    ItemStack inventoryItem = inventory.GetSlot(inventorySlot);
    ItemStack chestItem = chest.GetSlot(chestSlot);
    ItemStack source = withdraw ? chestItem : inventoryItem;
    ItemStack destination = withdraw ? inventoryItem : chestItem;
    if (source.IsEmpty || !destination.IsEmpty)
    {
      return false;
    }

    if (!chest.TryIncrementRevision())
    {
      return false;
    }

    ItemInstanceSnapshot[] inventoryBefore = CaptureInventorySlots(inventory);
    if (withdraw)
    {
      chest.SetSlot(chestSlot, ItemStack.Empty);
      inventory.SetSlot(inventorySlot, source);
    }
    else
    {
      inventory.SetSlot(inventorySlot, ItemStack.Empty);
      chest.SetSlot(chestSlot, source);
    }

    PublishInventoryChanges(player, inventory, inventoryBefore);
    return true;
  }

  private static bool IsChestInRange(ChestComponent chest, SimulationVector playerPosition)
  {
    float deltaX = playerPosition.X - chest.TileX;
    float deltaY = playerPosition.Y - chest.TileY;
    return float.IsFinite(playerPosition.X) && float.IsFinite(playerPosition.Y) &&
      deltaX * deltaX + deltaY * deltaY <= 6.0f * 6.0f;
  }

  private long NextChestMutationSequence()
  {
    if (_nextChestMutationSequence == long.MaxValue)
    {
      throw new InvalidOperationException("Chest mutation sequence was exhausted.");
    }

    long sequence = _nextChestMutationSequence;
    _nextChestMutationSequence++;
    return sequence;
  }

  private long NextLiquidSequence()
  {
    if (_nextLiquidSequence == long.MaxValue)
    {
      throw new InvalidOperationException("Liquid sequence was exhausted.");
    }

    long sequence = _nextLiquidSequence;
    _nextLiquidSequence++;
    return sequence;
  }

  public PlayerHandle CreatePlayer(SimulationVector spawn)
  {
    ThrowIfDisposed();

    if (_nextPlayerHandle == int.MaxValue)
    {
      throw new InvalidOperationException("Player handle allocator was exhausted.");
    }

    PlayerHandle player = new(_nextPlayerHandle);
    _nextPlayerHandle++;
    InventoryComponent inventory = new();
    Entity entity = World.Create(
      new PlayerTagComponent(),
      new PlayerIdentityComponent(
        player,
        (byte)Math.Clamp(player.Value - 1, 0, byte.MaxValue),
        string.Empty),
      new TransformComponent(spawn.X, spawn.Y),
      new VelocityComponent(0.0f, 0.0f),
      new FacingComponent(1),
      new ColliderComponent(1.0f, 2.0f),
      new PhysicsStateComponent { IsGrounded = spawn.Y <= 0.0f },
      new HealthComponent(100, 100),
      new HealthRegenerationComponent(),
      new ManaComponent(20, 20),
      new DefenseComponent(0),
      new ImmunityComponent(),
      new PlayerInputComponent(),
      new PlayerControlStateComponent(),
      new MovementIntentComponent(),
      new SelectedItemComponent(),
      new EquipmentLoadoutComponent(),
      inventory,
      new Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent(),
      new BuffCollectionComponent(),
      new WellFedStateComponent(),
      new PlayerInteractionComponent(),
    new PlayerLifecycleComponent { IsActive = true, Spawn = spawn },
      new EquipmentStateCollectionComponent());
    _players.Add(player, entity);
    return player;
  }

  public IReadOnlyList<WorldInvasionPlayerSnapshot> CreateWorldInvasionPlayerSnapshots()
  {
    ThrowIfDisposed();
    List<WorldInvasionPlayerSnapshot> snapshots = new(_players.Count);
    foreach (PlayerHandle player in _players.Keys.OrderBy(handle => handle.Value))
    {
      Entity entity = _players[player];
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entity);
      HealthComponent health = World.Get<HealthComponent>(entity);
      snapshots.Add(new WorldInvasionPlayerSnapshot(lifecycle.IsActive, health.Maximum));
    }

    return snapshots;
  }

  public PlayerHandle CreatePlayer(PlayerPersistentState account, SimulationVector spawn)
  {
    return CreatePlayer(account, spawn, (byte)Math.Clamp(_nextPlayerHandle - 1, 0, byte.MaxValue));
  }

  public PlayerHandle CreatePlayer(
    PlayerPersistentState account,
    SimulationVector spawn,
    byte assignedSlot)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(account);
    PlayerPersistentState serverAccount = ImportPlayerIfMissing(account);
    PlayerHandle player = CreatePlayer(spawn);
    Entity entity = _players[player];
    ref PlayerIdentityComponent identity = ref World.Get<PlayerIdentityComponent>(entity);
    identity.AssignedSlot = assignedSlot;
    identity.CanonicalAccountUuid = serverAccount.Uuid;
    ref HealthComponent health = ref World.Get<HealthComponent>(entity);
    health.Current = serverAccount.Life;
    health.Maximum = serverAccount.MaximumLife;
    ref ManaComponent mana = ref World.Get<ManaComponent>(entity);
    mana.Current = serverAccount.Mana;
    mana.Maximum = serverAccount.MaximumMana;
    ref EquipmentLoadoutComponent loadout = ref World.Get<EquipmentLoadoutComponent>(entity);
    _equipmentStatSystem.Apply(
      ref loadout,
      serverAccount.SelectedLoadout,
      serverAccount.AccessoryVisibility);
    BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(entity);
    for (int index = 0; index < serverAccount.Buffs.Count; index++)
    {
      buffs.Add(serverAccount.Buffs[index].Type, int.MaxValue, player);
    }

    World.Set(entity, new WellFedStateComponent(
      serverAccount.WellFedTimeLeftRank1,
      serverAccount.WellFedTimeLeftRank2,
      serverAccount.WellFedTimeLeftRank3));

    InventoryComponent inventory = World.Get<InventoryComponent>(entity);
    for (int slotId = 0; slotId < InventoryComponent.SlotCount; slotId++)
    {
      PlayerPersistentItem item = serverAccount.Items[slotId];
      if (item.ItemType > ushort.MaxValue || item.Stack <= 0 ||
          !_itemDefinitions.TryGet((ushort)item.ItemType, out ItemDefinition definition))
      {
        continue;
      }

      inventory.SetSlot(slotId, new ItemStack(
        (ushort)item.ItemType,
        Math.Min(item.Stack, definition.StackLimit)));
      inventory.SetInstanceState(
        slotId,
        new Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent(
          PrefixId: item.Prefix,
          VariantId: item.VariantId,
          Dye: item.Dye,
          Paint: item.Paint,
          IsFavorited: item.IsFavorited,
          IsNewAndShiny: item.IsNewAndShiny,
          NameOverride: item.NameOverride));
    }

    _itemInventorySanitizationSystem.Sanitize(inventory, _itemDefinitions);
    _playerAccountUuids.Add(player, serverAccount.Uuid);
    return player;
  }

  public PlayerPersistentState ImportPlayerIfMissing(PlayerPersistentState bootstrap)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(bootstrap);
    if (_playerAccounts.TryGetValue(bootstrap.Uuid, out PlayerPersistentState? existing))
    {
      return existing;
    }

    _playerAccounts.Add(bootstrap.Uuid, bootstrap);
    return bootstrap;
  }

  public bool TryGetPlayerPersistentState(string uuid, out PlayerPersistentState? account)
  {
    ThrowIfDisposed();
    ArgumentException.ThrowIfNullOrWhiteSpace(uuid);
    return _playerAccounts.TryGetValue(uuid, out account);
  }

  public NpcHandle CreateNpc(SimulationVector spawn)
  {
    return CreateNpc(spawn, DomeChaserNpcType);
  }

  public NpcHandle CreateNpc(SimulationVector spawn, int definitionId)
  {
    ThrowIfDisposed();

    if (_nextNpcHandle == int.MaxValue)
    {
      throw new InvalidOperationException("NPC handle allocator was exhausted.");
    }

    NpcHandle npc = new(_nextNpcHandle);
    _nextNpcHandle++;
    SpawnNpcCommand command = new(
      DefinitionId: definitionId,
      Position: spawn,
      Source: NpcComponents.NpcSpawnSource.Command);
    if (!_npcSpawnCommitSystem.TryCommit(
          World,
          WorldGrid,
          _npcDefinitions,
          command,
          npc.Value,
          out NpcSpawnCommitResult result,
          out string failureReason))
    {
      throw new InvalidOperationException(failureReason);
    }

    Entity entity = result.Entity;
    _npcs.Add(npc, entity);
    _npcReplications.Add(npc, new NpcReplicationSnapshot(
      npc.Value,
      definitionId,
      spawn,
      new SimulationVector(0.0f, 0.0f),
      100,
      IsActive: true,
      Revision: 1,
      GetSectionCoordinates(spawn)));
    return npc;
  }

  public void QueueNpcSpawn(SpawnNpcCommand command)
  {
    ThrowIfDisposed();
    if (command.DefinitionId <= 0 || command.DifficultyScale <= 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(command));
    }

    _commands.Enqueue(command);
  }

  public void QueueNpcDespawn(DespawnNpcCommand command)
  {
    ThrowIfDisposed();
    _commands.Enqueue(command);
  }

  public void QueueProjectileSpawn(SpawnProjectileCommand command)
  {
    ThrowIfDisposed();
    _commands.Enqueue(command);
  }

  public IReadOnlyList<NpcReplicationSnapshot> CreateNpcReplicationSnapshots()
  {
    ThrowIfDisposed();
    List<NpcReplicationSnapshot> snapshots = new(_npcReplications.Count);
    for (int replicationId = 1; replicationId < _nextNpcHandle; replicationId++)
    {
      NpcHandle handle = new(replicationId);
      if (_npcReplications.TryGetValue(handle, out NpcReplicationSnapshot snapshot))
      {
        snapshots.Add(snapshot);
      }
    }

    return snapshots;
  }

  public IReadOnlyList<NpcStateSnapshot> CreateNpcStateSnapshots()
  {
    ThrowIfDisposed();
    List<NpcStateSnapshot> snapshots = new(_npcs.Count);
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs.OrderBy(pair => pair.Key.Value))
    {
      Entity entity = entry.Value;
      NpcReplicationSnapshot replication = _npcReplications[entry.Key];
      NpcComponents.NpcDefinitionComponent definition =
        World.Get<NpcComponents.NpcDefinitionComponent>(entity);
      TransformComponent transform = World.Get<TransformComponent>(entity);
      NpcComponents.NpcTargetComponent target =
        World.Get<NpcComponents.NpcTargetComponent>(entity);
      NpcComponents.NpcBehaviorStateComponent behavior =
        World.Get<NpcComponents.NpcBehaviorStateComponent>(entity);
      NpcComponents.NpcSpawnStateComponent spawn =
        World.Get<NpcComponents.NpcSpawnStateComponent>(entity);
      NpcComponents.NpcLifecycleComponent lifecycle =
        World.Get<NpcComponents.NpcLifecycleComponent>(entity);
      HealthComponent health = World.Get<HealthComponent>(entity);
      FacingComponent facing = World.Get<FacingComponent>(entity);
      bool hasHome = World.Has<NpcComponents.NpcHomeComponent>(entity);
      bool hasSegment = World.Has<NpcComponents.NpcSegmentComponent>(entity);
      NpcComponents.NpcHomeComponent home = hasHome
        ? World.Get<NpcComponents.NpcHomeComponent>(entity)
        : default;
      NpcComponents.NpcSegmentComponent segment = hasSegment
        ? World.Get<NpcComponents.NpcSegmentComponent>(entity)
        : default;
      NpcReplicationSnapshot stateReplication = replication with
      {
        Position = new SimulationVector(transform.X, transform.Y),
        Velocity = new SimulationVector(
          World.Get<VelocityComponent>(entity).X,
          World.Get<VelocityComponent>(entity).Y),
        Health = health.Current,
        IsActive = lifecycle.IsActive && health.Current > 0,
        Section = GetSectionCoordinates(new SimulationVector(transform.X, transform.Y)),
        DefinitionId = definition.DefinitionId,
        MaximumHealth = health.Maximum,
        Facing = facing.Horizontal,
        TargetStableId = target.StableTargetId,
        HasTarget = target.HasTarget,
        BehaviorId = behavior.BehaviorId,
        SpawnSource = spawn.Source,
        DifficultyScale = spawn.DifficultyScale,
        ReleaseOwner = spawn.ReleaseOwner,
        SpawnedFromStatue = spawn.SpawnedFromStatue,
        TimeLeft = lifecycle.TimeLeft,
        DespawnReason = lifecycle.DespawnReason
      };
      snapshots.Add(new NpcStateSnapshot(
        stateReplication,
        definition.DefinitionId,
        definition.NetId,
        health.Maximum,
        facing.Horizontal,
        target.StableTargetId,
        target.HasTarget,
        behavior,
        spawn,
        lifecycle,
        HasHome: hasHome,
        Home: home,
        HasSegment: hasSegment,
        Segment: segment,
        Faction: definition.Faction,
        Category: definition.Category));
    }

    return snapshots;
  }

  public IReadOnlyList<ProjectileReplicationSnapshot> CreateProjectileReplicationSnapshots()
  {
    ThrowIfDisposed();
    List<ProjectileReplicationSnapshot> snapshots = new(_projectileReplications.Count);
    for (int replicationId = 1; replicationId < _nextProjectileReplicationId; replicationId++)
    {
      if (_projectileReplications.TryGetValue(replicationId, out ProjectileReplicationSnapshot snapshot))
      {
        snapshots.Add(snapshot);
      }
    }

    return snapshots;
  }

  public void QueueNpcDamage(NpcHandle npc, int amount)
  {
    ThrowIfDisposed();
    if (amount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    if (!_npcs.ContainsKey(npc))
    {
      throw new ArgumentException("NPC does not exist.", nameof(npc));
    }

    _commands.Enqueue(new DamageNpcCommand(npc, amount));
  }

  public bool DestroyPlayer(PlayerHandle player)
  {
    ThrowIfDisposed();
    if (!_players.Remove(player, out Entity entity))
    {
      return false;
    }

    SynchronizePlayerAccount(player);
    World.Destroy(entity);
    _playerAccountUuids.Remove(player);
    return true;
  }

  public InventoryComponent GetInventory(PlayerHandle player)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(player, out Entity entity) ||
        !World.Has<InventoryComponent>(entity))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    return World.Get<InventoryComponent>(entity);
  }

  public PlayerInventorySnapshot CreatePlayerInventorySnapshot(PlayerHandle player)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(player, out Entity entity) ||
        !World.Has<InventoryComponent>(entity))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    InventoryComponent inventory = World.Get<InventoryComponent>(entity);
    ItemStack[] items = inventory.Slots.ToArray();
    SelectedItemComponent selection = World.Get<SelectedItemComponent>(entity);
    EquipmentLoadoutComponent loadout = World.Get<EquipmentLoadoutComponent>(entity);
    return new PlayerInventorySnapshot(
      player,
      selection.SelectedSlot,
      selection.Revision,
      loadout,
      Array.AsReadOnly(items));
  }

  public InventorySnapshot CreateInventorySnapshot(PlayerHandle player)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(player, out Entity entity) ||
        !World.Has<InventoryComponent>(entity))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    InventoryComponent inventory = World.Get<InventoryComponent>(entity);
    List<ItemInstanceSnapshot> slots = new(InventoryComponent.SlotCount);
    for (int slotId = 0; slotId < InventoryComponent.SlotCount; slotId++)
    {
      slots.Add(new ItemInstanceSnapshot(
        inventory.GetSlot(slotId),
        inventory.GetInstanceState(slotId)));
    }

    return new InventorySnapshot(player, slots, inventory.SelectedSlot, inventory.Revision);
  }

  public ItemUseSnapshot CreateItemUseSnapshot(PlayerHandle player)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(player, out Entity entity))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    return new ItemUseSnapshot(
      player,
      World.Get<ItemUseStateComponent>(entity),
      TickNumber);
  }

  public EquipmentSnapshot CreateEquipmentSnapshot(PlayerHandle player)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(player, out Entity entity))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    EquipmentStateCollectionComponent states =
      World.Get<EquipmentStateCollectionComponent>(entity);
    ItemEquipmentStateComponent[] slots = states.States
      .OrderBy(entry => entry.Key)
      .Select(entry => entry.Value)
      .ToArray();
    return new EquipmentSnapshot(
      player,
      slots,
      states.Revision);
  }

  public int SpawnWorldItem(
    ItemStack stack,
    SimulationVector position,
    int pickupDelayTicks = 0)
  {
    return SpawnWorldItem(stack, default, position, pickupDelayTicks);
  }

  public int SpawnWorldItem(
    ItemStack stack,
    Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent instanceState,
    SimulationVector position,
    int pickupDelayTicks = 0)
  {
    ThrowIfDisposed();
    if (stack.IsEmpty)
    {
      throw new ArgumentOutOfRangeException(nameof(stack));
    }

    if (pickupDelayTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(pickupDelayTicks));
    }

    CreateWorldItemCommand command = new(
      stack,
      position,
      GetSectionCoordinates(position),
      0,
      instanceState,
      pickupDelayTicks);
    return CommitWorldItemSpawn(command);
  }

  private int CommitWorldItemSpawn(CreateWorldItemCommand command)
  {
    _ = _itemDefinitions.Get(command.Stack.ItemType);
    if (!_worldItemSpawnSystem.TryCreate(
          ref _nextWorldItemReplicationId,
          command,
          out WorldItemComponent item,
          out ItemCommandRejection rejection))
    {
      throw new InvalidOperationException(rejection.Reason);
    }

    _worldItems.Add(item);
    _worldItemCreatedEvents.Add(new WorldItemCreatedEvent(
      item.ReplicationId,
      item.Stack,
      item.Position,
      item.Revision));
    return item.ReplicationId;
  }

  public void QueuePickupWorldItem(PlayerHandle player, int worldItemId)
  {
    ThrowIfDisposed();
    if (!_players.ContainsKey(player))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    _commands.Enqueue(new PickupWorldItemCommand(player, worldItemId));
  }

  public void QueueMoveWorldItem(
    int worldItemId,
    SimulationVector position,
    long expectedRevision)
  {
    ThrowIfDisposed();
    _commands.Enqueue(new MoveWorldItemCommand(worldItemId, position, expectedRevision));
  }

  public void QueueDestroyWorldItem(int worldItemId, long expectedRevision)
  {
    ThrowIfDisposed();
    _commands.Enqueue(new DestroyWorldItemCommand(worldItemId, expectedRevision));
  }

  public void QueueTransferItem(
    PlayerHandle player,
    int sourceSlot,
    int destinationSlot,
    int quantity)
  {
    ThrowIfDisposed();
    ValidatePlayerCommandPlayer(player);
    _commands.Enqueue(new TransferItemCommand(
      player,
      sourceSlot,
      destinationSlot,
      quantity,
      TakeWiringSequence()));
  }

  public void QueueSplitItemStack(
    PlayerHandle player,
    int sourceSlot,
    int destinationSlot,
    int quantity)
  {
    ThrowIfDisposed();
    ValidatePlayerCommandPlayer(player);
    _commands.Enqueue(new SplitItemStackCommand(
      player,
      sourceSlot,
      destinationSlot,
      quantity,
      TakeWiringSequence()));
  }

  public void QueueMergeItemStack(
    PlayerHandle player,
    int sourceSlot,
    int destinationSlot,
    int quantity)
  {
    ThrowIfDisposed();
    ValidatePlayerCommandPlayer(player);
    _commands.Enqueue(new MergeItemStackCommand(
      player,
      sourceSlot,
      destinationSlot,
      quantity,
      TakeWiringSequence()));
  }

  public void QueueDropItem(PlayerHandle player, int sourceSlot, int quantity)
  {
    ThrowIfDisposed();
    ValidatePlayerCommandPlayer(player);
    _commands.Enqueue(new DropItemCommand(
      player,
      sourceSlot,
      quantity,
      TakeWiringSequence()));
  }

  public void QueuePlaceItem(PlayerHandle player, int sourceSlot, int x, int y)
  {
    ThrowIfDisposed();
    if (!_players.ContainsKey(player))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    _placeItemCommands.Add(new PlaceItemCommand(
      player,
      sourceSlot,
      x,
      y,
      TakeWiringSequence()));
  }

  public void QueueUseExtractinator(PlayerHandle player, int sourceSlot, int targetX, int targetY)
  {
    ThrowIfDisposed();
    ValidatePlayerCommandPlayer(player);
    _commands.Enqueue(new UseExtractinatorCommand(
      player,
      sourceSlot,
      targetX,
      targetY,
      TakeWiringSequence()));
  }

  public void QueueTriggerExtractinator(int targetX, int targetY)
  {
    ThrowIfDisposed();
    _commands.Enqueue(new TriggerExtractinatorCommand(
      targetX,
      targetY,
      TakeWiringSequence()));
  }

  public void QueueEquipItem(PlayerHandle player, int sourceSlot, bool isVanity)
  {
    ThrowIfDisposed();
    if (!_players.ContainsKey(player))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    _equipItemCommands.Add(new EquipItemCommand(
      player,
      sourceSlot,
      isVanity,
      TakeWiringSequence()));
  }

  public void QueueApplyItemPrefix(PlayerHandle player, int sourceSlot, ushort prefixId)
  {
    ThrowIfDisposed();
    if (!_players.ContainsKey(player))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    _itemPrefixCommands.Add(new ApplyItemPrefixCommand(
      player,
      sourceSlot,
      prefixId,
      TakeWiringSequence()));
  }

  public void QueueUnequipItem(PlayerHandle player, ItemEquipmentSlot slot)
  {
    ThrowIfDisposed();
    if (!_players.ContainsKey(player))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    _unequipItemCommands.Add(new UnequipItemCommand(
      player,
      slot,
      TakeWiringSequence()));
  }

  public void QueueApplyItemVariant(
    PlayerHandle player,
    int sourceSlot,
    ItemVariantDefinition variant)
  {
    ThrowIfDisposed();
    if (!_players.ContainsKey(player))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    _itemVariantCommands.Add(new ApplyItemVariantCommand(
      player,
      sourceSlot,
      variant,
      TakeWiringSequence()));
  }

  public void QueueProximityWorldItemPickups()
  {
    ThrowIfDisposed();
    List<int> worldItemIds = new(_worldItems.Keys);
    worldItemIds.Sort();
    List<PlayerHandle> players = new(_players.Keys);
    players.Sort((first, second) => first.Value.CompareTo(second.Value));
    foreach (int worldItemId in worldItemIds)
    {
      foreach (PlayerHandle player in players)
      {
        _commands.Enqueue(new PickupWorldItemCommand(player, worldItemId));
      }
    }
  }

  public IReadOnlyList<WorldItemSnapshot> CreateWorldItemSnapshots()
  {
    ThrowIfDisposed();
    List<WorldItemSnapshot> items = new(_worldItems.Count);
    foreach (WorldItemComponent item in _worldItems.Values)
    {
      items.Add(new WorldItemSnapshot(
        item.ReplicationId,
        item.Stack,
        item.Position,
        item.IsActive,
        item.InstanceState));
    }

    return items;
  }

  public IReadOnlyList<ExtractinatorResultEvent> CreateExtractinatorResultEvents()
  {
    ThrowIfDisposed();
    return _extractinatorResultEvents.ToArray();
  }

  public IReadOnlyList<ItemReplicationSnapshot> CreateItemReplicationSnapshots()
  {
    ThrowIfDisposed();
    List<ItemReplicationSnapshot> snapshots = new(_worldItems.Count);
    for (int replicationId = 1; replicationId < _nextWorldItemReplicationId; replicationId++)
    {
      if (!_worldItems.TryGetValue(replicationId, out WorldItemComponent item))
      {
        continue;
      }

      snapshots.Add(new ItemReplicationSnapshot(
        item.ReplicationId,
        item.Stack,
        item.Position,
        item.IsActive,
        item.Revision,
        item.Section,
        InstanceState: item.InstanceState,
        WorldState: item.WorldState));
    }

    return snapshots;
  }

  public PlayerStateSnapshot CreatePlayerStateSnapshot(PlayerHandle player)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(player, out Entity entity))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    HealthComponent health = World.Get<HealthComponent>(entity);
    ManaComponent mana = World.Get<ManaComponent>(entity);
    PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entity);
    PlayerIdentityComponent identity = World.Get<PlayerIdentityComponent>(entity);
    return new PlayerStateSnapshot(
      player,
      lifecycle.IsActive,
      health.Current,
      health.Maximum,
      lifecycle.RespawnTicks,
      identity.CanonicalAccountUuid,
      identity.AssignedSlot,
      mana.Current,
      mana.Maximum);
  }

  public IReadOnlyList<PlayerDamagedEvent> CreatePlayerDamagedEvents()
  {
    ThrowIfDisposed();
    return _playerDamagedEvents.ToArray();
  }

  public IReadOnlyList<PlayerDiedEvent> CreatePlayerDiedEvents()
  {
    ThrowIfDisposed();
    return _playerDiedEvents.ToArray();
  }

  public IReadOnlyList<PlayerRespawnedEvent> CreatePlayerRespawnedEvents()
  {
    ThrowIfDisposed();
    return _playerRespawnedEvents.ToArray();
  }

  public IReadOnlyList<ItemUsedEvent> CreateItemUsedEvents()
  {
    ThrowIfDisposed();
    return _itemUsedEvents.ToArray();
  }

  public IReadOnlyList<ItemEquippedEvent> CreateItemEquippedEvents()
  {
    ThrowIfDisposed();
    return _itemEquippedEvents.ToArray();
  }

  public IReadOnlyList<ItemPrefixChangedEvent> CreateItemPrefixChangedEvents()
  {
    ThrowIfDisposed();
    return _itemPrefixChangedEvents.ToArray();
  }

  public IReadOnlyList<InventoryChangedEvent> CreateInventoryChangedEvents()
  {
    ThrowIfDisposed();
    return _inventoryChangedEvents.ToArray();
  }

  public IReadOnlyList<WorldItemCreatedEvent> CreateWorldItemCreatedEvents()
  {
    ThrowIfDisposed();
    return _worldItemCreatedEvents.ToArray();
  }

  public IReadOnlyList<WorldItemPickedUpEvent> CreateWorldItemPickedUpEvents()
  {
    ThrowIfDisposed();
    return _worldItemPickedUpEvents.ToArray();
  }

  public IReadOnlyList<WorldItemDestroyedEvent> CreateWorldItemDestroyedEvents()
  {
    ThrowIfDisposed();
    return _worldItemDestroyedEvents.ToArray();
  }

  public IReadOnlyList<WorldSlimeRainWarningEvent> CreateWorldSlimeRainWarningEvents()
  {
    ThrowIfDisposed();
    return _worldSlimeRainWarningEvents.ToArray();
  }

  public IReadOnlyList<WorldInvasionCompletedEvent> CreateWorldInvasionCompletedEvents()
  {
    ThrowIfDisposed();
    return _worldInvasionCompletedEvents.ToArray();
  }

  public IReadOnlyList<ItemDroppedEvent> CreateItemDroppedEvents()
  {
    ThrowIfDisposed();
    return _itemDroppedEvents.ToArray();
  }

  public void QueuePlayerDamage(PlayerHandle player, int amount)
  {
    ThrowIfDisposed();
    if (amount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    if (!_players.ContainsKey(player))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    _commands.Enqueue(new DamagePlayerCommand(player, amount));
  }

  public void QueueRespawnPlayer(PlayerHandle player, SimulationVector spawn)
  {
    ThrowIfDisposed();
    if (!_players.ContainsKey(player))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    _commands.Enqueue(new RespawnPlayerCommand(player, spawn));
  }

  public void QueuePlayerInteraction(
    PlayerHandle player,
    int targetId,
    PlayerInteractionMode mode,
    SimulationVector targetPosition)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(player, out Entity entity))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entity);
    if (!lifecycle.IsActive)
    {
      throw new InvalidOperationException("Inactive players cannot issue interactions.");
    }

    TransformComponent transform = World.Get<TransformComponent>(entity);
    float deltaX = targetPosition.X - transform.X;
    float deltaY = targetPosition.Y - transform.Y;
    if (deltaX * deltaX + deltaY * deltaY >
        MaximumInteractionRange * MaximumInteractionRange)
    {
      throw new ArgumentOutOfRangeException(nameof(targetPosition));
    }

    ref PlayerInteractionComponent interaction = ref World.Get<PlayerInteractionComponent>(entity);
    interaction.SetTarget(targetId, mode);
    interaction.TargetPosition = targetPosition;
    _commands.Enqueue(new UsePlayerInteractionCommand(player, targetId, mode, targetPosition));
  }

  public SimulationSnapshot CreateSnapshot()
  {
    ThrowIfDisposed();

    List<PlayerSnapshot> players = new(_players.Count);
    foreach (KeyValuePair<PlayerHandle, Entity> entry in _players)
    {
      TransformComponent transform = World.Get<TransformComponent>(entry.Value);
      VelocityComponent velocity = World.Get<VelocityComponent>(entry.Value);
      FacingComponent facing = World.Get<FacingComponent>(entry.Value);
      PhysicsStateComponent physics = World.Get<PhysicsStateComponent>(entry.Value);
      HealthComponent health = World.Get<HealthComponent>(entry.Value);
      ManaComponent mana = World.Get<ManaComponent>(entry.Value);
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entry.Value);
      PlayerIdentityComponent identity = World.Get<PlayerIdentityComponent>(entry.Value);
      players.Add(new PlayerSnapshot(
        entry.Key,
        new SimulationVector(transform.X, transform.Y),
        new SimulationVector(velocity.X, velocity.Y),
        facing.Horizontal,
        physics.IsGrounded,
        health.Current,
        lifecycle.IsActive,
        lifecycle.RespawnTicks,
        identity.CanonicalAccountUuid,
        identity.AssignedSlot,
        mana.Current,
        mana.Maximum));
    }

    List<NpcSnapshot> npcs = new(_npcs.Count);
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      TransformComponent transform = World.Get<TransformComponent>(entry.Value);
      HealthComponent health = World.Get<HealthComponent>(entry.Value);
      NpcTargetComponent target = World.Get<NpcTargetComponent>(entry.Value);
      npcs.Add(new NpcSnapshot(
        entry.Key,
        new SimulationVector(transform.X, transform.Y),
        health.Current,
        target.HasTarget));
    }

    List<ProjectileSnapshot> projectiles = new();
    World.Query(
      in _projectileQuery,
      (Entity entity, ref TransformComponent transform,
        ref ProjectileLifetimeComponent lifetime) =>
      {
        projectiles.Add(new ProjectileSnapshot(
          new SimulationVector(transform.X, transform.Y),
          lifetime.RemainingTicks));
      });

    return new SimulationSnapshot(TickNumber, players, npcs, projectiles);
  }

  public DomeSimulationSnapshot CreatePersistenceSnapshot(
    WorldMetadata metadata,
    WorldRuleState? worldRules = null,
    WorldProgressionState? progression = null)
  {
    ThrowIfDisposed();
    _worldMetadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    WorldClockSnapshot worldClock = _worldClock.CreateSnapshot();
    List<PlayerHandle> accountPlayers = new(_playerAccountUuids.Keys);
    for (int index = 0; index < accountPlayers.Count; index++)
    {
      SynchronizePlayerAccount(accountPlayers[index]);
    }

    return new DomeSimulationSnapshot(
      WorldGrid.CreateSnapshot(metadata),
      CreateNpcReplicationSnapshots(),
      CreateItemReplicationSnapshots(),
      worldClock.TickNumber,
      _playerAccounts.Values.OrderBy(account => account.Uuid, StringComparer.Ordinal).ToArray(),
      CreateChestPersistentSnapshots(),
      CreateSignPersistentSnapshots(),
      CreateTileEntitySnapshots(),
      worldClock: worldClock,
      npcStates: CreateNpcStateSnapshots(),
      worldRules: worldRules ?? _worldRules,
      progression: progression ?? _worldProgression,
      worldEventRandomState: _worldEventRandomState,
      worldTimeRate: _worldTimeRate);
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    World.Dispose();
  }

  public void Tick(SimulationInputBatch inputBatch)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(inputBatch);

    SimulationTickContext tick = SimulationTickSchedule.Begin();
    tick.Enter(SimulationTickPhase.BeginTick);
    _playerDamagedEvents.Clear();
    _playerDiedEvents.Clear();
    _playerRespawnedEvents.Clear();
    _itemUsedEvents.Clear();
    _itemEquippedEvents.Clear();
    _itemPrefixChangedEvents.Clear();
    _inventoryChangedEvents.Clear();
    _worldItemCreatedEvents.Clear();
    _worldItemPickedUpEvents.Clear();
    _worldItemDestroyedEvents.Clear();
    _itemDroppedEvents.Clear();
    _extractinatorResultEvents.Clear();
    _worldSlimeRainWarningEvents.Clear();
    _worldInvasionCompletedEvents.Clear();
    tick.Enter(SimulationTickPhase.ApplyWorldClock);
    if (_worldTimeRateInput is WorldTimeRateInput timeRateInput)
    {
      _worldTimeRate = _worldTimeRateSystem.Resolve(timeRateInput);
    }

    _worldClockSystem.Tick(_worldClock, _worldTimeRate);
    if (_worldClock.IsPaused)
    {
      tick.CompleteWhilePaused();
      _lastTickPhases = tick.Phases;
      return;
    }

    _worldRules = _worldWeatherSystem.Advance(
      _worldClock.CreateSnapshot(),
      _worldRules,
      _worldRainStartRequests,
      _worldWindChangeRequests,
      _worldProgression.IsLanternNight || HasPendingLanternNightStart());
    _worldRainStartRequests.Clear();
    _worldWindChangeRequests.Clear();

    WorldProgressionState previousProgression = _worldProgression;
    _worldProgression = _worldProgressionSystem.Advance(
      _worldClock.CreateSnapshot(),
      _worldProgression,
      _worldEventStartRequests,
      _worldLanternNightScheduleRequests,
      _worldInvasionStartRequests,
      _worldInvasionProgressRequests,
      _worldSlimeRainStartRequests,
      _worldSlimeRainStopRequests);
    AdvanceWorldInvasionTravel();
    if (previousProgression.InvasionType > 0 &&
        previousProgression.InvasionSize > 0 &&
        _worldProgression.InvasionType == 0 &&
        _worldProgression.InvasionSize == 0)
    {
      WorldInvasionClearFlag clearFlag =
        _worldInvasionClearFlagSystem.Resolve(previousProgression.InvasionType);
      _worldProgression = _worldProgression.WithInvasionClearFlag(clearFlag);
      _worldInvasionCompletedEvents.Add(
        new WorldInvasionCompletedEvent(
          previousProgression.InvasionType,
          clearFlag));
    }
    if (previousProgression.SlimeRainWarningTicks > 0 &&
        _worldProgression.SlimeRainWarningTicks == 0)
    {
      _worldSlimeRainWarningEvents.Add(
        new WorldSlimeRainWarningEvent(_worldProgression.IsSlimeRaining));
    }
    ResolveScheduledWorldMeteorImpacts();
    _worldProgression = _worldMeteorScheduleSystem.Advance(
      _worldClock.CreateSnapshot(),
      _worldProgression,
      _worldMeteorScheduleRequests);
    _worldEventStartRequests.Clear();
    _worldLanternNightScheduleRequests.Clear();
    _worldInvasionStartRequests.Clear();
    _worldInvasionProgressRequests.Clear();
    _worldSlimeRainStartRequests.Clear();
    _worldSlimeRainStopRequests.Clear();
    _worldMeteorScheduleRequests.Clear();

    tick.Enter(SimulationTickPhase.ApplyPlayerInputs);
    _playerInputApplySystem.Apply(World, _players, FilterActivePlayerInputs(inputBatch));
    tick.Enter(SimulationTickPhase.ApplyPlayerControl);
    AdvanceItemUseCooldowns();
    QueueItemUses();
    AdvancePlayerLifecycle();
    IReadOnlyList<Entity> activePlayers = GetActivePlayers();
    _playerControlSystem.Apply(World, activePlayers);
    _playerGravitySystem.Apply(World, activePlayers);
    tick.Enter(SimulationTickPhase.ResolveTileCollision);
    ResolvePlayerTileCollision();
    _playerVitalRegenSystem.Apply(World, activePlayers);
    AdvancePlayerBuffs(activePlayers);
    AdvancePlayerImmunity();
    EvaluateTrainingDummyLifecycle(activePlayers);
    List<string> npcPipelineStages = new();
    tick.Enter(SimulationTickPhase.SelectNpcTargets);
    RunNpcSystemPipeline(
      NpcSystemStage.SpawnEligibility,
      NpcSystemStage.MovementAndCollision,
      npcPipelineStages);
    tick.Enter(SimulationTickPhase.ApplyNpcAi);
    tick.Enter(SimulationTickPhase.MoveEntities);
    tick.Enter(SimulationTickPhase.AdvanceProjectiles);
    RequestProjectiles();
    MoveProjectiles();
    DetectProjectileHits();
    tick.Enter(SimulationTickPhase.ResolveCombat);
    RunNpcSystemPipeline(
      NpcSystemStage.ContactEffect,
      NpcSystemStage.Replication,
      npcPipelineStages);
    _lastNpcPipelineSystemNames = npcPipelineStages;
    tick.Enter(SimulationTickPhase.CommitDomainCommands);
    AdvanceWiring();
    AdvanceLiquid();
    new HitImmunitySystem().Tick(_projectileHitImmunity);
    CommitCommands();
    tick.Enter(SimulationTickPhase.PublishSnapshot);
    _lastPublishedSnapshot = CreateSnapshot();
    tick.Enter(SimulationTickPhase.EndTick);
    tick.CompleteNormally();
    _lastTickPhases = tick.Phases;
  }

  private void AdvanceWorldInvasionTravel()
  {
    if (_worldMetadata is null ||
        !_worldTimeRate.IsAvailable ||
        _worldProgression.InvasionType == 0 ||
        _worldProgression.InvasionSize == 0 ||
        _worldProgression.InvasionX == _worldMetadata.SpawnX)
    {
      return;
    }

    WorldInvasionTravelResult travel = _worldInvasionTravelSystem.Advance(
      _worldProgression.InvasionX,
      _worldMetadata.SpawnX,
      _worldTimeRate.Rate);
    _worldProgression = _worldProgression.WithInvasionX(travel.Position);
  }

  private void AdvanceLiquid()
  {
    List<PipelineLiquidChangeCommand> committed = new();
    LiquidTransferResult transfer = _liquidTransferSystem.CreateChanges(
      WorldGrid,
      _commands.LiquidTransferCommands,
      _nextLiquidSequence);
    _nextLiquidSequence = transfer.NextSequence;
    if (transfer.Changes.Count > 0)
    {
      if (!_liquidCommitSystem.TryCommit(
            WorldGrid,
            transfer.Changes,
            _liquidDirtySections,
            out LiquidCommitResult transferResult))
      {
        throw new InvalidOperationException(
          transferResult.FailureReason ?? "Liquid transfer failed.");
      }

      committed.AddRange(transfer.Changes);
      for (int index = 0; index < transfer.Changes.Count; index++)
      {
        PipelineLiquidChangeCommand change = transfer.Changes[index];
        if (change.Amount == 0)
        {
          continue;
        }

        _ = TryQueueLiquidSource(new PipelineLiquidSourceComponent(
          change.X,
          change.Y,
          change.Amount,
          (LiquidType)change.Type,
          NextLiquidSequence()));
      }
    }

    SortedSet<(int X, int Y)> mergeCoordinates = new();
    IReadOnlyList<LiquidUpdateNode> pendingNodes = _liquidQueue.PendingNodes;
    for (int index = 0; index < pendingNodes.Count; index++)
    {
      mergeCoordinates.Add((pendingNodes[index].X, pendingNodes[index].Y));
    }

    AddLiquidMergeCoordinates(mergeCoordinates, transfer.Changes);
    List<SimulationLiquidMerge> merges = EvaluateLiquidMerges(mergeCoordinates);
    if (_liquidQueue.Count == 0)
    {
      committed.AddRange(CommitLiquidMergeChanges(merges));
      EnqueueLiquidMergeTileChanges(merges);
      _lastLiquidCommands = committed;
      return;
    }

    LiquidPropagationResult propagation = _liquidPropagationSystem.Advance(
      WorldGrid,
      _liquidQueue,
      _liquidState,
      _nextLiquidSequence);
    _nextLiquidSequence = propagation.NextSequence;
    EnqueueLiquidContactTileChanges(propagation.TileCommands);
    if (propagation.Commands.Count == 0)
    {
      committed.AddRange(CommitLiquidMergeChanges(merges));
      EnqueueLiquidMergeTileChanges(merges);
      _lastLiquidCommands = committed;
      return;
    }

    if (!_liquidCommitSystem.TryCommit(
          WorldGrid,
          propagation.Commands,
          _liquidDirtySections,
          out LiquidCommitResult result))
    {
      throw new InvalidOperationException(result.FailureReason ?? "Liquid commit failed.");
    }

    committed.AddRange(propagation.Commands);
    AddLiquidMergeCoordinates(mergeCoordinates, propagation.Commands);
    merges.AddRange(EvaluateLiquidMerges(mergeCoordinates));
    committed.AddRange(CommitLiquidMergeChanges(merges));
    EnqueueLiquidMergeTileChanges(merges);
    _lastLiquidCommands = committed;
  }

  private static void AddLiquidMergeCoordinates(
    ISet<(int X, int Y)> coordinates,
    IReadOnlyCollection<PipelineLiquidChangeCommand> changes)
  {
    foreach (PipelineLiquidChangeCommand change in changes)
    {
      coordinates.Add((change.X, change.Y));
    }
  }

  private List<SimulationLiquidMerge> EvaluateLiquidMerges(
    IReadOnlyCollection<(int X, int Y)> coordinates)
  {
    List<SimulationLiquidMerge> merges = new();
    foreach ((int x, int y) in coordinates)
    {
      if (_liquidMergeSystem.TryEvaluate(WorldGrid, x, y, out SimulationLiquidMerge merge))
      {
        merges.Add(merge);
      }
    }

    return merges;
  }

  private IReadOnlyList<PipelineLiquidChangeCommand> CommitLiquidMergeChanges(
    IReadOnlyCollection<SimulationLiquidMerge> merges)
  {
    SortedDictionary<(int X, int Y), byte> clearTypes = new();
    foreach (SimulationLiquidMerge merge in merges)
    {
      AddLiquidMergeClear(clearTypes, merge.X, merge.Y);
      AddLiquidMergeClear(clearTypes, merge.NeighborX, merge.NeighborY);
    }

    if (clearTypes.Count == 0)
    {
      return [];
    }

    List<PipelineLiquidChangeCommand> changes = new(clearTypes.Count);
    foreach (KeyValuePair<(int X, int Y), byte> entry in clearTypes)
    {
      changes.Add(new PipelineLiquidChangeCommand(
        NextLiquidSequence(),
        entry.Key.X,
        entry.Key.Y,
        0,
        entry.Value));
    }

    if (!_liquidCommitSystem.TryCommit(
          WorldGrid,
          changes,
          _liquidDirtySections,
          out LiquidCommitResult result))
    {
      throw new InvalidOperationException(
        result.FailureReason ?? "Liquid merge commit failed.");
    }

    foreach (SimulationLiquidMerge merge in merges)
    {
      _liquidQueue.Cancel(merge.X, merge.Y);
      _liquidQueue.Cancel(merge.NeighborX, merge.NeighborY);
    }

    return changes;
  }

  private void AddLiquidMergeClear(
    IDictionary<(int X, int Y), byte> clearTypes,
    int x,
    int y)
  {
    if (!WorldGrid.Contains(x, y))
    {
      return;
    }

    WorldTile tile = WorldGrid.GetTile(x, y);
    if (tile.LiquidAmount > 0)
    {
      _ = clearTypes.TryAdd((x, y), tile.LiquidType);
    }
  }

  private long TakeWiringSequence()
  {
    if (!TryAdvanceWiringSequence(1))
    {
      throw new InvalidOperationException("Wiring sequence space is exhausted.");
    }

    return _nextWiringSequence - 1;
  }

  private bool TryAdvanceWiringSequence(int count)
  {
    if (count < 0 || _nextWiringSequence > long.MaxValue - count)
    {
      return false;
    }

    _nextWiringSequence += count;
    return true;
  }

  private void EnqueueLiquidMergeTileChanges(IReadOnlyCollection<SimulationLiquidMerge> merges)
  {
    SortedDictionary<(int X, int Y), SimulationLiquidMerge> uniqueMerges = new();
    foreach (SimulationLiquidMerge merge in merges)
    {
      if (merge.ResultTileType != 0)
      {
        _ = uniqueMerges.TryAdd((merge.X, merge.Y), merge);
      }
    }

    foreach (KeyValuePair<(int X, int Y), SimulationLiquidMerge> entry in uniqueMerges)
    {
      SimulationLiquidMerge merge = entry.Value;
      WorldGrid.EnqueueTileChange(new TileChangeCommand(
        TakeWiringSequence(),
        merge.X,
        merge.Y,
        TileChangeKind.Place,
        merge.ResultTileType));
    }
  }

  private void EnqueueLiquidContactTileChanges(IReadOnlyList<TileChangeCommand> commands)
  {
    for (int index = 0; index < commands.Count; index++)
    {
      TileChangeCommand command = commands[index];
      WorldGrid.EnqueueTileChange(command with { Sequence = TakeWiringSequence() });
    }
  }

  private bool CanKillTileForActuator(int tileX, int tileY)
  {
    return LegacyCanKillTileQuery.TryGetCanKill(
      WorldGrid,
      tileX,
      tileY,
      _worldProgression.IsHardMode,
      _chests,
      _chestIndexSystem,
      out bool canKill) && canKill;
  }

  private void AdvanceWiring()
  {
    if (_wiringInputs.Count == 0 && _pressurePlates.Count == 0)
    {
      return;
    }

    List<MechanismActivationCommand> candidates = new();
    HashSet<int> activatedMechanisms = new();
    HashSet<int> consumedTriggers = new();
    List<WiringActorSnapshot> actors = new(_players.Count);
    foreach (Entity playerEntity in GetActivePlayers())
    {
      TransformComponent transform = World.Get<TransformComponent>(playerEntity);
      actors.Add(new WiringActorSnapshot(new SimulationVector(transform.X, transform.Y), true));
    }

    IReadOnlyList<MechanismActivationCommand> pressureActivations = _pressurePlateDetectionSystem.Detect(
      _pressurePlates,
      actors,
      _nextWiringSequence);
    if (!TryAdvanceWiringSequence(pressureActivations.Count))
    {
      return;
    }
    for (int index = 0; index < pressureActivations.Count; index++)
    {
      MechanismActivationCommand activation = pressureActivations[index];
      if (activatedMechanisms.Add(activation.MechanismId))
      {
        candidates.Add(activation);
      }
    }

    _wiringInputs.Sort(WiringInputCommandComparer.Instance);
    for (int inputIndex = 0; inputIndex < _wiringInputs.Count; inputIndex++)
    {
      WiringInputCommand input = _wiringInputs[inputIndex];
      if (!_wiringInputValidationSystem.TryValidate(
            WorldGrid,
            _wireNetwork,
            input,
            maximumRadius: 6))
      {
        continue;
      }

      WireTraversalResult traversal = _wireTraversalSystem.Traverse(
        _wireNetwork,
        new WireTraversalStateComponent(),
        input.X,
        input.Y,
        input.Color,
        maximumNodes: 4096);
      if (traversal.BudgetExceeded)
      {
        continue;
      }

      for (int nodeIndex = 0; nodeIndex < traversal.Nodes.Count; nodeIndex++)
      {
        WireTraversalNode node = traversal.Nodes[nodeIndex];
        if (!_wireTriggers.TryGetValue((node.X, node.Y), out TriggerComponent trigger) ||
            trigger.OneShot && consumedTriggers.Contains(trigger.TriggerId) ||
            !activatedMechanisms.Add(trigger.TargetMechanismId))
        {
          continue;
        }

        candidates.Add(new MechanismActivationCommand(
          TakeWiringSequence(),
          trigger.TargetMechanismId,
          MechanismActivationKind.Activate,
          trigger.TriggerId));
        if (trigger.OneShot)
        {
          consumedTriggers.Add(trigger.TriggerId);
        }
      }
    }

    _wiringInputs.Clear();
    Dictionary<int, bool> mechanismStates = new(_mechanisms.Count);
    foreach (KeyValuePair<int, MechanismComponent> entry in _mechanisms)
    {
      mechanismStates.Add(entry.Key, entry.Value.IsActive);
    }

    for (int index = 0; index < _logicGates.Count; index++)
    {
      LogicGateComponent gate = _logicGates[index];
      MechanismActivationCommand? evaluated = _logicGateEvaluationSystem.Evaluate(
        gate,
        mechanismStates,
        _nextWiringSequence);
      bool output = evaluated is not null;
      bool wasActive = _logicGateOutputs[gate.MechanismId];
      _logicGateOutputs[gate.MechanismId] = output;
      if (evaluated is not MechanismActivationCommand activation || wasActive ||
          !activatedMechanisms.Add(gate.OutputMechanismId))
      {
        continue;
      }

      candidates.Add(activation);
      _ = TakeWiringSequence();
    }

    IReadOnlyList<MechanismActivationCommand> accepted = _mechanismActivationSystem.Apply(
      _mechanisms,
      candidates);
    for (int index = 0; index < accepted.Count; index++)
    {
      MechanismActivationCommand activation = accepted[index];
      _commands.Enqueue(activation);
      if (_doorMechanisms.TryGetValue(activation.MechanismId, out int doorId) &&
          _doors.TryGetValue(doorId, out DoorSnapshot door))
      {
        DoorTransition transition = door.IsOpen ? DoorTransition.CloseDoor : DoorTransition.OpenDoor;
        _ = ApplyDoorTransition(door, transition);
      }

      if (_actuators.TryGetValue(activation.MechanismId, out ActuatorComponent? actuator) &&
          actuator is not null)
      {
        IReadOnlyList<TileChangeCommand> tileChanges = _actuatorCommandSystem.CreateCommands(
        WorldGrid,
        actuator,
        activation,
        _nextWiringSequence,
        CanKillTileForActuator,
        _worldMetadata,
        _worldProgression);
        if (!TryAdvanceWiringSequence(tileChanges.Count))
        {
          return;
        }
        for (int commandIndex = 0; commandIndex < tileChanges.Count; commandIndex++)
        {
          WorldGrid.EnqueueTileChange(tileChanges[commandIndex]);
        }
      }

      if (_lamps.TryGetValue(activation.MechanismId, out LampComponent? lamp))
      {
        bool isFrameBacked = _lampCommandSystem.TryCreateFrameCommands(
          WorldGrid,
          LampDefinitionRegistry.SourceDerived,
          lamp.Tile.X,
          lamp.Tile.Y,
          activation.Kind,
          _nextWiringSequence,
          out IReadOnlyList<TileFrameCommand> frameChanges);
        if (isFrameBacked)
        {
          if (frameChanges.Count != 0)
          {
            if (!TryAdvanceWiringSequence(frameChanges.Count))
            {
              return;
            }
            for (int commandIndex = 0; commandIndex < frameChanges.Count; commandIndex++)
            {
              WorldGrid.EnqueueTileFrameChange(frameChanges[commandIndex]);
            }
          }

          if (activation.Kind == MechanismActivationKind.Close)
          {
            lamp.SetLit(false);
          }
          else if (activation.Kind == MechanismActivationKind.Activate ||
              activation.Kind == MechanismActivationKind.Open)
          {
            lamp.SetLit(true);
          }
          else if (activation.Kind == MechanismActivationKind.Toggle)
          {
            lamp.SetLit(!lamp.IsLit);
          }

          continue;
        }

        IReadOnlyList<TileChangeCommand> tileChanges = _lampCommandSystem.CreateCommands(
          lamp,
          activation,
          _nextWiringSequence);
        if (!TryAdvanceWiringSequence(tileChanges.Count))
        {
          return;
        }
        for (int commandIndex = 0; commandIndex < tileChanges.Count; commandIndex++)
        {
          WorldGrid.EnqueueTileChange(tileChanges[commandIndex]);
        }
      }

      if (_pumps.TryGetValue(activation.MechanismId, out PumpComponent pump))
      {
        LiquidTransferCommand? transfer = _pumpCommandSystem.CreateCommand(
          pump,
          activation,
          TakeWiringSequence(),
          LiquidType.Water);
        if (transfer is LiquidTransferCommand command)
        {
          _commands.Enqueue(command);
        }
      }
    }
  }

  private void ApplyPlayerControl()
  {
    foreach (Entity entity in _players.Values)
    {
      ref PlayerInputComponent input = ref World.Get<PlayerInputComponent>(entity);
      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(entity);
      ref FacingComponent facing = ref World.Get<FacingComponent>(entity);
      float direction = 0.0f;

      if (input.MoveLeft && !input.MoveRight)
      {
        direction = -1.0f;
      }
      else if (input.MoveRight && !input.MoveLeft)
      {
        direction = 1.0f;
      }

      velocity.X = direction * PlayerSpeed;
      if (direction != 0.0f)
      {
        facing.Horizontal = direction > 0.0f ? 1 : -1;
      }

      PhysicsStateComponent physics = World.Get<PhysicsStateComponent>(entity);
      if (input.Jump && physics.IsGrounded)
      {
        velocity.Y = JumpSpeed;
      }
    }
  }

  private SimulationInputBatch FilterActivePlayerInputs(SimulationInputBatch inputBatch)
  {
    List<PlayerInput> activeInputs = new(inputBatch.Inputs.Count);
    for (int index = 0; index < inputBatch.Inputs.Count; index++)
    {
      PlayerInput input = inputBatch.Inputs[index];
      if (!_players.TryGetValue(input.Player, out Entity entity))
      {
        throw new ArgumentException("Input references an unknown player.", nameof(inputBatch));
      }

      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entity);
      if (lifecycle.IsActive)
      {
        if (input.SelectedSlot < 0 || input.SelectedSlot >= InventoryComponent.HotbarSlotCount)
        {
          continue;
        }

        InventoryComponent inventory = World.Get<InventoryComponent>(entity);
        inventory.SetSelectedSlot(input.SelectedSlot);
        ref SelectedItemComponent selection = ref World.Get<SelectedItemComponent>(entity);
        _itemSelectionSystem.Apply(
          inventory,
          ref selection,
          input.SelectedSlot);
        activeInputs.Add(input);
      }
    }

    return new SimulationInputBatch([.. activeInputs]);
  }

  private IReadOnlyList<Entity> GetActivePlayers()
  {
    List<Entity> activePlayers = new(_players.Count);
    foreach (Entity entity in _players.Values)
    {
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entity);
      if (lifecycle.IsActive)
      {
        activePlayers.Add(entity);
      }
    }

    return activePlayers;
  }

  private void ResolvePlayerTileCollision()
  {
    foreach (Entity entity in _players.Values)
    {
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entity);
      if (!lifecycle.IsActive)
      {
        continue;
      }

      ref TransformComponent transform = ref World.Get<TransformComponent>(entity);
      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(entity);
      ref PhysicsStateComponent physics = ref World.Get<PhysicsStateComponent>(entity);
      PlayerInputComponent input = World.Get<PlayerInputComponent>(entity);
      ColliderComponent collider = World.Get<ColliderComponent>(entity);
      _tileCollisionSystem.MoveAndResolve(
        WorldGrid,
        ref transform,
        ref velocity,
        ref physics,
        collider,
        input.Down,
        deferTopSlopeCollision: true);
      _topSlopeContactSystem.Resolve(
        WorldGrid,
        ref transform,
        ref velocity,
        ref physics,
        collider);
    }
  }

  private void CommitCommands()
  {
    CommitPlayerInteractions();
    CommitInventoryCommands();
    _extractinatorTargetsCommittedThisTick.Clear();
    CommitExtractinatorUses();
    CommitTriggeredExtractinators();
    CommitItemUses();
    CommitItemPrefixes();
    CommitItemVariants();
    CommitItemUnequipment();
    CommitItemEquipment();
    RecalculateEquipmentStats();
    CommitItemPlacements();
    CommitWorldItemMoves();
    CommitWorldItemDestructions();
    CommitWorldItemPassiveStacking();
    CommitWorldItemPickups();
    AdvanceWorldItemPickupDelays();
    CommitWorldMeteorImpacts();
    for (int index = 0; index < _commands.DamagePlayerCommands.Count; index++)
    {
      DamagePlayerCommand command = _commands.DamagePlayerCommands[index];
      if (!_players.TryGetValue(command.Player, out Entity playerEntity))
      {
        continue;
      }

      ref PlayerLifecycleComponent lifecycle = ref World.Get<PlayerLifecycleComponent>(playerEntity);
      if (!lifecycle.IsActive)
      {
        continue;
      }

      ref HealthComponent health = ref World.Get<HealthComponent>(playerEntity);
      ref ImmunityComponent immunity = ref World.Get<ImmunityComponent>(playerEntity);
      DefenseComponent defense = World.Get<DefenseComponent>(playerEntity);
      immunity.RemainingTicks = 0;
      bool applied = _damageResolutionSystem.TryResolve(
        ref health,
        defense,
        ref immunity,
        command.Amount,
        DamageTargetKind.Player,
        _worldRules,
        out int appliedAmount);
      if (applied)
      {
        ref HealthRegenerationComponent healthRegeneration =
          ref World.Get<HealthRegenerationComponent>(playerEntity);
        healthRegeneration.ResetDelay();
        immunity.RemainingTicks = NpcHitImmunityTicks;
        _playerDamagedEvents.Add(new PlayerDamagedEvent(
          command.Player,
          command.Amount,
          appliedAmount,
          health.Current));
      }

      if (_playerDeathSystem.TryBeginDeath(
            ref lifecycle,
            health,
            PlayerRespawnDelayTicks))
      {
        _playerDiedEvents.Add(new PlayerDiedEvent(command.Player, PlayerRespawnDelayTicks));
      }
    }

    for (int index = 0; index < _commands.RespawnPlayerCommands.Count; index++)
    {
      RespawnPlayerCommand command = _commands.RespawnPlayerCommands[index];
      if (!_players.TryGetValue(command.Player, out Entity playerEntity))
      {
        continue;
      }

      ref PlayerLifecycleComponent lifecycle = ref World.Get<PlayerLifecycleComponent>(playerEntity);
      ref TransformComponent transform = ref World.Get<TransformComponent>(playerEntity);
      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(playerEntity);
      ref HealthComponent health = ref World.Get<HealthComponent>(playerEntity);
      if (!_playerRespawnSystem.TryRespawn(
            ref lifecycle,
            ref transform,
            ref velocity,
            ref health,
            command.Spawn))
      {
        continue;
      }

      _playerRespawnedEvents.Add(new PlayerRespawnedEvent(command.Player, command.Spawn));
    }

    for (int index = 0; index < _commands.SpawnProjectileCommands.Count; index++)
    {
      SpawnProjectileCommand command = _commands.SpawnProjectileCommands[index];
      if (!_players.TryGetValue(command.Owner, out Entity ownerEntity) ||
          !World.Get<PlayerLifecycleComponent>(ownerEntity).IsActive ||
          !float.IsFinite(command.X) ||
          !float.IsFinite(command.Y) ||
          !float.IsFinite(command.InitialVelocityY) ||
          !float.IsFinite(command.ProjectileSpeed) ||
          command.LifetimeTicks <= 0 ||
          command.MaximumPenetration == 0 ||
          command.MaximumPenetration < -1)
      {
        continue;
      }

      if (!_projectileDefinitions.TryGet(command.ProjectileType,
          out ProjectileDefinition definition))
      {
        continue;
      }

      if (_nextProjectileReplicationId == int.MaxValue)
      {
        continue;
      }

      int replicationId = _nextProjectileReplicationId;
      _nextProjectileReplicationId++;
      Entity projectile = _projectileSpawnSystem.Spawn(World, command, definition, replicationId);
      TransformComponent transform = World.Get<TransformComponent>(projectile);
      VelocityComponent velocityComponent = World.Get<VelocityComponent>(projectile);
      SimulationVector position = new(transform.X, transform.Y);
      SimulationVector velocity = new(velocityComponent.X, velocityComponent.Y);
      _projectileIdsByEntity.Add(projectile, replicationId);
      _projectileReplications.Add(replicationId, new ProjectileReplicationSnapshot(
        replicationId,
        command.ProjectileType,
        command.Owner,
        position,
        velocity,
        command.Damage,
        command.LifetimeTicks,
        IsActive: true,
        Revision: 1,
        GetSectionCoordinates(position),
        Identity: replicationId));
    }

    for (int index = 0; index < _commands.DamageCommands.Count; index++)
    {
      DamageCommand command = _commands.DamageCommands[index];
      ref HealthComponent health = ref World.Get<HealthComponent>(command.Target);
      DefenseComponent defense = World.Get<DefenseComponent>(command.Target);
      int appliedAmount = _damageCalculationSystem.CalculateAppliedAmount(
        command.Amount,
        defense.Value,
        DamageTargetKind.Npc,
        _worldRules);
      health.Current = Math.Max(0, health.Current - appliedAmount);

      UpdateNpcReplication(command.Target);
    }

    using CommandBuffer commandBuffer = new();
    HashSet<Entity> despawnedEntities = new();
    for (int index = 0; index < _commands.DespawnEntityCommands.Count; index++)
    {
      DespawnEntityCommand command = _commands.DespawnEntityCommands[index];
      if (!despawnedEntities.Add(command.Target))
      {
        continue;
      }

      MarkProjectileInactive(command.Target);
      commandBuffer.Destroy(command.Target);
    }

    commandBuffer.Playback(World);
    WorldGrid.CommitTileChanges();
    _commands.Clear();
  }

  private void CommitWorldMeteorImpacts()
  {
    IReadOnlyCollection<WorldMeteorOccupant> occupants = CreateMeteorOccupants();
    for (int index = 0; index < _worldMeteorImpactRequests.Count; index++)
    {
      WorldMeteorImpactCommand impact = _worldMeteorImpactRequests[index];
      if (!_worldMeteorImpactSystem.TryCreateCommands(
            WorldGrid,
            impact,
            occupants,
            _nextWiringSequence,
            out IReadOnlyList<TileChangeCommand> changes,
            out _))
      {
        continue;
      }

      if (!TryAdvanceWiringSequence(changes.Count))
      {
        _worldMeteorImpactRequests.Clear();
        return;
      }

      for (int commandIndex = 0; commandIndex < changes.Count; commandIndex++)
      {
        WorldGrid.EnqueueTileChange(changes[commandIndex]);
      }
    }

    _worldMeteorImpactRequests.Clear();
  }

  private void ResolveScheduledWorldMeteorImpacts()
  {
    for (int index = 0; index < _scheduledMeteorImpactRequests.Count; index++)
    {
      WorldMeteorImpactCommand impact = _scheduledMeteorImpactRequests[index];
      if (!_worldMeteorScheduleSystem.IsReadyForResolution(
            _worldClock.CreateSnapshot(),
            _worldProgression,
            impact))
      {
        continue;
      }

      _worldMeteorImpactRequests.Add(impact);
      _worldProgression = _worldProgression.WithMeteorScheduled(false);
      break;
    }

    _scheduledMeteorImpactRequests.Clear();
  }

  private IReadOnlyCollection<WorldMeteorOccupant> CreateMeteorOccupants()
  {
    List<WorldMeteorOccupant> occupants = new(_players.Count + _npcs.Count);
    foreach (Entity playerEntity in GetActivePlayers())
    {
      TransformComponent transform = World.Get<TransformComponent>(playerEntity);
      ColliderComponent collider = World.Get<ColliderComponent>(playerEntity);
      occupants.Add(new WorldMeteorOccupant(
        transform.X,
        transform.Y,
        collider.Width,
        collider.Height));
    }

    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      Entity npcEntity = entry.Value;
      TransformComponent transform = World.Get<TransformComponent>(npcEntity);
      NpcComponents.NpcDefinitionComponent definition =
        World.Get<NpcComponents.NpcDefinitionComponent>(npcEntity);
      NpcDefinition npc = _npcDefinitions.GetRequired(definition.DefinitionId);
      occupants.Add(new WorldMeteorOccupant(
        transform.X,
        transform.Y,
        npc.ColliderWidth,
        npc.ColliderHeight));
    }

    return occupants;
  }

  private void CommitNpcSpawnCommands()
  {
    for (int index = 0; index < _commands.SpawnNpcCommands.Count; index++)
    {
      SpawnNpcCommand command = _commands.SpawnNpcCommands[index];
      int replicationId = command.RequestedReplicationId > 0
        ? command.RequestedReplicationId
        : _nextNpcHandle;
      if (replicationId <= 0 || replicationId == int.MaxValue ||
          _npcReplications.ContainsKey(new NpcHandle(replicationId)))
      {
        continue;
      }

      if (!_npcSpawnCommitSystem.TryCommit(
            World,
            WorldGrid,
            _npcDefinitions,
            command,
            replicationId,
            out NpcSpawnCommitResult result,
            out _))
      {
        continue;
      }

      NpcHandle npc = new(replicationId);
      NpcDefinition definition = _npcDefinitions.GetRequired(command.DefinitionId);
      HealthComponent health = World.Get<HealthComponent>(result.Entity);
      _npcs.Add(npc, result.Entity);
      _npcReplications.Add(npc, new NpcReplicationSnapshot(
        replicationId,
        definition.NetId,
        command.Position,
        new SimulationVector(0.0f, 0.0f),
        health.Current,
        IsActive: true,
        Revision: 1,
        GetSectionCoordinates(command.Position)));
      _nextNpcHandle = Math.Max(_nextNpcHandle, replicationId + 1);

      if (command.Source == NpcComponents.NpcSpawnSource.TileEntity &&
          _pendingTrainingDummyActivations.Count > 0)
      {
        int entityId = _pendingTrainingDummyActivations[0];
        _pendingTrainingDummyActivations.RemoveAt(0);
        TrainingDummyNpcLinkSnapshot link = new(
          IsActive: true,
          NpcType: definition.NetId,
          AiTileX: (int)command.Position.X,
          AiTileY: (int)command.Position.Y);
        TryLinkTrainingDummyNpc(entityId, npc, link, out _);
      }
    }

    _pendingTrainingDummyActivations.Clear();
  }

  private void CommitNpcDespawnCommands()
  {
    for (int index = 0; index < _commands.DespawnNpcCommands.Count; index++)
    {
      DespawnNpcCommand command = _commands.DespawnNpcCommands[index];
      if (!_npcs.TryGetValue(command.Npc, out Entity entity) ||
          !_npcReplications.TryGetValue(command.Npc, out NpcReplicationSnapshot current) ||
          !current.IsActive || current.Revision == long.MaxValue)
      {
        continue;
      }

      ref NpcComponents.NpcLifecycleComponent lifecycle =
        ref World.Get<NpcComponents.NpcLifecycleComponent>(entity);
      lifecycle.IsActive = false;
      lifecycle.DespawnReason = command.Reason;
      _npcReplications[command.Npc] = current with { IsActive = false, Revision = current.Revision + 1 };
      ClearTrainingDummyOwnershipForNpc(command.Npc, current with { IsActive = false });
    }
  }

  private void ClearTrainingDummyOwnershipForNpc(
    NpcHandle npc,
    NpcReplicationSnapshot replication)
  {
    foreach (KeyValuePair<int, TrainingDummyOwnershipState> entry in _trainingDummyOwnerships)
    {
      if (entry.Value.Npc != npc ||
          !_tileEntities.TryGetValue(entry.Key, out TileEntityPersistentState? persistent) ||
          !TrainingDummyTileEntityState.TryRead(
            persistent,
            out TrainingDummyTileEntityState entity))
      {
        continue;
      }

      TrainingDummyNpcLinkSnapshot link = new(
        replication.IsActive,
        replication.NpcType,
        entity.TileX,
        entity.TileY);
      _ = TryClearTrainingDummyNpc(entity.EntityId, link, out _);
    }
  }

  private void CommitPlayerInteractions()
  {
    for (int index = 0; index < _commands.UsePlayerInteractionCommands.Count; index++)
    {
      UsePlayerInteractionCommand command = _commands.UsePlayerInteractionCommands[index];
      if (!_players.TryGetValue(command.Player, out Entity entity))
      {
        continue;
      }

      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entity);
      PlayerInteractionComponent interaction = World.Get<PlayerInteractionComponent>(entity);
      if (!lifecycle.IsActive || !interaction.HasTarget || interaction.TargetId != command.TargetId ||
          interaction.Mode != command.Mode || interaction.TargetPosition != command.TargetPosition)
      {
        continue;
      }
    }
  }

  private void QueueItemUses()
  {
    foreach (KeyValuePair<PlayerHandle, Entity> entry in _players)
    {
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entry.Value);
      if (!lifecycle.IsActive)
      {
        continue;
      }

      PlayerInputComponent input = World.Get<PlayerInputComponent>(entry.Value);
      if (input.UseItem)
      {
        InventoryComponent inventory = World.Get<InventoryComponent>(entry.Value);
        _commands.Enqueue(new UseItemCommand(entry.Key, inventory.SelectedSlot));
      }
    }
  }

  private void AdvanceItemUseCooldowns()
  {
    foreach (Entity entity in _players.Values)
    {
      ref PlayerControlStateComponent control = ref World.Get<PlayerControlStateComponent>(entity);
      if (control.ItemUseCooldownTicks > 0)
      {
        control.ItemUseCooldownTicks--;
      }

      ref Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent useState =
        ref World.Get<Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent>(entity);
      _itemUseCooldownSystem.Tick(ref useState);
    }
  }

  private void CommitInventoryCommands()
  {
    for (int index = 0; index < _commands.TransferItemCommands.Count; index++)
    {
      TransferItemCommand command = _commands.TransferItemCommands[index];
      if (!TryGetActiveInventory(command.Player, out InventoryComponent inventory))
      {
        continue;
      }

      ItemInstanceSnapshot[] inventoryBefore = CaptureInventorySlots(inventory);
      if (_inventoryCommandSystem.TryTransfer(
            inventory,
            command.SourceSlot,
            command.DestinationSlot,
            command.Quantity,
            _itemDefinitions,
            out _))
      {
        PublishInventoryChanges(command.Player, inventory, inventoryBefore);
      }
    }

    for (int index = 0; index < _commands.SplitItemStackCommands.Count; index++)
    {
      SplitItemStackCommand command = _commands.SplitItemStackCommands[index];
      if (!TryGetActiveInventory(command.Player, out InventoryComponent inventory))
      {
        continue;
      }

      ItemInstanceSnapshot[] inventoryBefore = CaptureInventorySlots(inventory);
      if (_inventoryCommandSystem.TrySplit(
            inventory,
            command.SourceSlot,
            command.DestinationSlot,
            command.Quantity,
            _itemDefinitions,
            out _))
      {
        PublishInventoryChanges(command.Player, inventory, inventoryBefore);
      }
    }

    for (int index = 0; index < _commands.MergeItemStackCommands.Count; index++)
    {
      MergeItemStackCommand command = _commands.MergeItemStackCommands[index];
      if (!TryGetActiveInventory(command.Player, out InventoryComponent inventory))
      {
        continue;
      }

      ItemInstanceSnapshot[] inventoryBefore = CaptureInventorySlots(inventory);
      if (_inventoryCommandSystem.TryMerge(
            inventory,
            command.SourceSlot,
            command.DestinationSlot,
            command.Quantity,
            _itemDefinitions,
            out _))
      {
        PublishInventoryChanges(command.Player, inventory, inventoryBefore);
      }
    }

    CommitItemDrops();
  }

  private bool TryGetActiveInventory(
    PlayerHandle player,
    out InventoryComponent inventory)
  {
    if (!_players.TryGetValue(player, out Entity playerEntity) ||
        !World.Has<InventoryComponent>(playerEntity))
    {
      inventory = null!;
      return false;
    }

    inventory = World.Get<InventoryComponent>(playerEntity);
    PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(playerEntity);
    return lifecycle.IsActive;
  }

  private void ValidatePlayerCommandPlayer(PlayerHandle player)
  {
    if (!_players.ContainsKey(player))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }
  }

  private void CommitItemDrops()
  {
    for (int index = 0; index < _commands.DropItemCommands.Count; index++)
    {
      DropItemCommand command = _commands.DropItemCommands[index];
      if (!TryGetActiveInventory(command.Player, out InventoryComponent inventory) ||
          command.SourceSlot < 0 || command.SourceSlot >= InventoryComponent.SlotCount ||
          command.Quantity <= 0)
      {
        continue;
      }

      ItemStack stack = inventory.GetSlot(command.SourceSlot);
      if (stack.IsEmpty || command.Quantity > stack.Quantity ||
          !_itemDefinitions.TryGet(stack.ItemType, out _))
      {
        continue;
      }

      Entity playerEntity = _players[command.Player];
      TransformComponent transform = World.Get<TransformComponent>(playerEntity);
      ItemInstanceStateComponent instanceState = inventory.GetInstanceState(command.SourceSlot);
      ItemInstanceSnapshot[] inventoryBefore = CaptureInventorySlots(inventory);
      CommitWorldItemSpawn(new CreateWorldItemCommand(
        stack.WithQuantity(command.Quantity),
        new SimulationVector(transform.X, transform.Y),
        GetSectionCoordinates(new SimulationVector(transform.X, transform.Y)),
        command.Player.Value,
        instanceState));
      inventory.SetSlot(command.SourceSlot, stack.WithQuantity(stack.Quantity - command.Quantity));
      PublishInventoryChanges(command.Player, inventory, inventoryBefore);
      _itemDroppedEvents.Add(new ItemDroppedEvent(
        command.Player.Value,
        stack.ItemType,
        command.Quantity,
        TickNumber));
    }
  }

  private void CommitExtractinatorUses()
  {
    for (int index = 0; index < _commands.UseExtractinatorCommands.Count; index++)
    {
      UseExtractinatorCommand command = _commands.UseExtractinatorCommands[index];
      if (_extractinatorTargetsCommittedThisTick.Contains((command.TargetX, command.TargetY)) ||
          !_players.TryGetValue(command.Player, out Entity playerEntity) ||
          !World.Has<InventoryComponent>(playerEntity) ||
          !WorldGrid.Contains(command.TargetX, command.TargetY))
      {
        continue;
      }

      InventoryComponent inventory = World.Get<InventoryComponent>(playerEntity);
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(playerEntity);
      if (!lifecycle.IsActive || command.SourceSlot != inventory.SelectedSlot ||
          command.SourceSlot < 0 || command.SourceSlot >= InventoryComponent.SlotCount)
      {
        continue;
      }

      TransformComponent transform = World.Get<TransformComponent>(playerEntity);
      float deltaX = transform.X - command.TargetX;
      float deltaY = transform.Y - command.TargetY;
      if (deltaX * deltaX + deltaY * deltaY > MaximumInteractionRange * MaximumInteractionRange)
      {
        continue;
      }

      WorldTile target = WorldGrid.GetTile(command.TargetX, command.TargetY);
      ItemStack input = inventory.GetSlot(command.SourceSlot);
      if (!target.IsActive || input.IsEmpty || !_itemDefinitions.TryGet(input.ItemType, out ItemDefinition definition))
      {
        continue;
      }

      int extractionMode = definition.Extractinator?.ExtractionMode ?? -1;
      if (target.Type == ExtractinatorSystem.ExtractinatorTileType && extractionMode < 0 &&
          !_extractinatorRules.TryGetMode(input.ItemType, out extractionMode))
      {
        continue;
      }

      int seed = unchecked(_worldSeed.Value ^ (int)TickNumber ^ (int)command.Sequence ^
        input.ItemType * 1103515245 ^ command.TargetX * 486187739 ^ command.TargetY);
      ExtractinatorResult result = _extractinatorSystem.Roll(
        _extractinatorRules,
        extractionMode,
        target.Type,
        input.ItemType,
        _worldProgression.IsHardMode,
        new ExtractinatorRandom(seed));
      if (!result.IsAccepted || result.Output.IsEmpty ||
          !_itemDefinitions.TryGet(result.Output.ItemType, out _))
      {
        continue;
      }

      _extractinatorTargetsCommittedThisTick.Add((command.TargetX, command.TargetY));

      ItemInstanceSnapshot[] inventoryBefore = CaptureInventorySlots(inventory);
      inventory.SetSlot(command.SourceSlot, input.WithQuantity(input.Quantity - 1));
      int remainder = result.Output.Quantity;
      for (int slot = 0; slot < InventoryComponent.SlotCount && remainder > 0; slot++)
      {
        remainder = _inventoryTransfers.TransferIntoSlot(
          inventory,
          slot,
          new ItemStack(result.Output.ItemType, remainder),
          _itemDefinitions);
      }

      if (remainder > 0)
      {
        _ = CommitWorldItemSpawn(new CreateWorldItemCommand(
          new ItemStack(result.Output.ItemType, remainder),
          new SimulationVector(command.TargetX, command.TargetY),
          GetSectionCoordinates(new SimulationVector(command.TargetX, command.TargetY)),
          command.Player.Value));
      }

      PublishInventoryChanges(command.Player, inventory, inventoryBefore);
      _extractinatorResultEvents.Add(new ExtractinatorResultEvent(
        command.Player,
        input.WithQuantity(1),
        result.Output,
        target.Type,
        command.Sequence));
    }
  }

  private void CommitTriggeredExtractinators()
  {
    for (int index = 0; index < _commands.TriggerExtractinatorCommands.Count; index++)
    {
      TriggerExtractinatorCommand command = _commands.TriggerExtractinatorCommands[index];
      if (!WorldGrid.Contains(command.TargetX, command.TargetY) ||
          _extractinatorTargetsCommittedThisTick.Contains((command.TargetX, command.TargetY)) ||
          (_extractinatorCooldowns.TryGetValue((command.TargetX, command.TargetY), out long lastTick) &&
           TickNumber - lastTick < 60))
      {
        continue;
      }

      WorldTile sourceTile = WorldGrid.GetTile(command.TargetX, command.TargetY);
      if (!sourceTile.IsActive ||
          (sourceTile.Type != ExtractinatorSystem.ExtractinatorTileType &&
           sourceTile.Type != ExtractinatorSystem.ChlorophyteExtractinatorTileType))
      {
        continue;
      }

      int originX = command.TargetX - (sourceTile.FrameX % 54) / 18;
      int originY = command.TargetY - (sourceTile.FrameY % 54) / 18;
      ChestComponent? chest = FindExtractinatorChest(originX, originY);
      if (chest is null)
      {
        continue;
      }

      for (int slot = ChestComponent.SlotCount - 1; slot >= 0; slot--)
      {
        ItemStack input = chest.GetSlot(slot);
        if (input.IsEmpty || !_itemDefinitions.TryGet(input.ItemType, out ItemDefinition definition))
        {
          continue;
        }

        int extractionMode = definition.Extractinator?.ExtractionMode ?? -1;
        if (sourceTile.Type == ExtractinatorSystem.ExtractinatorTileType && extractionMode < 0 &&
            !_extractinatorRules.TryGetMode(input.ItemType, out extractionMode))
        {
          continue;
        }

        int seed = unchecked(_worldSeed.Value ^ (int)TickNumber ^ (int)command.Sequence ^
          input.ItemType * 1103515245 ^ originX * 486187739 ^ originY);
        ExtractinatorResult result = _extractinatorSystem.Roll(
          _extractinatorRules,
          extractionMode,
          sourceTile.Type,
          input.ItemType,
          _worldProgression.IsHardMode,
          new ExtractinatorRandom(seed));
        if (!result.IsAccepted || result.Output.IsEmpty ||
            !_itemDefinitions.TryGet(result.Output.ItemType, out _))
        {
          continue;
        }

        if (!chest.TryIncrementRevision())
        {
          continue;
        }

        chest.SetSlot(slot, input.WithQuantity(input.Quantity - 1));
        _ = CommitWorldItemSpawn(new CreateWorldItemCommand(
          result.Output,
          new SimulationVector(originX, originY),
          GetSectionCoordinates(new SimulationVector(originX, originY)),
          0));
        _extractinatorCooldowns[(command.TargetX, command.TargetY)] = TickNumber;
        _extractinatorTargetsCommittedThisTick.Add((command.TargetX, command.TargetY));
        _extractinatorResultEvents.Add(new ExtractinatorResultEvent(
          default,
          input.WithQuantity(1),
          result.Output,
          sourceTile.Type,
          command.Sequence,
          ExtractinatorSourceKind.Wiring));
        break;
      }
    }
  }

  private ChestComponent? FindExtractinatorChest(int originX, int originY)
  {
    ChestComponent? selected = null;
    foreach (ChestComponent chest in _chests.Values)
    {
      if (chest.IsLocked || chest.Opener is not null || chest.TileX < originX - 2 ||
          chest.TileX > originX + 5 ||
          chest.TileY < originY - 2 || chest.TileY > originY + 5)
      {
        continue;
      }

      if (selected is null || chest.ChestId < selected.ChestId)
      {
        selected = chest;
      }
    }

    return selected;
  }

  private void CommitItemUses()
  {
    for (int index = 0; index < _commands.UseItemCommands.Count; index++)
    {
      UseItemCommand command = _commands.UseItemCommands[index];
      if (!_players.TryGetValue(command.Player, out Entity playerEntity) ||
          !World.Has<InventoryComponent>(playerEntity))
      {
        continue;
      }

      InventoryComponent inventory = World.Get<InventoryComponent>(playerEntity);
      if (command.SelectedSlot != inventory.SelectedSlot)
      {
        continue;
      }
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(playerEntity);
      ItemInputValidationResult inputValidation = _itemInputValidationSystem.Validate(
        lifecycle.IsActive,
        inventory,
        command.SelectedSlot,
        _itemDefinitions);
      if (!inputValidation.IsAccepted ||
          !_itemDefinitions.TryGet(inputValidation.Stack.ItemType, out ItemDefinition definition))
      {
        continue;
      }

      bool consumesAmmo = TryGetRequiredAmmo(definition, out ushort ammoType);
      if (consumesAmmo &&
          !_itemAmmoConsumptionSystem.HasAmmo(inventory, ammoType, _itemDefinitions))
      {
        continue;
      }

      ref Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent useState =
        ref World.Get<Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent>(playerEntity);
      Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent originalUseState = useState;
      BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(playerEntity);
      ref HealthComponent health = ref World.Get<HealthComponent>(playerEntity);
      ref ManaComponent mana = ref World.Get<ManaComponent>(playerEntity);
      ItemUseResult result = _itemUseSystem.TryUse(
        command.Player,
        command.SelectedSlot,
        inputValidation.Stack,
        definition,
        ref useState,
        health.Current,
        health.Maximum,
        mana.Current,
        mana.Maximum,
        command.Sequence);
      if (!result.IsAccepted)
      {
        continue;
      }

      if (result.ProjectileType != 0 &&
          !_projectileDefinitions.TryGet(result.ProjectileType, out ProjectileDefinition _))
      {
        useState = originalUseState;
        continue;
      }

      if (result.BuffType != 0 && !buffs.CanAccept(result.BuffType))
      {
        useState = originalUseState;
        continue;
      }

      ItemInstanceSnapshot[] inventoryBefore = CaptureInventorySlots(inventory);
      if (consumesAmmo &&
          !_itemAmmoConsumptionSystem.TryConsume(inventory, ammoType, _itemDefinitions, out _))
      {
        useState = originalUseState;
        continue;
      }

      health.Current = result.Health;
      mana.Current = result.Mana;
      if (result.BuffType != 0)
      {
        buffs.Add(result.BuffType, result.BuffDurationTicks, command.Player);
      }
      if (result.ProjectileType != 0)
      {
        TransformComponent transform = World.Get<TransformComponent>(playerEntity);
        FacingComponent facing = World.Get<FacingComponent>(playerEntity);
        int projectileDamage = definition.Combat?.Damage ?? ProjectileDamage;
        _commands.Enqueue(new SpawnProjectileCommand(
          command.Player,
          transform.X,
          transform.Y + 0.75f,
          facing.Horizontal,
          projectileDamage,
          ProjectileLifetimeTicks,
          ProjectileType: result.ProjectileType,
          ProjectileSpeed: result.ProjectileSpeed));
      }
      if (result.ConsumedQuantity > 0)
      {
        inventory.SetSlot(
          command.SelectedSlot,
          inputValidation.Stack.WithQuantity(
            inputValidation.Stack.Quantity - result.ConsumedQuantity));
      }

      PublishInventoryChanges(command.Player, inventory, inventoryBefore);
      _itemUsedEvents.Add(result.Event);
    }
  }

  private static bool TryGetRequiredAmmo(ItemDefinition definition, out ushort ammoType)
  {
    if (definition.Use is ItemUseDefinition use && use.ConsumesAmmo)
    {
      ammoType = use.AmmoType;
      return true;
    }

    if (definition.Combat is ItemCombatDefinition combat && combat.ConsumesAmmo)
    {
      ammoType = combat.AmmoType;
      return true;
    }

    ammoType = 0;
    return false;
  }

  private static ItemInstanceSnapshot[] CaptureInventorySlots(InventoryComponent inventory)
  {
    ItemInstanceSnapshot[] slots = new ItemInstanceSnapshot[InventoryComponent.SlotCount];
    for (int slot = 0; slot < slots.Length; slot++)
    {
      slots[slot] = new ItemInstanceSnapshot(
        inventory.GetSlot(slot),
        inventory.GetInstanceState(slot));
    }

    return slots;
  }

  private void PublishInventoryChanges(
    PlayerHandle player,
    InventoryComponent inventory,
    IReadOnlyList<ItemInstanceSnapshot> before)
  {
    for (int slot = 0; slot < InventoryComponent.SlotCount; slot++)
    {
      ItemInstanceSnapshot previous = before[slot];
      ItemStack stack = inventory.GetSlot(slot);
      ItemInstanceStateComponent instanceState = inventory.GetInstanceState(slot);
      if (previous.Stack == stack && previous.State == instanceState)
      {
        continue;
      }

      _inventoryChangedEvents.Add(new InventoryChangedEvent(
        player,
        slot,
        stack,
        instanceState,
        inventory.Revision));
    }
  }

  private void CommitItemPlacements()
  {
    HashSet<(int X, int Y)> targets = new();
    for (int index = 0; index < _placeItemCommands.Count; index++)
    {
      PlaceItemCommand command = _placeItemCommands[index];
      if (!_players.TryGetValue(command.Player, out Entity playerEntity) ||
          !World.Has<InventoryComponent>(playerEntity) ||
          command.SourceSlot < 0 || command.SourceSlot >= InventoryComponent.SlotCount ||
          !WorldGrid.Contains(command.X, command.Y) ||
          !targets.Add((command.X, command.Y)))
      {
        continue;
      }

      InventoryComponent inventory = World.Get<InventoryComponent>(playerEntity);
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(playerEntity);
      ItemStack stack = inventory.GetSlot(command.SourceSlot);
      if (!lifecycle.IsActive || stack.IsEmpty ||
          !_itemDefinitions.TryGet(stack.ItemType, out ItemDefinition definition))
      {
        continue;
      }

      TransformComponent transform = World.Get<TransformComponent>(playerEntity);
      float deltaX = transform.X - command.X;
      float deltaY = transform.Y - command.Y;
      if (deltaX * deltaX + deltaY * deltaY > MaximumInteractionRange * MaximumInteractionRange)
      {
        continue;
      }

      WorldTile targetTile = WorldGrid.GetTile(command.X, command.Y);
      if (!_itemPlacementSystem.TryCreateTileCommand(
          definition,
          command.X,
          command.Y,
          command.Sequence,
          out TileChangeCommand tileCommand,
          out _))
      {
        continue;
      }

      if ((tileCommand.Kind == TileChangeKind.Place && targetTile.IsActive) ||
          (tileCommand.Kind == TileChangeKind.SetWall && targetTile.WallType != 0))
      {
        continue;
      }

      WorldGrid.EnqueueTileChange(tileCommand);
      ItemInstanceSnapshot[] inventoryBefore = CaptureInventorySlots(inventory);
      inventory.SetSlot(command.SourceSlot, stack.WithQuantity(stack.Quantity - 1));
      PublishInventoryChanges(command.Player, inventory, inventoryBefore);
    }

    _placeItemCommands.Clear();
  }

  private void CommitItemEquipment()
  {
    for (int index = 0; index < _equipItemCommands.Count; index++)
    {
      EquipItemCommand command = _equipItemCommands[index];
      if (!_players.TryGetValue(command.Player, out Entity playerEntity) ||
          !World.Has<InventoryComponent>(playerEntity) ||
          !World.Has<EquipmentStateCollectionComponent>(playerEntity))
      {
        continue;
      }

      InventoryComponent inventory = World.Get<InventoryComponent>(playerEntity);
      EquipmentStateCollectionComponent states =
        World.Get<EquipmentStateCollectionComponent>(playerEntity);
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(playerEntity);
      if (!lifecycle.IsActive || command.SourceSlot < 0 ||
          command.SourceSlot >= InventoryComponent.SlotCount)
      {
        continue;
      }

      ItemStack stack = inventory.GetSlot(command.SourceSlot);
      if (stack.IsEmpty || !_itemDefinitions.TryGet(stack.ItemType, out ItemDefinition definition) ||
          definition.Equipment is not ItemEquipmentDefinition equipmentDefinition ||
          states.Contains(equipmentDefinition.Slot))
      {
        continue;
      }

      if (!_itemEquipmentSystem.TryEquip(
            command.Player,
            stack,
            command.SourceSlot,
            equipmentDefinition,
            existing: null,
            command.IsVanity,
            out ItemEquipmentStateComponent state,
            out ItemEquippedEvent equippedEvent,
            out _))
      {
        continue;
      }

      states.Add(state);
      _itemEquippedEvents.Add(equippedEvent);
    }

    _equipItemCommands.Clear();
  }

  private void CommitItemUnequipment()
  {
    for (int index = 0; index < _unequipItemCommands.Count; index++)
    {
      UnequipItemCommand command = _unequipItemCommands[index];
      if (!_players.TryGetValue(command.Player, out Entity playerEntity) ||
          !World.Has<EquipmentStateCollectionComponent>(playerEntity))
      {
        continue;
      }

      EquipmentStateCollectionComponent states =
        World.Get<EquipmentStateCollectionComponent>(playerEntity);
      states.Remove(command.Slot);
    }

    _unequipItemCommands.Clear();
  }

  private void RecalculateEquipmentStats()
  {
    foreach (Entity playerEntity in _players.Values)
    {
      if (!World.Has<InventoryComponent>(playerEntity) ||
          !World.Has<EquipmentStateCollectionComponent>(playerEntity))
      {
        continue;
      }

      ref DefenseComponent defense = ref World.Get<DefenseComponent>(playerEntity);
      ref HealthRegenerationComponent healthRegeneration =
        ref World.Get<HealthRegenerationComponent>(playerEntity);
      EquipmentStateCollectionComponent equipmentStates =
        World.Get<EquipmentStateCollectionComponent>(playerEntity);
      InventoryComponent inventory = World.Get<InventoryComponent>(playerEntity);
      _equipmentStatSystem.Apply(
        ref defense,
        ref healthRegeneration,
        equipmentStates,
        inventory,
        _itemDefinitions);
    }
  }

  private void CommitItemPrefixes()
  {
    for (int index = 0; index < _itemPrefixCommands.Count; index++)
    {
      ApplyItemPrefixCommand command = _itemPrefixCommands[index];
      if (!_players.TryGetValue(command.Player, out Entity playerEntity) ||
          !World.Has<InventoryComponent>(playerEntity) ||
          command.SourceSlot < 0 || command.SourceSlot >= InventoryComponent.SlotCount)
      {
        continue;
      }

      InventoryComponent inventory = World.Get<InventoryComponent>(playerEntity);
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(playerEntity);
      ItemStack stack = inventory.GetSlot(command.SourceSlot);
      if (!lifecycle.IsActive || stack.IsEmpty)
      {
        continue;
      }

      ItemInstanceStateComponent state = inventory.GetInstanceState(command.SourceSlot);
      if (!_itemDefinitions.TryGet(stack.ItemType, out ItemDefinition definition))
      {
        continue;
      }

      if (!_itemPrefixSystem.TryApply(
            stack,
            definition,
            ref state,
            command.PrefixId,
            out ItemPrefixChangedEvent changedEvent,
            out _))
      {
        continue;
      }

      ItemInstanceSnapshot[] inventoryBefore = CaptureInventorySlots(inventory);
      inventory.SetInstanceState(command.SourceSlot, state);
      PublishInventoryChanges(command.Player, inventory, inventoryBefore);
      _itemPrefixChangedEvents.Add(changedEvent);
    }

    _itemPrefixCommands.Clear();
  }

  private void CommitItemVariants()
  {
    for (int index = 0; index < _itemVariantCommands.Count; index++)
    {
      ApplyItemVariantCommand command = _itemVariantCommands[index];
      if (!_players.TryGetValue(command.Player, out Entity playerEntity) ||
          !World.Has<InventoryComponent>(playerEntity) ||
          command.SourceSlot < 0 || command.SourceSlot >= InventoryComponent.SlotCount)
      {
        continue;
      }

      InventoryComponent inventory = World.Get<InventoryComponent>(playerEntity);
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(playerEntity);
      ItemStack stack = inventory.GetSlot(command.SourceSlot);
      if (!lifecycle.IsActive || stack.IsEmpty)
      {
        continue;
      }

      ItemInstanceStateComponent state = inventory.GetInstanceState(command.SourceSlot);
      if (!_itemVariantSystem.TryApply(
            stack,
            ref state,
            command.Variant,
            out ItemStack result,
            out _) || !_itemDefinitions.TryGet(result.ItemType, out _))
      {
        continue;
      }

      ItemInstanceSnapshot[] inventoryBefore = CaptureInventorySlots(inventory);
      inventory.SetSlot(command.SourceSlot, result);
      inventory.SetInstanceState(command.SourceSlot, state);
      PublishInventoryChanges(command.Player, inventory, inventoryBefore);
    }

    _itemVariantCommands.Clear();
  }

  private void CommitWorldItemMoves()
  {
    for (int index = 0; index < _commands.MoveWorldItemCommands.Count; index++)
    {
      MoveWorldItemCommand command = _commands.MoveWorldItemCommands[index];
      if (!_worldItems.TryGetValue(command.ReplicationId, out WorldItemComponent item) ||
          !_worldItemMotionSystem.TryMove(
            item,
            command,
            GetSectionCoordinates(command.Position),
            out WorldItemComponent moved))
      {
        continue;
      }

      _worldItems[command.ReplicationId] = moved;
    }
  }

  private void CommitWorldItemDestructions()
  {
    for (int index = 0; index < _commands.DestroyWorldItemCommands.Count; index++)
    {
      DestroyWorldItemCommand command = _commands.DestroyWorldItemCommands[index];
      if (!_worldItems.TryGetValue(command.ReplicationId, out WorldItemComponent item) ||
          !_worldItemDestroySystem.TryDestroy(
            item,
            command,
            out WorldItemComponent destroyed,
            out WorldItemDestroyedEvent destroyedEvent))
      {
        continue;
      }

      _worldItems[command.ReplicationId] = destroyed;
      _worldItemDestroyedEvents.Add(destroyedEvent);
    }
  }

  private void CommitWorldItemPassiveStacking()
  {
    List<int> worldItemIds = new(_worldItems.Keys);
    worldItemIds.Sort();
    for (int receiverIndex = 0; receiverIndex < worldItemIds.Count; receiverIndex++)
    {
      int receiverId = worldItemIds[receiverIndex];
      if (!_worldItems.TryGetValue(receiverId, out WorldItemComponent receiver) ||
          !receiver.IsActive)
      {
        continue;
      }

      for (int donorIndex = receiverIndex + 1; donorIndex < worldItemIds.Count; donorIndex++)
      {
        int donorId = worldItemIds[donorIndex];
        if (!_worldItems.TryGetValue(donorId, out WorldItemComponent donor) ||
            !_worldItemStackingSystem.TryMerge(
              receiver,
              donor,
              _itemDefinitions,
              TickNumber,
              WorldItemStackingRange,
              out WorldItemComponent mergedReceiver,
              out WorldItemComponent mergedDonor))
        {
          continue;
        }

        receiver = mergedReceiver;
        _worldItems[receiverId] = receiver;
        _worldItems[donorId] = mergedDonor;
        if (!receiver.IsActive)
        {
          break;
        }
      }
    }
  }

  private void CommitWorldItemPickups()
  {
    HashSet<int> pickupWinners = new();
    for (int index = 0; index < _commands.PickupWorldItemCommands.Count; index++)
    {
      PickupWorldItemCommand command = _commands.PickupWorldItemCommands[index];
      if (pickupWinners.Contains(command.WorldItemId))
      {
        continue;
      }

      if (!_players.TryGetValue(command.Player, out Entity playerEntity) ||
          !_worldItems.TryGetValue(command.WorldItemId, out WorldItemComponent item) ||
          !item.IsActive)
      {
        continue;
      }

      if (!World.Get<PlayerLifecycleComponent>(playerEntity).IsActive ||
          !World.Has<InventoryComponent>(playerEntity))
      {
        continue;
      }

      InventoryComponent inventory = World.Get<InventoryComponent>(playerEntity);
      TransformComponent playerTransform = World.Get<TransformComponent>(playerEntity);
      SimulationVector playerPosition = new(playerTransform.X, playerTransform.Y);
      ItemInstanceSnapshot[] inventoryBefore = CaptureInventorySlots(inventory);
      if (!_worldItemPickupSystem.TryPickup(
            ref item,
            command,
            playerPosition,
            inventory,
            _itemDefinitions,
            PickupRange,
            out WorldItemPickedUpEvent pickupEvent,
            out _))
      {
        continue;
      }

      _worldItems[command.WorldItemId] = item;
      _worldItems.RecordPickup(command.WorldItemId, command.Player);
      pickupWinners.Add(command.WorldItemId);
      PublishInventoryChanges(command.Player, inventory, inventoryBefore);
      _worldItemPickedUpEvents.Add(pickupEvent);
    }
  }

  private void AdvanceWorldItemPickupDelays()
  {
    List<int> worldItemIds = new(_worldItems.Keys);
    worldItemIds.Sort();
    for (int index = 0; index < worldItemIds.Count; index++)
    {
      int worldItemId = worldItemIds[index];
      if (!_worldItems.TryGetValue(worldItemId, out WorldItemComponent item) ||
          !_worldItemPickupDelaySystem.TryAdvance(item, out WorldItemComponent advanced))
      {
        continue;
      }

      _worldItems[worldItemId] = advanced;
    }
  }

  private void CommitNpcDamageCommands()
  {
    for (int index = 0; index < _commands.DamageNpcCommands.Count; index++)
    {
      DamageNpcCommand command = _commands.DamageNpcCommands[index];
      if (!_npcs.TryGetValue(command.Npc, out Entity npcEntity) ||
          !_npcReplications[command.Npc].IsActive)
      {
        continue;
      }

      ref HealthComponent health = ref World.Get<HealthComponent>(npcEntity);
      NpcComponents.NpcAuthorityComponent authority =
        World.Get<NpcComponents.NpcAuthorityComponent>(npcEntity);
      ref ImmunityComponent immunity = ref World.Get<ImmunityComponent>(npcEntity);
      DefenseComponent defense = World.Get<DefenseComponent>(npcEntity);
      bool applied = _damageResolutionSystem.TryResolve(
        ref health,
        defense,
        ref immunity,
        command.Amount,
        DamageTargetKind.Npc,
        _worldRules,
        out _);
      if (applied)
      {
        immunity.RemainingTicks = NpcHitImmunityTicks;
      }

      ref NpcComponents.NpcLifecycleComponent lifecycle =
        ref World.Get<NpcComponents.NpcLifecycleComponent>(npcEntity);
      _ = _npcLifecycleSystem.Advance(
        ref lifecycle,
        health.Current,
        authority.IsImmortal);
      UpdateNpcReplication(npcEntity);
    }
  }

  private void DetectProjectileHits()
  {
    List<DamageRequestedEvent> candidates = new();
    HashSet<Entity> tileDespawned = new();
    World.Query(
      in _projectileQuery,
      (Entity projectileEntity, ref TransformComponent projectileTransform,
        ref ColliderComponent projectileCollider, ref ProjectileDamageComponent damage,
        ref VelocityComponent projectileVelocity,
        ref ProjectileNetworkIdentityComponent identity,
        ref ProjectileDefinitionComponent definition) =>
      {
        if (!_projectileTargetEligibilitySystem.CanDamageNpc(definition))
        {
          return;
        }

        TransformComponent previousTransform = new(
          projectileTransform.X - projectileVelocity.X,
          projectileTransform.Y - projectileVelocity.Y);
        if (_projectileCollisionSystem.PathHitsSolidTile(
          WorldGrid,
          previousTransform,
          projectileTransform,
          projectileCollider))
        {
          _commands.Enqueue(new DespawnEntityCommand(projectileEntity));
          tileDespawned.Add(projectileEntity);
          return;
        }

        foreach (KeyValuePair<NpcHandle, Entity> npcEntry in _npcs)
        {
          Entity npcEntity = npcEntry.Value;
          HealthComponent npcHealth = World.Get<HealthComponent>(npcEntity);
          if (npcHealth.Current <= 0)
          {
            continue;
          }

          TransformComponent npcTransform = World.Get<TransformComponent>(npcEntity);
          ColliderComponent npcCollider = World.Get<ColliderComponent>(npcEntity);
          if (!PathOverlaps(
            previousTransform,
            projectileTransform,
            projectileCollider,
            npcTransform,
            npcCollider))
          {
            continue;
          }

          candidates.Add(new DamageRequestedEvent(
            projectileEntity,
            npcEntity,
            damage.Amount,
            identity.Identity,
            npcEntry.Key.Value));
          break;
        }
      });

    IReadOnlyList<DamageRequestedEvent> accepted = _projectileDamageSystem.Resolve(
      World,
      candidates,
      _projectileHitImmunity);
    for (int index = 0; index < accepted.Count; index++)
    {
      DamageRequestedEvent candidate = accepted[index];
      _commands.Enqueue(new DamageCommand(candidate.Projectile, candidate.Target, candidate.Amount));
      ref ProjectilePenetrationComponent penetration =
        ref World.Get<ProjectilePenetrationComponent>(candidate.Projectile);
      if (penetration.RemainingPenetration == 0 && tileDespawned.Add(candidate.Projectile))
      {
        _commands.Enqueue(new DespawnEntityCommand(candidate.Projectile));
      }
    }
  }

  private void AdvancePlayerLifecycle()
  {
    _playerLifecycleSystem.Advance(
      World,
      _players,
      (player, spawn) => _commands.Enqueue(new RespawnPlayerCommand(player, spawn)));
  }

  private void AdvancePlayerImmunity()
  {
    foreach (Entity entity in _players.Values)
    {
      ref ImmunityComponent immunity = ref World.Get<ImmunityComponent>(entity);
      _immunitySystem.Tick(ref immunity);
    }
  }

  private void AdvancePlayerBuffs(IReadOnlyList<Entity> activePlayers)
  {
    for (int index = 0; index < activePlayers.Count; index++)
    {
      Entity entity = activePlayers[index];
      BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(entity);
      _buffDurationSystem.Tick(buffs);
      World.Get<WellFedStateComponent>(entity).Update();
      ref ManaComponent mana = ref World.Get<ManaComponent>(entity);
      _buffEffectSystem.Apply(buffs, ref mana);
    }
  }

  public bool EatWellFed(PlayerHandle player, int foodRank, int foodBuffTime)
  {
    return ApplyConsumeWellFedCommand(new ConsumeWellFedCommand(player, foodRank, foodBuffTime));
  }

  public bool ApplyConsumeWellFedCommand(ConsumeWellFedCommand command)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(command.Player, out Entity entity) ||
        !World.Get<PlayerLifecycleComponent>(entity).IsActive)
    {
      return false;
    }

    World.Get<WellFedStateComponent>(entity).Eat(command.FoodRank, command.FoodBuffTime);
    SynchronizePlayerAccount(command.Player);
    return true;
  }

  public bool ClearWellFed(PlayerHandle player)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(player, out Entity entity) ||
        !World.Get<PlayerLifecycleComponent>(entity).IsActive)
    {
      return false;
    }

    World.Get<WellFedStateComponent>(entity).Clear();
    SynchronizePlayerAccount(player);
    return true;
  }

  public bool TryGetWellFedState(PlayerHandle player, out WellFedStateComponent? state)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(player, out Entity entity))
    {
      state = null;
      return false;
    }

    state = World.Get<WellFedStateComponent>(entity);
    return true;
  }

  private void DetectNpcContactDamage()
  {
    List<NpcContactCandidate> npcs = new(_npcs.Count);
    foreach (KeyValuePair<NpcHandle, Entity> npcEntry in _npcs)
    {
      if (!_npcReplications[npcEntry.Key].IsActive)
      {
        continue;
      }

      NpcComponents.NpcBehaviorStateComponent behavior =
        World.Get<NpcComponents.NpcBehaviorStateComponent>(npcEntry.Value);
      if (behavior.BehaviorId == NpcBehaviorId.TownHome)
      {
        continue;
      }

      TransformComponent npcTransform = World.Get<TransformComponent>(npcEntry.Value);
      ColliderComponent npcCollider = World.Get<ColliderComponent>(npcEntry.Value);
      npcs.Add(new NpcContactCandidate(
        npcEntry.Key,
        new SimulationVector(npcTransform.X, npcTransform.Y),
        npcCollider,
        IsActive: true));
    }

    List<PlayerContactCandidate> players = new(_players.Count);
    foreach (KeyValuePair<PlayerHandle, Entity> playerEntry in _players)
    {
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(playerEntry.Value);
      TransformComponent playerTransform = World.Get<TransformComponent>(playerEntry.Value);
      ColliderComponent playerCollider = World.Get<ColliderComponent>(playerEntry.Value);
      ImmunityComponent immunity = World.Get<ImmunityComponent>(playerEntry.Value);
      players.Add(new PlayerContactCandidate(
        playerEntry.Key,
        new SimulationVector(playerTransform.X, playerTransform.Y),
        playerCollider,
        lifecycle.IsActive,
        immunity.RemainingTicks));
    }

    IReadOnlyList<DamagePlayerCommand> commands = _npcContactEffectSystem.ProduceDamageCommands(
      npcs,
      players,
      NpcContactDamage);
    for (int index = 0; index < commands.Count; index++)
    {
      _commands.Enqueue(commands[index]);
    }
  }

  private void ApplyPlayerInputs(SimulationInputBatch inputBatch)
  {
    foreach (Entity entity in _players.Values)
    {
      ref PlayerInputComponent input = ref World.Get<PlayerInputComponent>(entity);
      input = new PlayerInputComponent();
    }

    for (int index = 0; index < inputBatch.Inputs.Count; index++)
    {
      PlayerInput supplied = inputBatch.Inputs[index];
      if (!_players.TryGetValue(supplied.Player, out Entity entity))
      {
        throw new ArgumentException("Input references an unknown player.", nameof(inputBatch));
      }

      ref PlayerInputComponent input = ref World.Get<PlayerInputComponent>(entity);
      input.MoveLeft = supplied.MoveLeft;
      input.MoveRight = supplied.MoveRight;
      input.Jump = supplied.Jump;
      input.Fire = supplied.Fire;
    }
  }

  private void MovePlayers()
  {
    World.Query(
      in _playerMovementQuery,
      (Entity entity, ref TransformComponent transform, ref VelocityComponent velocity) =>
      {
        transform.X += velocity.X;
        transform.Y += velocity.Y;
      });
  }

  private void ApplyPlayerGravity()
  {
    foreach (Entity entity in _players.Values)
    {
      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(entity);
      velocity.Y += GravityPerTick;
    }
  }

  private void ResolvePlayerGroundCollision()
  {
    foreach (Entity entity in _players.Values)
    {
      ref TransformComponent transform = ref World.Get<TransformComponent>(entity);
      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(entity);
      ref PhysicsStateComponent physics = ref World.Get<PhysicsStateComponent>(entity);
      if (transform.Y > 0.0f)
      {
        physics.IsGrounded = false;
        continue;
      }

      transform.Y = 0.0f;
      velocity.Y = 0.0f;
      physics.IsGrounded = true;
    }
  }

  private void SelectNpcTargets()
  {
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      if (!_npcReplications[entry.Key].IsActive)
      {
        continue;
      }

      Entity npcEntity = entry.Value;
      TransformComponent npcTransform = World.Get<TransformComponent>(npcEntity);
      Entity closestPlayer = default;
      PlayerHandle closestPlayerHandle = default;
      float closestDistanceSquared = float.MaxValue;
      bool hasTarget = false;

      foreach (KeyValuePair<PlayerHandle, Entity> playerEntry in _players)
      {
        Entity playerEntity = playerEntry.Value;
        HealthComponent playerHealth = World.Get<HealthComponent>(playerEntity);
        if (playerHealth.Current <= 0)
        {
          continue;
        }

        TransformComponent playerTransform = World.Get<TransformComponent>(playerEntity);
        float horizontalDistance = playerTransform.X - npcTransform.X;
        float verticalDistance = playerTransform.Y - npcTransform.Y;
        float distanceSquared = horizontalDistance * horizontalDistance +
          verticalDistance * verticalDistance;
        if (!hasTarget || distanceSquared < closestDistanceSquared ||
            distanceSquared == closestDistanceSquared &&
            playerEntry.Key.Value < closestPlayerHandle.Value)
        {
          closestDistanceSquared = distanceSquared;
          closestPlayer = playerEntity;
          closestPlayerHandle = playerEntry.Key;
          hasTarget = true;
        }
      }

      ref NpcTargetComponent target = ref World.Get<NpcTargetComponent>(npcEntity);
      target.HasTarget = hasTarget;
      target.Target = closestPlayer;
      ref NpcComponents.NpcTargetComponent typedTarget =
        ref World.Get<NpcComponents.NpcTargetComponent>(npcEntity);
      typedTarget = hasTarget
        ? new NpcComponents.NpcTargetComponent(
          closestPlayer,
          closestPlayerHandle.Value,
          NpcComponents.NpcTargetLockReason.NearestActivePlayer)
        : new NpcComponents.NpcTargetComponent(
          default,
          0,
          NpcComponents.NpcTargetLockReason.NoValidTarget);
    }
  }

  private void ApplyNpcAi()
  {
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      if (!_npcReplications[entry.Key].IsActive)
      {
        continue;
      }

      _npcMovementIntentSystem.Apply(World, entry.Value, IsDayTime);
    }
  }

  private void MoveNpcs()
  {
    World.Query(
      in _npcMovementQuery,
      (Entity entity, ref TransformComponent transform, ref VelocityComponent velocity) =>
      {
        transform.X += velocity.X;
        transform.Y += velocity.Y;
      });
  }

  private void AdvanceNpcLifecycles()
  {
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      ref NpcComponents.NpcLifecycleComponent lifecycle =
        ref World.Get<NpcComponents.NpcLifecycleComponent>(entry.Value);
      HealthComponent health = World.Get<HealthComponent>(entry.Value);
      NpcComponents.NpcAuthorityComponent authority =
        World.Get<NpcComponents.NpcAuthorityComponent>(entry.Value);
      if (!_npcReplications[entry.Key].IsActive &&
          (health.Current > 0 ||
            lifecycle.DespawnReason !=
              Terraria.Dome.Simulation.Npc.Components.NpcDespawnReason.None))
      {
        continue;
      }

      _ = _npcLifecycleSystem.Advance(
        ref lifecycle,
        health.Current,
        authority.IsImmortal);
    }
  }

  private PlayerHandle FindPlayerHandle(Entity entity)
  {
    foreach (KeyValuePair<PlayerHandle, Entity> entry in _players)
    {
      if (entry.Value == entity)
      {
        return entry.Key;
      }
    }

    throw new InvalidOperationException("Projectile owner did not map to a server player.");
  }

  private WorldSectionCoordinates GetSectionCoordinates(SimulationVector position)
  {
    int tileX = Math.Clamp((int)MathF.Floor(position.X), 0, WorldGrid.Width - 1);
    int tileY = Math.Clamp((int)MathF.Floor(position.Y), 0, WorldGrid.Height - 1);
    return WorldGrid.GetSectionCoordinates(tileX, tileY);
  }

  private void MarkProjectileInactive(Entity entity)
  {
    if (!_projectileIdsByEntity.Remove(entity, out int replicationId) ||
        !_projectileReplications.TryGetValue(replicationId, out ProjectileReplicationSnapshot snapshot) ||
        !snapshot.IsActive ||
        snapshot.Revision == long.MaxValue)
    {
      return;
    }

    _projectileReplications[replicationId] = snapshot with
    {
      IsActive = false,
      Revision = snapshot.Revision + 1
    };
  }

  private void MoveActiveNpcs()
  {
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      if (!_npcReplications[entry.Key].IsActive)
      {
        continue;
      }

      ref TransformComponent transform = ref World.Get<TransformComponent>(entry.Value);
      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(entry.Value);
      ref PhysicsStateComponent physics = ref World.Get<PhysicsStateComponent>(entry.Value);
      MovementIntentComponent intent = World.Get<MovementIntentComponent>(entry.Value);
      ColliderComponent collider = World.Get<ColliderComponent>(entry.Value);
      NpcComponents.NpcBehaviorStateComponent behavior =
        World.Get<NpcComponents.NpcBehaviorStateComponent>(entry.Value);
      if (behavior.BehaviorId == NpcBehaviorId.TrainingDummy)
      {
        velocity = new VelocityComponent(0.0f, 0.0f);
        continue;
      }

      if (intent.HasNpcIntent)
      {
        velocity.X = intent.NpcHorizontalVelocity;
        velocity.Y = intent.NpcVerticalVelocity + GravityPerTick;
        ref FacingComponent facing = ref World.Get<FacingComponent>(entry.Value);
        if (intent.HorizontalDirection != 0)
        {
          facing.Horizontal = intent.HorizontalDirection;
        }
      }

      _tileCollisionSystem.MoveAndResolve(
        WorldGrid,
        ref transform,
        ref velocity,
        ref physics,
        collider);
  }
  }

  private void RefreshNpcReplications()
  {
    foreach (Entity entity in _npcs.Values)
    {
      UpdateNpcReplication(entity);
    }
  }

  private void UpdateNpcReplication(Entity entity)
  {
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      if (entry.Value != entity)
      {
        continue;
      }

      NpcReplicationSnapshot current = _npcReplications[entry.Key];
      TransformComponent transform = World.Get<TransformComponent>(entity);
      VelocityComponent velocity = World.Get<VelocityComponent>(entity);
      HealthComponent health = World.Get<HealthComponent>(entity);
      NpcComponents.NpcLifecycleComponent lifecycle =
        World.Get<NpcComponents.NpcLifecycleComponent>(entity);
      SimulationVector position = new(transform.X, transform.Y);
      SimulationVector replicatedVelocity = new(velocity.X, velocity.Y);
      bool isActive = lifecycle.IsActive && health.Current > 0;
      NpcReplicationSnapshot updated = current with
      {
        Position = position,
        Velocity = replicatedVelocity,
        Health = health.Current,
        IsActive = isActive,
        Section = GetSectionCoordinates(position)
      };
      if (updated == current)
      {
        return;
      }

      if (current.Revision == long.MaxValue)
      {
        return;
      }

      updated = updated with { Revision = current.Revision + 1 };
      _npcReplications[entry.Key] = updated;
      return;
    }
  }

  private void UpdateProjectileReplication(
    Entity entity,
    TransformComponent transform,
    VelocityComponent velocity,
    ProjectileLifetimeComponent lifetime)
  {
    if (!_projectileIdsByEntity.TryGetValue(entity, out int replicationId) ||
        !_projectileReplications.TryGetValue(replicationId, out ProjectileReplicationSnapshot current))
    {
      return;
    }

    if (current.Revision == long.MaxValue)
    {
      return;
    }

    SimulationVector position = new(transform.X, transform.Y);
    ProjectileReplicationSnapshot updated = current with
    {
      Position = position,
      Velocity = new SimulationVector(velocity.X, velocity.Y),
      RemainingLifetime = lifetime.RemainingTicks,
      Section = GetSectionCoordinates(position),
      Revision = current.Revision + 1
    };
    _projectileReplications[replicationId] = updated;
  }

  private void SpawnNpcLoot(NpcDeathResult death)
  {
    ItemStack stack = _npcLootSystem.Roll(death.LootTableId, death.Npc.Value);
    CreateWorldItemCommand command = new(
      stack,
      death.Position,
      GetSectionCoordinates(death.Position),
      death.Npc.Value);
    _ = CommitWorldItemSpawn(command);
    _itemDroppedEvents.Add(new ItemDroppedEvent(
      death.Npc.Value,
      stack.ItemType,
      stack.Quantity,
      TickNumber));
  }

  private void RestoreNpc(NpcReplicationSnapshot snapshot)
  {
    RestoreNpc(NpcStateSnapshot.FromReplication(snapshot));
  }

  private void RestoreNpc(NpcStateSnapshot state)
  {
    NpcReplicationSnapshot snapshot = state.Replication;
    if (snapshot.ReplicationId == int.MaxValue || snapshot.Revision < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(state),
        "Persistence snapshot NPC identity must use a non-negative revision and leave room " +
        "for the next ID.");
    }

    NpcHandle npc = new(snapshot.ReplicationId);
    NpcComponents.NpcAuthorityComponent authority = ResolveNpcAuthority(state);
    if (!state.Lifecycle.IsActive &&
        state.Lifecycle.DespawnReason == NpcComponents.NpcDespawnReason.Killed)
    {
      _publishedNpcDeaths.Add(npc);
    }
    Entity entity = World.Create(
      new NpcTagComponent(),
      new TransformComponent(snapshot.Position.X, snapshot.Position.Y),
      new VelocityComponent(snapshot.Velocity.X, snapshot.Velocity.Y),
      new FacingComponent(-1),
      new ColliderComponent(1.0f, 2.0f),
      new PhysicsStateComponent { IsGrounded = snapshot.Position.Y <= 0.0f },
      new HealthComponent(snapshot.Health, state.MaximumHealth),
      new HealthRegenerationComponent(),
      new DefenseComponent(0),
      new ImmunityComponent(),
      new MovementIntentComponent(),
      new NpcTargetComponent(),
      new NpcAiStateComponent(1.0f),
      new NpcComponents.NpcDefinitionComponent(
        state.DefinitionId,
        state.NetId,
        state.Faction,
        state.Category),
      authority,
      new NpcComponents.NpcTargetComponent(
        default,
        state.TargetStableId,
        state.HasTarget
          ? NpcComponents.NpcTargetLockReason.RetainedValidTarget
          : NpcComponents.NpcTargetLockReason.NoValidTarget),
      state.Behavior,
      state.Spawn,
      state.Lifecycle,
      new NpcComponents.NpcReplicationComponent(snapshot.ReplicationId, snapshot.Revision));
    if (state.HasHome)
    {
      NpcComponents.NpcHomeComponent home = state.Home;
      World.Add(entity, in home);
    }

    if (state.HasSegment)
    {
      NpcComponents.NpcSegmentComponent segment = state.Segment;
      World.Add(entity, in segment);
    }
    _npcs.Add(npc, entity);
    _npcReplications.Add(npc, snapshot);
    _nextNpcHandle = Math.Max(_nextNpcHandle, snapshot.ReplicationId + 1);
  }

  private NpcComponents.NpcAuthorityComponent ResolveNpcAuthority(NpcStateSnapshot state)
  {
    if (_npcDefinitions.TryGet(state.DefinitionId, out NpcDefinition definition))
    {
      return new NpcComponents.NpcAuthorityComponent(
        definition.AiStyle,
        definition.IsImmortal,
        definition.AlwaysReplicate);
    }

    bool isTrainingDummy = state.NetId == 488 &&
      state.Behavior.BehaviorId == NpcBehaviorId.TrainingDummy;
    return new NpcComponents.NpcAuthorityComponent(
      isTrainingDummy ? 92 : 0,
      isTrainingDummy,
      isTrainingDummy);
  }

  private void RestoreChest(ChestPersistentState snapshot)
  {
    if (snapshot.ChestId == int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(
        nameof(snapshot),
        "Persistence snapshot chest IDs must leave room for the next ID.");
    }

    ChestComponent chest = new(
      snapshot.ChestId,
      snapshot.TileX,
      snapshot.TileY,
      snapshot.Revision,
      snapshot.Name);
    ChestCreateCommand command = new(
      NextChestMutationSequence(),
      snapshot.ChestId,
      snapshot.TileX,
      snapshot.TileY);
    if (!_chestMutationCommitSystem.TryRestore(
          WorldGrid,
          _chests,
          _chestIndexSystem,
          chest,
          command))
    {
      throw new ArgumentException(
        "Persistence snapshot contains duplicate chest IDs or coordinates.",
        nameof(snapshot));
    }

    for (int slot = 0; slot < snapshot.Slots.Count; slot++)
    {
      chest.SetSlot(slot, snapshot.Slots[slot]);
    }

    chest.SetLocked(snapshot.IsLocked);

    _nextChestId = Math.Max(_nextChestId, snapshot.ChestId + 1);
  }

  private void RestoreSign(SignPersistentState snapshot)
  {
    if (snapshot.SignId == int.MaxValue || snapshot.Revision < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(snapshot),
        "Persistence snapshot sign identity must use a non-negative revision and leave room " +
        "for the next ID.");
    }

    if (snapshot.SignId < 0 || !_signs.TryAdd(
      snapshot.SignId,
      new SignSnapshot(
        snapshot.SignId,
        snapshot.TileX,
        snapshot.TileY,
        snapshot.Text,
        snapshot.Revision,
        WorldGrid.GetSectionCoordinates(snapshot.TileX, snapshot.TileY))))
    {
      throw new ArgumentException("Persistence snapshot contains duplicate sign IDs.", nameof(snapshot));
    }

    _nextSignId = Math.Max(_nextSignId, snapshot.SignId + 1);
  }

  private void MoveProjectiles()
  {
    World.Query(
      in _projectileQuery,
      (Entity projectileEntity, ref TransformComponent transform,
        ref VelocityComponent velocity, ref ProjectileLifetimeComponent lifetime) =>
      {
        if (!_projectileBehaviorSystem.TryAdvance(
            projectileEntity,
            World,
            checked((int)TickNumber),
            out _))
        {
          _commands.Enqueue(new DespawnEntityCommand(projectileEntity));
          return;
        }

        transform = World.Get<TransformComponent>(projectileEntity);
        velocity = World.Get<VelocityComponent>(projectileEntity);
        bool expired = _projectileLifetimeSystem.Advance(projectileEntity, World);
        UpdateProjectileReplication(projectileEntity, transform, velocity, lifetime);
        if (expired)
        {
          _commands.Enqueue(new DespawnEntityCommand(projectileEntity));
        }
      });
  }

  private static bool Overlaps(
    TransformComponent firstTransform,
    ColliderComponent firstCollider,
    TransformComponent secondTransform,
    ColliderComponent secondCollider)
  {
    return firstTransform.X < secondTransform.X + secondCollider.Width &&
      firstTransform.X + firstCollider.Width > secondTransform.X &&
      firstTransform.Y < secondTransform.Y + secondCollider.Height &&
      firstTransform.Y + firstCollider.Height > secondTransform.Y;
  }

  private static bool PathOverlaps(
    TransformComponent previousTransform,
    TransformComponent currentTransform,
    ColliderComponent projectileCollider,
    TransformComponent targetTransform,
    ColliderComponent targetCollider)
  {
    float deltaX = currentTransform.X - previousTransform.X;
    float deltaY = currentTransform.Y - previousTransform.Y;
    int steps = Math.Max(1, (int)MathF.Ceiling(MathF.Max(MathF.Abs(deltaX), MathF.Abs(deltaY))));
    for (int index = 0; index <= steps; index++)
    {
      float progress = (float)index / steps;
      TransformComponent sample = new(
        previousTransform.X + deltaX * progress,
        previousTransform.Y + deltaY * progress);
      if (Overlaps(sample, projectileCollider, targetTransform, targetCollider))
      {
        return true;
      }
    }

    return false;
  }

  private void RequestProjectiles()
  {
    foreach (Entity playerEntity in _players.Values)
    {
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(playerEntity);
      if (!lifecycle.IsActive)
      {
        continue;
      }

      PlayerInputComponent input = World.Get<PlayerInputComponent>(playerEntity);
      ref PlayerControlStateComponent control = ref World.Get<PlayerControlStateComponent>(playerEntity);
      if (control.FireCooldownTicks > 0)
      {
        control.FireCooldownTicks--;
      }

      if (!input.Fire || control.FireCooldownTicks > 0)
      {
        continue;
      }

      TransformComponent transform = World.Get<TransformComponent>(playerEntity);
      VelocityComponent playerVelocity = World.Get<VelocityComponent>(playerEntity);
      FacingComponent facing = World.Get<FacingComponent>(playerEntity);
      PlayerHandle owner = FindPlayerHandle(playerEntity);
      _commands.Enqueue(new SpawnProjectileCommand(
        owner,
        transform.X,
        transform.Y + 0.75f,
        facing.Horizontal,
        ProjectileDamage,
        ProjectileLifetimeTicks,
        InitialVelocityY: playerVelocity.Y));
      control.FireCooldownTicks = 10;
    }
  }

  private void SynchronizePlayerAccount(PlayerHandle player)
  {
    if (!_playerAccountUuids.TryGetValue(player, out string? uuid) ||
        !_playerAccounts.TryGetValue(uuid, out PlayerPersistentState? account) ||
        !_players.TryGetValue(player, out Entity playerEntity) ||
        !World.Has<InventoryComponent>(playerEntity))
    {
      return;
    }

    InventoryComponent inventory = World.Get<InventoryComponent>(playerEntity);
    _itemInventorySanitizationSystem.Sanitize(inventory, _itemDefinitions);
    PlayerPersistentItem[] items = account.Items.ToArray();
    for (int slotId = 0; slotId < InventoryComponent.HotbarSlotCount; slotId++)
    {
      ItemStack runtimeItem = inventory.GetSlot(slotId);
      if (runtimeItem.IsEmpty)
      {
        items[slotId] = new PlayerPersistentItem(slotId, 0, 0, 0, false, false);
        continue;
      }

      PlayerPersistentItem persistentItem = items[slotId];
      Terraria.Dome.Simulation.Items.Components.ItemInstanceStateComponent instanceState =
        inventory.GetInstanceState(slotId);
      if (instanceState.PrefixId > byte.MaxValue)
      {
        throw new InvalidOperationException(
          "An item prefix cannot be represented by the V1456 persistence field.");
      }

      items[slotId] = persistentItem with
      {
        Stack = runtimeItem.Quantity,
        Prefix = (byte)instanceState.PrefixId,
        ItemType = runtimeItem.ItemType,
        IsFavorited = instanceState.IsFavorited,
        IsNewAndShiny = instanceState.IsNewAndShiny,
        VariantId = instanceState.VariantId,
        Dye = instanceState.Dye,
        Paint = instanceState.Paint,
        NameOverride = instanceState.NameOverride
      };
    }

    _playerAccounts[uuid] = new PlayerPersistentState(
      account.Uuid,
      account.Profile,
      account.Life,
      account.MaximumLife,
      account.Mana,
      account.MaximumMana,
      account.Buffs,
      account.SelectedLoadout,
      account.AccessoryVisibility,
      items,
      World.Get<WellFedStateComponent>(playerEntity).TimeLeftRank1,
      World.Get<WellFedStateComponent>(playerEntity).TimeLeftRank2,
      World.Get<WellFedStateComponent>(playerEntity).TimeLeftRank3);
  }

  private bool ApplyDoorTransition(DoorSnapshot door, DoorTransition transition)
  {
    if (door.Revision == long.MaxValue)
    {
      return false;
    }

    if (transition == DoorTransition.OpenDoor && !door.IsOpen)
    {
      _doors[door.DoorId] = door with { IsOpen = true, Revision = door.Revision + 1 };
      return true;
    }

    if (transition == DoorTransition.CloseDoor && door.IsOpen)
    {
      _doors[door.DoorId] = door with { IsOpen = false, Revision = door.Revision + 1 };
      return true;
    }

    return false;
  }

  private bool ApplyTallGateTransition(DoorSnapshot door, DoorTransition transition)
  {
    if (door.Revision == long.MaxValue)
    {
      return false;
    }

    ushort targetType = transition switch
    {
      DoorTransition.OpenTallGate when !door.IsOpen => OpenTallGateTileType,
      DoorTransition.CloseTallGate when door.IsOpen => ClosedTallGateTileType,
      _ => 0
    };
    if (targetType == 0)
    {
      return false;
    }

    for (int row = 0; row < 5; row++)
    {
      WorldTile tile = WorldGrid.GetTile(door.TileX, door.TileY + row);
      if (!tile.IsActive || tile.Type != (door.IsOpen ? OpenTallGateTileType : ClosedTallGateTileType))
      {
        return false;
      }
    }

    for (int row = 0; row < 5; row++)
    {
      WorldTile tile = WorldGrid.GetTile(door.TileX, door.TileY + row);
      _ = WorldGrid.TrySetTile(door.TileX, door.TileY + row, tile with { Type = targetType });
    }

    _doors[door.DoorId] = door with { IsOpen = !door.IsOpen, Revision = door.Revision + 1 };
    return true;
  }

  private bool ApplyTrapdoorTransition(
    DoorSnapshot door,
    DoorTransition transition,
    bool direction)
  {
    if (door.Revision == long.MaxValue)
    {
      return false;
    }

    if (transition == DoorTransition.OpenTrapdoor && !door.IsOpen)
    {
      return OpenTrapdoor(door);
    }

    if (transition == DoorTransition.CloseTrapdoor && door.IsOpen)
    {
      return CloseTrapdoor(door, direction);
    }

    return false;
  }

  private bool CloseTrapdoor(DoorSnapshot door, bool playerAbove)
  {
    if (door.Revision == long.MaxValue)
    {
      return false;
    }

    for (int column = 0; column < 2; column++)
    {
      WorldTile tile = WorldGrid.GetTile(door.TileX + column, door.TileY);
      if (!tile.IsActive || tile.Type != OpenTrapdoorTileType)
      {
        return false;
      }
    }

    int closedTopY = door.TileY - (playerAbove ? 0 : 1);
    int frameXOffset = playerAbove ? 36 : 0;
    for (int row = 0; row < 2; row++)
    {
      for (int column = 0; column < 2; column++)
      {
        _ = WorldGrid.TrySetTile(door.TileX + column, closedTopY + row, new WorldTile(
          true,
          ClosedTrapdoorTileType,
          FrameX: (short)(frameXOffset + column * 18),
          FrameY: (short)(row * 18)));
      }
    }

    _doors[door.DoorId] = door with
    {
      TileY = closedTopY,
      IsOpen = false,
      Revision = door.Revision + 1,
      Section = WorldGrid.GetSectionCoordinates(door.TileX, closedTopY)
    };
    return true;
  }

  private bool IsInDoorRange(DoorSnapshot door, SimulationVector playerPosition)
  {
    float deltaX = playerPosition.X - door.TileX;
    float deltaY = playerPosition.Y - door.TileY;
    return float.IsFinite(playerPosition.X) && float.IsFinite(playerPosition.Y) &&
      deltaX * deltaX + deltaY * deltaY <= 6.0f * 6.0f;
  }

  private static bool IsDoorAt(DoorSnapshot door, int tileX, int tileY)
  {
    int width = door.ObjectKind == DoorObjectKind.Trapdoor ? 2 : 1;
    int height = door.ObjectKind switch
    {
      DoorObjectKind.Trapdoor when !door.IsOpen => 2,
      DoorObjectKind.Trapdoor => 1,
      DoorObjectKind.TallGate => 5,
      _ => 1
    };
    return tileX >= door.TileX && tileX < door.TileX + width &&
           tileY >= door.TileY && tileY < door.TileY + height;
  }

  private sealed class WiringInputCommandComparer : IComparer<WiringInputCommand>
  {
    public static readonly WiringInputCommandComparer Instance = new();

    public int Compare(WiringInputCommand first, WiringInputCommand second)
    {
      int sequence = first.Sequence.CompareTo(second.Sequence);
      if (sequence != 0)
      {
        return sequence;
      }

      int color = first.Color.CompareTo(second.Color);
      if (color != 0)
      {
        return color;
      }

      int x = first.X.CompareTo(second.X);
      return x != 0 ? x : first.Y.CompareTo(second.Y);
    }
  }

  private bool OpenTrapdoor(DoorSnapshot door)
  {
    if (door.Revision == long.MaxValue)
    {
      return false;
    }

    bool opensDown = WorldGrid.GetTile(door.TileX, door.TileY).FrameX < 36;
    int openY = door.TileY + (opensDown ? 1 : 0);
    for (int row = 0; row < 2; row++)
    {
      for (int column = 0; column < 2; column++)
      {
        WorldTile tile = WorldGrid.GetTile(door.TileX + column, door.TileY + row);
        if (!tile.IsActive || tile.Type != ClosedTrapdoorTileType)
        {
          return false;
        }
      }
    }

    int clearedY = door.TileY + (opensDown ? 0 : 1);
    for (int column = 0; column < 2; column++)
    {
      _ = WorldGrid.TrySetTile(door.TileX + column, clearedY, default);
      _ = WorldGrid.TrySetTile(door.TileX + column, openY, new WorldTile(
        true,
        OpenTrapdoorTileType,
        FrameX: (short)(column * 18),
        FrameY: 0));
    }

    _doors[door.DoorId] = door with
    {
      TileY = openY,
      IsOpen = true,
      Revision = door.Revision + 1,
      Section = WorldGrid.GetSectionCoordinates(door.TileX, openY)
    };
    return true;
  }

  private void ThrowIfDisposed()
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(DomeSimulation));
    }
  }
}
