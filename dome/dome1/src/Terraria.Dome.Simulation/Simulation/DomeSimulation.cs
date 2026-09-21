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
using Terraria.Dome.Simulation.Player;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.Player.Definitions;
using Terraria.Dome.Simulation.Player.Commands;
using Terraria.Dome.Simulation.Player.Events;
using Terraria.Dome.Simulation.Player.Systems;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.StatusEffects.Components;
using Terraria.Dome.Simulation.StatusEffects.Commands;
using Terraria.Dome.Simulation.StatusEffects.Definitions;
using Terraria.Dome.Simulation.StatusEffects.Snapshots;
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
using Terraria.Dome.Simulation.Projectile.Commands;
using Terraria.Dome.Simulation.Projectile.Definitions;
using Terraria.Dome.Simulation.Projectile;
using Terraria.Dome.Simulation.Combat.Events;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.WorldObjects.Chest.Commands;
using Terraria.Dome.Simulation.WorldObjects.Chest.Systems;
using Terraria.Dome.Simulation.WorldObjects.Definitions;
using Terraria.Dome.Simulation.WorldObjects.Sign;
using Terraria.Dome.Simulation.WorldObjects.Sign.Commands;
using Terraria.Dome.Simulation.WorldObjects.Placement;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Npc.Events;
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

using EntityEcs.Components;
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
  private const int NpcLavaContactDamage = 50;
  private const int NpcHitImmunityTicks = 2;
  private const int NpcLavaImmunityTicks = 30;
  private const float PickupRange = 3.0f;
  private const float WorldItemStackingRange = 1.0f;
  private const float MaximumInteractionRange = 12.0f;
  private const int DomeChaserNpcType = 1;
  private const int VerificationProjectileType = 43;
  private const int Style17SignObjectType = 85;
  private const int Style17SignStyle = 0;
  private const int Style17SignWidth = 2;
  private const int Style17SignHeight = 2;
  private const float Style17GroundDrag = 0.98f;
  public const int FixtureNpcType = 2;
  public const int FixtureItemNpcSummonType = 13;
  public const int FixtureItemSentryType = 14;
  public const int FixtureItemSentryEquipmentType = 15;
  public const int FixtureItemMeleeUseType = 16;
  public const int FixtureItemNoMeleeType = 17;
  public const int FixtureItemFishingPoleType = 18;
  public const int FixtureItemDd2SummonType = 19;
  public const int FixtureItemPotionType = 20;
  public const int FixtureFishingBobberProjectileType = 1001;
  public const int FixtureDd2SummonProjectileType = 663;
  private const int DomeBoltProjectileType = 1;
  private const int MaximumSignCount = 32000;
  private const int TrainingDummyNpcType = 488;
  private const int Type658ChildProjectileType = 657;
  private const int Type658ChildDamage = 30;
  private const int Type658ChildExpertDamage = 22;
  private const float Type658ChildKnockback = 3.0f;
  private const float Type281ReleaseDifficultyScale = 1.0f;
  private readonly WorldClock _worldClock;
  private readonly WorldClockSystem _worldClockSystem = new();
  private readonly WorldGameUpdateCountProjection _gameUpdateCountProjection = new();
  private readonly WorldInvasionTravelSystem _worldInvasionTravelSystem = new();
  private readonly WorldProgressionSystem _worldProgressionSystem = new();
  private readonly WorldTimeRateSystem _worldTimeRateSystem = new();
  private readonly WorldInvasionStartEligibilitySystem _worldInvasionStartEligibilitySystem = new();
  private readonly WorldInvasionClearFlagSystem _worldInvasionClearFlagSystem = new();
  private readonly NpcEventSpawnSystem _npcEventSpawnSystem = new();
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
  private readonly Dictionary<int, NpcProjectileReplicationSnapshot> _npcProjectileReplications =
    new();
  private readonly Dictionary<string, PlayerPersistentState> _playerAccounts = new(
    StringComparer.Ordinal);
  private readonly Dictionary<PlayerHandle, string> _playerAccountUuids = new();
  private readonly WorldItemStore _worldItems;
  private readonly SimulationEntityLimits _entityLimits;
  private readonly Dictionary<int, DoorSnapshot> _doors = new();
  private readonly Dictionary<int, SignSnapshot> _signs = new();
  private readonly WorldObjectPlacementCommitSystem _worldObjectPlacementCommitSystem = new();
  private readonly HashSet<long> _committedWorldObjectPlacementSequences = new();
  private readonly List<WorldObjectPlacementCommittedEvent> _worldObjectPlacementEvents = new();
  private readonly Dictionary<Entity, ProjectileWorldObjectPlacementCommand>
    _pendingProjectileWorldObjectPlacements = new();
  private readonly Dictionary<long, Entity> _verificationPlacementProjectiles = new();
  private readonly Dictionary<int, SignTombstoneSnapshot> _signTombstones = new();
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
  private SimulationTickTrace? _lastTickTrace;
  private WorldClockTransition? _lastWorldClockTransition;
  private WorldEnvironmentTransition? _lastWorldEnvironmentTransition;
  private WorldProgressionTransition? _lastWorldProgressionTransition;
  private WorldInvasionTransition? _lastWorldInvasionTransition;
  private WorldMeteorTransition? _lastWorldMeteorTransition;
  private WorldSlimeRainTransition? _lastWorldSlimeRainTransition;
  private WorldLanternNightTransition? _lastWorldLanternNightTransition;
  private SimulationSnapshot? _lastPublishedSnapshot;
  private readonly List<PlayerDamagedEvent> _playerDamagedEvents = new();
  private readonly List<PlayerDiedEvent> _playerDiedEvents = new();
  private readonly List<PlayerRespawnedEvent> _playerRespawnedEvents = new();
  private readonly List<ItemUsedEvent> _itemUsedEvents = new();
  private readonly List<ShopPurchaseReceipt> _shopPurchaseReceipts = new();
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
  private readonly TileEntityStore _tileEntityStore = new();
  private readonly Dictionary<int, TrainingDummyOwnershipState> _trainingDummyOwnerships = new();
  private readonly List<int> _pendingTrainingDummyActivations = new();
  private readonly List<SpawnProjectileCommand> _pendingProjectileSpawnCommands = new();
  private readonly List<SpawnNpcCommand> _pendingNpcSpawnCommands = new();
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
      11,
      99,
      Placement: new ItemPlacementDefinition(TileType: 12, TileBoost: 3)),
    new ItemDefinition(
      12,
      99,
      Placement: new ItemPlacementDefinition(WallType: 7, TileBoost: 3)),
    new ItemDefinition(
      FixtureItemNpcSummonType,
      20,
      Use: new ItemUseDefinition(UseTime: 1, Consumable: true),
      Summoning: new ItemSummoningDefinition(NpcType: FixtureNpcType)),
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
    new ItemDefinition(
      FixtureItemSentryType,
      20,
      Use: new ItemUseDefinition(
        UseTime: 1,
        Consumable: true,
        ShootType: 2,
        ShootSpeed: 5.0f),
      Combat: new ItemCombatDefinition(Damage: 14),
      IsSentry: true),
    new ItemDefinition(
      FixtureItemSentryEquipmentType,
      1,
      Equipment: new ItemEquipmentDefinition(
        ItemEquipmentSlot.Accessory,
        Accessory: true,
        SentryCapacityBonus: 1)),
    new ItemDefinition(
      FixtureItemMeleeUseType,
      1,
      Use: new ItemUseDefinition(UseTime: 1, UseAnimation: 1),
      Combat: new ItemCombatDefinition(
        Damage: 10,
        DamageClass: ItemDamageClass.Melee)),
    new ItemDefinition(
      FixtureItemNoMeleeType,
      1,
      Use: new ItemUseDefinition(UseTime: 1, UseAnimation: 1),
      Combat: new ItemCombatDefinition(
        Damage: 10,
        DamageClass: ItemDamageClass.Melee,
        NoMelee: true)),
    new ItemDefinition(
      FixtureItemFishingPoleType,
      20,
      Use: new ItemUseDefinition(
        UseTime: 1,
        UseAnimation: 1,
        ShootType: FixtureFishingBobberProjectileType,
        ShootSpeed: 6.0f),
      Gathering: new ItemGatheringDefinition(FishingPolePower: 30)),
    new ItemDefinition(
      FixtureItemDd2SummonType,
      20,
      Use: new ItemUseDefinition(
        UseTime: 1,
        Consumable: true,
        ShootType: FixtureDd2SummonProjectileType,
        ShootSpeed: 1.0f),
      Combat: new ItemCombatDefinition(
        Damage: 17,
        Knockback: 3.0f,
        DamageClass: ItemDamageClass.Summon),
      IsSentry: true,
      Dd2Summon: true),
    new ItemDefinition(
      FixtureItemPotionType,
      20,
      Use: new ItemUseDefinition(
        UseTime: 1,
        UseAnimation: 1,
        Potion: true,
        Consumable: true,
        HealthRestore: 25)),
    ..LegacyFlaskDefinitionRegistry.CreateDefinitions(),
    ..LegacySentryEquipmentRegistry.CreateDefinitions(),
    ..LegacySentryArmorSetRegistry.SupplementalDefinitions,
    new ItemDefinition(9, 1),
    new ItemDefinition(
      10,
      99,
      Use: new ItemUseDefinition(
        UseTime: 1,
        AutoReuse: true,
        CooldownTicks: 3)),
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
  private readonly ItemPrefixSystem _itemPrefixSystem;
  private readonly ItemVariantSystem _itemVariantSystem;
  private readonly WorldItemPickupSystem _worldItemPickupSystem = new();
  private readonly WorldItemPickupDelaySystem _worldItemPickupDelaySystem = new();
  private readonly WorldItemSpawnSystem _worldItemSpawnSystem;
  private readonly WorldItemMotionSystem _worldItemMotionSystem = new();
  private readonly WorldItemDestroySystem _worldItemDestroySystem = new();
  private readonly ItemSelectionSystem _itemSelectionSystem = new();
  private readonly EquipmentStatSystem _equipmentStatSystem = new();
  private readonly PlayerSentryEquipmentSystem _playerSentryEquipmentSystem = new();
  private readonly PlayerSentryBuffSystem _playerSentryBuffSystem = new();
  private readonly PlayerSentryArmorSetSystem _playerSentryArmorSetSystem = new();
  private readonly PlayerSleepAuthoritySystem _playerSleepAuthoritySystem = new();
  private readonly PlayerSleepSystem _playerSleepSystem = new();
  private readonly PlayerFishingUseSystem _playerFishingUseSystem = new();
  private readonly PlayerPotionDelaySystem _playerPotionDelaySystem = new();
  private readonly PlayerNpcTargetingSystem _playerNpcTargetingSystem = new();
  private readonly NpcNoAggroCapabilityRegistry _npcNoAggroCapabilities =
    NpcNoAggroCapabilityRegistry.CreateVersion1456Item3090();
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
      DefinitionId: 26,
      NetId: 26,
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
      LootTableId: 1,
      Faction: NpcFaction.Town,
      Category: NpcCategory.Town,
      AiStyle: 7,
      IsLikeTownNpc: true),
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
      AlwaysReplicate: true),
    new NpcDefinition(
      DefinitionId: 614,
      NetId: 614,
      MaximumHealth: 100,
      Defense: 0,
      ColliderWidth: 1.0f,
      ColliderHeight: 1.0f,
      BehaviorId: NpcBehaviorId.FloatingEye,
      LootTableId: 1)],
    NpcTargetCapabilityRegistry.CreateVersion1456());
  private readonly NpcLootSystem _npcLootSystem;
  private readonly NpcTargetSelectionSystem _npcTargetSelectionSystem = new();
  private readonly NpcSystemPipeline _npcSystemPipeline = new();
  private readonly PlayerLifecycleSystem _playerLifecycleSystem = new();
  private readonly NpcSpawnCommitSystem _npcSpawnCommitSystem = new();
  private readonly NpcSlotAllocator _npcSlotAllocator = new();
  private readonly NpcSlotAccountingSystem _npcSlotAccountingSystem = new();
  private readonly NpcMovementIntentSystem _npcMovementIntentSystem = new();
  private readonly NpcHomeTimeoutSystem _npcHomeTimeoutSystem = new();
  private readonly NpcContactEffectSystem _npcContactEffectSystem = new();
  private readonly NpcLavaContactSystem _npcLavaContactSystem = new();
  private readonly NpcLifecycleSystem _npcLifecycleSystem = new();
  private readonly NpcDeathSystem _npcDeathSystem = new();
  private readonly NpcLootEmissionLedger _npcLootEmissionLedger = new();
  private readonly List<NpcDeathResult> _pendingNpcDeaths = new();
  private readonly List<string> _npcSpawnRejectionReasons = new();
  private readonly HashSet<NpcHandle> _publishedNpcDeaths = new();
  private readonly WorldSeed _worldSeed;
  private readonly GroundCollisionSystem _groundCollisionSystem = new();
  private readonly TileCollisionSystem _tileCollisionSystem = new();
  private readonly TopSlopeContactSystem _topSlopeContactSystem = new();
  private readonly MovementSystem _movementSystem = new();
  private readonly PlayerControlSystem _playerControlSystem = new();
  private readonly PlayerDeathSystem _playerDeathSystem = new();
  private readonly PlayerGravitySystem _playerGravitySystem = new();
  private readonly PlayerInputApplySystem _playerInputApplySystem = new();
  private readonly PlayerRespawnSystem _playerRespawnSystem = new();
  private readonly PlayerSentryAuthoritySystem _playerSentryAuthoritySystem = new();
  private readonly DamageResolutionSystem _damageResolutionSystem = new();
  private readonly ImmunitySystem _immunitySystem = new();
  private readonly PlayerVitalRegenSystem _playerVitalRegenSystem = new();
  private readonly BuffDurationSystem _buffDurationSystem = new();
  private readonly BuffEffectSystem _buffEffectSystem = new();
  private readonly ProjectileCollisionSystem _projectileCollisionSystem = new();
  private readonly ProjectileOwnerHitCheckSystem _projectileOwnerHitCheckSystem = new();
  private readonly ProjectileDefinitionRegistry _projectileDefinitions;
  private readonly ProjectileSpawnSystem _projectileSpawnSystem = new();
  private readonly ProjectileSentryPlacementSystem _projectileSentryPlacementSystem = new();
  private readonly ProjectileSentryLimitSystem _projectileSentryLimitSystem = new();
  private readonly NpcProjectileSpawnSystem _npcProjectileSpawnSystem = new();
  private readonly ProjectileLifetimeSystem _projectileLifetimeSystem = new();
  private readonly NpcProjectileLifetimeSystem _npcProjectileLifetimeSystem = new();
  private readonly ProjectileRestrikeDelaySystem _projectileRestrikeDelaySystem = new();
  private readonly ProjectileNetworkUpdatePolicy _projectileNetworkUpdatePolicy = new();
  private readonly ProjectileSoundDelayPolicy _projectileSoundDelayPolicy = new();
  private readonly ProjectileTileCollisionPolicy _projectileTileCollisionPolicy = new();
  private readonly ProjectileBehaviorSystem _projectileBehaviorSystem =
    ProjectileBehaviorSystem.CreateDefault();
  private readonly ProjectileOwnerAnchoredMeleeSystem _projectileOwnerAnchoredMeleeSystem = new();
  private readonly ProjectileBehaviorEffectSystem _projectileBehaviorEffectSystem = new();
  private readonly ProjectileReplicationSystem _projectileReplicationSystem = new();
  private readonly NpcProjectileReplicationSystem _npcProjectileReplicationSystem = new();
  private readonly ProjectileDamageSystem _projectileDamageSystem = new();
  private readonly ProjectileHostileDamageScalingSystem _projectileHostileDamageScalingSystem = new();
  private readonly ProjectileTargetEligibilitySystem _projectileTargetEligibilitySystem = new();
  private readonly ProjectileHitImmunityComponent _projectileHitImmunity = new();
  private readonly HitImmunitySystem _projectileHitImmunitySystem = new();
  private readonly QueryDescription _npcMovementQuery = new QueryDescription()
    .WithAll<NpcTagComponent, LocationComponent, VelocityComponent>();
  private readonly QueryDescription _playerMovementQuery = new QueryDescription()
    .WithAll<PlayerTagComponent, LocationComponent, VelocityComponent>();
  private readonly QueryDescription _projectileQuery = new QueryDescription()
    .WithAll<ProjectileTagComponent, LocationComponent, VelocityComponent,
      ProjectileDamageComponent, ProjectileLifetimeComponent, ProjectileDefinitionComponent,
      ProjectileBehaviorComponent, ProjectileNetworkIdentityComponent,
      ProjectilePenetrationComponent, ProjectileBounceComponent, ProjectileUpdateCountComponent,
      ProjectileRestrikeDelayComponent, ProjectileNetworkUpdateComponent,
      ProjectileSoundDelayComponent, ProjectileTileCollisionComponent,
      ProjectileFriendlyStateComponent>();
  private readonly QueryDescription _npcProjectileQuery = new QueryDescription()
    .WithAll<ProjectileTagComponent, LocationComponent, VelocityComponent,
      ProjectileDamageComponent, ProjectileLifetimeComponent, ProjectileDefinitionComponent,
      ProjectileBehaviorComponent, NpcProjectileNetworkIdentityComponent,
      NpcProjectileOwnerComponent, ProjectilePenetrationComponent, ProjectileBounceComponent,
      ProjectileUpdateCountComponent, ProjectileRestrikeDelayComponent,
      ProjectileNetworkUpdateComponent, ProjectileSoundDelayComponent,
      ProjectileTileCollisionComponent, ProjectileFriendlyStateComponent>();
  private int _nextPlayerHandle;
  private int _nextNpcHandle;
  private long _nextNpcDeathSequence;
  private readonly ProjectileIdentityAllocator _projectileIdentityAllocator = new();
  private int _nextWorldItemReplicationId = 1;
  private int _nextChestId = 1;
  private long _nextChestMutationSequence;
  private int _nextDoorId = 1;
  private int _nextSignId;
  private int _nextTileEntityId = 1;
  private long _nextLiquidSequence;
  private long _nextWiringSequence;
  private long _nextProjectileWorldObjectPlacementSequence;
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
  private bool _sentryEventActive;
  private IReadOnlyList<string> _lastNpcPipelineSystemNames = [];

  public DomeSimulation()
    : this(new WorldGrid(4200, 1200), new WorldSeed(1), new SimulationEntityLimits())
  {
  }

  public DomeSimulation(WorldGrid worldGrid)
    : this(worldGrid, new WorldSeed(1), new SimulationEntityLimits())
  {
  }

  public DomeSimulation(WorldGrid worldGrid, WorldSeed worldSeed)
    : this(worldGrid, worldSeed, new SimulationEntityLimits())
  {
  }

  public DomeSimulation(
    WorldGrid worldGrid,
    ProjectileDefinitionRegistry projectileDefinitions)
    : this(
      worldGrid,
      new WorldSeed(1),
      new SimulationEntityLimits(),
      projectileDefinitions)
  {
  }

  public DomeSimulation(
    WorldGrid worldGrid,
    WorldSeed worldSeed,
    SimulationEntityLimits entityLimits)
    : this(
      worldGrid,
      worldSeed,
      entityLimits,
      ProjectileDefinitionRegistry.CreateDefault())
  {
  }

  private DomeSimulation(
    WorldGrid worldGrid,
    WorldSeed worldSeed,
    SimulationEntityLimits entityLimits,
    ProjectileDefinitionRegistry projectileDefinitions)
  {
    WorldGrid = worldGrid ?? throw new ArgumentNullException(nameof(worldGrid));
    _entityLimits = (entityLimits ?? throw new ArgumentNullException(nameof(entityLimits))).Validate();
    _projectileDefinitions = projectileDefinitions ??
      throw new ArgumentNullException(nameof(projectileDefinitions));
    _worldSeed = worldSeed;
    _shopPurchaseSystem = new(_shopOffers);
    _shopOfferCatalogSystem = new(_shopOffers);
    _worldItemSpawnSystem = new(_itemDefinitions);
    _itemPrefixSystem = new(_itemDefinitions);
    _itemVariantSystem = new(_itemDefinitions);
    _npcLootSystem = new(
      new NpcLootDefinitionRegistry([
        new NpcLootDefinition(
          LootTableId: 1,
          ItemType: 1,
          MinimumQuantity: 1,
          MaximumQuantity: 2)],
        _itemDefinitions),
      new WorldSeed(1));
    _worldEventRandomState = new WorldEventRandomState(unchecked((uint)worldSeed.Value));
    _worldClock = new WorldClock();
    _nextNpcHandle = 1;
    _nextPlayerHandle = 1;
    World = Arch.Core.World.Create();
    _worldItems = new WorldItemStore(World, _itemDefinitions);
    _npcSystemPipeline.ValidateRegistration();
  }

  public DomeSimulation(DomeSimulationSnapshot snapshot)
    : this(
      WorldGrid.FromSnapshot(snapshot?.World ?? throw new ArgumentNullException(nameof(snapshot))),
      snapshot)
  {
  }

  public DomeSimulation(WorldGrid worldGrid, DomeSimulationSnapshot snapshot)
    : this(
      worldGrid,
      snapshot?.World.Metadata.Seed ?? throw new ArgumentNullException(nameof(snapshot)),
      new SimulationEntityLimits())
  {
    RestoreSnapshot(snapshot);
  }

  public DomeSimulation(
    WorldGrid worldGrid,
    DomeSimulationSnapshot snapshot,
    SimulationEntityLimits entityLimits)
    : this(
      worldGrid,
      snapshot?.World.Metadata.Seed ?? throw new ArgumentNullException(nameof(snapshot)),
      entityLimits)
  {
    RestoreSnapshot(snapshot);
  }

  private void RestoreSnapshot(DomeSimulationSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    _worldClock.Restore(snapshot.Clock);
    _worldMetadata = snapshot.World.Metadata;
    _worldRules = snapshot.WorldRules;
    _worldProgression = snapshot.Progression;
    _worldEventRandomState = snapshot.WorldEventRandomState;
    _worldTimeRate = snapshot.WorldTimeRate;
    _nextChestMutationSequence = snapshot.NextChestMutationSequence;
    _nextLiquidSequence = snapshot.NextLiquidSequence;
    _nextWiringSequence = snapshot.NextWiringSequence;
    _projectileIdentityAllocator.Restore(snapshot.NextProjectileIdentity);
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
      ValidateRestoredWorldItem(item, worldState, snapshot);
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

    for (int index = 0; index < snapshot.SignTombstones.Count; index++)
    {
      RestoreSignTombstone(snapshot.SignTombstones[index]);
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

      if (!_tileEntityStore.TryRestore(
            new TileEntityIdentityComponent(entity.Id, entity.Type),
            new TileEntityAnchorComponent(entity.TileX, entity.TileY),
            entity.IsOpaque))
      {
        _ = _tileEntities.Remove(entity.Id);
        throw new ArgumentException(
          "Persistence snapshot contains duplicate tile entity positions or unknown types.",
          nameof(snapshot));
      }

      _nextTileEntityId = Math.Max(_nextTileEntityId, entity.Id + 1);

      TryRestoreTrainingDummyOwnership(entity);
    }
  }

  private void ValidateRestoredWorldItem(
    ItemReplicationSnapshot item,
    ItemWorldStateComponent worldState,
    DomeSimulationSnapshot snapshot)
  {
    if (item.ReplicationId <= 0 || item.Revision < 0 ||
        !float.IsFinite(item.Position.X) || !float.IsFinite(item.Position.Y) ||
        (item.Stack.IsEmpty && item.Stack != ItemStack.Empty) ||
        item.IsActive != !item.Stack.IsEmpty ||
        worldState.IsActive != item.IsActive || worldState.PickupDelayTicks < 0 ||
        worldState.SpawnSource < 0 || worldState.LastOwnerRevision < 0 ||
        worldState.Revision < 0 || worldState.ReservedPlayerId < 0 ||
        worldState.ReservedPlayerId > ItemWorldStateComponent.UnreservedPlayerId ||
        worldState.ReservationAgeTicks < ItemWorldStateComponent.NoReservationAge)
    {
      throw new ArgumentOutOfRangeException(
        nameof(snapshot),
        "Persistence snapshot contains an invalid world-item value domain.");
    }

    item.InstanceState.Validate();
    if (item.Stack.IsEmpty)
    {
      if (item.InstanceState != default)
      {
        throw new ArgumentException(
          "An inactive world item cannot carry instance state.",
          nameof(snapshot));
      }

      return;
    }

    if (!_itemDefinitions.TryGet(item.Stack.ItemType, out ItemDefinition definition) ||
        item.Stack.Quantity > definition.StackLimit)
    {
      throw new ArgumentOutOfRangeException(
        nameof(snapshot),
        "Persistence snapshot contains an unknown or over-limit world-item stack.");
    }
  }

  public Arch.Core.World World { get; }
  public WorldGrid WorldGrid { get; }
  public ItemDefinitionRegistry ItemDefinitions => _itemDefinitions;
  public int NpcCount => _npcReplications.Count;
  public uint GameUpdateCount => _gameUpdateCountProjection.Value;
  public long TickNumber => _worldClock.TickNumber;
  public double TimeOfDay => _worldClock.TimeOfDay;
  public bool IsDayTime => _worldClock.IsDayTime;
  public byte MoonPhase => _worldClock.MoonPhase;
  public IReadOnlyList<string> LastNpcPipelineSystemNames => _lastNpcPipelineSystemNames;
  public IReadOnlyList<string> NpcSpawnRejectionReasons => _npcSpawnRejectionReasons;
  public IReadOnlyList<string> NpcPipelineSystemNames => _npcSystemPipeline.SystemNames;
  public IReadOnlyList<SimulationTickPhase> LastTickPhases => _lastTickPhases;
  public SimulationTickTrace? LastTickTrace => _lastTickTrace;
  public WorldClockTransition? LastWorldClockTransition => _lastWorldClockTransition;
  public WorldEnvironmentTransition? LastWorldEnvironmentTransition =>
    _lastWorldEnvironmentTransition;
  public WorldProgressionTransition? LastWorldProgressionTransition =>
    _lastWorldProgressionTransition;
  public WorldInvasionTransition? LastWorldInvasionTransition => _lastWorldInvasionTransition;
  public WorldMeteorTransition? LastWorldMeteorTransition => _lastWorldMeteorTransition;
  public WorldSlimeRainTransition? LastWorldSlimeRainTransition =>
    _lastWorldSlimeRainTransition;
  public WorldLanternNightTransition? LastWorldLanternNightTransition =>
    _lastWorldLanternNightTransition;
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
      if (_worldEventStartRequests[index].Kind == command.Kind ||
          _worldEventStartRequests[index].Sequence == command.Sequence)
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
      WorldEventKind.BloodMoon => !_worldClock.IsDayTime &&
        !_worldProgression.IsBloodMoon &&
        !_worldProgression.IsLanternNight &&
        !_worldProgression.IsMeteorScheduled &&
        _worldProgression.InvasionType == 0,
      WorldEventKind.Eclipse => _worldClock.IsDayTime && _worldProgression.IsHardMode &&
        _worldProgression.DefeatedMechanicalBoss && !_worldProgression.IsEclipse,
      WorldEventKind.LanternNight => !_worldClock.IsDayTime &&
        !_worldProgression.IsLanternNight &&
        !_worldProgression.IsMeteorScheduled &&
        !_worldProgression.IsBloodMoon &&
        _worldProgression.InvasionType == 0,
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

    LocationComponent transform = World.Get<LocationComponent>(entity);
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

  private bool TryReserveSignSnapshot(
    int tileX,
    int tileY,
    string text,
    out SignSnapshot sign)
  {
    sign = default;
    if (text is null || text.Length > 100 || !WorldGrid.Contains(tileX, tileY) ||
        _nextSignId >= MaximumSignCount)
    {
      return false;
    }

    WorldSectionCoordinates section = WorldGrid.GetSectionCoordinates(tileX, tileY);
    sign = new SignSnapshot(_nextSignId, tileX, tileY, text, 1, section);
    _nextSignId++;
    return true;
  }

  private void ReleaseSignReservation(SignSnapshot sign)
  {
    if (!_signs.ContainsKey(sign.SignId) && _nextSignId == sign.SignId + 1)
    {
      _nextSignId = sign.SignId;
    }
  }

  private void CommitReservedSign(SignSnapshot sign)
  {
    _signs[sign.SignId] = sign;
  }

  private IReadOnlyDictionary<WorldSectionCoordinates, long> CapturePlacementSectionVersions(
    IReadOnlyList<WorldSectionCoordinates> sections)
  {
    Dictionary<WorldSectionCoordinates, long> versions = new();
    for (int index = 0; index < sections.Count; index++)
    {
      WorldSectionCoordinates section = sections[index];
      versions[section] = WorldGrid.GetSectionVersion(section);
    }

    return versions;
  }

  private WorldObjectPlacementCommittedEvent CreatePlacementEvent(
    WorldObjectPlacementPlan plan,
    WorldObjectPlacementResult result,
    SignSnapshot sign,
    int projectileIdentity,
    Guid? projectileUuid,
    ProjectileWorldObjectPlacementCommand command)
  {
    WorldSectionCoordinates originSection = WorldGrid.GetSectionCoordinates(
      command.OriginX,
      command.OriginY);
    return new WorldObjectPlacementCommittedEvent(plan, sign, result.Sections)
    {
      SectionVersion = WorldGrid.GetSectionVersion(originSection),
      SectionVersions = CapturePlacementSectionVersions(result.Sections),
      ProjectileIdentity = projectileIdentity,
      ProjectileUuid = projectileUuid,
      ProjectileTombstoneReason = command.TombstoneReason
    };
  }

  private bool TryGetPlacementProjectileLink(
    Entity projectile,
    out int projectileIdentity,
    out Guid? projectileUuid)
  {
    projectileIdentity = -1;
    projectileUuid = null;
    if (!IsAuthoritativeActiveProjectile(projectile) ||
        !_projectileIdsByEntity.TryGetValue(projectile, out projectileIdentity))
    {
      return false;
    }

    if (World.Has<ProjectileNetworkIdentityComponent>(projectile))
    {
      projectileUuid = World.Get<ProjectileNetworkIdentityComponent>(projectile).ProjectileUuid;
    }

    return true;
  }

  public WorldObjectPlacementResult TryCommitProjectileWorldObjectPlacement(
    ProjectileWorldObjectPlacementCommand command,
    IReadOnlyList<WorldObjectTileMutation> footprint,
    bool ownerActive,
    bool projectileActive)
  {
    ThrowIfDisposed();
    ObserveProjectileWorldObjectPlacementSequence(command.Sequence);
    bool resolvedOwnerActive = ownerActive && IsAuthoritativeActivePlayer(command.Owner);
    bool resolvedProjectileActive = projectileActive &&
      IsAuthoritativeActiveProjectile(command.Projectile);
    if (!resolvedOwnerActive)
    {
      return WorldObjectPlacementResult.Rejected(
        command.Sequence,
        WorldObjectPlacementFailureCode.OwnerInactive);
    }

    if (!resolvedProjectileActive)
    {
      return WorldObjectPlacementResult.Rejected(
        command.Sequence,
        WorldObjectPlacementFailureCode.ProjectileInactive);
    }

    WorldObjectPlacementRequest request = command.ToRequest();
    WorldObjectPlacementFailureCode validation = request.Validate(WorldGrid);
    if (validation != WorldObjectPlacementFailureCode.None)
    {
      return WorldObjectPlacementResult.Rejected(command.Sequence, validation);
    }

    if (!TryGetPlacementProjectileLink(
          command.Projectile,
          out int projectileIdentity,
          out Guid? projectileUuid))
    {
      return WorldObjectPlacementResult.Rejected(
        command.Sequence,
        WorldObjectPlacementFailureCode.ProjectileInactive);
    }

    if (_committedWorldObjectPlacementSequences.Contains(command.Sequence))
    {
      return WorldObjectPlacementResult.Rejected(
        command.Sequence,
        WorldObjectPlacementFailureCode.DuplicateSequence);
    }

    Dictionary<WorldSectionCoordinates, long> sectionVersions = new();
    for (int index = 0; index < footprint.Count; index++)
    {
      WorldObjectTileMutation mutation = footprint[index];
      if (!WorldGrid.Contains(mutation.X, mutation.Y))
      {
        continue;
      }

      WorldSectionCoordinates section = WorldGrid.GetSectionCoordinates(mutation.X, mutation.Y);
      sectionVersions.TryAdd(section, WorldGrid.GetSectionVersion(section));
    }

    WorldObjectPlacementPlan plan = new(request, footprint, sectionVersions);
    if (!TryReserveSignSnapshot(
          command.OriginX,
          command.OriginY,
          command.SignText,
          out SignSnapshot sign))
    {
      return WorldObjectPlacementResult.Rejected(
        command.Sequence,
        WorldObjectPlacementFailureCode.InvalidFootprint);
    }

    WorldObjectPlacementResult result;
    try
    {
      result = _worldObjectPlacementCommitSystem.Commit(
        WorldGrid,
        plan,
        resolvedOwnerActive,
        resolvedProjectileActive,
        _committedWorldObjectPlacementSequences);
    }
    catch
    {
      ReleaseSignReservation(sign);
      throw;
    }

    if (!result.Committed)
    {
      ReleaseSignReservation(sign);
      return result;
    }

    CommitReservedSign(sign);
    _worldObjectPlacementEvents.Add(CreatePlacementEvent(
      plan,
      result,
      sign,
      projectileIdentity,
      projectileUuid,
      command));
    _commands.Enqueue(new DespawnEntityCommand(
      command.Projectile,
      command.TombstoneReason));
    return result;
  }

  public WorldObjectPlacementResult TryCommitProjectileSignPlacement(
    ProjectileWorldObjectPlacementCommand command)
  {
    return TryCommitProjectileSignPlacement(
      command,
      IsAuthoritativeActivePlayer(command.Owner),
      IsAuthoritativeActiveProjectile(command.Projectile));
  }

  private WorldObjectPlacementResult TryCommitProjectileSignPlacement(
    ProjectileWorldObjectPlacementCommand command,
    bool ownerActive,
    bool projectileActive)
  {
    ThrowIfDisposed();
    ObserveProjectileWorldObjectPlacementSequence(command.Sequence);
    if (!ownerActive)
    {
      return WorldObjectPlacementResult.Rejected(
        command.Sequence,
        WorldObjectPlacementFailureCode.OwnerInactive);
    }

    if (!projectileActive)
    {
      return WorldObjectPlacementResult.Rejected(
        command.Sequence,
        WorldObjectPlacementFailureCode.ProjectileInactive);
    }

    if (_committedWorldObjectPlacementSequences.Contains(command.Sequence))
    {
      return WorldObjectPlacementResult.Rejected(
        command.Sequence,
        WorldObjectPlacementFailureCode.DuplicateSequence);
    }

    if (!SignObjectPlacementPlanFactory.TryCreate(
          WorldGrid,
          command.ToRequest(),
          out WorldObjectPlacementPlan plan,
          out WorldObjectPlacementFailureCode failureCode))
    {
      return WorldObjectPlacementResult.Rejected(command.Sequence, failureCode);
    }

    if (!TryGetPlacementProjectileLink(
          command.Projectile,
          out int projectileIdentity,
          out Guid? projectileUuid))
    {
      return WorldObjectPlacementResult.Rejected(
        command.Sequence,
        WorldObjectPlacementFailureCode.ProjectileInactive);
    }

    if (!TryReserveSignSnapshot(
          command.OriginX,
          command.OriginY,
          command.SignText,
          out SignSnapshot sign))
    {
      return WorldObjectPlacementResult.Rejected(
        command.Sequence,
        WorldObjectPlacementFailureCode.InvalidFootprint);
    }

    WorldObjectPlacementResult result;
    try
    {
      result = _worldObjectPlacementCommitSystem.Commit(
        WorldGrid,
        plan,
        ownerActive,
        projectileActive,
        _committedWorldObjectPlacementSequences);
    }
    catch
    {
      ReleaseSignReservation(sign);
      throw;
    }

    if (!result.Committed)
    {
      ReleaseSignReservation(sign);
      return result;
    }

    CommitReservedSign(sign);
    _worldObjectPlacementEvents.Add(CreatePlacementEvent(
      plan,
      result,
      sign,
      projectileIdentity,
      projectileUuid,
      command));
    _commands.Enqueue(new DespawnEntityCommand(
      command.Projectile,
      command.TombstoneReason));
    return result;
  }

  public bool PrepareProjectileSignPlacementFixture(
    PlayerHandle owner,
    long sequence,
    int originX,
    int originY,
    out WorldObjectPlacementFailureCode failureCode)
  {
    ThrowIfDisposed();
    failureCode = WorldObjectPlacementFailureCode.None;
    if (!_players.TryGetValue(owner, out Entity ownerEntity) ||
        !World.Get<PlayerLifecycleComponent>(ownerEntity).IsActive)
    {
      failureCode = WorldObjectPlacementFailureCode.OwnerInactive;
      return false;
    }

    if (sequence < 0)
    {
      failureCode = WorldObjectPlacementFailureCode.InvalidSequence;
      return false;
    }

    if (_committedWorldObjectPlacementSequences.Contains(sequence))
    {
      failureCode = WorldObjectPlacementFailureCode.DuplicateSequence;
      return false;
    }

    if (_verificationPlacementProjectiles.ContainsKey(sequence))
    {
      failureCode = WorldObjectPlacementFailureCode.InvalidSequence;
      return false;
    }

    if (!_projectileDefinitions.TryGet(
          VerificationProjectileType,
          out ProjectileDefinition definition) ||
        _projectileIdentityAllocator.NextIdentity == int.MaxValue)
    {
      failureCode = WorldObjectPlacementFailureCode.ProjectileInactive;
      return false;
    }

    int identity = _projectileIdentityAllocator.Allocate();
    SpawnProjectileCommand spawn = new(
      owner,
      originX,
      originY,
      1,
      definition.Damage,
      definition.LifetimeTicks,
      ProjectileType: definition.ProjectileType,
      InitialVelocityY: -100.0f);
    Entity projectile = _projectileSpawnSystem.Spawn(
      World,
      spawn,
      definition,
      identity,
      _projectileHitImmunity);
    ProjectileDefinitionComponent definitionComponent =
      World.Get<ProjectileDefinitionComponent>(projectile);
    ProjectileNetworkIdentityComponent networkIdentity =
      World.Get<ProjectileNetworkIdentityComponent>(projectile);
    ProjectileBehaviorReplicationState behaviorState =
      ProjectileBehaviorStateProjection.Project(
        World.Get<ProjectileBehaviorComponent>(projectile));
    LocationComponent transform = World.Get<LocationComponent>(projectile);
    VelocityComponent velocity = World.Get<VelocityComponent>(projectile);
    _projectileIdsByEntity.Add(projectile, identity);
    _projectileReplications.Add(identity, new ProjectileReplicationSnapshot(
      identity,
      definition.ProjectileType,
      owner,
      new SimulationVector(transform.X, transform.Y),
      new SimulationVector(velocity.X, velocity.Y),
      definition.Damage,
      definition.LifetimeTicks,
      IsActive: true,
      Revision: 1,
      GetSectionCoordinates(new SimulationVector(transform.X, transform.Y)),
      Identity: identity,
      ProjectileUuid: networkIdentity.ProjectileUuid,
      Ai0: behaviorState.Ai0,
      Ai1: behaviorState.Ai1,
      Ai2: behaviorState.Ai2,
      Banner: World.Get<ProjectileBannerResponseComponent>(projectile).BannerId,
      DefinitionKnockback: definition.Knockback,
      DefinitionOriginalDamage: definition.OriginalDamage == 0
        ? definition.Damage
        : definition.OriginalDamage,
      Reflected: false,
      LegacyAiStyle: definition.LegacyAiStyle,
      MaximumPenetration: definition.MaximumPenetration,
      DecidesManualFallThrough: definitionComponent.DecidesManualFallThrough,
      ShouldFallThrough: false,
      Direction: World.Get<ProjectileDirectionComponent>(projectile).Horizontal,
      ManualDirectionChange: definitionComponent.ManualDirectionChange,
      UsesOwnerMeleeHitCooldown: definitionComponent.UsesOwnerMeleeHitCooldown,
      CopiesOwnerAttackCooldownToLocalImmunityOnSpawn:
        definitionComponent.CopiesOwnerAttackCooldownToLocalImmunityOnSpawn,
      HostileDamageScaling: definitionComponent.HostileDamageScaling,
      CollidesWithTiles: definitionComponent.CollidesWithTiles,
      TileCollisionEnabled: World.Get<ProjectileTileCollisionComponent>(projectile).Enabled,
      PrimaryUpdatePending: World.Get<ProjectileNetworkUpdateComponent>(projectile)
        .PrimaryUpdatePending,
      IgnoreWater: definitionComponent.IgnoreWater,
      ReflectsFromTiles: definitionComponent.ReflectsFromTiles,
      CorrectSlopeCollision: definitionComponent.CorrectSlopeCollision,
      MaximumBounces: definitionComponent.MaximumBounces,
      BounceVelocityMultiplier: definitionComponent.BounceVelocityMultiplier,
      MinimumBounceSpeed: definitionComponent.MinimumBounceSpeed,
      ChildSpawn: definitionComponent.ChildSpawn,
      OnHitStatusEffect: definitionComponent.OnHitStatusEffect,
      OnDespawnStatusEffect: definitionComponent.OnDespawnStatusEffect,
      OnDespawnAreaDamage: definitionComponent.OnDespawnAreaDamage,
      BehaviorId: definitionComponent.BehaviorId,
      Friendly: definitionComponent.Friendly,
      Hostile: definitionComponent.Hostile,
      PlayerDamagePolicy: definitionComponent.PlayerDamagePolicy));
    _verificationPlacementProjectiles.Add(sequence, projectile);
    return true;
  }

  public WorldObjectPlacementResult TryCommitProjectileSignPlacementFixture(
    PlayerHandle owner,
    long sequence,
    int originX,
    int originY,
    int style,
    int direction,
    string signText)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(owner, out Entity ownerEntity) ||
        !World.Get<PlayerLifecycleComponent>(ownerEntity).IsActive)
    {
      return WorldObjectPlacementResult.Rejected(
        sequence,
        WorldObjectPlacementFailureCode.OwnerInactive);
    }

    if (!_verificationPlacementProjectiles.TryGetValue(sequence, out Entity projectile) &&
        !PrepareProjectileSignPlacementFixture(
          owner,
          sequence,
          originX,
          originY,
          out WorldObjectPlacementFailureCode prepareFailure))
    {
      return WorldObjectPlacementResult.Rejected(
        sequence,
        prepareFailure);
    }

    projectile = _verificationPlacementProjectiles[sequence];
    if (!World.IsAlive(projectile))
    {
      _verificationPlacementProjectiles.Remove(sequence);
      return WorldObjectPlacementResult.Rejected(
        sequence,
        WorldObjectPlacementFailureCode.ProjectileInactive);
    }

    WorldObjectPlacementResult result = TryCommitProjectileSignPlacement(
      new ProjectileWorldObjectPlacementCommand(
        sequence,
        projectile,
        ownerEntity,
        originX,
        originY,
        ObjectType: 85,
        style,
        direction,
        signText));
    if (result.Committed)
    {
      _verificationPlacementProjectiles.Remove(sequence);
    }

    return result;
  }

  private void ObserveProjectileWorldObjectPlacementSequence(long sequence)
  {
    if (sequence >= 0 && sequence < long.MaxValue &&
        _nextProjectileWorldObjectPlacementSequence <= sequence)
    {
      _nextProjectileWorldObjectPlacementSequence = sequence + 1;
    }
  }

  private bool TryTakeProjectileWorldObjectPlacementSequence(out long sequence)
  {
    if (_nextProjectileWorldObjectPlacementSequence == long.MaxValue)
    {
      sequence = 0;
      return false;
    }

    sequence = _nextProjectileWorldObjectPlacementSequence;
    _nextProjectileWorldObjectPlacementSequence++;
    return true;
  }

  public IReadOnlyList<WorldObjectPlacementCommittedEvent> CreateWorldObjectPlacementEvents()
  {
    ThrowIfDisposed();
    return _worldObjectPlacementEvents.ToArray();
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

  public IReadOnlyList<SignTombstoneSnapshot> CreateSignTombstoneSnapshots()
  {
    ThrowIfDisposed();
    return _signTombstones.Values
      .OrderBy(tombstone => tombstone.SignId)
      .ToArray();
  }

  public bool TryDeleteSign(DeleteSignCommand command)
  {
    ThrowIfDisposed();
    if (!_signs.TryGetValue(command.SignId, out SignSnapshot sign) ||
        sign.Revision != command.ExpectedRevision || sign.Revision == long.MaxValue)
    {
      return false;
    }

    SignComponent component = new(
      sign.SignId,
      sign.TileX,
      sign.TileY,
      sign.Text,
      sign.Revision);
    if (!component.TryDelete(command.ExpectedRevision, out SignLifecycleTransition transition))
    {
      return false;
    }

    _signs.Remove(command.SignId);
    _signTombstones[command.SignId] = new SignTombstoneSnapshot(
      command.SignId,
      transition.Revision,
      SignTombstoneReason.Deleted,
      sign.Section);
    return true;
  }

  public bool TryUpdateSign(int signId, SimulationVector playerPosition, string text)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(text);
    if (!_signs.TryGetValue(signId, out SignSnapshot sign) ||
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

    SignComponent component = new(
      sign.SignId,
      sign.TileX,
      sign.TileY,
      sign.Text,
      sign.Revision);
    if (!component.TryEdit(text, new SignAuthorizationPolicy(true), out SignComponent edited))
    {
      return false;
    }

    _signs[signId] = sign with { Text = edited.Text, Revision = edited.Revision };
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

    if (_chests.Count >= _entityLimits.MaximumChests)
    {
      throw new InvalidOperationException("The configured chest capacity was exhausted.");
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
    ValidateChestItemStack(stack);

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
      ItemStack[] slots = new ItemStack[chest.Inventory.Capacity];
      for (int index = 0; index < slots.Length; index++)
      {
        ItemStack stack = chest.GetSlot(index);
        ValidateChestItemStack(stack);
        slots[index] = stack;
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
    TileEntityIdentityComponent identity = new(entityId, 0);
    TileEntityAnchorComponent anchor = new(tileX, tileY);
    if (!_tileEntityStore.TryRestore(identity, anchor))
    {
      return false;
    }

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

    _ = _tileEntityStore.Remove(entityId, new TileEntityAnchorComponent(tileX, tileY));

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
      LocationComponent transform = World.Get<LocationComponent>(player);
      ColliderComponent collider = World.Get<ColliderComponent>(player);
      players.Add(new TrainingDummyPlayerHitboxSnapshot(
        IsActive: true,
        X: checked((int)MathF.Floor(transform.X)),
        Y: checked((int)MathF.Floor(transform.Y)),
        Width: checked((int)MathF.Ceiling(collider.Width)),
        Height: checked((int)MathF.Ceiling(collider.Height))));
    }

    float activeNpcSlots = CalculateActiveNpcSlots();
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
          activeNpcSlots >= _entityLimits.MaximumNpcs);
      if (!decision.ShouldActivate)
      {
        continue;
      }

      _pendingTrainingDummyActivations.Add(entity.EntityId);
      _commands.Enqueue(new SpawnNpcCommand(
        DefinitionId: TrainingDummyNpcType,
        Position: new SimulationVector(entity.TileX, entity.TileY),
        Source: NpcComponents.NpcSpawnSource.TileEntity));
      activeNpcSlots += _npcDefinitions.GetRequired(TrainingDummyNpcType).NpcSlotCost;
    }
  }

  private float CalculateActiveNpcSlots()
  {
    List<NpcSlotAccount> accounts = new(_npcs.Count);
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs.OrderBy(entry => entry.Key.Value))
    {
      NpcComponents.NpcLifecycleComponent lifecycle =
        World.Get<NpcComponents.NpcLifecycleComponent>(entry.Value);
      NpcComponents.NpcCombatStateComponent combat =
        World.Get<NpcComponents.NpcCombatStateComponent>(entry.Value);
      NpcComponents.NpcAuthorityComponent authority =
        World.Get<NpcComponents.NpcAuthorityComponent>(entry.Value);
      accounts.Add(new NpcSlotAccount(lifecycle.IsActive, authority.NpcSlotCost));
    }

    return _npcSlotAccountingSystem.CalculateActiveSlots(accounts);
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
    if (source.IsEmpty || !destination.IsEmpty || !IsValidChestItemStack(source))
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

  private void ValidateChestItemStack(ItemStack stack)
  {
    if (!IsValidChestItemStack(stack))
    {
      throw new ArgumentOutOfRangeException(
        nameof(stack),
        "A chest item must match an authoritative item Definition and StackLimit.");
    }
  }

  private bool IsValidChestItemStack(ItemStack stack)
  {
    return stack.IsEmpty ||
      _itemDefinitions.TryGet(stack.ItemType, out ItemDefinition definition) &&
      stack.Quantity <= definition.StackLimit;
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

    if (_players.Count >= _entityLimits.MaximumPlayers)
    {
      throw new InvalidOperationException("The configured player limit has been reached.");
    }

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
      new LocationComponent(spawn.X, spawn.Y),
      new VelocityComponent(0.0f, 0.0f),
      new DirectionComponent(1),
      new ColliderComponent(1.0f, 2.0f),
      new PhysicsStateComponent { IsGrounded = spawn.Y <= 0.0f },
      new HealthComponent(100, 100),
      new HealthRegenerationComponent(),
      new ManaComponent(20, 20),
      new DefenseComponent(0),
      new ImmunityComponent(),
      new ControlInputComponent(),
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
      new PlayerSpawnStateComponent());
    PlayerMovementStateComponent movement = new();
    World.Add(entity, in movement);
    PlayerDefenseStateComponent playerDefense = new();
    World.Add(entity, in playerDefense);
    PlayerEnvironmentContactComponent environmentContact = new();
    World.Add(entity, in environmentContact);
    PlayerFishingStateComponent fishing = new();
    World.Add(entity, in fishing);
    PlayerGolfStateComponent golf = new();
    World.Add(entity, in golf);
    PlayerLuckStateComponent luck = new();
    World.Add(entity, in luck);
    PlayerFlightStateComponent flight = new();
    World.Add(entity, in flight);
    PlayerCooldownStateComponent cooldown = new();
    World.Add(entity, in cooldown);
    PlayerEquipmentModifierStateComponent equipmentModifier = new();
    World.Add(entity, in equipmentModifier);
    PlayerBankStateComponent bank = new();
    World.Add(entity, in bank);
    PlayerVoidVaultStateComponent voidVault = new();
    World.Add(entity, in voidVault);
    PlayerDodgeStateComponent dodge = new();
    World.Add(entity, in dodge);
    PlayerTrashSlotComponent trash = new();
    World.Add(entity, in trash);
    EquipmentStateCollectionComponent equipmentState = new();
    World.Add(entity, in equipmentState);
    PlayerPotionStateComponent potionState = new();
    World.Add(entity, in potionState);
    PlayerTargetingStateComponent targeting = new();
    World.Add(entity, in targeting);
    PlayerStealthStateComponent stealth = new() { Stealth = 1.0f };
    World.Add(entity, in stealth);
    PlayerMountStateComponent mount = new();
    World.Add(entity, in mount);
    PlayerGrappleStateComponent grapple = new();
    World.Add(entity, in grapple);
    PlayerDeathDropStateComponent deathDrop = new();
    World.Add(entity, in deathDrop);
    PlayerInventoryTransferPolicyComponent transferPolicy = new();
    World.Add(entity, in transferPolicy);
    PlayerEquipmentInventoryComponent equipmentInventory = new();
    World.Add(entity, equipmentInventory);
    PlayerBuffImmunityStateComponent buffImmunity = new();
    World.Add(entity, buffImmunity);
    PlayerSentryStateComponent sentryState = new();
    World.Add(entity, in sentryState);
    PlayerSleepComponent sleep = new();
    World.Add(entity, in sleep);
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
          !_itemDefinitions.TryGet((ushort)item.ItemType, out ItemDefinition definition) ||
          item.Stack > definition.StackLimit)
      {
        continue;
      }

      inventory.SetSlot(slotId, new ItemStack((ushort)item.ItemType, item.Stack));
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

  public bool TryQueuePlayerPvpBuff(
    PlayerHandle source,
    PlayerHandle target,
    ushort buffType,
    int durationTicks)
  {
    ThrowIfDisposed();
    if (!source.IsValid || !target.IsValid || source == target ||
        !LegacyPvpBuffRegistry.IsPvpBuff(buffType) || durationTicks <= 0 ||
        !_players.TryGetValue(source, out Entity sourceEntity) ||
        !_players.TryGetValue(target, out Entity targetEntity) ||
        !World.Get<PlayerLifecycleComponent>(sourceEntity).IsActive ||
        !World.Get<PlayerLifecycleComponent>(targetEntity).IsActive)
    {
      return false;
    }

    _commands.Enqueue(new ApplyTargetStatusEffectCommand(
      targetEntity,
      source,
      buffType,
      durationTicks));
    return true;
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
    SpawnNpcCommand command = new(
      DefinitionId: definitionId,
      Position: spawn,
      Source: NpcComponents.NpcSpawnSource.Command);
    if (!TryResolveNpcSlot(
          command,
          out NpcHandle npc,
          out bool reusesExisting,
          out Entity replacedEntity,
          out NpcReplicationSnapshot previousReplication,
          out string failureReason))
    {
      throw new InvalidOperationException(failureReason);
    }

    if (!TryCommitNpcSpawn(
          command,
          npc,
          reusesExisting,
          replacedEntity,
          previousReplication,
          out failureReason))
    {
      throw new InvalidOperationException(failureReason);
    }

    return npc;
  }

  private bool TryResolveNpcSlot(
    SpawnNpcCommand command,
    out NpcHandle npc,
    out bool reusesExisting,
    out Entity replacedEntity,
    out NpcReplicationSnapshot previousReplication,
    out string failureReason)
  {
    npc = default;
    reusesExisting = false;
    replacedEntity = default;
    previousReplication = default;
    failureReason = string.Empty;
    if (command.RequestedReplicationId < 0 || command.RequestedReplicationId == int.MaxValue)
    {
      failureReason = "NPC spawn identity is unavailable.";
      return false;
    }

    if (command.RequestedReplicationId > 0)
    {
      npc = new NpcHandle(command.RequestedReplicationId);
      if (_npcs.ContainsKey(npc) || _npcReplications.ContainsKey(npc))
      {
        failureReason = "NPC spawn identity is unavailable.";
        return false;
      }

      if (_npcs.Count >= _entityLimits.MaximumNpcs)
      {
        failureReason = "The configured NPC limit has been reached.";
        return false;
      }

      return true;
    }

    List<NpcSlotCandidate> candidates = new(_npcs.Count);
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      if (!World.IsAlive(entry.Value) ||
          !_npcReplications.TryGetValue(entry.Key, out NpcReplicationSnapshot replication))
      {
        failureReason = "NPC slot ownership is stale.";
        return false;
      }

      NpcComponents.NpcLifecycleComponent lifecycle =
        World.Get<NpcComponents.NpcLifecycleComponent>(entry.Value);
      candidates.Add(new NpcSlotCandidate(
        entry.Key,
        lifecycle.IsActive && replication.IsActive,
        lifecycle.CanBeReplaced,
        replication.Revision));
    }

    if (!_npcSlotAllocator.TrySelect(
          _entityLimits.MaximumNpcs,
          candidates,
          out NpcSlotSelection selection))
    {
      failureReason = "The configured NPC limit has been reached.";
      return false;
    }

    npc = selection.Handle;
    reusesExisting = selection.ReusesExisting;
    if (reusesExisting &&
        (!_npcs.TryGetValue(npc, out replacedEntity) ||
         !_npcReplications.TryGetValue(npc, out previousReplication)))
    {
      failureReason = "NPC slot ownership is stale.";
      return false;
    }

    return true;
  }

  private bool TryCommitNpcSpawn(
    SpawnNpcCommand command,
    NpcHandle npc,
    bool reusesExisting,
    Entity replacedEntity,
    NpcReplicationSnapshot previousReplication,
    out string failureReason)
  {
    failureReason = string.Empty;
    if (reusesExisting &&
        (!_npcs.TryGetValue(npc, out Entity currentEntity) || currentEntity != replacedEntity))
    {
      failureReason = "NPC slot ownership is stale.";
      return false;
    }

    if (!_npcSpawnCommitSystem.TryCommit(
          World,
          WorldGrid,
          _npcDefinitions,
          command,
          npc.Value,
          out NpcSpawnCommitResult result,
          out failureReason))
    {
      return false;
    }

    NpcDefinition definition = _npcDefinitions.GetRequired(command.DefinitionId);
    Entity entity = result.Entity;
    HealthComponent health = World.Get<HealthComponent>(entity);
    DirectionComponent facing = World.Get<DirectionComponent>(entity);
    NpcComponents.NpcLifecycleComponent lifecycle =
      World.Get<NpcComponents.NpcLifecycleComponent>(entity);
    NpcComponents.NpcSpawnStateComponent spawnState =
      World.Get<NpcComponents.NpcSpawnStateComponent>(entity);
    BuffCollectionComponent buffs = new();
    World.Add(entity, in buffs);
    long revision = reusesExisting ? previousReplication.Revision + 1 : 1;
    ref NpcComponents.NpcReplicationComponent entityReplication =
      ref World.Get<NpcComponents.NpcReplicationComponent>(entity);
    entityReplication.Revision = revision;
    NpcReplicationSnapshot replication = new(
      npc.Value,
      definition.NetId,
      command.Position,
      new SimulationVector(0.0f, 0.0f),
      health.Current,
      IsActive: true,
      Revision: revision,
      GetSectionCoordinates(command.Position),
      DefinitionId: definition.DefinitionId,
      MaximumHealth: health.Maximum,
      Facing: facing.Horizontal,
      BehaviorId: definition.BehaviorId,
      SpawnSource: command.Source,
      DifficultyScale: command.DifficultyScale,
      ReleaseOwner: command.ReleaseOwner,
      SpawnedFromStatue: spawnState.SpawnedFromStatue,
      TimeLeft: lifecycle.TimeLeft,
      DespawnReason: lifecycle.DespawnReason,
      Faction: definition.Faction,
      Category: definition.Category);

    if (reusesExisting)
    {
      ClearTrainingDummyOwnershipForNpc(npc, previousReplication with { IsActive = false });
      _publishedNpcDeaths.Remove(npc);
      if (!_npcs.Remove(npc, out Entity removedEntity))
      {
        World.Destroy(entity);
        failureReason = "NPC slot ownership is stale.";
        return false;
      }

      _projectileHitImmunitySystem.ResetNpcSlotData(_projectileHitImmunity, npc.Value);

      if (World.IsAlive(removedEntity))
      {
        World.Destroy(removedEntity);
      }
    }

    _npcs.Add(npc, entity);
    _npcReplications[npc] = replication;
    if (npc.Value >= _nextNpcHandle)
    {
      _nextNpcHandle = npc.Value == int.MaxValue - 1
        ? int.MaxValue
        : npc.Value + 1;
    }

    return true;
  }

  public bool TryGetPlayerMinionAttackTarget(PlayerHandle player, out NpcHandle target)
  {
    ThrowIfDisposed();
    target = default;
    if (!_players.TryGetValue(player, out Entity playerEntity))
    {
      return false;
    }

    NpcHandle? selected = World.Get<PlayerTargetingStateComponent>(playerEntity)
      .MinionAttackTarget;
    if (!selected.HasValue || !_npcs.TryGetValue(selected.Value, out Entity npcEntity) ||
        !World.Get<NpcComponents.NpcLifecycleComponent>(npcEntity).IsActive)
    {
      return false;
    }

    target = selected.Value;
    return true;
  }

  public bool TryGetProjectileOwnerMinionAttackTarget(
    int projectileReplicationId,
    out NpcHandle target)
  {
    ThrowIfDisposed();
    target = default;
    Entity projectile = default;
    bool hasProjectile = projectileReplicationId > 0 &&
      _projectileIdsByEntity.TryGetEntity(projectileReplicationId, out projectile);
    if (!hasProjectile ||
        !World.IsAlive(projectile) || !World.Has<ProjectileOwnerComponent>(projectile) ||
        !World.Has<ProjectileMinionComponent>(projectile))
    {
      return false;
    }

    PlayerHandle owner = World.Get<ProjectileOwnerComponent>(projectile).Owner;
    if (!_players.TryGetValue(owner, out Entity ownerEntity) ||
        !World.Has<PlayerTargetingStateComponent>(ownerEntity))
    {
      return false;
    }

    NpcHandle? requestedTarget = World.Get<PlayerTargetingStateComponent>(ownerEntity)
      .MinionAttackTarget;
    if (!requestedTarget.HasValue || !_npcs.TryGetValue(requestedTarget.Value, out Entity npc) ||
        !World.IsAlive(npc) || !World.Has<NpcComponents.NpcLifecycleComponent>(npc) ||
        !World.Has<NpcComponents.NpcBehaviorStateComponent>(npc) ||
        !World.Has<HealthComponent>(npc))
    {
      return false;
    }

    NpcComponents.NpcLifecycleComponent lifecycle =
      World.Get<NpcComponents.NpcLifecycleComponent>(npc);
    NpcComponents.NpcBehaviorStateComponent behavior =
      World.Get<NpcComponents.NpcBehaviorStateComponent>(npc);
    HealthComponent health = World.Get<HealthComponent>(npc);
    if (!ProjectileOwnerMinionTargetPolicy.CanResolve(
          owner,
          isMinion: true,
          requestedTarget,
          lifecycle.IsActive,
          behavior.IsChaseable,
          health.Current))
    {
      return false;
    }

    target = requestedTarget.Value;
    return true;
  }

  public bool TrySetPlayerMinionAttackTarget(PlayerHandle player, NpcHandle? target)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(player, out Entity playerEntity))
    {
      return false;
    }

    if (target.HasValue &&
        (!_npcs.TryGetValue(target.Value, out Entity npcEntity) ||
         !World.Get<NpcComponents.NpcLifecycleComponent>(npcEntity).IsActive))
    {
      return false;
    }

    ref PlayerTargetingStateComponent targeting =
      ref World.Get<PlayerTargetingStateComponent>(playerEntity);
    targeting.SetMinionAttackTarget(target);
    return true;
  }

  public bool TrySetPlayerMaximumTurrets(PlayerHandle player, int maximumTurrets)
  {
    ThrowIfDisposed();
    return _playerSentryAuthoritySystem.TrySetMaximumTurrets(
      World,
      _players,
      player,
      maximumTurrets);
  }

  public bool TryGetPlayerMaximumTurrets(PlayerHandle player, out int maximumTurrets)
  {
    ThrowIfDisposed();
    return _playerSentryAuthoritySystem.TryGetMaximumTurrets(
      World,
      _players,
      player,
      out maximumTurrets);
  }

  public void SetSentryEventActive(bool eventActive)
  {
    ThrowIfDisposed();
    if (_sentryEventActive == eventActive)
    {
      return;
    }

    _sentryEventActive = eventActive;
    RequestProjectileSentryReconciliation();
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

  public int QueueNpcEventSpawns(
    NpcEventSpawnTable table,
    SimulationVector position)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(table);
    IReadOnlyList<SpawnNpcCommand> commands = _npcEventSpawnSystem.EvaluateTable(
      _worldProgression,
      table,
      _npcDefinitions,
      CreateWorldInvasionPlayerSnapshots(),
      position,
      _npcs.Count,
      _entityLimits.MaximumNpcs);
    for (int index = 0; index < commands.Count; index++)
    {
      _commands.Enqueue(commands[index]);
    }

    return commands.Count;
  }

  public void QueueNpcDespawn(DespawnNpcCommand command)
  {
    ThrowIfDisposed();
    _commands.Enqueue(command);
  }

  public void QueueProjectileSpawn(SpawnProjectileCommand command)
  {
    ThrowIfDisposed();
    _commands.Enqueue(command with
    {
      AuthoritativeDamage = 0,
      AuthoritativeKnockback = 0.0f
    });
  }

  public NpcProjectileReplicationSnapshot CreateNpcProjectile(NpcProjectileSpawnRequest request)
  {
    ThrowIfDisposed();
    if (!_npcs.TryGetValue(request.SourceNpc, out Entity owner) ||
        !_npcReplications.TryGetValue(request.SourceNpc, out NpcReplicationSnapshot ownerState) ||
        !ownerState.IsActive || !World.Get<NpcComponents.NpcLifecycleComponent>(owner).IsActive)
    {
      throw new ArgumentException("NPC projectile owner is not active.", nameof(request));
    }

    if (_projectileReplications.Count + _npcProjectileReplications.Count >=
          _entityLimits.MaximumProjectiles ||
        _projectileIdentityAllocator.NextIdentity == int.MaxValue)
    {
      throw new InvalidOperationException(
        "The configured projectile limit or identity allocator was exhausted.");
    }

    int replicationId = _projectileIdentityAllocator.Allocate();
    Entity projectile = _npcProjectileSpawnSystem.Spawn(World, request, replicationId);
    _projectileIdsByEntity.Add(projectile, replicationId);
    NpcProjectileReplicationSnapshot snapshot = _npcProjectileReplicationSystem.Project(
      projectile,
      World,
      replicationId,
      revision: 1,
      GetSectionCoordinates(request.Position));
    _npcProjectileReplications.Add(replicationId, snapshot);
    return snapshot;
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
      LocationComponent transform = World.Get<LocationComponent>(entity);
      NpcComponents.NpcTargetComponent target =
        World.Get<NpcComponents.NpcTargetComponent>(entity);
      NpcComponents.NpcBehaviorStateComponent behavior =
        World.Get<NpcComponents.NpcBehaviorStateComponent>(entity);
      NpcComponents.NpcSpawnStateComponent spawn =
        World.Get<NpcComponents.NpcSpawnStateComponent>(entity);
      NpcComponents.NpcLifecycleComponent lifecycle =
        World.Get<NpcComponents.NpcLifecycleComponent>(entity);
      NpcComponents.NpcCombatStateComponent combat =
        World.Get<NpcComponents.NpcCombatStateComponent>(entity);
      NpcComponents.NpcMovementStateComponent movement =
        World.Get<NpcComponents.NpcMovementStateComponent>(entity);
      HealthComponent health = World.Get<HealthComponent>(entity);
      DirectionComponent facing = World.Get<DirectionComponent>(entity);
      bool hasHome = World.Has<NpcComponents.NpcHomeComponent>(entity);
      bool hasSegment = World.Has<NpcComponents.NpcSegmentComponent>(entity);
      NpcComponents.NpcHomeComponent home = hasHome
        ? World.Get<NpcComponents.NpcHomeComponent>(entity)
        : default;
      bool hasHomePublication = hasHome &&
        World.Has<NpcComponents.NpcHomePublicationComponent>(entity);
      NpcComponents.NpcHomePublicationComponent homePublication = hasHomePublication
        ? World.Get<NpcComponents.NpcHomePublicationComponent>(entity)
        : default;
      NpcComponents.NpcSegmentComponent segment = hasSegment
        ? World.Get<NpcComponents.NpcSegmentComponent>(entity)
        : default;
      NpcComponents.NpcGivenNameComponent givenName =
        World.Get<NpcComponents.NpcGivenNameComponent>(entity);
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
        Category: definition.Category,
        GivenName: givenName.GivenName,
        HasHomePublication: hasHomePublication,
        HomePublication: homePublication,
        Combat: combat,
        Movement: movement));
    }

    return snapshots;
  }

  public void ApplyNpcGivenNames(IReadOnlyDictionary<int, string> names)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(names);
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      if (names.TryGetValue(entry.Key.Value, out string? givenName))
      {
        NpcComponents.NpcGivenNameComponent component =
          World.Get<NpcComponents.NpcGivenNameComponent>(entry.Value);
        component.SetGivenName(givenName);
        World.Set(entry.Value, component);
      }
    }
  }

  public IReadOnlyList<StatusEffectSnapshot> CreateStatusEffectSnapshots()
  {
    ThrowIfDisposed();
    List<StatusEffectSnapshot> snapshots = new();
    foreach (KeyValuePair<PlayerHandle, Entity> entry in _players.OrderBy(pair => pair.Key.Value))
    {
      BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(entry.Value);
      AddStatusSnapshots(snapshots, StatusEffectTargetKind.Player, entry.Key.Value, buffs);
    }

    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs.OrderBy(pair => pair.Key.Value))
    {
      BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(entry.Value);
      AddStatusSnapshots(snapshots, StatusEffectTargetKind.Npc, entry.Key.Value, buffs);
    }

    return snapshots;
  }

  public IReadOnlyList<NpcStatusEffectStateSnapshot> CreateNpcStatusEffectStateSnapshots()
  {
    ThrowIfDisposed();
    List<NpcStatusEffectStateSnapshot> snapshots = new(_npcs.Count);
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs.OrderBy(pair => pair.Key.Value))
    {
      BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(entry.Value);
      List<StatusEffectSnapshot> effects = new(buffs.Count);
      AddStatusSnapshots(effects, StatusEffectTargetKind.Npc, entry.Key.Value, buffs);
      snapshots.Add(new NpcStatusEffectStateSnapshot(entry.Key.Value, buffs.Revision, effects));
    }

    return snapshots;
  }

  public IReadOnlyList<PlayerStatusEffectStateSnapshot> CreatePlayerStatusEffectStateSnapshots()
  {
    ThrowIfDisposed();
    List<PlayerStatusEffectStateSnapshot> snapshots = new(_players.Count);
    foreach (KeyValuePair<PlayerHandle, Entity> entry in _players.OrderBy(pair => pair.Key.Value))
    {
      BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(entry.Value);
      List<StatusEffectSnapshot> effects = new(buffs.Count);
      AddStatusSnapshots(effects, StatusEffectTargetKind.Player, entry.Key.Value, buffs);
      snapshots.Add(new PlayerStatusEffectStateSnapshot(entry.Key.Value, buffs.Revision, effects));
    }

    return snapshots;
  }

  public IReadOnlyList<ProjectileReplicationSnapshot> CreateProjectileReplicationSnapshots()
  {
    ThrowIfDisposed();
    List<ProjectileReplicationSnapshot> snapshots = new(_projectileReplications.Count);
    for (int replicationId = 1;
         replicationId < _projectileIdentityAllocator.NextIdentity;
         replicationId++)
    {
      if (_projectileReplications.TryGetValue(replicationId, out ProjectileReplicationSnapshot snapshot))
      {
        snapshots.Add(snapshot);
      }
    }

    return snapshots;
  }

  public bool CanWipeProjectileTurret(
    int replicationId,
    PlayerHandle localPlayer,
    bool eventActive)
  {
    ThrowIfDisposed();
    if (replicationId <= 0 || !localPlayer.IsValid ||
        !_projectileReplications.TryGetValue(replicationId, out ProjectileReplicationSnapshot snapshot) ||
        !snapshot.IsActive)
    {
      return false;
    }

    if (_projectileIdsByEntity.TryGetEntity(replicationId, out Entity projectile) &&
        World.IsAlive(projectile) && World.Has<ProjectileOwnerComponent>(projectile) &&
        World.Has<ProjectileDefinitionComponent>(projectile))
    {
      ProjectileOwnerComponent owner = World.Get<ProjectileOwnerComponent>(projectile);
      ProjectileDefinitionComponent definition = World.Get<ProjectileDefinitionComponent>(projectile);
      bool isSentry = World.Has<ProjectileSentryComponent>(projectile);
      return ProjectileTurretPersistencePolicy.CanWipe(
        definition.ProjectileType,
        owner.Owner,
        localPlayer,
        isSentry,
        eventActive,
        isDd2Summon: World.Has<ProjectileDd2SummonComponent>(projectile));
    }

    return false;
  }

  public IReadOnlyList<NpcProjectileReplicationSnapshot> CreateNpcProjectileReplicationSnapshots()
  {
    ThrowIfDisposed();
    List<NpcProjectileReplicationSnapshot> snapshots =
      new(_npcProjectileReplications.Count);
    foreach (NpcProjectileReplicationSnapshot snapshot in _npcProjectileReplications.Values)
    {
      if (snapshot.IsActive || TickNumber < snapshot.TombstoneRetainedUntilTick)
      {
        snapshots.Add(snapshot);
      }
    }

    snapshots.Sort(static (first, second) => first.ReplicationId.CompareTo(second.ReplicationId));
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

  public void QueueNpcDamage(DamageNpcCommand command)
  {
    ThrowIfDisposed();
    if (command.Amount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(command));
    }

    if (!_npcs.ContainsKey(command.Npc))
    {
      throw new ArgumentException("NPC does not exist.", nameof(command));
    }

    _commands.Enqueue(command);
  }

  public void QueueHostileNpcDamage(NpcHandle sourceNpc, NpcHandle targetNpc, int amount)
  {
    ThrowIfDisposed();
    if (amount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    if (!_npcs.ContainsKey(sourceNpc))
    {
      throw new ArgumentException("Source NPC does not exist.", nameof(sourceNpc));
    }

    if (!_npcs.ContainsKey(targetNpc))
    {
      throw new ArgumentException("Target NPC does not exist.", nameof(targetNpc));
    }

    _commands.Enqueue(new DamageNpcCommand(
      targetNpc,
      amount,
      SourceKind: NpcDamageSourceKind.HostileNpc,
      SourceNpc: sourceNpc));
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

  public bool TryGetPlayerInputEdges(PlayerHandle player, out PlayerInputEdges edges)
  {
    if (!_players.TryGetValue(player, out Entity entity))
    {
      edges = default;
      return false;
    }

    ControlInputComponent input = World.Get<ControlInputComponent>(entity);
    edges = new PlayerInputEdges(
      input.UseItemJustPressed,
      input.UseItemJustReleased,
      input.UseTileJustPressed,
      input.UseTileJustReleased,
      input.DashJustPressed,
      input.DashJustReleased);
    return true;
  }

  public bool TryGetPlayerInputState(PlayerHandle player, out PlayerInputState state)
  {
    if (!_players.TryGetValue(player, out Entity entity))
    {
      state = default;
      return false;
    }

    ControlInputComponent input = World.Get<ControlInputComponent>(entity);
    InventoryComponent inventory = World.Get<InventoryComponent>(entity);
    state = new PlayerInputState(
      input.MoveLeft,
      input.MoveRight,
      input.Jump,
      input.Fire,
      input.UseItem,
      input.Up,
      input.Down,
      input.UseTile,
      input.Dash,
      input.Facing,
      inventory.SelectedSlot);
    return true;
  }

  public void SetPlayerGravityDirection(PlayerHandle player, float direction)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(player, out Entity entity))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    if (!float.IsFinite(direction) || (direction != -1.0f && direction != 1.0f))
    {
      throw new ArgumentOutOfRangeException(nameof(direction));
    }

    ref PhysicsStateComponent physics = ref World.Get<PhysicsStateComponent>(entity);
    physics.GravityDirection = direction;
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
      TickNumber,
      Math.Max(0, World.Get<PlayerPotionStateComponent>(entity).PotionDelayTicks));
  }

  public bool TrySetPlayerSleeping(PlayerHandle player, bool isSleeping)
  {
    ThrowIfDisposed();
    return _playerSleepAuthoritySystem.TrySetSleeping(
      World,
      _players,
      player,
      isSleeping);
  }

  public bool TryGetPlayerSleepState(PlayerHandle player, out PlayerSleepComponent state)
  {
    ThrowIfDisposed();
    return _playerSleepAuthoritySystem.TryGetState(
      World,
      _players,
      player,
      out state);
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
    if (_worldItems.ActiveCount >= _entityLimits.MaximumWorldItems)
    {
      throw new InvalidOperationException("The configured world-item limit has been reached.");
    }

    ItemDefinition definition = _itemDefinitions.Get(command.Stack.ItemType);
    if (command.Stack.Quantity > definition.StackLimit)
    {
      throw new ArgumentOutOfRangeException(nameof(command), "World item quantity exceeds its stack limit.");
    }
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
        item.InstanceState,
        item.TimeSinceSpawnedTicks));
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
    HealthRegenerationComponent healthRegeneration =
      World.Get<HealthRegenerationComponent>(entity);
    DefenseComponent defense = World.Get<DefenseComponent>(entity);
    EquipmentLoadoutComponent loadout = World.Get<EquipmentLoadoutComponent>(entity);
    EquipmentStateCollectionComponent equipmentStates =
      World.Get<EquipmentStateCollectionComponent>(entity);
    InventoryComponent inventory = World.Get<InventoryComponent>(entity);
    WellFedStateComponent wellFed = World.Get<WellFedStateComponent>(entity);
    BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(entity);
    PlayerControlStateComponent control = World.Get<PlayerControlStateComponent>(entity);
    ItemUseStateComponent itemUse = World.Get<ItemUseStateComponent>(entity);
    ImmunityComponent immunity = World.Get<ImmunityComponent>(entity);
    PlayerTargetingStateComponent targeting = World.Get<PlayerTargetingStateComponent>(entity);
    PlayerInteractionComponent interaction = World.Get<PlayerInteractionComponent>(entity);
    PlayerMountStateComponent mount = World.Get<PlayerMountStateComponent>(entity);
    PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entity);
    PlayerSpawnStateComponent spawnState = World.Get<PlayerSpawnStateComponent>(entity);
    PlayerFishingStateComponent fishingState = World.Get<PlayerFishingStateComponent>(entity);
    PlayerGolfStateComponent golfState = World.Get<PlayerGolfStateComponent>(entity);
    PlayerLuckStateComponent luckState = World.Get<PlayerLuckStateComponent>(entity);
    PlayerDefenseStateComponent defenseState = World.Get<PlayerDefenseStateComponent>(entity);
    PlayerEnvironmentContactComponent environment =
      World.Get<PlayerEnvironmentContactComponent>(entity);
    PlayerFlightStateComponent flight = World.Get<PlayerFlightStateComponent>(entity);
    PlayerCooldownStateComponent cooldown = World.Get<PlayerCooldownStateComponent>(entity);
    PlayerEquipmentModifierStateComponent equipmentModifiers =
      World.Get<PlayerEquipmentModifierStateComponent>(entity);
    PlayerVoidVaultStateComponent voidVault = World.Get<PlayerVoidVaultStateComponent>(entity);
    PlayerDodgeStateComponent dodge = World.Get<PlayerDodgeStateComponent>(entity);
    PlayerTrashSlotComponent trash = World.Get<PlayerTrashSlotComponent>(entity);
    PlayerBuffImmunityStateComponent buffImmunity =
      World.Get<PlayerBuffImmunityStateComponent>(entity);
    PlayerEquipmentInventoryComponent equipmentInventory =
      World.Get<PlayerEquipmentInventoryComponent>(entity);
    PlayerPotionStateComponent potionState = World.Get<PlayerPotionStateComponent>(entity);
    PlayerIdentityComponent identity = World.Get<PlayerIdentityComponent>(entity);
    PlayerSleepComponent sleep = World.Get<PlayerSleepComponent>(entity);
    PhysicsStateComponent physics = World.Get<PhysicsStateComponent>(entity);
    DirectionComponent facing = World.Get<DirectionComponent>(entity);
    ControlInputComponent input = World.Get<ControlInputComponent>(entity);
    PlayerStealthStateComponent stealth = World.Get<PlayerStealthStateComponent>(entity);
    return new PlayerStateSnapshot(
      player,
      lifecycle.IsActive,
      health.Current,
      health.Maximum,
      lifecycle.RespawnTicks,
      identity.CanonicalAccountUuid,
      identity.AssignedSlot,
      mana.Current,
      mana.Maximum,
      physics.GravityDirection == 0.0f
        ? 1.0f
        : physics.GravityDirection,
      HealthRegenerationComponent.DefaultRegenUnitsPerTick +
      healthRegeneration.EquipmentRegenUnitsPerTick,
      mana.RegenerationDelayTicks,
      mana.RegenerationAccumulator,
      healthRegeneration.DelayTicks,
      healthRegeneration.RegenerationAccumulator,
      defense.Value,
      loadout.SelectedLoadout,
      loadout.AccessoryVisibility,
      equipmentStates.States.Count,
      equipmentStates.Revision,
      loadout.Revision,
      wellFed.Rank,
      wellFed.TimeLeft,
      buffs.Count,
      buffs.Revision,
      control.FireCooldownTicks,
      control.ItemUseCooldownTicks,
      inventory.SelectedSlot,
      itemUse.IsUsing,
      itemUse.UseRevision,
      itemUse.AnimationTicks,
      itemUse.IsChanneling,
      immunity.RemainingTicks,
      immunity.IsImmune,
      facing.Horizontal,
      physics.IsGrounded,
      targeting.Aggro,
      targeting.NoAggroNpcTypes?.Count ?? 0,
      interaction.HasTarget,
      interaction.Mode,
      mount.IsMounted ? checked((ushort)mount.MountType) : null,
      input.Down,
      input.Up,
      input.Fire,
      input.UseTile,
      input.Dash,
      input.MoveLeft,
      input.MoveRight,
      input.Jump,
      input.UseItem,
      stealth.Stealth,
      stealth.IsInvisible,
      stealth.HasShroomiteStealth,
      stealth.IsVortexStealthActive,
      stealth.StealthTimer,
      itemUse.JustStarted,
      mount.FlightTimeRemaining,
      mount.FatigueRemaining,
      targeting.MinionAttackTarget,
      sleep.IsSleeping,
      sleep.TimeSleeping,
      sleep.LastWakeReason,
      Math.Max(0, potionState.PotionDelayTicks),
      lifecycle.IsDead,
      lifecycle.DeadTime,
      spawnState.SpawnX,
      spawnState.SpawnY,
      fishingState.FishingSkill,
      golfState.ScoreAccumulated,
      luckState.CappedLuck,
      luckState.LuckPotion,
      defenseState.DefendedByPaladin,
      defenseState.HasPaladinShield,
      defenseState.Shield,
      defenseState.ShieldRaised,
      defenseState.ShieldParryTimeLeft,
      environment.Breath,
      environment.BreathMax,
      environment.LavaTime,
      environment.LavaMax,
      flight.Wings,
      flight.WingsLogic,
      flight.WingTime,
      flight.WingTimeMax,
      cooldown.ShadowDodgeTimer,
      cooldown.AttackTicks,
      cooldown.ItemAnimation,
      cooldown.ItemAnimationMax,
      cooldown.ItemTime,
      cooldown.ItemTimeMax,
      cooldown.ToolTime,
      equipmentModifiers.MeleeScaleGlove,
      voidVault.IsEnabled,
      dodge.IsEnabled,
      dodge.Remaining,
      buffImmunity.ImmuneTypes.Count,
      equipmentInventory.Revision,
      trash.Item);
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

  public IReadOnlyList<ShopPurchaseReceipt> CreateShopPurchaseReceipts()
  {
    ThrowIfDisposed();
    return _shopPurchaseReceipts.ToArray();
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

  public void QueuePlayerShadowDodge(PlayerHandle player)
  {
    ThrowIfDisposed();
    if (!_players.ContainsKey(player))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    _commands.Enqueue(new ApplyShadowDodgeCommand(player));
  }

  public void QueueRespawnPlayer(PlayerHandle player, SimulationVector spawn)
  {
    ThrowIfDisposed();
    if (!_players.ContainsKey(player))
    {
      throw new ArgumentException("Player does not exist.", nameof(player));
    }

    if (!float.IsFinite(spawn.X) || !float.IsFinite(spawn.Y))
    {
      throw new ArgumentOutOfRangeException(
        nameof(spawn),
        "Player respawn position must be finite.");
    }

    _commands.Enqueue(new RespawnPlayerCommand(player, spawn));
  }

  public void ApplyPlayerMountControl(PlayerHandle player, ushort? mountType)
  {
    ThrowIfDisposed();
    if (!_players.TryGetValue(player, out Entity entity) ||
        !World.Get<PlayerLifecycleComponent>(entity).IsActive)
    {
      throw new ArgumentException("Mount control references an inactive player.", nameof(player));
    }

    ref PlayerMountStateComponent mount = ref World.Get<PlayerMountStateComponent>(entity);
    _playerNpcTargetingSystem.TryApplyMountControl(ref mount, mountType);
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

    if (!Enum.IsDefined(mode))
    {
      throw new ArgumentOutOfRangeException(nameof(mode));
    }

    if (targetId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(targetId));
    }

    if (!float.IsFinite(targetPosition.X) || !float.IsFinite(targetPosition.Y))
    {
      throw new ArgumentOutOfRangeException(
        nameof(targetPosition),
        "Player interaction target position must be finite.");
    }

    PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entity);
    if (!lifecycle.IsActive)
    {
      throw new InvalidOperationException("Inactive players cannot issue interactions.");
    }

    LocationComponent transform = World.Get<LocationComponent>(entity);
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
      LocationComponent transform = World.Get<LocationComponent>(entry.Value);
      VelocityComponent velocity = World.Get<VelocityComponent>(entry.Value);
      DirectionComponent facing = World.Get<DirectionComponent>(entry.Value);
      PhysicsStateComponent physics = World.Get<PhysicsStateComponent>(entry.Value);
      HealthComponent health = World.Get<HealthComponent>(entry.Value);
      HealthRegenerationComponent healthRegeneration =
        World.Get<HealthRegenerationComponent>(entry.Value);
      ManaComponent mana = World.Get<ManaComponent>(entry.Value);
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entry.Value);
      PlayerIdentityComponent identity = World.Get<PlayerIdentityComponent>(entry.Value);
      ColliderComponent collider = World.Get<ColliderComponent>(entry.Value);
      DefenseComponent defense = World.Get<DefenseComponent>(entry.Value);
      EquipmentLoadoutComponent loadout = World.Get<EquipmentLoadoutComponent>(entry.Value);
      EquipmentStateCollectionComponent equipmentStates =
        World.Get<EquipmentStateCollectionComponent>(entry.Value);
      WellFedStateComponent wellFed = World.Get<WellFedStateComponent>(entry.Value);
      BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(entry.Value);
      InventoryComponent inventory = World.Get<InventoryComponent>(entry.Value);
      ItemUseStateComponent itemUse = World.Get<ItemUseStateComponent>(entry.Value);
      ImmunityComponent immunity = World.Get<ImmunityComponent>(entry.Value);
      PlayerControlStateComponent control =
        World.Get<PlayerControlStateComponent>(entry.Value);
      PlayerTargetingStateComponent targeting =
        World.Get<PlayerTargetingStateComponent>(entry.Value);
      PlayerInteractionComponent interaction =
        World.Get<PlayerInteractionComponent>(entry.Value);
      ControlInputComponent input = World.Get<ControlInputComponent>(entry.Value);
      PlayerStealthStateComponent stealth =
        World.Get<PlayerStealthStateComponent>(entry.Value);
      PlayerMountStateComponent mount = World.Get<PlayerMountStateComponent>(entry.Value);
      PlayerSleepComponent sleep = World.Get<PlayerSleepComponent>(entry.Value);
      PlayerPotionStateComponent potionState =
        World.Get<PlayerPotionStateComponent>(entry.Value);
      PlayerSpawnStateComponent spawnState =
        World.Get<PlayerSpawnStateComponent>(entry.Value);
      PlayerFishingStateComponent fishingState =
        World.Get<PlayerFishingStateComponent>(entry.Value);
      PlayerGolfStateComponent golfState =
        World.Get<PlayerGolfStateComponent>(entry.Value);
      PlayerLuckStateComponent luckState =
        World.Get<PlayerLuckStateComponent>(entry.Value);
      PlayerDefenseStateComponent defenseState =
        World.Get<PlayerDefenseStateComponent>(entry.Value);
      PlayerEnvironmentContactComponent environment =
        World.Get<PlayerEnvironmentContactComponent>(entry.Value);
      PlayerFlightStateComponent flight =
        World.Get<PlayerFlightStateComponent>(entry.Value);
      PlayerCooldownStateComponent cooldown =
        World.Get<PlayerCooldownStateComponent>(entry.Value);
      PlayerEquipmentModifierStateComponent equipmentModifiers =
        World.Get<PlayerEquipmentModifierStateComponent>(entry.Value);
      PlayerVoidVaultStateComponent voidVault =
        World.Get<PlayerVoidVaultStateComponent>(entry.Value);
      PlayerDodgeStateComponent dodge = World.Get<PlayerDodgeStateComponent>(entry.Value);
      PlayerTrashSlotComponent trash = World.Get<PlayerTrashSlotComponent>(entry.Value);
      PlayerBuffImmunityStateComponent buffImmunity =
        World.Get<PlayerBuffImmunityStateComponent>(entry.Value);
      PlayerEquipmentInventoryComponent equipmentInventory =
        World.Get<PlayerEquipmentInventoryComponent>(entry.Value);
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
        mana.Maximum,
        physics.GravityDirection == 0.0f ? 1.0f : physics.GravityDirection,
        collider.Width,
        collider.Height,
        health.Maximum,
        defense.Value,
        loadout.SelectedLoadout,
        loadout.AccessoryVisibility,
        equipmentStates.States.Count,
        equipmentStates.Revision,
        loadout.Revision,
        wellFed.Rank,
        wellFed.TimeLeft,
        buffs.Count,
        buffs.Revision,
        inventory.SelectedSlot,
        itemUse.IsUsing,
        itemUse.UseRevision,
        itemUse.CooldownTicks,
        itemUse.AnimationTicks,
        itemUse.IsChanneling,
        HealthRegenerationComponent.DefaultRegenUnitsPerTick +
        healthRegeneration.EquipmentRegenUnitsPerTick,
        healthRegeneration.DelayTicks,
        healthRegeneration.RegenerationAccumulator,
        mana.RegenerationDelayTicks,
        mana.RegenerationAccumulator,
        immunity.RemainingTicks,
        immunity.IsImmune,
        targeting.Aggro,
        targeting.NoAggroNpcTypes?.Count ?? 0,
        control.FireCooldownTicks,
        interaction.HasTarget,
        interaction.Mode,
        mount.IsMounted ? checked((ushort)mount.MountType) : null,
        input.Down,
        input.Up,
        input.Fire,
        input.UseTile,
        input.Dash,
        input.MoveLeft,
        input.MoveRight,
        input.Jump,
        input.UseItem,
        stealth.Stealth,
        stealth.IsInvisible,
        stealth.HasShroomiteStealth,
        stealth.IsVortexStealthActive,
        stealth.StealthTimer,
         itemUse.JustStarted,
         mount.FlightTimeRemaining,
         mount.FatigueRemaining,
         sleep.IsSleeping,
         sleep.TimeSleeping,
         sleep.LastWakeReason,
         Math.Max(0, potionState.PotionDelayTicks),
         lifecycle.IsDead,
         lifecycle.DeadTime,
         spawnState.SpawnX,
         spawnState.SpawnY,
         fishingState.FishingSkill,
         golfState.ScoreAccumulated,
         luckState.CappedLuck,
         luckState.LuckPotion,
         defenseState.DefendedByPaladin,
         defenseState.HasPaladinShield,
         defenseState.Shield,
         defenseState.ShieldRaised,
         defenseState.ShieldParryTimeLeft,
         environment.Breath,
         environment.BreathMax,
         environment.LavaTime,
         environment.LavaMax,
         flight.Wings,
         flight.WingsLogic,
         flight.WingTime,
         flight.WingTimeMax,
         cooldown.ShadowDodgeTimer,
         cooldown.AttackTicks,
         cooldown.ItemAnimation,
         cooldown.ItemAnimationMax,
         cooldown.ItemTime,
         cooldown.ItemTimeMax,
         cooldown.ToolTime,
         equipmentModifiers.MeleeScaleGlove,
         voidVault.IsEnabled,
         dodge.IsEnabled,
         dodge.Remaining,
         buffImmunity.ImmuneTypes.Count,
         equipmentInventory.Revision,
         trash.Item));
    }

    List<NpcSnapshot> npcs = new(_npcs.Count);
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      LocationComponent transform = World.Get<LocationComponent>(entry.Value);
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
      (Entity entity, ref LocationComponent transform,
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
      worldTimeRate: _worldTimeRate,
      signTombstones: CreateSignTombstoneSnapshots(),
      nextProjectileIdentity: _projectileIdentityAllocator.NextIdentity,
      nextChestMutationSequence: _nextChestMutationSequence,
      nextLiquidSequence: _nextLiquidSequence,
      nextWiringSequence: _nextWiringSequence);
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

  private static WorldProgressionTransition? CreateWorldProgressionTransition(
    WorldProgressionState previous,
    WorldProgressionState current,
    long tickNumber,
    IReadOnlyList<WorldEventStartCommand> requests)
  {
    if (previous.IsBloodMoon != current.IsBloodMoon)
    {
      return new WorldProgressionTransition(
        tickNumber,
        WorldEventKind.BloodMoon,
        Started: current.IsBloodMoon,
        Stopped: !current.IsBloodMoon)
      {
        Sequence = current.IsBloodMoon
          ? FindCommittedEventSequence(requests, WorldEventKind.BloodMoon)
          : -1
      };
    }

    if (previous.IsEclipse != current.IsEclipse)
    {
      return new WorldProgressionTransition(
        tickNumber,
        WorldEventKind.Eclipse,
        Started: current.IsEclipse,
        Stopped: !current.IsEclipse)
      {
        Sequence = current.IsEclipse
          ? FindCommittedEventSequence(requests, WorldEventKind.Eclipse)
          : -1
      };
    }

    return null;
  }

  private static long FindCommittedEventSequence(
    IReadOnlyList<WorldEventStartCommand> requests,
    WorldEventKind eventKind)
  {
    for (int index = 0; index < requests.Count; index++)
    {
      if (requests[index].IsValid && requests[index].Kind == eventKind)
      {
        return requests[index].Sequence;
      }
    }

    return -1;
  }

  private static WorldInvasionTransition? CreateWorldInvasionTransition(
    WorldProgressionState previous,
    WorldProgressionState current,
    long tickNumber,
    IReadOnlyList<WorldInvasionStartCommand> startRequests,
    IReadOnlyList<WorldInvasionProgressCommand> progressRequests)
  {
    bool started = previous.InvasionType == 0 && current.InvasionType != 0;
    bool completed = previous.InvasionType != 0 && current.InvasionType == 0;
    bool progressed = previous.InvasionSize != current.InvasionSize ||
      previous.InvasionX != current.InvasionX;
    if (!started && !completed && !progressed)
    {
      return null;
    }

    WorldInvasionClearFlag? clearFlag = completed
      ? new WorldInvasionClearFlagSystem().Resolve(previous.InvasionType)
      : null;
    return new WorldInvasionTransition(
      tickNumber,
      previous.InvasionType,
      current.InvasionType,
      previous.InvasionSize,
      current.InvasionSize,
      previous.InvasionX,
      current.InvasionX,
      clearFlag,
      started,
      progressed,
      completed)
    {
      Sequence = started
        ? FindFirstValidInvasionStartSequence(startRequests)
        : progressed
          ? FindFirstValidInvasionProgressSequence(progressRequests)
          : -1
    };
  }

  private static WorldMeteorTransition? CreateWorldMeteorTransition(
    WorldProgressionState previous,
    WorldProgressionState current,
    bool impactQueued,
    long tickNumber,
    IReadOnlyList<WorldMeteorScheduleCommand> scheduleRequests,
    IReadOnlyList<WorldMeteorImpactCommand> impactRequests)
  {
    bool scheduleStarted = !previous.IsMeteorScheduled && current.IsMeteorScheduled;
    bool scheduleCleared = previous.IsMeteorScheduled && !current.IsMeteorScheduled;
    if (!scheduleStarted && !scheduleCleared && !impactQueued)
    {
      return null;
    }

    return new WorldMeteorTransition(
      tickNumber,
      previous.IsMeteorScheduled,
      current.IsMeteorScheduled,
      scheduleStarted,
      scheduleCleared,
      impactQueued)
    {
      Sequence = scheduleStarted
        ? FindFirstValidMeteorScheduleSequence(scheduleRequests)
        : impactQueued
          ? FindFirstValidMeteorImpactSequence(impactRequests)
          : -1
    };
  }

  private static WorldSlimeRainTransition? CreateWorldSlimeRainTransition(
    WorldProgressionState previous,
    WorldProgressionState current,
    bool warningPublished,
    long tickNumber,
    IReadOnlyList<WorldSlimeRainStartCommand> startRequests,
    IReadOnlyList<WorldSlimeRainStopCommand> stopRequests)
  {
    bool started = !previous.IsSlimeRaining && current.IsSlimeRaining;
    bool stopped = previous.IsSlimeRaining && !current.IsSlimeRaining;
    bool cooldownStarted = !previous.IsSlimeRainCoolingDown && current.IsSlimeRainCoolingDown;
    bool cooldownEnded = previous.IsSlimeRainCoolingDown && !current.IsSlimeRainCoolingDown;
    bool changed = previous.SlimeRainTimeTicks != current.SlimeRainTimeTicks ||
      previous.SlimeRainCooldownTicks != current.SlimeRainCooldownTicks ||
      previous.SlimeRainWarningTicks != current.SlimeRainWarningTicks;
    if (!changed && !warningPublished)
    {
      return null;
    }

    return new WorldSlimeRainTransition(
      tickNumber,
      previous.SlimeRainTimeTicks,
      current.SlimeRainTimeTicks,
      previous.SlimeRainCooldownTicks,
      current.SlimeRainCooldownTicks,
      previous.SlimeRainWarningTicks,
      current.SlimeRainWarningTicks,
      started,
      stopped,
      warningPublished,
      cooldownStarted,
      cooldownEnded)
    {
      Sequence = started
        ? FindFirstValidSlimeStartSequence(startRequests)
        : stopped
          ? FindFirstValidSlimeStopSequence(stopRequests)
          : -1
    };
  }

  private static long FindFirstValidInvasionStartSequence(
    IReadOnlyList<WorldInvasionStartCommand> requests)
  {
    for (int index = 0; index < requests.Count; index++)
    {
      if (requests[index].IsValid)
      {
        return requests[index].Sequence;
      }
    }

    return -1;
  }

  private static long FindFirstValidInvasionProgressSequence(
    IReadOnlyList<WorldInvasionProgressCommand> requests)
  {
    for (int index = 0; index < requests.Count; index++)
    {
      if (requests[index].IsValid)
      {
        return requests[index].Sequence;
      }
    }

    return -1;
  }

  private static long FindFirstValidMeteorScheduleSequence(
    IReadOnlyList<WorldMeteorScheduleCommand> requests)
  {
    for (int index = 0; index < requests.Count; index++)
    {
      if (requests[index].IsValid)
      {
        return requests[index].Sequence;
      }
    }

    return -1;
  }

  private static long FindFirstValidMeteorImpactSequence(
    IReadOnlyList<WorldMeteorImpactCommand> requests)
  {
    for (int index = 0; index < requests.Count; index++)
    {
      if (requests[index].IsValid)
      {
        return requests[index].Sequence;
      }
    }

    return -1;
  }

  private static long FindFirstValidSlimeStartSequence(
    IReadOnlyList<WorldSlimeRainStartCommand> requests)
  {
    for (int index = 0; index < requests.Count; index++)
    {
      if (requests[index].IsValid)
      {
        return requests[index].Sequence;
      }
    }

    return -1;
  }

  private static long FindFirstValidSlimeStopSequence(
    IReadOnlyList<WorldSlimeRainStopCommand> requests)
  {
    for (int index = 0; index < requests.Count; index++)
    {
      if (requests[index].IsValid)
      {
        return requests[index].Sequence;
      }
    }

    return -1;
  }

  private static WorldLanternNightTransition? CreateWorldLanternNightTransition(
    WorldProgressionState previous,
    WorldProgressionState current,
    long tickNumber,
    IReadOnlyList<WorldEventStartCommand> eventRequests,
    long scheduleSequence)
  {
    bool scheduleConsumed = previous.IsNextNightLanternNight &&
      !current.IsNextNightLanternNight;
    bool started = !previous.IsLanternNight && current.IsLanternNight;
    bool stopped = previous.IsLanternNight && !current.IsLanternNight;
    if (!scheduleConsumed && !started && !stopped)
    {
      return null;
    }

    return new WorldLanternNightTransition(
      tickNumber,
      previous.IsNextNightLanternNight,
      current.IsNextNightLanternNight,
      previous.IsLanternNight,
      current.IsLanternNight,
      scheduleConsumed,
      started,
      stopped)
    {
      Sequence = started
        ? scheduleConsumed
          ? scheduleSequence
          : FindCommittedEventSequence(eventRequests, WorldEventKind.LanternNight)
        : -1
    };
  }

  public void Tick(SimulationInputBatch inputBatch)
  {
    ThrowIfDisposed();
    ArgumentNullException.ThrowIfNull(inputBatch);

    SimulationTickContext tick = SimulationTickSchedule.Begin();
    _lastWorldClockTransition = null;
    _lastWorldEnvironmentTransition = null;
    _lastWorldProgressionTransition = null;
    _lastWorldInvasionTransition = null;
    _lastWorldMeteorTransition = null;
    _lastWorldSlimeRainTransition = null;
    _lastWorldLanternNightTransition = null;
    tick.Enter(SimulationTickPhase.BeginTick);
    _playerDamagedEvents.Clear();
    _playerDiedEvents.Clear();
    _playerRespawnedEvents.Clear();
    _itemUsedEvents.Clear();
    _shopPurchaseReceipts.Clear();
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

    _lastWorldClockTransition = _worldClockSystem.Tick(_worldClock, _worldTimeRate);
    if (_worldClock.IsPaused)
    {
      tick.CompleteWhilePaused();
      _lastTickPhases = tick.Phases;
      _lastTickTrace = CreateTickTrace(inputBatch, tick, commandCount: 0);
      return;
    }

    int weatherTicks = _worldTimeRate.IsAvailable
      ? _worldTimeRate.Rate
      : _worldClock.TicksPerUpdate;
    WorldEnvironmentTransitionResult environment = _worldWeatherSystem.AdvanceWithTransition(
      _worldClock.CreateSnapshot(),
      weatherTicks,
      _worldRules,
      _worldRainStartRequests,
      _worldWindChangeRequests,
      _worldProgression.IsLanternNight || HasPendingLanternNightStart());
    _worldRules = environment.State;
    _lastWorldEnvironmentTransition = environment.Transition;
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
    _lastWorldProgressionTransition = CreateWorldProgressionTransition(
      previousProgression,
      _worldProgression,
      _worldClock.TickNumber,
      _worldEventStartRequests);
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
    _lastWorldInvasionTransition = CreateWorldInvasionTransition(
      previousProgression,
      _worldProgression,
      _worldClock.TickNumber,
      _worldInvasionStartRequests,
      _worldInvasionProgressRequests);
    if (previousProgression.SlimeRainWarningTicks > 0 &&
        _worldProgression.SlimeRainWarningTicks == 0)
    {
      _worldSlimeRainWarningEvents.Add(
        new WorldSlimeRainWarningEvent(_worldProgression.IsSlimeRaining));
    }
    _lastWorldSlimeRainTransition = CreateWorldSlimeRainTransition(
      previousProgression,
      _worldProgression,
      _worldSlimeRainWarningEvents.Count > 0,
      _worldClock.TickNumber,
      _worldSlimeRainStartRequests,
      _worldSlimeRainStopRequests);
    _lastWorldLanternNightTransition = CreateWorldLanternNightTransition(
      previousProgression,
      _worldProgression,
      _worldClock.TickNumber,
      _worldEventStartRequests,
      previousProgression.LanternNightScheduleSequence);
    if (_lastWorldLanternNightTransition is { ScheduleConsumed: true })
    {
      _worldProgression = _worldProgression.WithLanternNightScheduleSequence(-1);
    }
    ResolveScheduledWorldMeteorImpacts();
    _worldProgression = _worldMeteorScheduleSystem.Advance(
      _worldClock.CreateSnapshot(),
      _worldProgression,
      _worldMeteorScheduleRequests);
    _lastWorldMeteorTransition = CreateWorldMeteorTransition(
      previousProgression,
      _worldProgression,
      _worldMeteorImpactRequests.Count > 0,
      _worldClock.TickNumber,
      _worldMeteorScheduleRequests,
      _worldMeteorImpactRequests);
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
    _playerSleepSystem.Advance(World, _players, _itemDefinitions);
    AdvanceItemUseCooldowns();
    AdvancePlayerCooldowns();
    QueueItemUses();
    AdvancePlayerLifecycle();
    IReadOnlyList<Entity> activePlayers = GetActivePlayers();
    _playerControlSystem.Apply(World, activePlayers);
    _playerGravitySystem.Apply(World, activePlayers);
    tick.Enter(SimulationTickPhase.ResolveTileCollision);
    ResolvePlayerTileCollision();
    AdvancePlayerStealth(activePlayers);
    _playerVitalRegenSystem.Apply(World, activePlayers);
    AdvancePlayerBuffs(activePlayers);
    AdvancePlayerLuckPotions();
    AdvanceNpcBuffs();
    AdvancePlayerImmunity();
    AdvanceNpcImmunity();
    EvaluateTrainingDummyLifecycle(activePlayers);
    _gameUpdateCountProjection.Advance();
    List<string> npcPipelineStages = new();
    tick.Enter(SimulationTickPhase.SelectNpcTargets);
    EnqueuePendingNpcSpawns();
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
    MoveNpcProjectiles();
    DetectNpcProjectileHits();
    tick.Enter(SimulationTickPhase.ResolveCombat);
    RunNpcSystemPipeline(
      NpcSystemStage.ContactEffect,
      NpcSystemStage.Replication,
      npcPipelineStages);
    _lastNpcPipelineSystemNames = npcPipelineStages;
    tick.Enter(SimulationTickPhase.CommitDomainCommands);
    AdvanceWiring();
    AdvanceLiquid();
    _projectileHitImmunitySystem.Tick(_projectileHitImmunity);
    int commandCount = _commands.Count;
    CommitCommands();
    tick.Enter(SimulationTickPhase.PublishSnapshot);
    _lastPublishedSnapshot = CreateSnapshot();
    tick.Enter(SimulationTickPhase.EndTick);
    tick.CompleteNormally();
    _lastTickPhases = tick.Phases;
    _lastTickTrace = CreateTickTrace(inputBatch, tick, commandCount);
  }

  private SimulationTickTrace CreateTickTrace(
    SimulationInputBatch inputBatch,
    SimulationTickContext tick,
    int commandCount)
  {
    return new SimulationTickTrace(
      TickNumber,
      inputBatch.InputSequenceStart,
      inputBatch.InputSequenceEnd,
      commandCount,
      CountPublishedEvents(),
      tick.Phases);
  }

  private int CountPublishedEvents()
  {
    return _playerDamagedEvents.Count +
      _playerDiedEvents.Count +
      _playerRespawnedEvents.Count +
      _itemUsedEvents.Count +
      _itemEquippedEvents.Count +
      _itemPrefixChangedEvents.Count +
      _inventoryChangedEvents.Count +
      _worldItemCreatedEvents.Count +
      _worldItemPickedUpEvents.Count +
      _worldItemDestroyedEvents.Count +
      _itemDroppedEvents.Count +
      _shopPurchaseReceipts.Count +
      _extractinatorResultEvents.Count +
      _worldSlimeRainWarningEvents.Count +
      _worldInvasionCompletedEvents.Count;
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
      LocationComponent transform = World.Get<LocationComponent>(playerEntity);
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
      ref ControlInputComponent input = ref World.Get<ControlInputComponent>(entity);
      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(entity);
      ref DirectionComponent facing = ref World.Get<DirectionComponent>(entity);
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

      ref LocationComponent transform = ref World.Get<LocationComponent>(entity);
      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(entity);
      ref PhysicsStateComponent physics = ref World.Get<PhysicsStateComponent>(entity);
      ControlInputComponent input = World.Get<ControlInputComponent>(entity);
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
    CommitNpcInteractions();
    CommitInventoryCommands();
    CommitShopPurchases();
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
    CommitProjectileWorldObjectPlacements();
    EmitProjectileOnDespawnAreaDamageCommands();
    CommitAreaStatusEffectCommands();
    CommitTargetStatusEffectCommands();
    for (int index = 0; index < _commands.ShadowDodgeCommands.Count; index++)
    {
      ApplyShadowDodgeCommand command = _commands.ShadowDodgeCommands[index];
      if (!_players.TryGetValue(command.Player, out Entity playerEntity) ||
          !World.Get<PlayerLifecycleComponent>(playerEntity).IsActive)
      {
        continue;
      }

      ref ImmunityComponent immunity = ref World.Get<ImmunityComponent>(playerEntity);
      immunity.ApplyShadowDodge();
    }

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
            ref World.Get<PlayerDeathDropStateComponent>(playerEntity),
            health,
            PlayerRespawnDelayTicks))
      {
        ref PlayerPotionStateComponent potionState =
          ref World.Get<PlayerPotionStateComponent>(playerEntity);
        _playerPotionDelaySystem.Clear(
          ref potionState,
          World.Get<BuffCollectionComponent>(playerEntity));
        ref PlayerMountStateComponent mount = ref World.Get<PlayerMountStateComponent>(playerEntity);
        mount.Set(-1);
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
      ref LocationComponent transform = ref World.Get<LocationComponent>(playerEntity);
      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(playerEntity);
      ref HealthComponent health = ref World.Get<HealthComponent>(playerEntity);
      if (!PlayerSpawnAreaPolicy.IsValid(WorldGrid, command.Spawn))
      {
        continue;
      }

      if (!_playerRespawnSystem.TryRespawn(
            ref lifecycle,
            ref World.Get<PlayerDeathDropStateComponent>(playerEntity),
            ref transform,
            ref velocity,
            ref health,
            command.Spawn))
      {
        continue;
      }

      ref PlayerMountStateComponent mount = ref World.Get<PlayerMountStateComponent>(playerEntity);
      mount.Set(-1);
      _playerRespawnedEvents.Add(new PlayerRespawnedEvent(command.Player, command.Spawn));
    }

    for (int index = 0; index < _pendingProjectileSpawnCommands.Count; index++)
    {
      _commands.Enqueue(_pendingProjectileSpawnCommands[index]);
    }

    _pendingProjectileSpawnCommands.Clear();
    List<SpawnProjectileCommand> admittedSentryCommands = new();
    for (int index = 0; index < _commands.SpawnProjectileCommands.Count; index++)
    {
      if (_projectileReplications.Count >= _entityLimits.MaximumProjectiles)
      {
        break;
      }

      SpawnProjectileCommand command = _commands.SpawnProjectileCommands[index];
      if (!_projectileDefinitions.TryGet(command.ProjectileType,
          out ProjectileDefinition definition))
      {
        continue;
      }

      if (!_players.TryGetValue(command.Owner, out Entity ownerEntity) ||
          !World.Get<PlayerLifecycleComponent>(ownerEntity).IsActive ||
          !float.IsFinite(command.X) ||
          !float.IsFinite(command.Y) ||
          command.MiscText is null ||
          command.MiscText.Length > 100 ||
          !float.IsFinite(command.InitialVelocityY) ||
          !float.IsFinite(command.ProjectileSpeed) || !float.IsFinite(command.Ai0) ||
          !float.IsFinite(command.Ai1) || !float.IsFinite(command.Ai2))
      {
        continue;
      }

      bool isSentry = command.IsSentry || definition.IsSentry;
      if (command.IsDd2Summon && !isSentry)
      {
        continue;
      }

      if (isSentry &&
          !_projectileSentryPlacementSystem.CanPlace(
            World,
            command.X,
            command.Y,
            definition,
            _projectileDefinitions,
            admittedSentryCommands))
      {
        continue;
      }

      if (_projectileIdentityAllocator.NextIdentity == int.MaxValue)
      {
        continue;
      }

      int replicationId = _projectileIdentityAllocator.Allocate();
      ProjectileDefinition spawnDefinition = definition;
      if (command.AuthoritativeDamage > 0)
      {
        spawnDefinition = spawnDefinition with { Damage = command.AuthoritativeDamage };
      }

      if (command.AuthoritativeKnockback > 0.0f)
      {
        spawnDefinition = spawnDefinition with
        {
          Knockback = command.AuthoritativeKnockback
        };
      }

      if (command.AuthoritativeArmorPenetration > 0)
      {
        spawnDefinition = spawnDefinition with
        {
          ArmorPenetration = command.AuthoritativeArmorPenetration
        };
      }

      if (command.AuthoritativeBonusCritChance > 0)
      {
        spawnDefinition = spawnDefinition with
        {
          BonusCritChance = command.AuthoritativeBonusCritChance
        };
      }

      if (command.AuthoritativeScale > 0.0f)
      {
        spawnDefinition = spawnDefinition with
        {
          Scale = command.AuthoritativeScale
        };
      }
      Entity projectile = _projectileSpawnSystem.Spawn(
        World,
        command,
        spawnDefinition,
        replicationId,
        _projectileHitImmunity);
      LocationComponent transform = World.Get<LocationComponent>(projectile);
      VelocityComponent velocityComponent = World.Get<VelocityComponent>(projectile);
      ProjectileBehaviorComponent behavior = World.Get<ProjectileBehaviorComponent>(projectile);
      ProjectileNetworkIdentityComponent networkIdentity =
        World.Get<ProjectileNetworkIdentityComponent>(projectile);
      ProjectileDefinitionComponent definitionComponent =
        World.Get<ProjectileDefinitionComponent>(projectile);
      ProjectileBehaviorReplicationState behaviorState =
        ProjectileBehaviorStateProjection.Project(behavior);
      SimulationVector position = new(transform.X, transform.Y);
      SimulationVector velocity = new(velocityComponent.X, velocityComponent.Y);
      _projectileIdsByEntity.Add(projectile, replicationId);
      _projectileReplications.Add(replicationId, new ProjectileReplicationSnapshot(
        replicationId,
        command.ProjectileType,
        command.Owner,
        position,
        velocity,
        spawnDefinition.Damage,
        definitionComponent.DefaultLifetimeTicks,
        IsActive: true,
        Revision: 1,
        GetSectionCoordinates(position),
        Identity: replicationId,
        ProjectileUuid: networkIdentity.ProjectileUuid,
        Ai0: behaviorState.Ai0,
        Ai1: behaviorState.Ai1,
        Ai2: behaviorState.Ai2,
        Banner: World.Get<ProjectileBannerResponseComponent>(projectile).BannerId,
        DefinitionKnockback: spawnDefinition.Knockback,
        DefinitionOriginalDamage: spawnDefinition.OriginalDamage == 0
          ? spawnDefinition.Damage
          : spawnDefinition.OriginalDamage,
        Reflected: false,
        LegacyAiStyle: spawnDefinition.LegacyAiStyle,
        MaximumPenetration: spawnDefinition.MaximumPenetration,
        DecidesManualFallThrough: definitionComponent.DecidesManualFallThrough,
        ShouldFallThrough: World.Has<ProjectileFallThroughComponent>(projectile) &&
          World.Get<ProjectileFallThroughComponent>(projectile).ShouldFallThrough,
        Direction: World.Get<ProjectileDirectionComponent>(projectile).Horizontal,
        IsSentry: definitionComponent.IsSentry,
        IsDd2Summon: World.Has<ProjectileDd2SummonComponent>(projectile),
        IsBobber: World.Has<ProjectileBobberComponent>(projectile),
        IsMinion: World.Has<ProjectileMinionComponent>(projectile),
        MinionSlots: World.Has<ProjectileMinionComponent>(projectile)
          ? World.Get<ProjectileMinionComponent>(projectile).Slots
          : 0.0f,
        MinionPosition: World.Has<ProjectileMinionComponent>(projectile)
          ? World.Get<ProjectileMinionComponent>(projectile).Position
          : 0,
        ManualDirectionChange: definitionComponent.ManualDirectionChange,
        UsesOwnerMeleeHitCooldown: definitionComponent.UsesOwnerMeleeHitCooldown,
        CopiesOwnerAttackCooldownToLocalImmunityOnSpawn:
          definitionComponent.CopiesOwnerAttackCooldownToLocalImmunityOnSpawn,
        HostileDamageScaling: definitionComponent.HostileDamageScaling,
        CollidesWithTiles: definitionComponent.CollidesWithTiles,
        TileCollisionEnabled: World.Get<ProjectileTileCollisionComponent>(projectile).Enabled,
        PrimaryUpdatePending: World.Get<ProjectileNetworkUpdateComponent>(projectile)
          .PrimaryUpdatePending,
        IgnoreWater: definitionComponent.IgnoreWater,
        ReflectsFromTiles: definitionComponent.ReflectsFromTiles,
        CorrectSlopeCollision: definitionComponent.CorrectSlopeCollision,
        MaximumBounces: definitionComponent.MaximumBounces,
        BounceVelocityMultiplier: definitionComponent.BounceVelocityMultiplier,
        MinimumBounceSpeed: definitionComponent.MinimumBounceSpeed,
        ChildSpawn: definitionComponent.ChildSpawn,
        OnHitStatusEffect: definitionComponent.OnHitStatusEffect,
        OnDespawnStatusEffect: definitionComponent.OnDespawnStatusEffect,
        OnDespawnAreaDamage: definitionComponent.OnDespawnAreaDamage,
        BehaviorId: definitionComponent.BehaviorId,
        Friendly: definitionComponent.Friendly,
        Hostile: definitionComponent.Hostile,
        PlayerDamagePolicy: definitionComponent.PlayerDamagePolicy));
      RefreshProjectileOwnerMinionTarget(projectile);
      if (isSentry)
      {
        ref PlayerSentryStateComponent sentryState =
          ref World.Get<PlayerSentryStateComponent>(ownerEntity);
        sentryState.RequestReconcile();
        admittedSentryCommands.Add(command);
      }
    }

    ApplyProjectileSentryLimits(_sentryEventActive);

    for (int index = 0; index < _commands.BounceProjectileCommands.Count; index++)
    {
      BounceProjectileCommand command = _commands.BounceProjectileCommands[index];
      if (!World.IsAlive(command.Target))
      {
        continue;
      }

      ref ProjectileBounceComponent bounce =
        ref World.Get<ProjectileBounceComponent>(command.Target);
      ProjectileDefinitionComponent definition =
        World.Get<ProjectileDefinitionComponent>(command.Target);
      ref ProjectilePenetrationComponent penetration =
        ref World.Get<ProjectilePenetrationComponent>(command.Target);
      if (bounce.RemainingBounces <= 0 && !definition.ReflectsFromTiles &&
          definition.ProjectileType is not (357 or 645))
      {
        continue;
      }

      if (definition.ProjectileType == 357)
      {
        ref ProjectileDamageComponent damage =
          ref World.Get<ProjectileDamageComponent>(command.Target);
        _projectileBehaviorEffectSystem.ApplyTileCollisionDamage(
          ref damage,
          ref penetration,
          definition);
      }

      ref VelocityComponent velocity = ref World.Get<VelocityComponent>(command.Target);
      ref LocationComponent transform = ref World.Get<LocationComponent>(command.Target);
      if (command.ReflectHorizontal)
      {
        velocity.X = -velocity.X * definition.BounceVelocityMultiplier;
      }

      if (command.ReflectVertical)
      {
        velocity.Y = -velocity.Y * definition.BounceVelocityMultiplier;
      }

      _projectileBehaviorEffectSystem.ApplyTileCollisionBehavior(
        ref World.Get<ProjectileBehaviorComponent>(command.Target),
        ref velocity,
        ref World.Get<ProjectileNetworkUpdateComponent>(command.Target),
        definition);

      ref ProjectileReflectionComponent reflection =
        ref World.Get<ProjectileReflectionComponent>(command.Target);
      reflection.HasReflected = true;

      transform.X = command.SafeX;
      transform.Y = command.SafeY;
      if (bounce.RemainingBounces > 0)
      {
        bounce.RemainingBounces--;
      }

      ProjectileLifetimeComponent lifetime =
        World.Get<ProjectileLifetimeComponent>(command.Target);
      if (World.Has<NpcProjectileOwnerComponent>(command.Target))
      {
        UpdateNpcProjectileReplication(command.Target, transform, velocity, lifetime);
      }
      else
      {
        UpdateProjectileReplication(command.Target, transform, velocity, lifetime);
      }
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
        _worldRules,
        command.ArmorPenetration);
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

      EmitProjectileChildSpawnCommands(command.Target);
      EmitProjectileTileConversionCommand(command.Target);
      EmitType281TerminalRelease(command.Target);
      _pendingProjectileWorldObjectPlacements.Remove(command.Target);
      MarkProjectileInactive(command.Target, command.ProjectileReason);
      commandBuffer.Destroy(command.Target);
    }

    commandBuffer.Playback(World);
    WorldGrid.CommitTileChanges();
    _commands.Clear();
  }

  public int ReconcileProjectileSentryLimits(bool eventActive)
  {
    ThrowIfDisposed();
    RequestProjectileSentryReconciliation();
    return ApplyProjectileSentryLimits(eventActive);
  }

  private void RequestProjectileSentryReconciliation()
  {
    foreach (KeyValuePair<PlayerHandle, Entity> entry in _players)
    {
      Entity playerEntity = entry.Value;
      if (!World.IsAlive(playerEntity) ||
          !World.Get<PlayerLifecycleComponent>(playerEntity).IsActive ||
          !World.Has<PlayerSentryStateComponent>(playerEntity))
      {
        continue;
      }

      World.Get<PlayerSentryStateComponent>(playerEntity).RequestReconcile();
    }
  }

  private int ApplyProjectileSentryLimits(bool eventActive)
  {
    int commandCount = 0;
    foreach (KeyValuePair<PlayerHandle, Entity> entry in _players.OrderBy(pair => pair.Key.Value))
    {
      Entity playerEntity = entry.Value;
      if (!World.IsAlive(playerEntity) ||
          !World.Get<PlayerLifecycleComponent>(playerEntity).IsActive ||
          !World.Has<PlayerSentryStateComponent>(playerEntity))
      {
        continue;
      }

      ref PlayerSentryStateComponent state =
        ref World.Get<PlayerSentryStateComponent>(playerEntity);
      if (!state.ShouldReconcile)
      {
        continue;
      }

      IReadOnlyList<DespawnEntityCommand> commands =
        _projectileSentryLimitSystem.CollectDespawnCommands(
          World,
          _projectileIdsByEntity,
          entry.Key,
          state.MaximumTurrets,
          eventActive);
      for (int index = 0; index < commands.Count; index++)
      {
        _commands.Enqueue(commands[index]);
      }

      commandCount += commands.Count;
      state.MarkReconciled();
    }

    return commandCount;
  }

  private void CommitProjectileWorldObjectPlacements()
  {
    for (int index = 0; index < _commands.ProjectileWorldObjectPlacementCommands.Count; index++)
    {
      ProjectileWorldObjectPlacementCommand command =
        _commands.ProjectileWorldObjectPlacementCommands[index];
      bool ownerActive = World.IsAlive(command.Owner) &&
        World.Has<PlayerLifecycleComponent>(command.Owner) &&
        World.Get<PlayerLifecycleComponent>(command.Owner).IsActive;
      bool projectileActive = World.IsAlive(command.Projectile) &&
        _projectileIdsByEntity.TryGetValue(command.Projectile, out int replicationId) &&
        _projectileReplications.TryGetValue(replicationId, out ProjectileReplicationSnapshot snapshot) &&
        snapshot.IsActive;
      WorldObjectPlacementResult result;
      if (!ownerActive)
      {
        result = WorldObjectPlacementResult.Rejected(
          command.Sequence,
          WorldObjectPlacementFailureCode.OwnerInactive);
      }
      else if (!projectileActive)
      {
        result = WorldObjectPlacementResult.Rejected(
          command.Sequence,
          WorldObjectPlacementFailureCode.ProjectileInactive);
      }
      else
      {
        result = TryCommitProjectileSignPlacement(command, ownerActive, projectileActive);
      }

      if (result.Committed)
      {
        _pendingProjectileWorldObjectPlacements.Remove(command.Projectile);
      }
      else if (result.FailureCode is not (
                 WorldObjectPlacementFailureCode.OccupiedTile or
                 WorldObjectPlacementFailureCode.VersionConflict))
      {
        _pendingProjectileWorldObjectPlacements.Remove(command.Projectile);
      }
    }
  }

  private void EmitType281TerminalRelease(Entity projectile)
  {
    if (!World.IsAlive(projectile) || !World.Has<ProjectileDefinitionComponent>(projectile) ||
        !World.Has<ProjectileOwnerComponent>(projectile))
    {
      return;
    }

    ProjectileDefinitionComponent definition = World.Get<ProjectileDefinitionComponent>(projectile);
    if (definition.ProjectileType != 281)
    {
      return;
    }

    LocationComponent transform = World.Get<LocationComponent>(projectile);
    ColliderComponent collider = World.Get<ColliderComponent>(projectile);
    ProjectileOwnerComponent owner = World.Get<ProjectileOwnerComponent>(projectile);
    int releaseVariant = World.Get<VelocityComponent>(projectile).X > 0.0f ? 3 : 4;
    _pendingNpcSpawnCommands.Add(new SpawnNpcCommand(
      614,
      new SimulationVector(
        transform.X + collider.Width * 0.5f,
        MathF.Max(0.0f, transform.Y + collider.Height - 4.0f)),
      NpcComponents.NpcSpawnSource.Command,
      DifficultyScale: Type281ReleaseDifficultyScale,
      ReleaseOwner: owner.Owner.Value,
      ReleaseVariant: releaseVariant));
  }

  private void EnqueuePendingNpcSpawns()
  {
    for (int index = 0; index < _pendingNpcSpawnCommands.Count; index++)
    {
      _commands.Enqueue(_pendingNpcSpawnCommands[index]);
    }

    _pendingNpcSpawnCommands.Clear();
  }

  private void EmitProjectileOnDespawnAreaDamageCommands()
  {
    for (int index = 0; index < _commands.DespawnEntityCommands.Count; index++)
    {
      DespawnEntityCommand command = _commands.DespawnEntityCommands[index];
      if (!World.IsAlive(command.Target) ||
          !World.Has<ProjectileDefinitionComponent>(command.Target))
      {
        continue;
      }

      ProjectileDefinitionComponent definition =
        World.Get<ProjectileDefinitionComponent>(command.Target);
      ProjectileOnDespawnStatusEffect statusEffect = definition.OnDespawnStatusEffect;
      if (statusEffect.IsEnabled && World.Has<ProjectileOwnerComponent>(command.Target))
      {
        LocationComponent transform = World.Get<LocationComponent>(command.Target);
        ColliderComponent collider = World.Get<ColliderComponent>(command.Target);
        ProjectileOwnerComponent owner = World.Get<ProjectileOwnerComponent>(command.Target);
        _commands.Enqueue(new ApplyAreaStatusEffectCommand(
          owner.Owner,
          transform.X + collider.Width * 0.5f,
          transform.Y + collider.Height * 0.5f,
          statusEffect.Radius,
          statusEffect.Type,
          statusEffect.DurationTicks));
      }

      ProjectileOnDespawnAreaDamage area = definition.OnDespawnAreaDamage;
      if (!area.IsEnabled)
      {
        continue;
      }

      ProjectileDamageComponent damage = World.Get<ProjectileDamageComponent>(command.Target);
      LocationComponent projectileTransform = World.Get<LocationComponent>(command.Target);
      ColliderComponent projectileCollider = World.Get<ColliderComponent>(command.Target);
      float centerX = projectileTransform.X + projectileCollider.Width * 0.5f;
      float centerY = projectileTransform.Y + projectileCollider.Height * 0.5f;
      float left = centerX - area.Width * 0.5f;
      float top = centerY - area.Height * 0.5f;
      ProjectileDefinitionComponent projectileDefinition = definition;

      if (projectileDefinition.PlayerDamagePolicy != PlayerDamagePolicy.None)
      {
        foreach (KeyValuePair<PlayerHandle, Entity> entry in _players)
        {
          if (!World.Get<PlayerLifecycleComponent>(entry.Value).IsActive)
          {
            continue;
          }

          bool canDamagePlayer = World.Has<NpcProjectileOwnerComponent>(command.Target)
            ? projectileDefinition.Hostile &&
              projectileDefinition.PlayerDamagePolicy == PlayerDamagePolicy.HostileNonPvp
            : _projectileTargetEligibilitySystem.CanDamagePlayerTarget(
              projectileDefinition,
              World.Get<ProjectileOwnerComponent>(command.Target).Owner,
              entry.Key,
              _worldRules.IsPvpEnabled);
          if (!canDamagePlayer)
          {
            continue;
          }

          LocationComponent targetTransform = World.Get<LocationComponent>(entry.Value);
          ColliderComponent targetCollider = World.Get<ColliderComponent>(entry.Value);
          if (OverlapsArea(left, top, area, targetTransform, targetCollider))
          {
            _commands.Enqueue(new DamagePlayerCommand(entry.Key, damage.Amount));
          }
        }
      }

      if (projectileDefinition.Friendly)
      {
        foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
        {
          if (!_npcReplications[entry.Key].IsActive)
          {
            continue;
          }

          LocationComponent targetTransform = World.Get<LocationComponent>(entry.Value);
          ColliderComponent targetCollider = World.Get<ColliderComponent>(entry.Value);
          if (OverlapsArea(left, top, area, targetTransform, targetCollider))
          {
            _commands.Enqueue(new DamageCommand(command.Target, entry.Value, damage.Amount));
          }
        }
      }
    }
  }

  private void CommitAreaStatusEffectCommands()
  {
    for (int index = 0; index < _commands.AreaStatusEffectCommands.Count; index++)
    {
      ApplyAreaStatusEffectCommand command = _commands.AreaStatusEffectCommands[index];
      foreach (KeyValuePair<PlayerHandle, Entity> entry in _players)
      {
        PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entry.Value);
        HealthComponent health = World.Get<HealthComponent>(entry.Value);
        if (!lifecycle.IsActive || health.Current <= 0 ||
            !IsWithinStatusEffectRange(command, entry.Value))
        {
          continue;
        }

        BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(entry.Value);
        if (!buffs.CanAccept(command.Type))
        {
          continue;
        }

        buffs.Add(command.Type, command.DurationTicks, command.Source);
        if (command.Type == PlayerPotionStateComponent.PotionSicknessBuffType)
        {
          ref PlayerPotionStateComponent potionState =
            ref World.Get<PlayerPotionStateComponent>(entry.Value);
          _playerPotionDelaySystem.SynchronizeFromBuffs(ref potionState, buffs);
        }
      }

      foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
      {
        NpcComponents.NpcLifecycleComponent lifecycle =
          World.Get<NpcComponents.NpcLifecycleComponent>(entry.Value);
        HealthComponent health = World.Get<HealthComponent>(entry.Value);
        if (!lifecycle.IsActive || health.Current <= 0 ||
            !IsWithinStatusEffectRange(command, entry.Value))
        {
          continue;
        }

        BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(entry.Value);
        if (buffs.CanAccept(command.Type))
        {
          buffs.Add(command.Type, command.DurationTicks, command.Source);
        }
      }
    }
  }

  private static void AddStatusSnapshots(
    ICollection<StatusEffectSnapshot> snapshots,
    StatusEffectTargetKind targetKind,
    int targetId,
    BuffCollectionComponent buffs)
  {
    for (int index = 0; index < buffs.Count; index++)
    {
      BuffEntry entry = buffs.Entries[index];
      snapshots.Add(new StatusEffectSnapshot(
        targetKind,
        targetId,
        buffs.Revision,
        entry.Type,
        entry.RemainingTicks));
    }
  }

  private bool IsWithinStatusEffectRange(
    ApplyAreaStatusEffectCommand command,
    Entity target)
  {
    LocationComponent transform = World.Get<LocationComponent>(target);
    ColliderComponent collider = World.Get<ColliderComponent>(target);
    float deltaX = transform.X + collider.Width * 0.5f - command.CenterX;
    float deltaY = transform.Y + collider.Height * 0.5f - command.CenterY;
    return deltaX * deltaX + deltaY * deltaY < command.Radius * command.Radius;
  }

  private static bool OverlapsArea(
    float left,
    float top,
    ProjectileOnDespawnAreaDamage area,
    LocationComponent targetTransform,
    ColliderComponent targetCollider)
  {
    return left < targetTransform.X + targetCollider.Width &&
      left + area.Width > targetTransform.X &&
      top < targetTransform.Y + targetCollider.Height &&
      top + area.Height > targetTransform.Y;
  }

  private void EmitProjectileChildSpawnCommands(Entity parent)
  {
    if (!World.IsAlive(parent) || !World.Has<ProjectileOwnerComponent>(parent))
    {
      return;
    }

    ProjectileDefinitionComponent definition = World.Get<ProjectileDefinitionComponent>(parent);
    ProjectileChildSpawn childSpawn = definition.ChildSpawn;
    if (!childSpawn.IsEnabled)
    {
      return;
    }

    ProjectileOwnerComponent owner = World.Get<ProjectileOwnerComponent>(parent);
    ProjectileNetworkIdentityComponent identity =
      World.Get<ProjectileNetworkIdentityComponent>(parent);
    ProjectileDamageComponent damage = World.Get<ProjectileDamageComponent>(parent);
    ProjectileBannerResponseComponent banner =
      World.Get<ProjectileBannerResponseComponent>(parent);
    LocationComponent transform = World.Get<LocationComponent>(parent);
    ColliderComponent collider = World.Get<ColliderComponent>(parent);
    int count = childSpawn.MinimumCount +
      (int)(GetChildSpawnRandom(identity.Identity, 0) %
        (uint)(childSpawn.MaximumCount - childSpawn.MinimumCount + 1));
    int childDamage = (int)MathF.Floor(damage.Amount * childSpawn.DamageMultiplier);
    float childKnockback = definition.Knockback * childSpawn.KnockbackMultiplier;
    float centerX = transform.X + collider.Width * 0.5f;
    float centerY = transform.Y + collider.Height * 0.5f;
    for (int index = 0; index < count; index++)
    {
      uint xRandom = GetChildSpawnRandom(identity.Identity, index * 3 + 1);
      uint yRandom = GetChildSpawnRandom(identity.Identity, index * 3 + 2);
      uint speedRandom = GetChildSpawnRandom(identity.Identity, index * 3 + 3);
      float directionX = (int)(xRandom % 201) - 100;
      float directionY = (int)(yRandom % 201) - 100;
      if (directionX == 0.0f && directionY == 0.0f)
      {
        directionX = 1.0f;
      }

      float directionLength = MathF.Sqrt(directionX * directionX + directionY * directionY);
      float speed = childSpawn.MinimumSpeed +
        (childSpawn.MaximumSpeed - childSpawn.MinimumSpeed) * (speedRandom / (float)uint.MaxValue);
      float velocityX = directionX / directionLength * speed;
      float velocityY = directionY / directionLength * speed;
      _pendingProjectileSpawnCommands.Add(new SpawnProjectileCommand(
        owner.Owner,
        centerX,
        centerY,
        velocityX < 0.0f ? -1 : 1,
        childDamage,
        41,
        InitialVelocityY: velocityY,
        ProjectileType: childSpawn.ProjectileType,
        ProjectileSpeed: MathF.Abs(velocityX),
        AuthoritativeDamage: childDamage,
        AuthoritativeKnockback: childKnockback,
        BannerIdToRespondTo: banner.BannerId));
    }
  }

  private void EmitType658ChildSpawnCommand(Entity parent)
  {
    if (!World.IsAlive(parent) || !World.Has<ProjectileOwnerComponent>(parent) ||
        !World.Has<ProjectileDefinitionComponent>(parent) ||
        !World.Has<ProjectileBehaviorComponent>(parent))
    {
      return;
    }

    ProjectileDefinitionComponent definition = World.Get<ProjectileDefinitionComponent>(parent);
    ProjectileBehaviorComponent behavior = World.Get<ProjectileBehaviorComponent>(parent);
    if (!_projectileBehaviorEffectSystem.ShouldSpawnType658Child(definition, behavior) ||
        !_projectileDefinitions.TryGet(
          Type658ChildProjectileType,
          out ProjectileDefinition childDefinition))
    {
      return;
    }

    ProjectileOwnerComponent owner = World.Get<ProjectileOwnerComponent>(parent);
    if (!owner.Owner.IsValid || !World.Has<LocationComponent>(parent) ||
        !World.Has<ColliderComponent>(parent))
    {
      return;
    }

    LocationComponent transform = World.Get<LocationComponent>(parent);
    ColliderComponent collider = World.Get<ColliderComponent>(parent);
    int childDamage = _worldRules.IsExpertMode
      ? Type658ChildExpertDamage
      : Type658ChildDamage;
    _pendingProjectileSpawnCommands.Add(new SpawnProjectileCommand(
      owner.Owner,
      transform.X + collider.Width * 0.5f,
      transform.Y + collider.Height * 0.5f,
      1,
      childDamage,
      childDefinition.LifetimeTicks,
      ProjectileType: Type658ChildProjectileType,
      AuthoritativeDamage: childDamage,
      AuthoritativeKnockback: Type658ChildKnockback,
      UseZeroVelocity: true));
  }

  private void EmitProjectileTileConversionCommand(Entity projectile)
  {
    if (!World.IsAlive(projectile) || !World.Has<ProjectileDefinitionComponent>(projectile) ||
        !World.Has<LocationComponent>(projectile))
    {
      return;
    }

    ProjectileDefinitionComponent definition = World.Get<ProjectileDefinitionComponent>(projectile);
    byte conversionType = definition.ProjectileType switch
    {
      69 => 2,
      70 => 1,
      621 => 4,
      _ => 0
    };
    if (conversionType == 0)
    {
      return;
    }

    LocationComponent transform = World.Get<LocationComponent>(projectile);
    int x = Math.Clamp((int)MathF.Floor(transform.X), 0, WorldGrid.Width - 1);
    int y = Math.Clamp((int)MathF.Floor(transform.Y), 0, WorldGrid.Height - 1);
    WorldGrid.EnqueueProjectileTileConversion(new ProjectileTileConversionCommand(
      definition.ProjectileType, x, y, conversionType, NextLiquidSequence()));
  }

  private static uint GetChildSpawnRandom(int parentIdentity, int sequence)
  {
    uint value = unchecked((uint)parentIdentity) ^ unchecked((uint)sequence * 0x9E3779B9u);
    value ^= value >> 16;
    value *= 0x7FEB352Du;
    value ^= value >> 15;
    value *= 0x846CA68Bu;
    return value ^ (value >> 16);
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
      LocationComponent transform = World.Get<LocationComponent>(playerEntity);
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
      LocationComponent transform = World.Get<LocationComponent>(npcEntity);
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
    _npcSpawnRejectionReasons.Clear();
    for (int index = 0; index < _commands.SpawnNpcCommands.Count; index++)
    {
      SpawnNpcCommand command = _commands.SpawnNpcCommands[index];
      if (!TryResolveNpcSlot(
            command,
            out NpcHandle npc,
            out bool reusesExisting,
            out Entity replacedEntity,
            out NpcReplicationSnapshot previousReplication,
            out string failureReason))
      {
        _npcSpawnRejectionReasons.Add(failureReason);
        continue;
      }

      if (!TryCommitNpcSpawn(
            command,
            npc,
            reusesExisting,
            replacedEntity,
            previousReplication,
            out failureReason))
      {
        _npcSpawnRejectionReasons.Add(failureReason);
        continue;
      }

      NpcDefinition definition = _npcDefinitions.GetRequired(command.DefinitionId);

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

      ControlInputComponent input = World.Get<ControlInputComponent>(entry.Value);
      InventoryComponent inventory = World.Get<InventoryComponent>(entry.Value);
      if (input.UseItemJustPressed)
      {
        _commands.Enqueue(new UseItemCommand(
          entry.Key,
          inventory.SelectedSlot,
          TakeWiringSequence()));
        continue;
      }

      if (!input.UseItem)
      {
        continue;
      }

      ItemStack selectedStack = inventory.GetSlot(inventory.SelectedSlot);
      if (selectedStack.IsEmpty ||
          !_itemDefinitions.TryGet(selectedStack.ItemType, out ItemDefinition definition) ||
          selectedStack.Quantity > definition.StackLimit ||
          !definition.AutoReuse)
      {
        continue;
      }

      ItemUseStateComponent useState = World.Get<ItemUseStateComponent>(entry.Value);
      if (useState.CanUse)
      {
        _commands.Enqueue(new UseItemCommand(
          entry.Key,
          inventory.SelectedSlot,
          TakeWiringSequence()));
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
          !_itemDefinitions.TryGet(stack.ItemType, out ItemDefinition definition) ||
          stack.Quantity > definition.StackLimit)
      {
        continue;
      }

      Entity playerEntity = _players[command.Player];
      LocationComponent transform = World.Get<LocationComponent>(playerEntity);
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

      LocationComponent transform = World.Get<LocationComponent>(playerEntity);
      float deltaX = transform.X - command.TargetX;
      float deltaY = transform.Y - command.TargetY;
      if (deltaX * deltaX + deltaY * deltaY > MaximumInteractionRange * MaximumInteractionRange)
      {
        continue;
      }

      WorldTile target = WorldGrid.GetTile(command.TargetX, command.TargetY);
      ItemStack input = inventory.GetSlot(command.SourceSlot);
      if (!target.IsActive || input.IsEmpty ||
          !_itemDefinitions.TryGet(input.ItemType, out ItemDefinition definition) ||
          input.Quantity > definition.StackLimit)
      {
        continue;
      }

      int extractionMode = definition.Extractinator?.ExtractionMode ?? -1;
      if (definition.IsChlorophyteExtractinatorConsumable &&
          target.Type != ExtractinatorSystem.ChlorophyteExtractinatorTileType)
      {
        continue;
      }
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

      for (int slot = chest.Inventory.Capacity - 1; slot >= 0; slot--)
      {
        ItemStack input = chest.GetSlot(slot);
        if (input.IsEmpty || !_itemDefinitions.TryGet(input.ItemType, out ItemDefinition definition) ||
            input.Quantity > definition.StackLimit)
        {
          continue;
        }

        int extractionMode = definition.Extractinator?.ExtractionMode ?? -1;
        if (definition.IsChlorophyteExtractinatorConsumable &&
            sourceTile.Type != ExtractinatorSystem.ChlorophyteExtractinatorTileType)
        {
          continue;
        }
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

      if (definition.IsExpertOnly && !_worldRules.IsExpertMode && !_worldRules.IsMasterMode)
      {
        continue;
      }

      if (definition.UseTurn)
      {
        ref DirectionComponent facing = ref World.Get<DirectionComponent>(playerEntity);
        if (facing.Horizontal == 0)
        {
          facing.Horizontal = 1;
        }
      }

      if (!_playerFishingUseSystem.CanUse(
            World,
            _players,
            _projectileIdsByEntity,
            command.Player,
            definition))
      {
        continue;
      }

      bool requiresBait = definition.Gathering is ItemGatheringDefinition fishingDefinition &&
        fishingDefinition.FishingPolePower > 0;
      if (requiresBait && !_playerFishingUseSystem.HasBait(inventory, _itemDefinitions))
      {
        continue;
      }

      LocationComponent transform = World.Get<LocationComponent>(playerEntity);
      int summonNpcType = definition.Summoning?.NpcType ?? 0;
      if (summonNpcType > 0 &&
          (!_npcDefinitions.TryGet(summonNpcType, out _) ||
           !float.IsFinite(transform.X) || !float.IsFinite(transform.Y)))
      {
        continue;
      }

      if (definition.ShootsEveryUse && definition.Shoot == 0)
      {
        continue;
      }

      bool consumesAmmo = TryGetRequiredAmmo(definition, out ushort ammoType);
      if (consumesAmmo &&
          !_itemAmmoConsumptionSystem.HasAmmo(inventory, ammoType, _itemDefinitions))
      {
        continue;
      }

      if (TryGetItemProjectileType(definition, out ushort sentryProjectileType) &&
          _projectileDefinitions.TryGet(
            sentryProjectileType,
            out ProjectileDefinition sentryDefinition) &&
          (definition.IsSentry || sentryDefinition.IsSentry) &&
          !_projectileSentryPlacementSystem.CanPlace(
            World,
            transform.X,
            transform.Y + 0.75f,
            sentryDefinition,
            _projectileDefinitions,
            _commands.SpawnProjectileCommands))
      {
        continue;
      }

      ref Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent useState =
        ref World.Get<Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent>(
          playerEntity);
      Terraria.Dome.Simulation.Items.Components.ItemUseStateComponent originalUseState = useState;
      ref PlayerPotionStateComponent potionState =
        ref World.Get<PlayerPotionStateComponent>(playerEntity);
      PlayerPotionStateComponent originalPotionState = potionState;
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
        command.Sequence,
        potionDelayTicks: potionState.PotionDelayTicks);
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

      if (result.ShootsEveryUse && result.ProjectileType == 0)
      {
        useState = originalUseState;
        potionState = originalPotionState;
        continue;
      }

      if (result.BuffType != 0 && !buffs.CanAccept(result.BuffType))
      {
        useState = originalUseState;
        continue;
      }

      int reservedBuffSlots = result.BuffType != 0 &&
        result.BuffType != PlayerPotionStateComponent.PotionSicknessBuffType &&
        !buffs.Entries.Any(entry => entry.Type == result.BuffType)
        ? 1
        : 0;
      if (result.PotionDelayTicks > 0 &&
          !_playerPotionDelaySystem.CanApplyPotionUse(
            potionState,
            buffs,
            command.Player,
            reservedBuffSlots))
      {
        useState = originalUseState;
        potionState = originalPotionState;
        continue;
      }

      ItemInstanceSnapshot[] inventoryBefore = CaptureInventorySlots(inventory);
      if (consumesAmmo &&
          !_itemAmmoConsumptionSystem.TryConsume(inventory, ammoType, _itemDefinitions, out _))
      {
        useState = originalUseState;
        potionState = originalPotionState;
        continue;
      }

      if (result.PotionDelayTicks > 0)
      {
        if (!_playerPotionDelaySystem.TryApplyPotionUse(
              ref potionState,
              buffs,
              command.Player,
              reservedBuffSlots))
        {
          useState = originalUseState;
          potionState = originalPotionState;
          continue;
        }
      }

      if (requiresBait && !_playerFishingUseSystem.TryConsumeBait(
            inventory,
            _itemDefinitions,
            out _))
      {
        useState = originalUseState;
        potionState = originalPotionState;
        continue;
      }

      if (definition.Recovery is ItemRecoveryDefinition recoveryDefinition &&
          recoveryDefinition.DelayKind != ItemRecoveryDelayKind.None &&
          result.RecoveryDelayTicks > 0)
      {
        ref PlayerCooldownStateComponent cooldownState =
          ref World.Get<PlayerCooldownStateComponent>(playerEntity);
        cooldownState.ApplyRecoveryDelay(
          recoveryDefinition.DelayKind,
          result.RecoveryDelayTicks);
      }

      if (result.NpcType > 0)
      {
        _pendingNpcSpawnCommands.Add(new SpawnNpcCommand(
          DefinitionId: result.NpcType,
          Position: new SimulationVector(transform.X, transform.Y),
          Source: NpcComponents.NpcSpawnSource.Command));
      }

      health.Current = result.Health;
      mana.Current = result.Mana;
      if (result.LuckPotionLevel > 0 && result.LuckPotionDurationTicks > 0)
      {
        ref PlayerLuckStateComponent luckState = ref World.Get<PlayerLuckStateComponent>(playerEntity);
        luckState.ApplyLuckPotion(result.LuckPotionLevel, result.LuckPotionDurationTicks);
      }
      ref PlayerMountStateComponent mount = ref World.Get<PlayerMountStateComponent>(playerEntity);
      _playerNpcTargetingSystem.TryApplyMountSummon(ref mount, definition.Summoning);
      if (result.BuffType != 0)
      {
        buffs.Add(result.BuffType, result.BuffDurationTicks, command.Player);
      }
      if (result.ProjectileType != 0)
      {
        DirectionComponent facing = World.Get<DirectionComponent>(playerEntity);
        int projectileDamage = definition.Damage > 0 ? definition.Damage : ProjectileDamage;
        float projectileKnockback = definition.Knockback;
        bool isMinionProjectile = _projectileDefinitions.TryGet(
          result.ProjectileType,
          out ProjectileDefinition projectileDefinition) && projectileDefinition.IsMinion;
        _commands.Enqueue(new SpawnProjectileCommand(
          command.Player,
          transform.X + result.ItemWidth / 32.0f,
          transform.Y + MathF.Max(0.75f, result.ItemHeight / 32.0f),
          facing.Horizontal,
          projectileDamage,
          ProjectileLifetimeTicks,
          ProjectileType: result.ProjectileType,
          ProjectileSpeed: result.ProjectileSpeed,
          AuthoritativeDamage: projectileDamage,
          AuthoritativeKnockback: projectileKnockback,
          AuthoritativeArmorPenetration: definition.ArmorPenetration,
          AuthoritativeBonusCritChance: definition.CriticalChance,
          AuthoritativeDamageClass: result.DamageClass,
          ShootsEveryUse: result.ShootsEveryUse,
          AuthoritativeScale: definition.Scale,
          AuthoritativeBonusTagDamage: result.BonusTagDamage,
          IsSentry: result.IsSentry,
          IsDd2Summon: result.IsDd2Summon,
          MinionSpawnItemType: isMinionProjectile ? result.Event.ItemType : (ushort)0,
          MinionSpawnItemPrefix: isMinionProjectile ? result.Event.ItemPrefix : 0));
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
    if (definition.UsesAmmo)
    {
      ammoType = definition.UseAmmo != 0 ? definition.UseAmmo : definition.Ammo;
      return true;
    }

    ammoType = 0;
    return false;
  }

  private static bool TryGetItemProjectileType(
    ItemDefinition definition,
    out ushort projectileType)
  {
    projectileType = definition.Shoot;
    return projectileType != 0;
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
          !_itemDefinitions.TryGet(stack.ItemType, out ItemDefinition definition) ||
          stack.Quantity > definition.StackLimit)
      {
        continue;
      }

      LocationComponent transform = World.Get<LocationComponent>(playerEntity);
      float deltaX = transform.X - command.X;
      float deltaY = transform.Y - command.Y;
      float placementRange = MaximumInteractionRange + (definition.Placement?.TileBoost ?? 0);
      if (deltaX * deltaX + deltaY * deltaY > placementRange * placementRange)
      {
        continue;
      }

      WorldTile targetTile = WorldGrid.GetTile(command.X, command.Y);
      if (definition.PreventsWetPlacement && targetTile.LiquidAmount > 0)
      {
        continue;
      }
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
          stack.Quantity > definition.StackLimit ||
          definition.EquipmentSlot == ItemEquipmentSlot.None ||
          states.Contains(definition.EquipmentSlot))
      {
        continue;
      }

      if (!_itemEquipmentSystem.TryEquip(
            command.Player,
            stack,
            command.SourceSlot,
            definition,
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
    foreach (KeyValuePair<PlayerHandle, Entity> entry in _players)
    {
      Entity playerEntity = entry.Value;
      if (!World.Has<InventoryComponent>(playerEntity) ||
          !World.Has<EquipmentStateCollectionComponent>(playerEntity) ||
          !World.Has<PlayerLifecycleComponent>(playerEntity) ||
          !World.Get<PlayerLifecycleComponent>(playerEntity).IsActive ||
          !World.Has<PlayerSentryStateComponent>(playerEntity))
      {
        continue;
      }

      ref DefenseComponent defense = ref World.Get<DefenseComponent>(playerEntity);
      ref HealthRegenerationComponent healthRegeneration =
        ref World.Get<HealthRegenerationComponent>(playerEntity);
      ref ManaComponent mana = ref World.Get<ManaComponent>(playerEntity);
      EquipmentStateCollectionComponent equipmentStates =
        World.Get<EquipmentStateCollectionComponent>(playerEntity);
      InventoryComponent inventory = World.Get<InventoryComponent>(playerEntity);
      _equipmentStatSystem.Apply(
        ref defense,
        ref healthRegeneration,
        ref mana,
        equipmentStates,
        inventory,
        _itemDefinitions);
      int sentryCapacityBonus = _playerSentryEquipmentSystem.CalculateCapacityBonus(
        equipmentStates,
        inventory,
        _itemDefinitions);
      if (!_playerSentryAuthoritySystem.TrySetEquipmentCapacityBonus(
            World,
            _players,
            entry.Key,
            sentryCapacityBonus))
      {
        throw new InvalidOperationException(
          $"Player sentry equipment authority rejected active player {entry.Key.Value}.");
      }
      BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(playerEntity);
      int sentryBuffCapacityBonus = _playerSentryBuffSystem.CalculateCapacityBonus(buffs);
      if (!_playerSentryAuthoritySystem.TrySetBuffCapacityBonus(
            World,
            _players,
            entry.Key,
            sentryBuffCapacityBonus))
      {
        throw new InvalidOperationException(
          $"Player sentry buff authority rejected active player {entry.Key.Value}.");
      }
      int sentryArmorSetCapacityBonus = _playerSentryArmorSetSystem.CalculateCapacityBonus(
        equipmentStates,
        inventory,
        _itemDefinitions);
      if (!_playerSentryAuthoritySystem.TrySetArmorSetCapacityBonus(
            World,
            _players,
            entry.Key,
            sentryArmorSetCapacityBonus))
      {
        throw new InvalidOperationException(
          $"Player sentry armor-set authority rejected active player {entry.Key.Value}.");
      }
      ref PlayerTargetingStateComponent targeting =
        ref World.Get<PlayerTargetingStateComponent>(playerEntity);
      _playerNpcTargetingSystem.RefreshFromEquipment(
        ref targeting,
        equipmentStates,
        inventory,
        _npcNoAggroCapabilities);
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
            out _) ||
          !_itemDefinitions.TryGet(result.ItemType, out ItemDefinition resultDefinition) ||
          result.Quantity > resultDefinition.StackLimit)
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
      LocationComponent playerTransform = World.Get<LocationComponent>(playerEntity);
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
    IReadOnlySet<int> activeNpcTypes = CreateActiveNpcTypes();
    for (int index = 0; index < _commands.DamageNpcCommands.Count; index++)
    {
      DamageNpcCommand command = _commands.DamageNpcCommands[index];
      if (!_npcs.TryGetValue(command.Npc, out Entity npcEntity) ||
          !_npcReplications[command.Npc].IsActive ||
          !Enum.IsDefined(command.SourceKind))
      {
        continue;
      }

      bool isHostileNpcSource = false;
      if (command.SourceKind == NpcDamageSourceKind.HostileNpc)
      {
        if (!command.SourceNpc.IsValid || command.SourceNpc == command.Npc ||
            !_npcs.TryGetValue(command.SourceNpc, out Entity sourceEntity) ||
            !_npcReplications[command.SourceNpc].IsActive)
        {
          continue;
        }

        NpcComponents.NpcDefinitionComponent sourceDefinition =
          World.Get<NpcComponents.NpcDefinitionComponent>(sourceEntity);
        isHostileNpcSource = sourceDefinition.Faction == NpcFaction.Hostile;
        if (!isHostileNpcSource)
        {
          continue;
        }
      }

      NpcComponents.NpcCombatStateComponent combat =
        World.Get<NpcComponents.NpcCombatStateComponent>(npcEntity);
      if (isHostileNpcSource && combat.DoesNotTakeDamageFromHostiles)
      {
        continue;
      }

      ref HealthComponent health = ref World.Get<HealthComponent>(npcEntity);
      if (command.SourceKind == NpcDamageSourceKind.Lava &&
          (command.SourceIdentity != 0 || command.SourceNpc.IsValid || combat.LavaImmune))
      {
        continue;
      }

      ref ImmunityComponent immunity = ref World.Get<ImmunityComponent>(npcEntity);
      DefenseComponent defense = new(combat.Defense);
      bool applied = _damageResolutionSystem.TryResolve(
        ref health,
        defense,
        ref immunity,
        command.Amount,
        DamageTargetKind.Npc,
        _worldRules,
        out _,
         combat.TakenDamageMultiplier);
      if (applied)
      {
        immunity.RemainingTicks = command.SourceKind == NpcDamageSourceKind.Lava
          ? NpcLavaImmunityTicks
          : NpcHitImmunityTicks;
      }

      ref NpcComponents.NpcLifecycleComponent lifecycle =
        ref World.Get<NpcComponents.NpcLifecycleComponent>(npcEntity);
      NpcComponents.NpcDefinitionComponent definition =
        World.Get<NpcComponents.NpcDefinitionComponent>(npcEntity);
      _ = _npcLifecycleSystem.Advance(
        ref lifecycle,
        health.Current,
        combat.Immortal,
        LegacyNpcInactivityRegistry.DoesNotDespawnToInactivity(
          definition.NetId,
          activeNpcTypes),
        isTownNpc: LegacyNpcTownRegistry.IsTownNpc(definition.NetId));
      UpdateNpcReplication(npcEntity);
    }
  }

  private void DetectProjectileHits()
  {
    List<DamageRequestedEvent> candidates = new();
    HashSet<Entity> tileDespawned = new();
    HashSet<Entity> playerTargets = new();
    World.Query(
      in _projectileQuery,
      (Entity projectileEntity, ref LocationComponent projectileTransform,
        ref ColliderComponent projectileCollider, ref ProjectileDamageComponent damage,
        ref VelocityComponent projectileVelocity,
        ref ProjectileTileCollisionComponent tileCollision,
        ref ProjectileNetworkIdentityComponent identity,
        ref ProjectileDefinitionComponent definition,
        ref ProjectileFriendlyStateComponent friendlyState,
        ref ProjectileOwnerComponent owner) =>
      {
        if (IsAutomaticStyle17Placement(definition))
        {
          QueueStyle17PlacementRequest(
            projectileEntity,
            ref projectileTransform,
            ref projectileVelocity,
            projectileCollider,
            owner);
          return;
        }

        if (!_projectileTargetEligibilitySystem.CanDamageNpc(definition, friendlyState.IsFriendly) &&
            !_projectileTargetEligibilitySystem.CanDamagePlayer(definition))
        {
          return;
        }

        LocationComponent previousTransform = new(
          projectileTransform.X - projectileVelocity.X,
          projectileTransform.Y - projectileVelocity.Y);
        bool reflectHorizontal = false;
        bool reflectVertical = false;
        bool hitsTile = _projectileTileCollisionPolicy.ShouldCollide(tileCollision) &&
          _projectileCollisionSystem.TryGetSolidTileImpact(
            WorldGrid,
            previousTransform,
            projectileTransform,
            projectileCollider,
            out reflectHorizontal,
            out reflectVertical,
            definition.CorrectSlopeCollision,
            World.Has<ProjectileFallThroughComponent>(projectileEntity) &&
              World.Get<ProjectileFallThroughComponent>(projectileEntity).ShouldFallThrough);
        if (hitsTile)
        {
          ref ProjectileBounceComponent bounce =
            ref World.Get<ProjectileBounceComponent>(projectileEntity);
          float speed = MathF.Abs(projectileVelocity.X) + MathF.Abs(projectileVelocity.Y);
          if ((bounce.RemainingBounces > 0 || definition.ReflectsFromTiles ||
               definition.ProjectileType == 357 &&
                 World.Get<ProjectilePenetrationComponent>(projectileEntity)
                   .RemainingPenetration > 0) &&
              speed >= definition.MinimumBounceSpeed)
          {
            _commands.Enqueue(new BounceProjectileCommand(
              projectileEntity,
              reflectHorizontal,
              reflectVertical,
              previousTransform.X,
              previousTransform.Y));
          }
          else
          {
            _commands.Enqueue(new DespawnEntityCommand(
              projectileEntity,
              ProjectileTombstoneReason.TileHit));
            tileDespawned.Add(projectileEntity);
          }

          return;
        }

        if (!definition.IgnoreWater && definition.LiquidPolicy == ProjectileLiquidPolicy.Destroy &&
            _projectileCollisionSystem.HitsLiquid(
              WorldGrid,
              projectileTransform,
              projectileCollider))
        {
          _commands.Enqueue(new DespawnEntityCommand(
            projectileEntity,
            ProjectileTombstoneReason.LiquidHit));
          tileDespawned.Add(projectileEntity);
          return;
        }

        if (_projectileTargetEligibilitySystem.CanDamageNpc(definition, friendlyState.IsFriendly))
        {
          foreach (KeyValuePair<NpcHandle, Entity> npcEntry in _npcs)
          {
            Entity npcEntity = npcEntry.Value;
            HealthComponent npcHealth = World.Get<HealthComponent>(npcEntity);
            if (npcHealth.Current <= 0)
            {
              continue;
            }

            NpcComponents.NpcDefinitionComponent npcDefinition =
              World.Get<NpcComponents.NpcDefinitionComponent>(npcEntity);
            NpcComponents.NpcAuthorityComponent npcAuthority =
              World.Get<NpcComponents.NpcAuthorityComponent>(npcEntity);
            NpcComponents.NpcBehaviorStateComponent npcBehavior =
              World.Get<NpcComponents.NpcBehaviorStateComponent>(npcEntity);
            NpcComponents.NpcLifecycleComponent npcLifecycle =
              World.Get<NpcComponents.NpcLifecycleComponent>(npcEntity);
            if (!_projectileTargetEligibilitySystem.CanTargetNpc(
                  definition,
                  npcDefinition,
                  npcAuthority,
                  npcBehavior,
                   npcLifecycle,
                   npcHealth,
                   ignoreDoesNotTakeDamage: false,
                   allowImmortalTargetDummy:
                     npcDefinition.DefinitionId == TrainingDummyNpcType,
                   runtimeFriendly: friendlyState.IsFriendly))
            {
              continue;
            }

            LocationComponent npcTransform = World.Get<LocationComponent>(npcEntity);
            ColliderComponent npcCollider = World.Get<ColliderComponent>(npcEntity);
            if (definition.OwnerHitCheck &&
                (!_players.TryGetValue(owner.Owner, out Entity ownerEntity) ||
                 !_projectileOwnerHitCheckSystem.CanHit(
                   WorldGrid,
                   World.Get<LocationComponent>(ownerEntity),
                   World.Get<ColliderComponent>(ownerEntity),
                   World.Get<DirectionComponent>(ownerEntity).Horizontal,
                   World.Get<PhysicsStateComponent>(ownerEntity).GravityDirection,
                   npcTransform,
                   npcCollider,
                   definition.OwnerHitCheckDistance)))
            {
              continue;
            }

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
              npcEntry.Key.Value,
              definition.DamageClass,
              definition.IsColdDamage,
              definition.ArmorPenetration,
              definition.BonusCritChance,
              definition.BonusTagDamage,
              definition.TagEffectType));
            break;
          }
        }
        else
        {
          foreach (KeyValuePair<PlayerHandle, Entity> playerEntry in _players)
          {
            if (!_projectileTargetEligibilitySystem.CanDamagePlayerTarget(
                  definition,
                  owner.Owner,
                  playerEntry.Key,
                  _worldRules.IsPvpEnabled) ||
                !World.Get<PlayerLifecycleComponent>(playerEntry.Value).IsActive)
            {
              continue;
            }

            HealthComponent playerHealth = World.Get<HealthComponent>(playerEntry.Value);
            if (playerHealth.Current <= 0)
            {
              continue;
            }

            LocationComponent playerTransform = World.Get<LocationComponent>(playerEntry.Value);
            ColliderComponent playerCollider = World.Get<ColliderComponent>(playerEntry.Value);
            if (!PathOverlaps(
              previousTransform,
              projectileTransform,
              projectileCollider,
              playerTransform,
              playerCollider))
            {
              continue;
            }

            candidates.Add(new DamageRequestedEvent(
              projectileEntity,
              playerEntry.Value,
              damage.Amount,
              identity.Identity,
              playerEntry.Key.Value,
              definition.DamageClass,
              definition.IsColdDamage,
              definition.ArmorPenetration,
              definition.BonusCritChance,
              definition.BonusTagDamage,
              definition.TagEffectType));
            playerTargets.Add(playerEntry.Value);
            break;
          }
        }
      });

    IReadOnlyList<DamageRequestedEvent> accepted = _projectileDamageSystem.Resolve(
      World,
      candidates,
      _projectileHitImmunity,
      GetOwnerMeleeHitCooldownTicks);
    for (int index = 0; index < accepted.Count; index++)
    {
      DamageRequestedEvent candidate = accepted[index];
      if (World.Has<ProjectileBehaviorComponent>(candidate.Projectile) &&
          World.Has<ProjectileDefinitionComponent>(candidate.Projectile) &&
          World.Has<ProjectileFriendlyStateComponent>(candidate.Projectile) &&
          World.Has<ProjectileNetworkUpdateComponent>(candidate.Projectile))
      {
        ref ProjectileBehaviorComponent behavior =
          ref World.Get<ProjectileBehaviorComponent>(candidate.Projectile);
        ref ProjectileFriendlyStateComponent friendlyState =
          ref World.Get<ProjectileFriendlyStateComponent>(candidate.Projectile);
        ref ProjectileNetworkUpdateComponent networkUpdate =
          ref World.Get<ProjectileNetworkUpdateComponent>(candidate.Projectile);
        _projectileBehaviorEffectSystem.ApplyAcceptedHit(
          ref behavior,
          ref friendlyState,
          ref networkUpdate,
          World.Get<ProjectileDefinitionComponent>(candidate.Projectile),
          World.Get<ProjectilePenetrationComponent>(candidate.Projectile).RemainingPenetration + 1);
        if (World.Has<ProjectileTileCollisionComponent>(candidate.Projectile))
        {
          _projectileBehaviorEffectSystem.ApplyAcceptedHitTileCollision(
            ref behavior,
            ref World.Get<ProjectileTileCollisionComponent>(candidate.Projectile),
            ref networkUpdate,
            World.Get<ProjectileDefinitionComponent>(candidate.Projectile));
        }
      }

      if (playerTargets.Contains(candidate.Target))
      {
        ProjectileDefinitionComponent definition =
          World.Get<ProjectileDefinitionComponent>(candidate.Projectile);
        _commands.Enqueue(new DamagePlayerCommand(
          FindPlayerHandle(candidate.Target),
          _projectileHostileDamageScalingSystem.ScalePlayerDamage(
            candidate.Amount,
            definition,
            _worldRules),
          candidate.DamageClass,
          candidate.IsColdDamage));
      }
      else
      {
        _commands.Enqueue(new DamageCommand(
          candidate.Projectile,
          candidate.Target,
          candidate.Amount,
          candidate.DamageClass,
          candidate.IsColdDamage,
          candidate.ArmorPenetration,
          candidate.BonusCritChance,
          candidate.BonusTagDamage,
          candidate.TagEffectType));
      }

      if (World.Has<ProjectileDamageComponent>(candidate.Projectile) &&
          World.Has<ProjectileDefinitionComponent>(candidate.Projectile))
      {
        ref ProjectileDamageComponent projectileDamage =
          ref World.Get<ProjectileDamageComponent>(candidate.Projectile);
        _projectileBehaviorEffectSystem.ApplyAcceptedHitDamage(
          ref projectileDamage,
          World.Get<ProjectileDefinitionComponent>(candidate.Projectile));
        _projectileBehaviorEffectSystem.ApplyAcceptedHitKnockback(
          ref World.Get<ProjectileDefinitionComponent>(candidate.Projectile));
      }

      if (World.Has<ProjectileDefinitionComponent>(candidate.Projectile) &&
          World.Has<VelocityComponent>(candidate.Projectile))
      {
        _projectileBehaviorEffectSystem.ApplyAcceptedHitVelocity(
          ref World.Get<VelocityComponent>(candidate.Projectile),
          World.Get<ProjectileDefinitionComponent>(candidate.Projectile));
      }

      if (World.Has<ProjectileDamageComponent>(candidate.Projectile) &&
          World.Has<ProjectilePenetrationComponent>(candidate.Projectile) &&
          World.Has<ProjectileBehaviorComponent>(candidate.Projectile) &&
          World.Has<ProjectileFriendlyStateComponent>(candidate.Projectile) &&
          World.Has<ProjectileNetworkUpdateComponent>(candidate.Projectile))
      {
        _projectileBehaviorEffectSystem.ApplyAcceptedHitPenetration(
          ref World.Get<ProjectileBehaviorComponent>(candidate.Projectile),
          ref World.Get<ProjectileFriendlyStateComponent>(candidate.Projectile),
          ref World.Get<ProjectileNetworkUpdateComponent>(candidate.Projectile),
          ref World.Get<ProjectilePenetrationComponent>(candidate.Projectile),
          ref World.Get<ProjectileDamageComponent>(candidate.Projectile),
          World.Get<ProjectileDefinitionComponent>(candidate.Projectile));
      }

      if (World.Has<ProjectileDefinitionComponent>(candidate.Projectile) &&
          World.Has<ProjectileOwnerComponent>(candidate.Projectile))
      {
        ProjectileDefinitionComponent definition =
          World.Get<ProjectileDefinitionComponent>(candidate.Projectile);
        ProjectileOnHitStatusEffect effect = definition.OnHitStatusEffect;
        if (effect.IsEnabled &&
            ShouldApplyOnHitStatus(candidate.ProjectileIdentity, effect))
        {
          int duration = SelectOnHitStatusDuration(candidate.ProjectileIdentity, effect);
          _commands.Enqueue(new ApplyTargetStatusEffectCommand(
            candidate.Target,
            World.Get<ProjectileOwnerComponent>(candidate.Projectile).Owner,
            effect.Type,
            duration));
        }
      }
      ref ProjectilePenetrationComponent penetration =
        ref World.Get<ProjectilePenetrationComponent>(candidate.Projectile);
      if (penetration.RemainingPenetration == 0 && tileDespawned.Add(candidate.Projectile))
      {
        _commands.Enqueue(new DespawnEntityCommand(
          candidate.Projectile,
          ProjectileTombstoneReason.Penetrated));
      }
    }
  }

  private bool IsAutomaticStyle17Placement(ProjectileDefinitionComponent definition)
  {
    return definition.ProjectileType == VerificationProjectileType &&
      definition.LegacyAiStyle == 17;
  }

  private void QueueStyle17PlacementRequest(
    Entity projectile,
    ref LocationComponent transform,
    ref VelocityComponent velocity,
    ColliderComponent collider,
    ProjectileOwnerComponent owner)
  {
    if (_pendingProjectileWorldObjectPlacements.TryGetValue(
          projectile,
          out ProjectileWorldObjectPlacementCommand pendingPlacement))
    {
      WorldSectionCoordinates originSection = WorldGrid.GetSectionCoordinates(
        pendingPlacement.OriginX,
        pendingPlacement.OriginY);
      long currentSectionVersion = WorldGrid.GetSectionVersion(originSection);
      if (pendingPlacement.ExpectedSectionVersion != currentSectionVersion)
      {
        pendingPlacement = pendingPlacement with
        {
          ExpectedSectionVersion = currentSectionVersion
        };
        _pendingProjectileWorldObjectPlacements[projectile] = pendingPlacement;
      }

      _commands.Enqueue(pendingPlacement);
      return;
    }

    if (velocity.Y <= 0.0f ||
        !_projectileCollisionSystem.TryGetSolidTileImpactTile(
          WorldGrid,
          new LocationComponent(transform.X - velocity.X, transform.Y - velocity.Y),
          transform,
          collider,
          out _,
          out int supportTileY))
    {
      return;
    }

    if (!_players.TryGetValue(owner.Owner, out Entity ownerEntity) ||
        !World.Has<ProjectileMiscTextComponent>(projectile) ||
        !World.Has<ProjectileDirectionComponent>(projectile) ||
        !TryTakeProjectileWorldObjectPlacementSequence(out long sequence))
    {
      return;
    }

    int originX = (int)MathF.Floor(transform.X + collider.Width * 0.5f);
    int originY = supportTileY - Style17SignHeight;
    if (!WorldGrid.Contains(originX, originY) ||
        !WorldGrid.Contains(originX + Style17SignWidth - 1, originY + Style17SignHeight - 1))
    {
      return;
    }

    WorldSectionCoordinates section = WorldGrid.GetSectionCoordinates(originX, originY);
    ProjectileWorldObjectPlacementCommand command = new(
      sequence,
      projectile,
      ownerEntity,
      originX,
      originY,
      (ushort)Style17SignObjectType,
      Style17SignStyle,
      World.Get<ProjectileDirectionComponent>(projectile).Horizontal,
      World.Get<ProjectileMiscTextComponent>(projectile).Value,
      WorldGrid.GetSectionVersion(section));
    _pendingProjectileWorldObjectPlacements.Add(projectile, command);
    transform.Y = supportTileY - collider.Height;
    velocity.X *= Style17GroundDrag;
    velocity.Y = 0.0f;
    UpdateProjectileReplication(
      projectile,
      transform,
      velocity,
      World.Get<ProjectileLifetimeComponent>(projectile));
    _commands.Enqueue(command);
  }

  private void CommitTargetStatusEffectCommands()
  {
    for (int index = 0; index < _commands.TargetStatusEffectCommands.Count; index++)
    {
      ApplyTargetStatusEffectCommand command = _commands.TargetStatusEffectCommands[index];
      if (!World.IsAlive(command.Target) || !World.Has<BuffCollectionComponent>(command.Target))
      {
        continue;
      }

      if (World.Has<PlayerLifecycleComponent>(command.Target) &&
          (!World.Has<HealthComponent>(command.Target) ||
           !World.Get<PlayerLifecycleComponent>(command.Target).IsActive ||
           World.Get<HealthComponent>(command.Target).Current <= 0))
      {
        continue;
      }

      BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(command.Target);
      if (!buffs.CanAccept(command.Type))
      {
        continue;
      }

      buffs.Add(command.Type, command.DurationTicks, command.Source);
      if (command.Type == PlayerPotionStateComponent.PotionSicknessBuffType &&
          World.Has<PlayerPotionStateComponent>(command.Target))
      {
        ref PlayerPotionStateComponent potionState =
          ref World.Get<PlayerPotionStateComponent>(command.Target);
        _playerPotionDelaySystem.SynchronizeFromBuffs(ref potionState, buffs);
      }
    }
  }

  private bool ShouldApplyOnHitStatus(int projectileIdentity, ProjectileOnHitStatusEffect effect)
  {
    uint sample = unchecked((uint)projectileIdentity) + 1U;
    return sample % (uint)effect.ChanceDenominator < (uint)effect.ChanceNumerator;
  }

  private int SelectOnHitStatusDuration(int projectileIdentity, ProjectileOnHitStatusEffect effect)
  {
    int span = effect.MaximumDurationTicks - effect.MinimumDurationTicks + 1;
    uint sample = unchecked((uint)projectileIdentity) + 1U;
    uint bucket = sample / (uint)effect.ChanceDenominator % 3U;
    int selectedSpan = bucket == 0 ? span : Math.Max(1, span / 2);
    uint offset = sample / ((uint)effect.ChanceDenominator * 3U) % (uint)selectedSpan;
    return effect.MinimumDurationTicks + (int)offset;
  }

  private void AdvancePlayerLifecycle()
  {
    _playerLifecycleSystem.Advance(
      World,
      _players,
      (player, spawn) => _commands.Enqueue(new RespawnPlayerCommand(player, spawn)));

    foreach (Entity entity in _players.Values)
    {
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entity);
      if (lifecycle.IsActive)
      {
        continue;
      }

      ref PlayerPotionStateComponent potionState =
        ref World.Get<PlayerPotionStateComponent>(entity);
      _playerPotionDelaySystem.Clear(
        ref potionState,
        World.Get<BuffCollectionComponent>(entity));
    }
  }

  private void AdvancePlayerImmunity()
  {
    foreach (Entity entity in _players.Values)
    {
      ref ImmunityComponent immunity = ref World.Get<ImmunityComponent>(entity);
      _immunitySystem.Tick(ref immunity);
    }
  }

  private void AdvancePlayerCooldowns()
  {
    foreach (Entity entity in _players.Values)
    {
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(entity);
      ref PlayerCooldownStateComponent cooldown =
        ref World.Get<PlayerCooldownStateComponent>(entity);
      if (!lifecycle.IsActive || lifecycle.IsDead)
      {
        cooldown.ClearTransient();
        continue;
      }

      cooldown.Tick();
    }
  }

  private void AdvancePlayerLuckPotions()
  {
    foreach (Entity entity in _players.Values)
    {
      ref PlayerLuckStateComponent luck = ref World.Get<PlayerLuckStateComponent>(entity);
      if (!World.Get<PlayerLifecycleComponent>(entity).IsActive)
      {
        luck.LuckPotion = 0;
        luck.LuckPotionTicks = 0;
        continue;
      }

      luck.TickLuckPotion();
    }
  }

  private void AdvanceNpcImmunity()
  {
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      if (!World.IsAlive(entry.Value))
      {
        continue;
      }

      NpcComponents.NpcLifecycleComponent lifecycle =
        World.Get<NpcComponents.NpcLifecycleComponent>(entry.Value);
      if (!lifecycle.IsActive)
      {
        continue;
      }

      ref ImmunityComponent immunity = ref World.Get<ImmunityComponent>(entry.Value);
      _immunitySystem.Tick(ref immunity);
    }
  }

  private void AdvancePlayerStealth(IReadOnlyList<Entity> activePlayers)
  {
    for (int index = 0; index < activePlayers.Count; index++)
    {
      Entity entity = activePlayers[index];
      ref PlayerStealthStateComponent stealth =
        ref World.Get<PlayerStealthStateComponent>(entity);
      VelocityComponent velocity = World.Get<VelocityComponent>(entity);
      ItemUseStateComponent itemUse = World.Get<ItemUseStateComponent>(entity);
      PlayerMountStateComponent mount = World.Get<PlayerMountStateComponent>(entity);
      _playerNpcTargetingSystem.AdvanceStealth(
        ref stealth,
        itemUse.IsUsing,
        velocity.X,
        velocity.Y,
        mount.IsMounted);
    }
  }

  private void AdvancePlayerBuffs(IReadOnlyList<Entity> activePlayers)
  {
    for (int index = 0; index < activePlayers.Count; index++)
    {
      Entity entity = activePlayers[index];
      BuffCollectionComponent buffs = World.Get<BuffCollectionComponent>(entity);
      _buffDurationSystem.Tick(buffs);
      ref PlayerPotionStateComponent potionState =
        ref World.Get<PlayerPotionStateComponent>(entity);
      _playerPotionDelaySystem.SynchronizeFromBuffs(ref potionState, buffs);
      World.Get<WellFedStateComponent>(entity).Update();
      ref ManaComponent mana = ref World.Get<ManaComponent>(entity);
      _buffEffectSystem.Apply(buffs, ref mana);
    }
  }

  private void AdvanceNpcBuffs()
  {
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      NpcComponents.NpcLifecycleComponent lifecycle =
        World.Get<NpcComponents.NpcLifecycleComponent>(entry.Value);
      HealthComponent health = World.Get<HealthComponent>(entry.Value);
      if (lifecycle.IsActive && health.Current > 0)
      {
        _buffDurationSystem.Tick(World.Get<BuffCollectionComponent>(entry.Value));
      }
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
    List<NpcLavaContactCandidate> lavaCandidates = new(_npcs.Count);
    foreach (KeyValuePair<NpcHandle, Entity> npcEntry in _npcs)
    {
      if (!_npcReplications[npcEntry.Key].IsActive)
      {
        continue;
      }

      LocationComponent npcTransform = World.Get<LocationComponent>(npcEntry.Value);
      ColliderComponent npcCollider = World.Get<ColliderComponent>(npcEntry.Value);
      NpcComponents.NpcDefinitionComponent definition =
        World.Get<NpcComponents.NpcDefinitionComponent>(npcEntry.Value);
      NpcComponents.NpcCombatStateComponent combat =
        World.Get<NpcComponents.NpcCombatStateComponent>(npcEntry.Value);
      NpcComponents.NpcLifecycleComponent lifecycle =
        World.Get<NpcComponents.NpcLifecycleComponent>(npcEntry.Value);
      HealthComponent health = World.Get<HealthComponent>(npcEntry.Value);
      ImmunityComponent immunity = World.Get<ImmunityComponent>(npcEntry.Value);
      npcs.Add(new NpcContactCandidate(
        npcEntry.Key,
        new SimulationVector(npcTransform.X, npcTransform.Y),
        npcCollider,
        definition.Faction,
        IsActive: true));
      lavaCandidates.Add(new NpcLavaContactCandidate(
        npcEntry.Key,
        new SimulationVector(npcTransform.X, npcTransform.Y),
        npcCollider,
         combat,
         lifecycle.IsActive,
         combat.DoesNotTakeDamage,
        health.Current,
        immunity));
    }

    List<PlayerContactCandidate> players = new(_players.Count);
    foreach (KeyValuePair<PlayerHandle, Entity> playerEntry in _players)
    {
      PlayerLifecycleComponent lifecycle = World.Get<PlayerLifecycleComponent>(playerEntry.Value);
      LocationComponent playerTransform = World.Get<LocationComponent>(playerEntry.Value);
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

    IReadOnlyList<DamageNpcCommand> lavaCommands = _npcLavaContactSystem.ProduceDamageCommands(
      WorldGrid,
      lavaCandidates,
      NpcLavaContactDamage);
    for (int index = 0; index < lavaCommands.Count; index++)
    {
      _commands.Enqueue(lavaCommands[index]);
    }
  }

  private void ApplyPlayerInputs(SimulationInputBatch inputBatch)
  {
    foreach (Entity entity in _players.Values)
    {
      ref ControlInputComponent input = ref World.Get<ControlInputComponent>(entity);
      input = new ControlInputComponent();
    }

    for (int index = 0; index < inputBatch.Inputs.Count; index++)
    {
      PlayerInput supplied = inputBatch.Inputs[index];
      if (!_players.TryGetValue(supplied.Player, out Entity entity))
      {
        throw new ArgumentException("Input references an unknown player.", nameof(inputBatch));
      }

      ref ControlInputComponent input = ref World.Get<ControlInputComponent>(entity);
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
      (Entity entity, ref LocationComponent transform, ref VelocityComponent velocity) =>
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
      PhysicsStateComponent physics = World.Get<PhysicsStateComponent>(entity);
      float gravityDirection = physics.GravityDirection == 0.0f
        ? 1.0f
        : physics.GravityDirection;
      velocity.Y += GravityPerTick * gravityDirection;
    }
  }

  private void ResolvePlayerGroundCollision()
  {
    foreach (Entity entity in _players.Values)
    {
      ref LocationComponent transform = ref World.Get<LocationComponent>(entity);
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
    List<NpcTargetCandidate> candidates = new();
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      if (!_npcReplications[entry.Key].IsActive)
      {
        continue;
      }

      Entity npcEntity = entry.Value;
      LocationComponent npcTransform = World.Get<LocationComponent>(npcEntity);
      NpcComponents.NpcDefinitionComponent npcDefinition =
        World.Get<NpcComponents.NpcDefinitionComponent>(npcEntity);
      DirectionComponent npcFacing = World.Get<DirectionComponent>(npcEntity);
      if (!float.IsFinite(npcTransform.X) || !float.IsFinite(npcTransform.Y))
      {
        ref NpcComponents.NpcTargetComponent invalidTarget =
          ref World.Get<NpcComponents.NpcTargetComponent>(npcEntity);
        invalidTarget = new NpcComponents.NpcTargetComponent(
          default,
          0,
          NpcComponents.NpcTargetLockReason.NoValidTarget);
        continue;
      }

      candidates.Clear();
      foreach (KeyValuePair<PlayerHandle, Entity> playerEntry in _players)
      {
        Entity playerEntity = playerEntry.Value;
        PlayerLifecycleComponent playerLifecycle =
          World.Get<PlayerLifecycleComponent>(playerEntity);
        HealthComponent playerHealth = World.Get<HealthComponent>(playerEntity);
        if (!playerLifecycle.IsActive || playerHealth.Current <= 0)
        {
          continue;
        }

        LocationComponent playerTransform = World.Get<LocationComponent>(playerEntity);
        if (!float.IsFinite(playerTransform.X) || !float.IsFinite(playerTransform.Y))
        {
          continue;
        }
        PlayerTargetingStateComponent targeting =
          World.Get<PlayerTargetingStateComponent>(playerEntity);
        bool hasPriority = _npcTargetSelectionSystem.TryCalculateTargetPriority(
          new SimulationVector(npcTransform.X, npcTransform.Y),
          new SimulationVector(playerTransform.X, playerTransform.Y),
          targeting.Aggro,
          targeting.IsNoAggroNpc(npcDefinition.DefinitionId),
          npcFacing.Horizontal != 0,
          out float targetPriority);
        candidates.Add(new NpcTargetCandidate(
          playerEntity,
          playerEntry.Key.Value,
          new SimulationVector(playerTransform.X, playerTransform.Y),
          true,
          playerHealth.Current,
          TargetPriority: hasPriority ? targetPriority : float.NaN));
      }

      NpcComponents.NpcTargetComponent selectedTarget = _npcTargetSelectionSystem.SelectTarget(
        new SimulationVector(npcTransform.X, npcTransform.Y),
        candidates);
      bool hasTarget = selectedTarget.HasTarget;
      ref NpcTargetComponent target = ref World.Get<NpcTargetComponent>(npcEntity);
      target.HasTarget = hasTarget;
      target.Target = selectedTarget.Target;
      target.StableTargetId = selectedTarget.StableTargetId;
      target.LockReason = (NpcTargetLockReason)selectedTarget.LockReason;
      ref NpcComponents.NpcTargetComponent typedTarget =
        ref World.Get<NpcComponents.NpcTargetComponent>(npcEntity);
      typedTarget = selectedTarget;
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
      (Entity entity, ref LocationComponent transform, ref VelocityComponent velocity) =>
      {
        transform.X += velocity.X;
        transform.Y += velocity.Y;
      });
  }

  private void AdvanceNpcLifecycles()
  {
    IReadOnlySet<int> activeNpcTypes = CreateActiveNpcTypes();
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      ref NpcComponents.NpcLifecycleComponent lifecycle =
        ref World.Get<NpcComponents.NpcLifecycleComponent>(entry.Value);
      HealthComponent health = World.Get<HealthComponent>(entry.Value);
      NpcComponents.NpcCombatStateComponent combat =
        World.Get<NpcComponents.NpcCombatStateComponent>(entry.Value);
      NpcComponents.NpcDefinitionComponent definition =
        World.Get<NpcComponents.NpcDefinitionComponent>(entry.Value);
      if (_npcReplications[entry.Key].IsActive && lifecycle.IsActive && health.Current > 0 &&
          World.Has<NpcComponents.NpcHomeComponent>(entry.Value))
      {
        ref NpcComponents.NpcHomeComponent home =
          ref World.Get<NpcComponents.NpcHomeComponent>(entry.Value);
        _npcHomeTimeoutSystem.Tick(ref home);
      }

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
        combat.Immortal,
        LegacyNpcInactivityRegistry.DoesNotDespawnToInactivity(
          definition.NetId,
          activeNpcTypes),
        isTownNpc: LegacyNpcTownRegistry.IsTownNpc(definition.NetId));
    }
  }

  private IReadOnlySet<int> CreateActiveNpcTypes()
  {
    HashSet<int> activeNpcTypes = new();
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      NpcComponents.NpcLifecycleComponent lifecycle =
        World.Get<NpcComponents.NpcLifecycleComponent>(entry.Value);
      if (!lifecycle.IsActive)
      {
        continue;
      }

      NpcComponents.NpcDefinitionComponent definition =
        World.Get<NpcComponents.NpcDefinitionComponent>(entry.Value);
      activeNpcTypes.Add(definition.NetId);
    }

    return activeNpcTypes;
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

  private bool IsAuthoritativeActivePlayer(Entity entity)
  {
    if (!World.IsAlive(entity) || !World.Has<PlayerLifecycleComponent>(entity) ||
        !World.Get<PlayerLifecycleComponent>(entity).IsActive)
    {
      return false;
    }

    foreach (Entity playerEntity in _players.Values)
    {
      if (playerEntity == entity)
      {
        return true;
      }
    }

    return false;
  }

  private bool IsAuthoritativeActiveProjectile(Entity entity)
  {
    return World.IsAlive(entity) &&
      _projectileIdsByEntity.TryGetValue(entity, out int replicationId) &&
      _projectileReplications.TryGetValue(
        replicationId,
        out ProjectileReplicationSnapshot snapshot) &&
      snapshot.IsActive;
  }

  private int GetOwnerMeleeHitCooldownTicks(PlayerHandle owner)
  {
    if (!_players.TryGetValue(owner, out Entity entity))
    {
      return 0;
    }

    return Math.Max(0, World.Get<ItemUseStateComponent>(entity).AnimationTicks);
  }

  private WorldSectionCoordinates GetSectionCoordinates(SimulationVector position)
  {
    return ProjectileSectionCoordinatePolicy.Resolve(position, WorldGrid.Width, WorldGrid.Height);
  }

  private void MarkProjectileInactive(Entity entity, ProjectileTombstoneReason reason)
  {
    if (!_projectileIdsByEntity.Remove(entity, out int replicationId))
    {
      return;
    }

    if (_projectileReplications.TryGetValue(replicationId, out ProjectileReplicationSnapshot snapshot))
    {
      if (!snapshot.IsActive || snapshot.Revision == long.MaxValue)
      {
        return;
      }

      _projectileReplications[replicationId] = snapshot with
      {
        IsActive = false,
        HitCount = World.IsAlive(entity) && World.Has<ProjectileDamageComponent>(entity)
          ? World.Get<ProjectileDamageComponent>(entity).HitCount
          : snapshot.HitCount,
        Revision = snapshot.Revision + 1,
        TombstoneReason = reason == ProjectileTombstoneReason.None
          ? ProjectileTombstoneReason.Administrative
          : reason,
        TombstoneRetainedUntilTick = ProjectileTombstonePolicy.CalculateRetentionUntil(TickNumber),
        SecondaryUpdatePending = false,
        NetSpam = 0,
        PrimaryUpdatePending = false,
        NetworkUpdateReady = true
      };
      return;
    }

    if (!_npcProjectileReplications.TryGetValue(
          replicationId,
          out NpcProjectileReplicationSnapshot npcSnapshot) ||
        !npcSnapshot.IsActive || npcSnapshot.Revision == long.MaxValue)
    {
      return;
    }

    _npcProjectileReplications[replicationId] = npcSnapshot with
    {
      IsActive = false,
      HitCount = World.IsAlive(entity) && World.Has<ProjectileDamageComponent>(entity)
        ? World.Get<ProjectileDamageComponent>(entity).HitCount
        : npcSnapshot.HitCount,
      RemainingLifetime = 0,
      Revision = npcSnapshot.Revision + 1,
      TombstoneReason = reason == ProjectileTombstoneReason.None
        ? ProjectileTombstoneReason.Administrative
        : reason,
      TombstoneRetainedUntilTick = ProjectileTombstonePolicy.CalculateRetentionUntil(TickNumber),
      SecondaryUpdatePending = false,
      NetSpam = 0,
      PrimaryUpdatePending = false,
      NetworkUpdateReady = true
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

      ref LocationComponent transform = ref World.Get<LocationComponent>(entry.Value);
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
        ref DirectionComponent facing = ref World.Get<DirectionComponent>(entry.Value);
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
      LocationComponent transform = World.Get<LocationComponent>(entity);
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
      ref NpcComponents.NpcReplicationComponent replication =
        ref World.Get<NpcComponents.NpcReplicationComponent>(entity);
      replication.MarkDirty();
      return;
    }
  }

  private void UpdateProjectileReplication(
    Entity entity,
    LocationComponent transform,
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
    ProjectileBehaviorReplicationState behaviorState =
      ProjectileBehaviorStateProjection.Project(World.Get<ProjectileBehaviorComponent>(entity));
    ProjectileDamageComponent damage = World.Get<ProjectileDamageComponent>(entity);
    ProjectileUpdateCountComponent updates = World.Get<ProjectileUpdateCountComponent>(entity);
    ProjectileReplicationSnapshot updated = current with
    {
      Position = position,
      Velocity = new SimulationVector(velocity.X, velocity.Y),
      Damage = damage.Amount,
      RemainingLifetime = lifetime.RemainingTicks,
      Section = GetSectionCoordinates(position),
      Ai0 = behaviorState.Ai0,
      Ai1 = behaviorState.Ai1,
      Ai2 = behaviorState.Ai2,
      LocalAi0 = World.Get<ProjectileBehaviorComponent>(entity).State.LocalAi0,
      LocalAi1 = World.Get<ProjectileBehaviorComponent>(entity).State.LocalAi1,
      LocalAi2 = World.Get<ProjectileBehaviorComponent>(entity).State.LocalAi2,
      SecondaryUpdatePending = World.Get<ProjectileNetworkUpdateComponent>(entity)
        .SecondaryUpdatePending,
      NetSpam = World.Get<ProjectileNetworkUpdateComponent>(entity).NetSpam,
      SoundDelay = World.Get<ProjectileSoundDelayComponent>(entity).RemainingTicks,
      TileCollisionEnabled = World.Get<ProjectileTileCollisionComponent>(entity).Enabled,
      PrimaryUpdatePending = World.Get<ProjectileNetworkUpdateComponent>(entity)
        .PrimaryUpdatePending,
      NetworkUpdateReady = World.Get<ProjectileNetworkUpdateComponent>(entity).SendRequested,
      Friendly = World.Get<ProjectileFriendlyStateComponent>(entity).IsFriendly,
      HitCount = damage.HitCount,
      UpdateCount = updates.Count,
      Reflected = World.Has<ProjectileReflectionComponent>(entity) &&
        World.Get<ProjectileReflectionComponent>(entity).HasReflected,
      LegacyAiStyle = World.Get<ProjectileDefinitionComponent>(entity).LegacyAiStyle,
      Banner = World.Get<ProjectileBannerResponseComponent>(entity).BannerId,
      Direction = World.Get<ProjectileDirectionComponent>(entity).Horizontal,
      ManualDirectionChange = World.Get<ProjectileDefinitionComponent>(entity).ManualDirectionChange,
      UsesOwnerMeleeHitCooldown =
        World.Get<ProjectileDefinitionComponent>(entity).UsesOwnerMeleeHitCooldown,
      CopiesOwnerAttackCooldownToLocalImmunityOnSpawn =
        World.Get<ProjectileDefinitionComponent>(entity)
          .CopiesOwnerAttackCooldownToLocalImmunityOnSpawn,
      Revision = current.Revision + 1
    };
    _projectileReplications[replicationId] = updated;
  }

  private void UpdateNpcProjectileReplication(
    Entity entity,
    LocationComponent transform,
    VelocityComponent velocity,
    ProjectileLifetimeComponent lifetime)
  {
    if (!_projectileIdsByEntity.TryGetValue(entity, out int replicationId) ||
        !_npcProjectileReplications.TryGetValue(
          replicationId,
          out NpcProjectileReplicationSnapshot current) ||
        !current.IsActive || current.Revision == long.MaxValue)
    {
      return;
    }

    ProjectileBehaviorReplicationState behaviorState = ProjectileBehaviorStateProjection.Project(
      World.Get<ProjectileBehaviorComponent>(entity));
    ProjectileDamageComponent damage = World.Get<ProjectileDamageComponent>(entity);
    ProjectileUpdateCountComponent updates = World.Get<ProjectileUpdateCountComponent>(entity);
    NpcProjectileReplicationSnapshot updated = current with
    {
      Position = new SimulationVector(transform.X, transform.Y),
      Velocity = new SimulationVector(velocity.X, velocity.Y),
      Damage = damage.Amount,
      RemainingLifetime = lifetime.RemainingTicks,
      Section = GetSectionCoordinates(new SimulationVector(transform.X, transform.Y)),
      Ai0 = behaviorState.Ai0,
      Ai1 = behaviorState.Ai1,
      Ai2 = behaviorState.Ai2,
      LocalAi0 = World.Get<ProjectileBehaviorComponent>(entity).State.LocalAi0,
      LocalAi1 = World.Get<ProjectileBehaviorComponent>(entity).State.LocalAi1,
      LocalAi2 = World.Get<ProjectileBehaviorComponent>(entity).State.LocalAi2,
      SecondaryUpdatePending = World.Get<ProjectileNetworkUpdateComponent>(entity)
        .SecondaryUpdatePending,
      NetSpam = World.Get<ProjectileNetworkUpdateComponent>(entity).NetSpam,
      SoundDelay = World.Get<ProjectileSoundDelayComponent>(entity).RemainingTicks,
      TileCollisionEnabled = World.Get<ProjectileTileCollisionComponent>(entity).Enabled,
      PrimaryUpdatePending = World.Get<ProjectileNetworkUpdateComponent>(entity)
        .PrimaryUpdatePending,
      NetworkUpdateReady = World.Get<ProjectileNetworkUpdateComponent>(entity).SendRequested,
      Friendly = World.Get<ProjectileFriendlyStateComponent>(entity).IsFriendly,
      HitCount = damage.HitCount,
      UpdateCount = updates.Count,
      Reflected = World.Has<ProjectileReflectionComponent>(entity) &&
        World.Get<ProjectileReflectionComponent>(entity).HasReflected,
      BannerIdToRespondTo = World.Get<ProjectileBannerResponseComponent>(entity).BannerId,
      Direction = World.Get<ProjectileDirectionComponent>(entity).Horizontal,
      ManualDirectionChange = World.Get<ProjectileDefinitionComponent>(entity).ManualDirectionChange,
      UsesOwnerMeleeHitCooldown =
        World.Get<ProjectileDefinitionComponent>(entity).UsesOwnerMeleeHitCooldown,
      CopiesOwnerAttackCooldownToLocalImmunityOnSpawn =
        World.Get<ProjectileDefinitionComponent>(entity)
          .CopiesOwnerAttackCooldownToLocalImmunityOnSpawn,
      Revision = current.Revision + 1
    };
    _npcProjectileReplications[replicationId] = updated;
  }

  private void SpawnNpcLoot(NpcDeathResult death)
  {
    NpcDeathEvent deathEvent = new(
      death.Npc,
      death.LootTableId,
      death.Position,
      GetSectionCoordinates(death.Position),
      TickNumber);
    if (!_npcLootEmissionLedger.TryEmit(
          _npcLootSystem,
          deathEvent,
          out NpcLootCommand lootCommand))
    {
      return;
    }

    _ = CommitWorldItemSpawn(lootCommand.WorldItem);
    ItemStack stack = lootCommand.WorldItem.Stack;
    _itemDroppedEvents.Add(new ItemDroppedEvent(
      lootCommand.SourceNpc.Value,
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
    if (snapshot.ReplicationId <= 0 || snapshot.ReplicationId == int.MaxValue ||
        snapshot.Revision < 0 || snapshot.Revision == long.MaxValue)
    {
      throw new ArgumentOutOfRangeException(
        nameof(state),
        "Persistence snapshot NPC identity must use a non-negative revision and leave room " +
        "for the next ID.");
    }

    if (!float.IsFinite(snapshot.Position.X) || !float.IsFinite(snapshot.Position.Y) ||
        !float.IsFinite(snapshot.Velocity.X) || !float.IsFinite(snapshot.Velocity.Y) ||
        snapshot.Health < 0 || state.MaximumHealth <= 0 || snapshot.Health > state.MaximumHealth ||
        state.Facing is < -1 or > 1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(state),
        "Persistence snapshot NPC state contains invalid position, velocity or health.");
    }

    if (state.HasHomePublication && !state.HasHome)
    {
      throw new ArgumentException(
        "NPC home publication state requires an explicit Home component.",
        nameof(state));
    }

    bool hasRegisteredDefinition = _npcDefinitions.TryGet(
      state.DefinitionId,
      out NpcDefinition definition);
    if (hasRegisteredDefinition &&
        (state.NetId != definition.NetId || state.MaximumHealth != definition.MaximumHealth ||
         state.Faction != definition.Faction || state.Category != definition.Category ||
         state.Behavior.BehaviorId != definition.BehaviorId))
    {
      throw new ArgumentException(
        "Persistence snapshot NPC definition fields do not match the registered definition.",
        nameof(state));
    }

    NpcHandle npc = new(snapshot.ReplicationId);
    NpcComponents.NpcAuthorityComponent authority = ResolveNpcAuthority(state);
    float colliderWidth = hasRegisteredDefinition ? definition.ColliderWidth : 1.0f;
    float colliderHeight = hasRegisteredDefinition ? definition.ColliderHeight : 2.0f;
    int defense = hasRegisteredDefinition ? definition.Defense : 0;
    NpcComponents.NpcCombatStateComponent combat = state.Combat;
    if (combat.MaximumHealth <= 0)
    {
      combat = new NpcComponents.NpcCombatStateComponent(
        hasRegisteredDefinition ? definition.Damage : 0,
        defense,
        state.MaximumHealth,
        hasRegisteredDefinition ? definition.TakenDamageMultiplier : 1.0f,
        hasRegisteredDefinition ? definition.KnockBackResist : 1.0f,
        hasRegisteredDefinition && definition.IsColdDamage,
        hasRegisteredDefinition && definition.IsTrapImmune,
        hasRegisteredDefinition && definition.IsLavaImmune,
        doesNotTakeDamageFromHostiles: false,
        immortal: hasRegisteredDefinition && definition.IsImmortal,
        friendly: hasRegisteredDefinition && definition.Faction == NpcFaction.Town);
    }
    if (!state.Lifecycle.IsActive &&
        state.Lifecycle.DespawnReason == NpcComponents.NpcDespawnReason.Killed)
    {
      _publishedNpcDeaths.Add(npc);
    }
    Entity entity = World.Create(
      new NpcTagComponent(),
      new LocationComponent(snapshot.Position.X, snapshot.Position.Y),
      new VelocityComponent(snapshot.Velocity.X, snapshot.Velocity.Y),
      new DirectionComponent(state.Facing),
      new ColliderComponent(colliderWidth, colliderHeight),
      new PhysicsStateComponent { IsGrounded = snapshot.Position.Y <= 0.0f },
      new HealthComponent(snapshot.Health, state.MaximumHealth),
      new HealthRegenerationComponent(),
      new DefenseComponent(defense),
      combat,
      state.Movement,
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
      new NpcComponents.NpcGivenNameComponent(state.GivenName),
      new BuffCollectionComponent(NpcLegacyFieldPolicy.MaximumBuffs),
      new NpcComponents.NpcReplicationComponent(snapshot.ReplicationId, snapshot.Revision));
    if (state.HasHome)
    {
      NpcComponents.NpcHomeComponent home = state.Home;
      World.Add(entity, in home);
    }

    if (state.HasHomePublication)
    {
      NpcComponents.NpcHomePublicationComponent homePublication = state.HomePublication;
      World.Add(entity, in homePublication);
    }

    if (state.HasSegment)
    {
      NpcComponents.NpcSegmentComponent segment = state.Segment;
      World.Add(entity, in segment);
    }
    _npcs.Add(npc, entity);
    _npcReplications.Add(npc, state.ToReplicationSnapshot());
    _nextNpcHandle = Math.Max(_nextNpcHandle, snapshot.ReplicationId + 1);
  }

  private NpcComponents.NpcAuthorityComponent ResolveNpcAuthority(NpcStateSnapshot state)
  {
    if (_npcDefinitions.TryGet(state.DefinitionId, out NpcDefinition definition))
    {
      return new NpcComponents.NpcAuthorityComponent(
        definition.AiStyle,
        definition.IsImmortal,
        definition.AlwaysReplicate,
        definition.TakenDamageMultiplier,
        definition.NpcSlotCost,
        definition.IsTrapImmune,
        definition.IsLavaImmune);
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
    ChestRestorePolicy restorePolicy = new();
    if (!restorePolicy.TryValidate(
          snapshot,
          new ChestCapacityDefinition(snapshot.Slots.Count),
          out string? restoreError))
    {
      throw new ArgumentException(restoreError, nameof(snapshot));
    }

    if (_chests.Count >= _entityLimits.MaximumChests)
    {
      throw new InvalidOperationException("The configured chest capacity was exceeded by persistence.");
    }

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
      snapshot.Name,
      snapshot.Slots.Count);
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
      ItemStack stack = snapshot.Slots[slot];
      if (!IsValidChestItemStack(stack))
      {
        throw new ArgumentOutOfRangeException(
          nameof(snapshot),
          "Persistence snapshot contains an unknown or over-limit chest item stack.");
      }

      chest.SetSlot(slot, stack);
    }

    chest.SetLocked(snapshot.IsLocked);

    _nextChestId = Math.Max(_nextChestId, snapshot.ChestId + 1);
  }

  private void RestoreSign(SignPersistentState snapshot)
  {
    SignRestorePolicy restorePolicy = new();
    if (!restorePolicy.TryValidate(snapshot.Text, out string? restoreError))
    {
      throw new ArgumentException(restoreError, nameof(snapshot));
    }

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

  private void RestoreSignTombstone(SignTombstoneSnapshot snapshot)
  {
    if (snapshot.SignId < 0 || snapshot.SignId == int.MaxValue || snapshot.Revision < 0 ||
        _signs.ContainsKey(snapshot.SignId) ||
        !_signTombstones.TryAdd(snapshot.SignId, snapshot))
    {
      throw new ArgumentException(
        "Persistence snapshot contains an invalid or duplicate sign tombstone.",
        nameof(snapshot));
    }

    _nextSignId = Math.Max(_nextSignId, snapshot.SignId + 1);
  }

  private void MoveProjectiles()
  {
    World.Query(
      in _projectileQuery,
      (Entity projectileEntity, ref LocationComponent transform,
        ref VelocityComponent velocity, ref ProjectileLifetimeComponent lifetime,
        ref ProjectileRestrikeDelayComponent restrikeDelay,
        ref ProjectileNetworkUpdateComponent networkUpdate,
        ref ProjectileSoundDelayComponent soundDelay,
        ref ProjectileTileCollisionComponent tileCollision) =>
      {
        restrikeDelay = _projectileRestrikeDelaySystem.Tick(restrikeDelay);
        networkUpdate = _projectileNetworkUpdatePolicy.Tick(networkUpdate).State;
        soundDelay = _projectileSoundDelayPolicy.Tick(soundDelay);
        if (_pendingProjectileWorldObjectPlacements.TryGetValue(
              projectileEntity,
              out _))
        {
          return;
        }

        ref ProjectileDefinitionComponent definition =
          ref World.Get<ProjectileDefinitionComponent>(projectileEntity);
        RefreshProjectileOwnerMinionTarget(projectileEntity);
        bool expired = false;
        int maxUpdates = ProjectileUpdateBudgetPolicy.GetMaxUpdates(definition.ExtraUpdates);
        for (int update = 0; update < maxUpdates; update++)
        {
          if (!_projectileBehaviorSystem.TryAdvance(
              projectileEntity, World, checked((int)TickNumber), out _))
          {
            _commands.Enqueue(new DespawnEntityCommand(
              projectileEntity, ProjectileTombstoneReason.BehaviorRejected));
            return;
          }

          ProjectileTileCollisionComponent previousTileCollision = tileCollision;
          tileCollision = _projectileTileCollisionPolicy.ApplyLegacyAiOverride(
            definition.ProjectileType,
            World.Get<ProjectileBehaviorComponent>(projectileEntity).State.Secondary,
            tileCollision);
          if (tileCollision != previousTileCollision)
          {
            networkUpdate = _projectileNetworkUpdatePolicy.RequestPrimaryUpdate(networkUpdate);
          }
          transform = World.Get<LocationComponent>(projectileEntity);
          velocity = World.Get<VelocityComponent>(projectileEntity);
          ProjectileOwnerComponent owner = World.Get<ProjectileOwnerComponent>(projectileEntity);
          if (!_players.TryGetValue(owner.Owner, out Entity ownerEntity) ||
              !_projectileOwnerAnchoredMeleeSystem.TryAdvance(
                World.Get<ProjectileBehaviorComponent>(projectileEntity),
                ref transform,
                velocity,
                World.Get<LocationComponent>(ownerEntity)))
          {
            _commands.Enqueue(new DespawnEntityCommand(
              projectileEntity,
              ProjectileTombstoneReason.Expired));
            return;
          }

          ref ProjectileDirectionComponent direction =
            ref World.Get<ProjectileDirectionComponent>(projectileEntity);
          ProjectileDirectionSystem.Update(
            ref direction,
            velocity,
            definition,
            World.Get<ProjectileBehaviorComponent>(projectileEntity));
          World.Get<ProjectileUpdateCountComponent>(projectileEntity).Advance();
          ref ProjectileDamageComponent damage =
            ref World.Get<ProjectileDamageComponent>(projectileEntity);
          ref ProjectileBehaviorComponent behavior =
            ref World.Get<ProjectileBehaviorComponent>(projectileEntity);
          _projectileBehaviorEffectSystem.ApplyType658TileCenterSnap(
            ref transform,
            ref direction,
            ref behavior,
            World.Get<ColliderComponent>(projectileEntity),
            definition);
          _projectileBehaviorEffectSystem.Apply(ref damage, ref definition, ref lifetime, behavior);
          _projectileBehaviorEffectSystem.ApplyType656Tick(
            ref damage,
            ref lifetime,
            ref soundDelay,
            ref behavior,
            ref networkUpdate,
            definition);
          _projectileBehaviorEffectSystem.ApplyType657Tick(
            ref lifetime,
            ref soundDelay,
            ref behavior,
            definition);
          _projectileBehaviorEffectSystem.ApplyType658Tick(
            ref lifetime,
            ref soundDelay,
            ref behavior,
            ref velocity,
            definition);
          EmitType658ChildSpawnCommand(projectileEntity);
          expired = _projectileLifetimeSystem.Advance(projectileEntity, World);
          if (expired)
          {
            break;
          }
        }
        UpdateProjectileReplication(projectileEntity, transform, velocity, lifetime);
        if (expired)
        {
          _commands.Enqueue(new DespawnEntityCommand(
            projectileEntity,
            ProjectileTombstoneReason.Expired));
        }
      });
  }

  private void RefreshProjectileOwnerMinionTarget(Entity projectile)
  {
    int targetId = 0;
    if (_projectileIdsByEntity.TryGetValue(projectile, out int replicationId) &&
        TryGetProjectileOwnerMinionAttackTarget(replicationId, out NpcHandle target))
    {
      targetId = target.Value;
    }

    ref ProjectileBehaviorComponent behavior = ref World.Get<ProjectileBehaviorComponent>(projectile);
    if (behavior.State.TargetId != targetId)
    {
      behavior.State = behavior.State with { TargetId = targetId };
    }
  }

  private void RefreshProjectileOwnerMinionTargets()
  {
    foreach (KeyValuePair<int, ProjectileReplicationSnapshot> entry in _projectileReplications)
    {
      if (!entry.Value.IsActive ||
          !_projectileIdsByEntity.TryGetEntity(entry.Key, out Entity projectile) ||
          !World.IsAlive(projectile) ||
          !World.Has<ProjectileOwnerComponent>(projectile) ||
          !World.Has<ProjectileMinionComponent>(projectile))
      {
        continue;
      }

      RefreshProjectileOwnerMinionTarget(projectile);
    }
  }

  private void MoveNpcProjectiles()
  {
    World.Query(
      in _npcProjectileQuery,
      (Entity projectileEntity, ref LocationComponent transform,
        ref VelocityComponent velocity, ref ProjectileLifetimeComponent lifetime,
        ref ProjectileRestrikeDelayComponent restrikeDelay,
        ref ProjectileNetworkUpdateComponent networkUpdate,
        ref ProjectileSoundDelayComponent soundDelay,
        ref ProjectileTileCollisionComponent tileCollision) =>
      {
        restrikeDelay = _projectileRestrikeDelaySystem.Tick(restrikeDelay);
        networkUpdate = _projectileNetworkUpdatePolicy.Tick(networkUpdate).State;
        soundDelay = _projectileSoundDelayPolicy.Tick(soundDelay);
        ref ProjectileDefinitionComponent definition =
          ref World.Get<ProjectileDefinitionComponent>(projectileEntity);
        bool expired = false;
        int maxUpdates = ProjectileUpdateBudgetPolicy.GetMaxUpdates(definition.ExtraUpdates);
        for (int update = 0; update < maxUpdates; update++)
        {
          if (!_projectileBehaviorSystem.TryAdvance(
                projectileEntity, World, checked((int)TickNumber), out _))
          {
            _commands.Enqueue(new DespawnEntityCommand(
              projectileEntity, ProjectileTombstoneReason.BehaviorRejected));
            return;
          }

          ProjectileTileCollisionComponent previousTileCollision = tileCollision;
          tileCollision = _projectileTileCollisionPolicy.ApplyLegacyAiOverride(
            definition.ProjectileType,
            World.Get<ProjectileBehaviorComponent>(projectileEntity).State.Secondary,
            tileCollision);
          if (tileCollision != previousTileCollision)
          {
            networkUpdate = _projectileNetworkUpdatePolicy.RequestPrimaryUpdate(networkUpdate);
          }
          velocity = World.Get<VelocityComponent>(projectileEntity);
          ref ProjectileDamageComponent damage =
            ref World.Get<ProjectileDamageComponent>(projectileEntity);
          ref ProjectileDirectionComponent direction =
            ref World.Get<ProjectileDirectionComponent>(projectileEntity);
          ProjectileDirectionSystem.Update(
            ref direction,
            velocity,
            definition,
            World.Get<ProjectileBehaviorComponent>(projectileEntity));
          World.Get<ProjectileUpdateCountComponent>(projectileEntity).Advance();
          ref ProjectileBehaviorComponent behavior =
            ref World.Get<ProjectileBehaviorComponent>(projectileEntity);
          _projectileBehaviorEffectSystem.ApplyType658TileCenterSnap(
            ref transform,
            ref direction,
            ref behavior,
            World.Get<ColliderComponent>(projectileEntity),
            definition);
          _projectileBehaviorEffectSystem.Apply(ref damage, ref definition, ref lifetime, behavior);
          _projectileBehaviorEffectSystem.ApplyType656Tick(
            ref damage,
            ref lifetime,
            ref soundDelay,
            ref behavior,
            ref networkUpdate,
            definition);
          _projectileBehaviorEffectSystem.ApplyType657Tick(
            ref lifetime,
            ref soundDelay,
            ref behavior,
            definition);
          _projectileBehaviorEffectSystem.ApplyType658Tick(
            ref lifetime,
            ref soundDelay,
            ref behavior,
            ref velocity,
            definition);
          expired = _npcProjectileLifetimeSystem.Advance(projectileEntity, World);
          if (expired)
          {
            break;
          }
        }
        UpdateNpcProjectileReplication(projectileEntity, transform, velocity, lifetime);
        if (expired)
        {
          _commands.Enqueue(new DespawnEntityCommand(
            projectileEntity,
            ProjectileTombstoneReason.Expired));
        }
      });
  }

  private void DetectNpcProjectileHits()
  {
    HashSet<Entity> despawned = new();
    List<DamageRequestedEvent> candidates = new();
    HashSet<Entity> playerTargets = new();
    World.Query(
      in _npcProjectileQuery,
      (Entity projectileEntity, ref LocationComponent transform,
        ref ColliderComponent collider, ref VelocityComponent velocity,
        ref ProjectileDamageComponent damage,
        ref ProjectileTileCollisionComponent tileCollision,
        ref NpcProjectileNetworkIdentityComponent identity,
        ref ProjectileDefinitionComponent definition,
        ref ProjectileFriendlyStateComponent friendlyState) =>
      {
        LocationComponent previous = new(transform.X - velocity.X, transform.Y - velocity.Y);
        if (_projectileTileCollisionPolicy.ShouldCollide(tileCollision) &&
            _projectileCollisionSystem.TryGetSolidTileImpact(
              WorldGrid,
              previous,
              transform,
              collider,
              out bool reflectHorizontal,
              out bool reflectVertical,
              definition.CorrectSlopeCollision,
              World.Has<ProjectileFallThroughComponent>(projectileEntity) &&
                World.Get<ProjectileFallThroughComponent>(projectileEntity).ShouldFallThrough))
        {
          ref ProjectileBounceComponent bounce = ref World.Get<ProjectileBounceComponent>(projectileEntity);
          float speed = MathF.Abs(velocity.X) + MathF.Abs(velocity.Y);
          if ((bounce.RemainingBounces > 0 || definition.ReflectsFromTiles ||
               definition.ProjectileType == 357 &&
                 World.Get<ProjectilePenetrationComponent>(projectileEntity)
                   .RemainingPenetration > 0) &&
              speed >= definition.MinimumBounceSpeed)
          {
            _commands.Enqueue(new BounceProjectileCommand(
              projectileEntity,
              reflectHorizontal,
              reflectVertical,
              previous.X,
              previous.Y));
          }
          else if (despawned.Add(projectileEntity))
          {
            _commands.Enqueue(new DespawnEntityCommand(
              projectileEntity,
              ProjectileTombstoneReason.TileHit));
          }

          return;
        }

        if (!definition.IgnoreWater && definition.LiquidPolicy == ProjectileLiquidPolicy.Destroy &&
            _projectileCollisionSystem.HitsLiquid(WorldGrid, transform, collider))
        {
          if (despawned.Add(projectileEntity))
          {
            _commands.Enqueue(new DespawnEntityCommand(
              projectileEntity,
              ProjectileTombstoneReason.LiquidHit));
          }

          return;
        }

        foreach (KeyValuePair<PlayerHandle, Entity> entry in _players)
        {
          if (!World.Get<PlayerLifecycleComponent>(entry.Value).IsActive ||
              World.Get<HealthComponent>(entry.Value).Current <= 0)
          {
            continue;
          }

          LocationComponent playerTransform = World.Get<LocationComponent>(entry.Value);
          ColliderComponent playerCollider = World.Get<ColliderComponent>(entry.Value);
          if (!PathOverlaps(previous, transform, collider, playerTransform, playerCollider))
          {
            continue;
          }

          candidates.Add(new DamageRequestedEvent(
            projectileEntity,
            entry.Value,
            damage.Amount,
            identity.Identity,
            entry.Key.Value,
            definition.DamageClass,
            definition.IsColdDamage,
            definition.ArmorPenetration,
            definition.BonusCritChance,
            definition.BonusTagDamage,
            definition.TagEffectType));
          playerTargets.Add(entry.Value);
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
      if (World.Has<ProjectileBehaviorComponent>(candidate.Projectile) &&
          World.Has<ProjectileDefinitionComponent>(candidate.Projectile) &&
          World.Has<ProjectileFriendlyStateComponent>(candidate.Projectile) &&
          World.Has<ProjectileNetworkUpdateComponent>(candidate.Projectile))
      {
        ref ProjectileBehaviorComponent behavior =
          ref World.Get<ProjectileBehaviorComponent>(candidate.Projectile);
        ref ProjectileFriendlyStateComponent friendlyState =
          ref World.Get<ProjectileFriendlyStateComponent>(candidate.Projectile);
        ref ProjectileNetworkUpdateComponent networkUpdate =
          ref World.Get<ProjectileNetworkUpdateComponent>(candidate.Projectile);
        _projectileBehaviorEffectSystem.ApplyAcceptedHit(
          ref behavior,
          ref friendlyState,
          ref networkUpdate,
          World.Get<ProjectileDefinitionComponent>(candidate.Projectile),
          World.Get<ProjectilePenetrationComponent>(candidate.Projectile).RemainingPenetration + 1);
        if (World.Has<ProjectileTileCollisionComponent>(candidate.Projectile))
        {
          _projectileBehaviorEffectSystem.ApplyAcceptedHitTileCollision(
            ref behavior,
            ref World.Get<ProjectileTileCollisionComponent>(candidate.Projectile),
            ref networkUpdate,
            World.Get<ProjectileDefinitionComponent>(candidate.Projectile));
        }
      }

      if (playerTargets.Contains(candidate.Target))
      {
        ProjectileDefinitionComponent definition =
          World.Get<ProjectileDefinitionComponent>(candidate.Projectile);
        _commands.Enqueue(new DamagePlayerCommand(
          FindPlayerHandle(candidate.Target),
          _projectileHostileDamageScalingSystem.ScalePlayerDamage(
            candidate.Amount,
            definition,
            _worldRules),
          candidate.DamageClass,
          candidate.IsColdDamage));
      }

      if (World.Has<ProjectileDamageComponent>(candidate.Projectile) &&
          World.Has<ProjectileDefinitionComponent>(candidate.Projectile))
      {
        ref ProjectileDamageComponent projectileDamage =
          ref World.Get<ProjectileDamageComponent>(candidate.Projectile);
        _projectileBehaviorEffectSystem.ApplyAcceptedHitDamage(
          ref projectileDamage,
          World.Get<ProjectileDefinitionComponent>(candidate.Projectile));
        _projectileBehaviorEffectSystem.ApplyAcceptedHitKnockback(
          ref World.Get<ProjectileDefinitionComponent>(candidate.Projectile));
      }

      if (World.Has<ProjectileDefinitionComponent>(candidate.Projectile) &&
          World.Has<VelocityComponent>(candidate.Projectile))
      {
        _projectileBehaviorEffectSystem.ApplyAcceptedHitVelocity(
          ref World.Get<VelocityComponent>(candidate.Projectile),
          World.Get<ProjectileDefinitionComponent>(candidate.Projectile));
      }

      if (World.Has<ProjectileDamageComponent>(candidate.Projectile) &&
          World.Has<ProjectilePenetrationComponent>(candidate.Projectile) &&
          World.Has<ProjectileBehaviorComponent>(candidate.Projectile) &&
          World.Has<ProjectileFriendlyStateComponent>(candidate.Projectile) &&
          World.Has<ProjectileNetworkUpdateComponent>(candidate.Projectile))
      {
        _projectileBehaviorEffectSystem.ApplyAcceptedHitPenetration(
          ref World.Get<ProjectileBehaviorComponent>(candidate.Projectile),
          ref World.Get<ProjectileFriendlyStateComponent>(candidate.Projectile),
          ref World.Get<ProjectileNetworkUpdateComponent>(candidate.Projectile),
          ref World.Get<ProjectilePenetrationComponent>(candidate.Projectile),
          ref World.Get<ProjectileDamageComponent>(candidate.Projectile),
          World.Get<ProjectileDefinitionComponent>(candidate.Projectile));
      }

      ref ProjectilePenetrationComponent penetration =
        ref World.Get<ProjectilePenetrationComponent>(candidate.Projectile);
      if (penetration.RemainingPenetration == 0 && despawned.Add(candidate.Projectile))
      {
        _commands.Enqueue(new DespawnEntityCommand(
          candidate.Projectile,
          ProjectileTombstoneReason.Penetrated));
      }
    }
  }

  private static bool Overlaps(
    LocationComponent firstTransform,
    ColliderComponent firstCollider,
    LocationComponent secondTransform,
    ColliderComponent secondCollider)
  {
    return firstTransform.X < secondTransform.X + secondCollider.Width &&
      firstTransform.X + firstCollider.Width > secondTransform.X &&
      firstTransform.Y < secondTransform.Y + secondCollider.Height &&
      firstTransform.Y + firstCollider.Height > secondTransform.Y;
  }

  private static bool PathOverlaps(
    LocationComponent previousTransform,
    LocationComponent currentTransform,
    ColliderComponent projectileCollider,
    LocationComponent targetTransform,
    ColliderComponent targetCollider)
  {
    float deltaX = currentTransform.X - previousTransform.X;
    float deltaY = currentTransform.Y - previousTransform.Y;
    int steps = Math.Max(1, (int)MathF.Ceiling(MathF.Max(MathF.Abs(deltaX), MathF.Abs(deltaY))));
    for (int index = 0; index <= steps; index++)
    {
      float progress = (float)index / steps;
      LocationComponent sample = new(
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

      ControlInputComponent input = World.Get<ControlInputComponent>(playerEntity);
      ref PlayerControlStateComponent control = ref World.Get<PlayerControlStateComponent>(playerEntity);
      if (control.FireCooldownTicks > 0)
      {
        control.FireCooldownTicks--;
      }

      if (!input.Fire || control.FireCooldownTicks > 0)
      {
        continue;
      }

      LocationComponent transform = World.Get<LocationComponent>(playerEntity);
      VelocityComponent playerVelocity = World.Get<VelocityComponent>(playerEntity);
      DirectionComponent facing = World.Get<DirectionComponent>(playerEntity);
      PlayerHandle owner = FindPlayerHandle(playerEntity);
      _commands.Enqueue(new SpawnProjectileCommand(
        owner,
        transform.X,
        transform.Y + 0.75f,
        facing.Horizontal,
        ProjectileDamage,
        ProjectileLifetimeTicks,
        InitialVelocityY: playerVelocity.Y));
      control.ApplyAttackCooldown(10);
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
