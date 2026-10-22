using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Combat.Npcs;
using MinimalImplementation.Combat.Projectiles;
using MinimalImplementation.Combat.TagEffects;
using MinimalImplementation.Contracts;
using MinimalImplementation.Modules.Creative;
using MinimalImplementation.Persistence;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.Crafting;
using MinimalImplementation.Server.Inventory.Handlers;
using MinimalImplementation.Server.Inventory.Protocol;
using MinimalImplementation.Server.Inventory.QuickStack;
using MinimalImplementation.Server.Inventory.Rules;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Mechanisms;
using MinimalImplementation.Server.Modules;
using MinimalImplementation.Server.Modules.Banners;
using MinimalImplementation.Server.Modules.Bestiary;
using MinimalImplementation.Server.Modules.Effects;
using MinimalImplementation.Server.Modules.Events;
using MinimalImplementation.Server.Modules.Protocol;
using MinimalImplementation.Server.Modules.Social.Chat;
using MinimalImplementation.Server.Modules.Social.Ping;
using MinimalImplementation.Server.Persistence;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NonAuthoritative.Platform;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;
using LeashedEntityMessage = MinimalImplementation.Server.Protocol.Version4ModulePayloads.LeashedEntityMessage;
using InvasionStateOwner = MinimalImplementation.Server.Modules.Events.InvasionState;
using LanternNightOwner = MinimalImplementation.Server.Modules.Events.LanternNightState;
using TileCoordinate = Terraria.WorldStorage.TileCoordinate;

namespace MinimalImplementation.Server.Host;

/// <summary>Central startup gate for the single server process.</summary>
public sealed class ServerComposition : IAsyncDisposable
{
  private static readonly object ProtocolGate = new();
  private static Version4ProtocolFactsInput? s_protocolFactsInput;

  private ServerComposition(ServerOptions options)
  {
    options.Validate();
    Options = options;
    CommitLoop = new ServerLoop(options.CommitQueueCapacity);
    IdentityRegistry = new EntityIdentityRegistry();
    StableWorldScope = IdentityRegistry.CurrentWorldScope;
    Version4WorldFacts = WorldFacts.CreateVersion4();
    WorldOwner = new WorldOwner(Version4WorldFacts, new ServerLiquidRandomSource());
    TreeHarvestRandom = new ServerTreeHarvestRandomSource();
    Version4ProtocolFactsInput = EnsureProtocolFacts(Version4WorldFacts);
    Version4WireProfile = new Version4WireProfile(Version4ProtocolFactsInput);
    WorldTemporaryAnimationPacketProducer = new WorldTemporaryAnimationPacketProducer(
      Version4WireProfile);
    Packet82IntentProducer = new Packet82ModuleIntentProducer(
      ProtocolInputs.Instance.ModuleCodecs,
      ProtocolInputs.Instance.TagEffectNpcSlotCount,
      ProtocolInputs.Instance.TagEffectUsesProcTimes);

    SessionBindings = new SessionBindingStore();
    Players = new PlayerOwner(IdentityRegistry);
    PlayerControlsWorldCapture = new PlayerControlsWorldCaptureSource(
      CommitLoop, WorldOwner, Players);
    SessionAdmissionPolicy = new SessionAdmissionPolicy(
      "Terraria319",
      SessionBindingStore.MaximumPlayerSlots,
      ignoreProtocolVersion: true);
    SessionAdmission = new SessionAdmissionCoordinator(
      SessionBindings,
      Players,
      SessionAdmissionPolicy);
    SessionHostAuthorization = new SessionHostAuthorization(
      SessionAdmissionPolicy,
      Players,
      SessionAdmission);
    JoinCoordinator = new JoinCoordinator(SessionAdmission, Players);
    SessionPacketHandlers = new SessionPacketHandlers(
      SessionAdmission,
      SessionHostAuthorization,
      JoinCoordinator);
    PlayerStatePacketHandlers = new PlayerStatePacketHandlers(
      Players, SessionAdmission, CapturePlayerProjectileTrackers);
    PlayerTeamChangeCoordinator = new PlayerTeamChangeCoordinator(Players, SessionAdmission);
    SessionCloseCoordinator = new SessionCloseCoordinator(
      SessionAdmission,
      SessionBindings,
      Players);

    InventoryCommitGate = new ServerStateCommitGate();
    ItemValidationPolicy = new ItemValidationPolicy();
    InventoryStore = new InventoryStore(
      InventorySlotLayout.Count,
      ItemValidationPolicy,
      InventoryCommitGate);
    ChestStore = new ChestStore(
      ContainerContentFacts.ChestIndexCount,
      ItemValidationPolicy,
      InventoryCommitGate);
    WorldItemStore = new WorldItemStore(400, ItemValidationPolicy, InventoryCommitGate);
    InstancedItemStore = new InstancedItemStore(ItemValidationPolicy, InventoryCommitGate);
    ChestLeaseStore = new ChestLeaseStore(InventoryCommitGate);
    WorldItemReservationStore = new WorldItemReservationStore(WorldItemStore);
    WorldItemTransactions = new WorldItemTransactionCoordinator(
      InventoryStore,
      WorldItemStore,
      WorldItemReservationStore);
    WorldItemDrops = new WorldItemDropCoordinator(InventoryStore, WorldItemStore,
      WorldItemReservationStore, ItemValidationPolicy, Players, SessionAdmission, WorldOwner);
    ClientWorldItems = new VanillaWorldItemOwnership(this);
    WorldItemLifecycle = new WorldItemLifecycleCoordinator(
      InventoryStore,
      WorldItemStore,
      WorldItemReservationStore,
      InstancedItemStore,
      ChestLeaseStore);
    InventoryLoadouts = new InventoryLoadoutCoordinator(InventoryStore, WorldItemStore);
    InventoryPaymentCoordinator = new InventoryPaymentCoordinator(
      InventoryStore,
      ItemValidationPolicy);
    ChestAccessPolicy = new ServerChestAccessPolicy();
    ChestAccessService = new ChestAccessService(
      ChestStore,
      ChestLeaseStore,
      ItemValidationPolicy,
      ChestAccessPolicy,
      TimeSpan.FromSeconds(30));
    InventoryTransactions = new InventoryTransactionCoordinator(InventoryStore, ChestStore);
    InventoryPacketCommands = new P04InventoryPacketCommandSurface(
      InventoryStore,
      WorldItemStore,
      WorldItemReservationStore,
      ChestStore,
      ChestAccessService,
      InstancedItemStore,
      InventoryPaymentCoordinator,
      ItemValidationPolicy,
      size => size is > 0 and <= ContainerContentFacts.AbsoluteMaximumChestSlotCountForVersion4,
      new ActiveSessionPlayerSlotResolver(SessionAdmission));
    QuickStackCategoryResolver = new ConfiguredQuickStackCategoryResolver(
      new Dictionary<int, int>());
    QuickStackTransferPlanner = new QuickStackTransferPlanner(
      ItemValidationPolicy,
      QuickStackCategoryResolver);
    QuickStackDestinationProvider = new QuickStackDestinationProvider(
      ChestStore,
      ChestLeaseStore,
      ChestAccessPolicy,
      new PlayerOwnerQuickStackPositionProvider(Players),
      maximumDistanceTiles: 40);
    QuickStackTransactions = new QuickStackTransactionCoordinator(
      InventoryStore,
      InventoryTransactions,
      QuickStackTransferPlanner,
      QuickStackDestinationProvider);
    CraftingMaterialCatalog = new ConfiguredCraftingMaterialCatalog(
      new Dictionary<int, IReadOnlyCollection<int>>());
    CraftingMaterialPlanner = new CraftingMaterialPlanner(CraftingMaterialCatalog);
    CraftingChestAccessPolicy = new LeaseAwareCraftingChestAccessPolicy(
      ChestLeaseStore,
      ChestAccessPolicy,
      InventoryCommitGate);
    CraftingTransactions = new CraftingTransactionCoordinator(
      ChestStore,
      CraftingMaterialPlanner,
      CraftingChestAccessPolicy);
    P04InventoryHandlers = RegisterP04InventoryHandlers(
      SessionAdmission,
      QuickStackTransactions,
      CraftingTransactions,
      Packet82IntentProducer);

    CombatIdentityAuthority = new CombatIdentityAuthority(IdentityRegistry);
    CombatWorldScope = IdentityRegistry.CurrentWorldScope;
    NpcManager = new NpcManager(
      CombatIdentityAuthority,
      NpcDefinitionCatalog.CreateVersion4Core());
    ProjectileTrackerManager = new ProjectileTrackerManager(CombatIdentityAuthority);
    NpcNetwork = new NpcNetworkProjection();
    NpcCombat = new NpcCombatPacketHandler(NpcManager);
    ProjectileDefinitions = ProjectileDefinitionCatalog.CreateVersion4Core();
    ProjectileManager = new ProjectileManager(
      CombatIdentityAuthority,
      ProjectileDefinitions);
    ProjectileManager.AttachTrackerManager(ProjectileTrackerManager);
    ProjectileTickPhaseOwner = new ProjectileServerTickPhaseOwner(ProjectileManager);
    TagEffectStateOwner = new TagEffectStateOwner(
      CombatIdentityAuthority,
      NpcManager,
      TagEffectDefinitionCatalog.CreateVersion4());

    MechanismIdentityAuthority = new MechanismIdentityAuthority(IdentityRegistry);
    MechanismFacts = MechanismFactsCatalog.CreateVersion4();
    MechanismWorldScope = IdentityRegistry.CurrentWorldScope;
    TileEntityRegistry = new TileEntityRegistry(
      MechanismFacts,
      MechanismIdentityAuthority,
      MechanismWorldScope);
    TileEntityProjection = new TileEntityProjection(MechanismWorldScope);
    TeleportPylonRegistry = new TeleportPylonRegistry(MechanismFacts, MechanismWorldScope);
    PylonInteractionService = new PylonInteractionService(MechanismFacts);
    TeleportWorldSources = new PlayerTeleportWorldCaptureSource(
      CommitLoop, WorldOwner, IdentityRegistry);
    TeleportPylonPort = new PlayerMechanismTeleportPort(
      CommitLoop,
      Players,
      SessionAdmission,
      WorldOwner,
      TeleportPylonRegistry,
      TeleportWorldSources,
      MechanismWorldScope);
    TeleportPylonModuleHandler = new TeleportPylonModuleHandler(TeleportPylonPort);
    TeleportPylonProducer = new TeleportPylonModuleProducer(
      Packet82IntentProducer,
      MechanismIdentityAuthority);
    LeashedEntityRegistry = new LeashedEntityRegistry(
      MechanismFacts,
      MechanismIdentityAuthority,
      MechanismIdentityAuthority.Create,
      MechanismWorldScope);
    SignRegistry = new SignRegistry(MechanismWorldScope);
    SignInteractionService = new SignInteractionService(SignRegistry);
    SignPacketHandler = new SignPacketHandler(SignInteractionService);
    WorldAttachmentHydrator = new WorldAttachmentHydrator(
      ChestStore,
      SignRegistry,
      TileEntityRegistry,
      MechanismIdentityAuthority,
      new WorldScopeId(MechanismWorldScope));
    LeashedEntityCommitPort = new LeashedEntityRegistryCommitPort(LeashedEntityRegistry);
    LeashedEntityModuleHandler = new LeashedEntityModuleHandler(
      LeashedEntityCommitPort,
      MechanismWorldScope);
    LeashedEntityProducer = new LeashedEntityModuleProducer(
      Packet82IntentProducer,
      MechanismIdentityAuthority);
    MechanismCommitVersionAllocator = new MechanismCommitVersionAllocator();
    TileEntityPacketHandler = new TileEntityPacketHandler(
      MechanismFacts,
      TileEntityRegistry,
      TileEntityProjection,
      TeleportPylonRegistry,
      MechanismIdentityAuthority,
      MechanismIdentityAuthority.Create,
      new MechanismWorldPort(WorldOwner));

    CreativePowerStateOwner = new CreativePowerStateOwner();
    CreativeUnlockReportOwner = new CreativeUnlockReportOwner();
    CreativeModuleHandler = new CreativeModuleHandler(
      CreativePowerStateOwner,
      CreativeUnlockReportOwner,
      new ServerCreativeSacrificeFacts());
    ChatCommandProcessor = new ChatCommandProcessor(
      [new PartyChatCommand(Packet82IntentProducer)]);
    TextModuleHandler = new TextModuleHandler(ChatCommandProcessor);
    PingModuleHandler = new PingModuleHandler(Packet82IntentProducer);
    AmbienceModuleHandler = new AmbienceModuleHandler(Packet82IntentProducer);
    ParticlesModuleHandler = new ParticlesModuleHandler(Packet82IntentProducer);
    BestiaryState = new BestiaryState(Packet82IntentProducer);
    BannerState = new BannerState(Packet82IntentProducer);
    BestiaryModuleHandler = new BestiaryModuleHandler(BestiaryState);
    BannerModuleHandler = new BannerModuleHandler(BannerState);
    InvasionState = new InvasionStateOwner(StableWorldScope);
    WorldTickInvasionBridge = new WorldTickInvasionBridge(InvasionState, StableWorldScope);
    WorldEventCoordinator = new WorldEventCoordinator();
    LanternNightState = new LanternNightOwner();

    LeashedEntityMotion = new WorldLeashedEntityMotionPort(WorldOwner);
    WorldFactory = new WorldDocumentWorldFactory(
      Version4WorldFacts,
      new WorldFileTilePayloadDecoder());
    WorldDocuments = new WorldDocumentLoader(
      new WorldFileStoreAdapter(),
      new WorldFileDocumentDecoder(),
      new WorldFileDocumentValidator());
    ModuleDispatcher = CreateModuleDispatcher();
  }

  public NpcNetworkProjection NpcNetwork { get; }

  public NpcCombatPacketHandler NpcCombat { get; }

  public ServerOptions Options { get; }

  public ServerLoop CommitLoop { get; }

  public EntityIdentityRegistry IdentityRegistry { get; }

  public long StableWorldScope { get; }

  public WorldFacts Version4WorldFacts { get; }

  public Version4ProtocolFactsInput Version4ProtocolFactsInput { get; }

  public Version4WireProfile Version4WireProfile { get; }

  public WorldTemporaryAnimationPacketProducer WorldTemporaryAnimationPacketProducer { get; }

  public Packet82ModuleIntentProducer Packet82IntentProducer { get; }

  public SessionBindingStore SessionBindings { get; }

  public PlayerOwner Players { get; }

  public SessionAdmissionPolicy SessionAdmissionPolicy { get; }

  public SessionAdmissionCoordinator SessionAdmission { get; }

  public SessionHostAuthorization SessionHostAuthorization { get; }

  public JoinCoordinator JoinCoordinator { get; }

  public SessionPacketHandlers SessionPacketHandlers { get; }

  public PlayerStatePacketHandlers PlayerStatePacketHandlers { get; }

  public PlayerTeamChangeCoordinator PlayerTeamChangeCoordinator { get; }

  public SessionCloseCoordinator SessionCloseCoordinator { get; }

  public ServerStateCommitGate InventoryCommitGate { get; }

  public ItemValidationPolicy ItemValidationPolicy { get; }

  public InventoryStore InventoryStore { get; }

  public ChestStore ChestStore { get; }

  public WorldItemStore WorldItemStore { get; }

  public InstancedItemStore InstancedItemStore { get; }

  public ChestLeaseStore ChestLeaseStore { get; }

  public WorldItemReservationStore WorldItemReservationStore { get; }

  public WorldItemTransactionCoordinator WorldItemTransactions { get; }
  public WorldItemDropCoordinator WorldItemDrops { get; }
  public VanillaWorldItemOwnership ClientWorldItems { get; }

  public WorldItemLifecycleCoordinator WorldItemLifecycle { get; }

  public InventoryLoadoutCoordinator InventoryLoadouts { get; }

  public P04InventoryPacketCommandSurface InventoryPacketCommands { get; }

  public InventoryPaymentCoordinator InventoryPaymentCoordinator { get; }

  public ServerChestAccessPolicy ChestAccessPolicy { get; }

  public ChestAccessService ChestAccessService { get; }

  public InventoryTransactionCoordinator InventoryTransactions { get; }

  public ConfiguredQuickStackCategoryResolver QuickStackCategoryResolver { get; }

  public QuickStackTransferPlanner QuickStackTransferPlanner { get; }

  public QuickStackDestinationProvider QuickStackDestinationProvider { get; }

  public QuickStackTransactionCoordinator QuickStackTransactions { get; }

  public ConfiguredCraftingMaterialCatalog CraftingMaterialCatalog { get; }

  public CraftingMaterialPlanner CraftingMaterialPlanner { get; }

  public LeaseAwareCraftingChestAccessPolicy CraftingChestAccessPolicy { get; }

  public CraftingTransactionCoordinator CraftingTransactions { get; }

  public CombatIdentityAuthority CombatIdentityAuthority { get; }

  internal long CombatWorldScope { get; }

  public NpcManager NpcManager { get; }

  public ProjectileTrackerManager ProjectileTrackerManager { get; }

  public ProjectileDefinitionCatalog ProjectileDefinitions { get; }

  public ProjectileManager ProjectileManager { get; }

  public ProjectileServerTickPhaseOwner ProjectileTickPhaseOwner { get; }

  public TagEffectStateOwner TagEffectStateOwner { get; }

  public MechanismIdentityAuthority MechanismIdentityAuthority { get; }

  public MechanismFactsCatalog MechanismFacts { get; }

  public long MechanismWorldScope { get; }

  public TileEntityRegistry TileEntityRegistry { get; }

  public TileEntityProjection TileEntityProjection { get; }

  public TeleportPylonRegistry TeleportPylonRegistry { get; }

  public PylonInteractionService PylonInteractionService { get; }

  public PlayerTeleportWorldCaptureSource TeleportWorldSources { get; }

  public PlayerControlsWorldCaptureSource PlayerControlsWorldCapture { get; }

  public PlayerMechanismTeleportPort TeleportPylonPort { get; }

  public TeleportPylonModuleHandler TeleportPylonModuleHandler { get; }

  public TeleportPylonModuleProducer TeleportPylonProducer { get; }

  public LeashedEntityRegistry LeashedEntityRegistry { get; }

  public SignRegistry SignRegistry { get; }

  public SignInteractionService SignInteractionService { get; }

  public SignPacketHandler SignPacketHandler { get; }

  public WorldAttachmentHydrator WorldAttachmentHydrator { get; }

  public LeashedEntityRegistryCommitPort LeashedEntityCommitPort { get; }

  public LeashedEntityModuleHandler LeashedEntityModuleHandler { get; }

  public LeashedEntityModuleProducer LeashedEntityProducer { get; }

  public MechanismCommitVersionAllocator MechanismCommitVersionAllocator { get; }

  public TileEntityPacketHandler TileEntityPacketHandler { get; }

  public CreativePowerStateOwner CreativePowerStateOwner { get; private set; }

  public CreativeUnlockReportOwner CreativeUnlockReportOwner { get; }

  public CreativeModuleHandler CreativeModuleHandler { get; private set; }

  public ChatCommandProcessor ChatCommandProcessor { get; }

  public TextModuleHandler TextModuleHandler { get; }

  public PingModuleHandler PingModuleHandler { get; }

  public AmbienceModuleHandler AmbienceModuleHandler { get; }

  public ParticlesModuleHandler ParticlesModuleHandler { get; }

  public BestiaryState BestiaryState { get; private set; }

  public BannerState BannerState { get; private set; }

  public BestiaryModuleHandler BestiaryModuleHandler { get; private set; }

  public BannerModuleHandler BannerModuleHandler { get; private set; }

  public InvasionState InvasionState { get; private set; }

  public WorldTickInvasionBridge WorldTickInvasionBridge { get; }

  public WorldEventCoordinator WorldEventCoordinator { get; }

  public LanternNightOwner LanternNightState { get; private set; }

  public WorldOwner WorldOwner { get; }

  internal ITreeHarvestRandomSource TreeHarvestRandom { get; }

  public WorldLeashedEntityMotionPort LeashedEntityMotion { get; }

  public WorldSpatialQueries CaptureSpatialQueries(WorldSectionBounds bounds) {
    return new WorldSpatialQueries(WorldOwner.CaptureSpatialSnapshot(bounds), NpcManager.Capture());
  }

  /// <summary>Resolves a live NPC slot in the current serialized owner window.</summary>
  internal EntityHandle? ResolveActiveNpcHandle(short slot) {
    if (!NpcManager.TryGet(slot, out NpcView? npc) || npc is null) {
      return null;
    }

    return CombatIdentityAuthority.TryResolveActiveHandle(npc.Entity, out EntityHandle handle)
      ? handle : null;
  }

  internal SyncProjectileTrackersPacket CapturePlayerProjectileTrackers(SessionBinding binding) {
    if (!IdentityRegistry.IsActive(binding.PlayerHandle) ||
        !ProjectileTrackerManager.TryCapture(binding.PlayerSlot, binding.Epoch, ProjectileManager,
          out ProjectileTrackerSnapshotView? snapshot) || snapshot is null) {
      throw new InvalidOperationException("The player's projectile tracker snapshot is unavailable.");
    }

    var packet = new SyncProjectileTrackersPacket {
      Player = snapshot.Player,
      PiggyBank = CreateTrackedProjectileReference(snapshot.PiggyBank),
      VoidLens = CreateTrackedProjectileReference(snapshot.VoidLens)
    };
    Version4WireProfile.Encode(WireDirection.ServerToClient, packet);
    return packet;
  }

  private static PacketTrackedProjectileRef CreateTrackedProjectileReference(
      ProjectileTrackerReference? reference) {
    return reference is null
      ? new PacketTrackedProjectileRef(-1, null, null)
      : new PacketTrackedProjectileRef(reference.Owner, reference.Identity, reference.Type);
  }

  public bool ContainsPylonTile(WorldSectionBounds interactionRectangle) {
    return CaptureSpatialQueries(interactionRectangle).ContainsPylonTile(interactionRectangle);
  }

  public WorldDocumentWorldFactory WorldFactory { get; }

  public WorldDocumentLoader WorldDocuments { get; }

  public ServerConnectionHost? ConnectionHost { get; private set; }

  public P04InventoryPacketHandlers? P04InventoryHandlers { get; private set; }

  public ModuleDispatcher ModuleDispatcher { get; private set; }

  public void RestoreWorldModules(WorldPersistenceDocument document, CommitVersion version) {
    if (ConnectionHost is not null) {
      throw new InvalidOperationException("World modules must be restored before the listener.");
    }
    WorldFileCreativePowersSection creativePowers = ReadWorldSection<
      WorldFileCreativePowersSection>(document, WorldFileCreativePowersSection.SectionId);
    CreativeWorldPowerSnapshot persistentCreativePowers =
      new CreativeWorldPowerDocumentCodec().Decode(creativePowers.SerializedPayload);
    CreativePowerSnapshot currentCreativePowers = CreativePowerStateOwner.CaptureSnapshot();
    CreativePowerSnapshot restoredCreativePowers = CreateRestoredCreativePowerSnapshot(
      currentCreativePowers,
      persistentCreativePowers,
      version);
    CreativePowerStateOwner = CreativePowerStateOwner.Restore(restoredCreativePowers);
    CreativeModuleHandler = new CreativeModuleHandler(
      CreativePowerStateOwner,
      CreativeUnlockReportOwner,
      new ServerCreativeSacrificeFacts());
    BestiaryState bestiary = BestiaryState.Restore(Packet82IntentProducer,
        ReadWorldSection<WorldFileBestiarySection>(document, WorldFileBestiarySection.SectionId),
        version);
    BannerState banners = BannerState.Restore(Packet82IntentProducer,
        ReadWorldSection<WorldFileBannerSection>(document, WorldFileBannerSection.SectionId),
        version);
    InvasionStateOwner invasion = InvasionStateOwner.Restore(
      StableWorldScope,
      version,
      ReadWorldSection<WorldFileProgressionSection>(
        document,
        WorldFileProgressionSection.SectionId),
      ReadWorldSection<WorldFileQuestSection>(document, WorldFileQuestSection.SectionId));
    LanternNightOwner lanternNight = LanternNightOwner.Restore(
      ReadWorldSection<WorldFileEventSection>(document, WorldFileEventSection.SectionId),
      version);
    ModulePeriodicSyncOwner.ValidateNpcCatalog(bestiary.CaptureSnapshot());
    BestiaryState = bestiary;
    BannerState = banners;
    InvasionState = invasion;
    WorldTickInvasionBridge.ReplaceState(invasion);
    LanternNightState = lanternNight;
    BestiaryModuleHandler = new BestiaryModuleHandler(bestiary);
    BannerModuleHandler = new BannerModuleHandler(banners);
    ModuleDispatcher = CreateModuleDispatcher();
  }

  public CreativeWorldPowerSnapshot CaptureCreativeWorldPowers()
  {
    CreativeWorldPowerState state = CreativePowerStateOwner.CaptureWorldState();
    return new CreativeWorldPowerSnapshot(
      state.FreezeTime,
      state.ModifyTimeRate,
      state.FreezeRain,
      state.FreezeWind,
      state.DifficultySlider,
      state.StopBiomeSpread);
  }

  private static CreativePowerSnapshot CreateRestoredCreativePowerSnapshot(
    CreativePowerSnapshot current,
    CreativeWorldPowerSnapshot persistent,
    CommitVersion version)
  {
    ArgumentNullException.ThrowIfNull(current);
    ArgumentNullException.ThrowIfNull(persistent);
    persistent.Validate();

    var sharedToggles = current.SharedToggles.ToDictionary(
      pair => pair.Key,
      pair => pair.Value);
    var sharedSliders = current.SharedSliders.ToDictionary(
      pair => pair.Key,
      pair => pair.Value);
    sharedToggles[0] = persistent.FreezeTime;
    sharedSliders[8] = persistent.ModifyTimeRate;
    sharedToggles[9] = persistent.FreezeRain;
    sharedToggles[10] = persistent.FreezeWind;
    sharedSliders[12] = persistent.DifficultySlider;
    sharedToggles[13] = persistent.StopBiomeSpread;

    return new CreativePowerSnapshot(
      version,
      sharedToggles,
      sharedSliders,
      current.PlayerToggles,
      current.PlayerSliders,
      current.Permissions);
  }

  private static TSection ReadWorldSection<TSection>(
      WorldPersistenceDocument document, string sectionId) where TSection : notnull {
    if (!document.TryGetSection(sectionId, out WorldLoadSection<TSection> section) ||
        !section.IsPresent) {
      throw new System.IO.InvalidDataException($"World module section '{sectionId}' is missing.");
    }
    return section.Value;
  }

  /// <summary>
  /// Attaches the one listener composition after facts and owner callbacks are ready.
  /// This method does not create a default packet handler or bypass admission.
  /// </summary>
  public ServerConnectionHost AttachConnectionHost(
    Version4WireProfile wireProfile,
    Func<SessionEpoch> allocateEpoch,
    Func<InboundRawFrame, InboundPacketEnvelope> createEnvelope,
    Func<InboundVersion4Packet, CancellationToken, ValueTask> onPacket,
    Func<ConnectionClosed, ValueTask> onClosed,
    Func<Exception, ValueTask> onConnectionFailure,
    TcpListenerAdapterOptions? listenerOptions = null,
    PacketTcpConnectionOptions? connectionOptions = null,
    TimeProvider? timeProvider = null,
    Func<SessionEpoch, ValueTask>? onConnected = null)
  {
    if (ConnectionHost is not null)
    {
      throw new InvalidOperationException("The server composition can attach only one connection host.");
    }

    ArgumentNullException.ThrowIfNull(wireProfile);
    ArgumentNullException.ThrowIfNull(allocateEpoch);
    ArgumentNullException.ThrowIfNull(createEnvelope);
    ArgumentNullException.ThrowIfNull(onPacket);
    ArgumentNullException.ThrowIfNull(onClosed);
    ArgumentNullException.ThrowIfNull(onConnectionFailure);
    if (!IPAddress.TryParse(Options.ListenAddress, out IPAddress? address))
    {
      throw new InvalidOperationException("The validated listen address could not be parsed.");
    }

    ConnectionHost = new ServerConnectionHost(
      new IPEndPoint(address, Options.ListenPort),
      wireProfile,
      allocateEpoch,
      createEnvelope,
      onPacket,
      onClosed,
      onConnectionFailure,
      listenerOptions,
      connectionOptions,
      timeProvider,
      onConnected: onConnected);
    return ConnectionHost;
  }

  /// <summary>Registers the P04 packet 85 and crafting module 11 owners once at the host boundary.</summary>
  public P04InventoryPacketHandlers RegisterP04InventoryHandlers(
    IAuthenticatedPlayerPacketGate sessions,
    QuickStackTransactionCoordinator quickStackTransactions,
    CraftingTransactionCoordinator craftingTransactions,
    Packet82ModuleIntentProducer moduleIntentProducer)
  {
    if (P04InventoryHandlers is not null)
    {
      throw new InvalidOperationException("P04 inventory packet handlers have already been registered.");
    }

    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(quickStackTransactions);
    ArgumentNullException.ThrowIfNull(craftingTransactions);
    ArgumentNullException.ThrowIfNull(moduleIntentProducer);

    P04InventoryHandlers = new P04InventoryPacketHandlers(
      new QuickStackPacket85Handler(sessions, quickStackTransactions),
      new CraftingModule11Handler(craftingTransactions, moduleIntentProducer));
    return P04InventoryHandlers;
  }

  private ModuleDispatcher CreateModuleDispatcher()
  {
    P04InventoryPacketHandlers inventoryHandlers = P04InventoryHandlers ??
      throw new InvalidOperationException(
        "P04 inventory handlers must be registered before building the module dispatcher.");
    IReadOnlyList<IModuleHandler> activeInventoryHandlers = inventoryHandlers.ModuleHandlers;
    List<IModuleHandler> handlers = [
      .. CreateP03ModuleHandlers(ResolveWorldEpoch),
      TextModuleHandler,
      PingModuleHandler,
      AmbienceModuleHandler,
      BestiaryModuleHandler,
      new CreativePowerModuleHandler(
        CreativeModuleHandler,
        Packet82IntentProducer),
      new CreativeUnlockReportModuleHandler(CreativeModuleHandler, Packet82IntentProducer),
      .. activeInventoryHandlers,
      new TeleportPylonModuleHandlerAdapter(
        TeleportPylonModuleHandler,
        () => SessionAdmission.CaptureActiveSessionEpochs()),
      new TagEffectModuleHandlerAdapter(TagEffectStateOwner, Packet82IntentProducer),
      new LeashedEntityModuleHandlerAdapter(
        LeashedEntityModuleHandler,
        LeashedEntityProducer,
        MechanismWorldScope,
        ResolveLeashedEntitySection,
        MechanismCommitVersionAllocator.Allocate,
        () => SessionAdmission.CaptureActiveSessionEpochs()),
      ParticlesModuleHandler,
      new CreativePermissionModuleHandler(CreativeModuleHandler),
      BannerModuleHandler,
    ];
    return new ModuleDispatcher(handlers);
  }

  private MechanismSectionCoordinate ResolveLeashedEntitySection(LeashedEntityMessage message)
  {
    ArgumentNullException.ThrowIfNull(message);
    if (message.AnchorX is short anchorX && message.AnchorY is short anchorY &&
        anchorX >= 0 && anchorY >= 0)
    {
      return new MechanismSectionCoordinate(
        anchorX / WorldGeometry.SectionWidth,
        anchorY / WorldGeometry.SectionHeight);
    }

    if (LeashedEntityRegistry.TryGet(message.Slot, out LeashedEntityRuntimeSnapshot? existing) &&
        existing is not null)
    {
      return existing.Section;
    }

    // Remove and partial messages for an unknown slot are rejected/no-op by the registry and do
    // not observe the section. A neutral valid coordinate preserves that owner decision.
    return new MechanismSectionCoordinate(0, 0);
  }

  private static ulong? ResolveWorldEpoch(InboundPacketEnvelope envelope)
  {
    if (envelope.WorldScope is not long worldScope || worldScope <= 0)
    {
      return null;
    }

    return (ulong)worldScope;
  }

  public IReadOnlyList<IModuleHandler> CreateP03ModuleHandlers(
    Func<InboundPacketEnvelope, ulong?> resolveWorldEpoch)
  {
    ArgumentNullException.ThrowIfNull(resolveWorldEpoch);
    IModuleHandler[] handlers =
    [
      new WorldLiquidModuleHandler(WorldOwner, resolveWorldEpoch),
      new UnbreakableWallModuleHandler()
    ];
    return Array.AsReadOnly(handlers);
  }

  public Task<WorldDocumentLoadResult> LoadConfiguredWorldDocumentAsync(
    CancellationToken cancellationToken = default)
  {
    return WorldDocuments.ReadAndValidateAsync(
      Options.WorldFilePath,
      Options.MaximumWorldFileBytes,
      cancellationToken);
  }

  /// <summary>
  /// Builds and validates the P03 candidate for the composition's current world epoch.
  /// Publication remains coordinated with the other world owners by the host.
  /// </summary>
  public WorldPreparedWorld PrepareWorldCandidate(WorldPersistenceDocument document)
  {
    ArgumentNullException.ThrowIfNull(document);
    WorldPreparationRequest request = WorldFactory.CreateCandidate(document);
    WorldPreparedWorld candidate = WorldOwner.PrepareWorld(request);
    WorldValidationResult validation = WorldOwner.ValidatePreparedWorld(candidate);
    if (!validation.IsValid)
    {
      throw new InvalidOperationException(
        $"The P03 world candidate failed validation: {string.Join("; ", validation.Errors)}");
    }

    return candidate;
  }

  public void RestoreWorldAttachments(
    WorldPersistenceDocument document,
    WorldSnapshot world)
  {
    ArgumentNullException.ThrowIfNull(document);
    ArgumentNullException.ThrowIfNull(world);
    WorldAttachmentHydrator.Restore(document, world);
    TeleportWorldSources.Bind(document);
  }

  public static ServerComposition Create(ServerOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);
    return new ServerComposition(options);
  }

  private static Version4ProtocolFactsInput EnsureProtocolFacts(WorldFacts facts)
  {
    ArgumentNullException.ThrowIfNull(facts);
    lock (ProtocolGate)
    {
      if (s_protocolFactsInput is Version4ProtocolFactsInput existing)
      {
        if (!ProtocolInputs.IsInitialized)
        {
          throw new InvalidOperationException(
            "The shared Version4 protocol facts input exists without its published ProtocolInputs.");
        }

        ProtocolFactsBootstrap.Validate(ProtocolInputs.Instance);
        return existing;
      }

      if (ProtocolInputs.IsInitialized)
      {
        throw new InvalidOperationException(
          "ProtocolInputs was initialized outside the server composition and cannot be adopted.");
      }

      Version4ProtocolFactsInput input = Version4ProductionFacts.Create(facts);
      ProtocolFactsBootstrap.Initialize(() => Version4ProtocolFactsFactory.CreateProduction(input));
      s_protocolFactsInput = input;
      return input;
    }
  }

  public static ServerReadiness CheckReadiness()
  {
    var blockers = new List<string>();
    AddProtocolBlockers(blockers);
    blockers.Add("The server composition has not been created.");
    return new ServerReadiness(blockers);
  }

  public static ServerReadiness CheckReadiness(ServerComposition composition)
  {
    ArgumentNullException.ThrowIfNull(composition);
    var blockers = new List<string>();
    AddProtocolBlockers(blockers);
    if (composition.ModuleDispatcher is null || !composition.ModuleDispatcher.IsComplete)
    {
      blockers.Add("The complete 15-module dispatcher is not registered.");
    }

    if (composition.ConnectionHost is null)
    {
      blockers.Add("The production TCP connection host is not attached.");
    }

    if (!composition.WorldOwner.IsReady)
    {
      blockers.Add("No world owner is ready to publish a validated candidate world.");
    }

    return new ServerReadiness(blockers);
  }

  private static void AddProtocolBlockers(ICollection<string> blockers)
  {
    if (!ProtocolInputs.IsInitialized)
    {
      blockers.Add("Complete Version4 protocol facts have not been initialized.");
      return;
    }

    try
    {
      ProtocolFactsBootstrap.Validate(ProtocolInputs.Instance);
    }
    catch (InvalidOperationException exception)
    {
      blockers.Add($"Version4 protocol facts are incomplete: {exception.Message}");
    }
  }

  public async ValueTask DisposeAsync()
  {
    if (ConnectionHost is not null)
    {
      await ConnectionHost.DisposeAsync().ConfigureAwait(false);
    }

    await CommitLoop.DisposeAsync().ConfigureAwait(false);
  }
}
