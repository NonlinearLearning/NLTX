using Terraria.Items.InventoryContainers;

namespace Terraria.Items.InventoryContainersVerification;

internal static class Program
{
  private static int Main()
  {
    TestItemDerivedValuesUseCatalogData();
    TestChestRejectsCapacityOverflow();
    TestQuickStackPlanningDoesNotMutateSources();
    TestTransferPolicyRunsCompletionEffectAfterCommit();
    TestWorldItemLifecycleAdvancesFromExplicitTicks();
    TestEquipmentAppearanceSnapshotIsStable();
    TestEmergencyStackPlanRejectsUnownedTransfer();
    TestWorldItemProjectionsExposeCanonicalItemData();
    TestCapabilityComponentsPreserveDeclaredPayloads();
    TestQuickStackCommitIsAtomicOnInvalidAmount();
    TestEmergencyStackPolicyOrdersGroupsDeterministically();
    TestLifecycleQueryUsesExplicitState();
    TestLifecycleQueryHonorsReservations();
    TestContainerProjectionIsVersionedAndDetached();
    TestTransferPolicyPresetsCoverLegacySettings();
    TestEquipmentAppearancePreservesAllSlots();
    TestWorldItemLifecycleTracksReservationAndIgnoreState();
    TestQuickStackScratchIsResetAndPlanIsDetached();
    TestChestIconCatalogUsesDetachedArraysAndBounds();
    TestEmergencyStackQueriesAndCommitAreIdempotent();
    TestLifecycleProjectionsAreDetached();
    TestQuickStackRetainsUntransferredSourceAmount();
    TestChestSlotsAreDetachedFromInternalStorage();
    TestItemRevisionDoesNotAdvanceForNoOpWrites();
    TestEmergencySchedulerReservesBothTransferItems();
    TestContainerPersistenceAndClientProjections();
    TestChestRegistryAndAccessAdaptersRejectDuplicates();
    TestQuickStackUsesExplicitReferenceSnapshots();
    Console.WriteLine("P09 verifier passed.");
    return 0;
  }

  private static void TestItemDerivedValuesUseCatalogData()
  {
    ItemDefinitionCatalog catalog = new();
    catalog.Register(new ItemDefinition(
      type: 1,
      name: "Copper Pickaxe",
      rarity: 2,
      damage: 5,
      defense: 0,
      value: 100,
      maxStack: 1));

    ItemIdentityAndStackComponent item = new(
      contentType: 1,
      stack: 1,
      maxStack: 1,
      uniqueStack: true,
      prefix: 0,
      variant: 0,
      favorited: false,
      nameOverride: null);

    ItemDerivedValues values = ItemDerivedQuery.Evaluate(item, catalog);

    Assert(values.IsActive, "A positive type and stack should be active.");
    Assert(values.Name == "Copper Pickaxe", "The catalog name should be projected.");
    Assert(values.OriginalDamage == 5, "The original damage should come from the catalog.");
  }

  private static void TestChestRejectsCapacityOverflow()
  {
    ChestSlotStorageComponent chest = new(capacity: 1);
    ItemIdentityAndStackComponent item = new(
      contentType: 1,
      stack: 1,
      maxStack: 1,
      uniqueStack: true,
      prefix: 0,
      variant: 0,
      favorited: false,
      nameOverride: null);

    Assert(chest.TrySetSlot(0, item), "The first slot write should succeed.");
    Assert(!chest.TrySetSlot(1, item), "A slot beyond capacity must be rejected.");
  }

  private static void TestQuickStackPlanningDoesNotMutateSources()
  {
    ItemStackSnapshot sourceItem = new(
      contentType: 1,
      stack: 3,
      maxStack: 10,
      uniqueStack: false,
      prefix: 0,
      variant: 0,
      favorited: false,
      nameOverride: null);
    QuickStackSourceSnapshot source = QuickStackSourceSnapshot.Create(
      containerKey: "player",
      position: new InventoryPosition(0, 0),
      new QuickStackSourceSlotSnapshot(
        new InventorySlotReference("player", 0),
        sourceItem));
    QuickStackDestinationSnapshot destination = QuickStackDestinationSnapshot.Create(
      containerKey: "chest",
      locked: false,
      transferBlocked: false,
      sourceItem);

    QuickStackPlan plan = QuickStackPlanQuery.CreatePlan(
      source,
      new[] { destination });

    Assert(plan.Intents.Count == 1, "A compatible destination should create one intent.");
    Assert(plan.Intents[0].Amount == 3, "The plan should transfer the available source stack.");
    ItemStackSnapshot? plannedItem = source.Slots[0].Item;
    Assert(plannedItem is not null, "Planning should retain the source item snapshot.");
    Assert(plannedItem?.Stack == 3, "Planning must not mutate the source snapshot.");
  }

  private static void TestTransferPolicyRunsCompletionEffectAfterCommit()
  {
    List<string> effects = new();
    TransferCompletionEffectPort effectPort = new(
      effect => effects.Add(effect.OperationKey));
    ItemTransferPolicyDefinition policy = new(
      longText: false,
      noText: true,
      canGoIntoVoidVault: true,
      noSound: true,
      noCoinMerge: false);
    TransferCommitResult commit = ItemTransferCommand.Commit(
      operationKey: "transfer-1",
      policy,
      effectPort,
      new TransferCompletionEffect("transfer-1", "player", "chest", null));

    Assert(commit.Committed, "A valid transfer command should commit.");
    Assert(effects.Count == 1 && effects[0] == "transfer-1", "The completion effect should run once.");
  }

  private static void TestWorldItemLifecycleAdvancesFromExplicitTicks()
  {
    WorldItemLifecycleComponent lifecycle = new();
    lifecycle.Initialize(ownTime: 2, keepTime: 5);
    lifecycle.Advance(new WorldItemLifecycleTick(2, 0.5f));

    Assert(lifecycle.OwnTime == 0, "Ownership time should advance from the explicit tick.");
    Assert(lifecycle.KeepTime == 3, "Keep time should advance from the explicit tick.");
    Assert(lifecycle.ShimmerTime == 0.5f, "Shimmer time should use the supplied delta.");
  }

  private static void TestEquipmentAppearanceSnapshotIsStable()
  {
    ItemEquipmentAppearanceComponent appearance = new(
      wornArmor: true,
      headSlot: 4,
      bodySlot: 5,
      legSlot: 6,
      social: false,
      vanity: true,
      newAndShiny: true,
      hasVanityEffects: true);
    ItemEquipmentAppearanceSnapshot snapshot = ItemEquipmentAppearanceQuery.CreateSnapshot(appearance);
    appearance.SetNewAndShiny(false);

    Assert(snapshot.HeadSlot == 4, "The snapshot should preserve the head slot.");
    Assert(snapshot.NewAndShiny, "The snapshot should be isolated from later mutations.");
  }

  private static void TestEmergencyStackPlanRejectsUnownedTransfer()
  {
    EmergencyStackingTransferPlan plan = EmergencyStackingTransferPlan.Create(
      source: new EmergencyStackingCandidateSnapshot(
        itemKey: "world-a",
        item: new ItemStackSnapshot(1, 4, 10, false, 0, 0, false, null),
        age: 2,
        isOnScreen: true,
        ownerPlayerIndex: 1),
      destination: new EmergencyStackingCandidateSnapshot(
        itemKey: "world-b",
        item: new ItemStackSnapshot(1, 2, 10, false, 0, 0, false, null),
        age: 1,
        isOnScreen: true,
        ownerPlayerIndex: 2),
      distanceOrder: 0,
      preservationOrder: 0,
      distance: 10);

    EmergencyStackingTransferDecision decision = EmergencyStackingTransferQuery.Evaluate(
      plan,
      currentPlayerIndex: 1);

    Assert(!decision.HasOwnership, "A destination owned by another player must be rejected.");
    Assert(decision.Amount == 0, "An unowned transfer must have no amount.");
  }

  private static void TestWorldItemProjectionsExposeCanonicalItemData()
  {
    ItemDefinitionCatalog catalog = new();
    catalog.Register(new ItemDefinition(
      type: 1,
      name: "World Stone",
      rarity: 3,
      damage: 0,
      defense: 0,
      value: 42,
      maxStack: 99));
    ItemIdentityAndStackComponent item = new(
      contentType: 1,
      stack: 7,
      maxStack: 99,
      uniqueStack: false,
      prefix: 0,
      variant: 0,
      favorited: true,
      nameOverride: null);

    WorldItemEconomyPayload economy = WorldItemEconomyProjection.Create(item, catalog);
    WorldItemUsePresentationPayload presentation = WorldItemUsePresentationProjection.Create(
      new WorldItemUsePresentationSource(
        newAndShiny: true,
        color: ItemPresentationColor.White,
        makeNpc: 0,
        useTime: 10,
        useAnimation: 10,
        useAmmo: 0,
        damage: 0,
        knockBack: 0,
        shootSpeed: 0,
        scale: 1,
        ammo: 0,
        notAmmo: false,
        shoot: 0,
        placeStyle: 0,
        createTile: -1,
        glowMask: -1,
        expert: false,
        alpha: 0,
        buffType: 0));

    Assert(economy.Stack == 7, "The economy projection should expose the canonical stack.");
    Assert(economy.Name == "World Stone", "The economy projection should expose the canonical name.");
    Assert(presentation.NewAndShiny, "The presentation projection should expose its source state.");
  }

  private static void TestCapabilityComponentsPreserveDeclaredPayloads()
  {
    ItemProgressionInteractionCapabilityComponent progression = new(
      questItem: true,
      flame: false,
      mech: true,
      tileWand: 4,
      fishingPole: 5,
      bait: 6,
      makeNpc: 7,
      expertOnly: true,
      expert: false);
    ItemBuffMountConsumableCapabilityComponent effects = new(
      buffType: 8,
      buffTime: 60,
      mountType: 9,
      cartTrack: true,
      chlorophyteExtractinatorConsumable: false,
      dd2Summon: true);
    ItemToolPlacementCapabilityComponent tools = new(
      pick: 10,
      axe: 11,
      hammer: 12,
      tileBoost: 1,
      createTile: 13,
      createWall: 14,
      placeStyle: 2,
      ammo: 15,
      notAmmo: false,
      useAmmo: 16,
      material: true);

    Assert(progression.QuestItem && progression.MakeNpc == 7, "Progression payload should be preserved.");
    Assert(effects.BuffTime == 60 && effects.DD2Summon, "Effect payload should be preserved.");
    Assert(tools.Pick == 10 && tools.UseAmmo == 16, "Tool payload should be preserved.");
  }

  private static void TestQuickStackCommitIsAtomicOnInvalidAmount()
  {
    ItemIdentityAndStackComponent source = new(1, 3, 10, false, 0, 0, false, null);
    ItemIdentityAndStackComponent destination = new(1, 2, 10, false, 0, 0, false, null);

    QuickStackCommitResult result = QuickStackCommitCommand.Execute(
      source,
      destination,
      amount: 4);

    Assert(!result.Committed, "An amount larger than the source must be rejected.");
    Assert(source.Stack == 3 && destination.Stack == 2, "A rejected commit must not mutate either item.");
  }

  private static void TestEmergencyStackPolicyOrdersGroupsDeterministically()
  {
    EmergencyStackingPolicyDefinition policy = EmergencyStackingPolicyDefinition.CreateDefault();
    IReadOnlyList<EmergencyStackingGroupDefinition> groups = policy.PreservationOrder;

    Assert(groups.Count == 6, "The default policy should contain six preservation groups.");
    Assert(groups[0].GroupKey == "RareCurrency", "Rare currency should be preserved first.");
    Assert(groups[^1].GroupKey == "Default", "Default items should be preserved last.");
  }

  private static void TestLifecycleQueryUsesExplicitState()
  {
    WorldItemLifecycleComponent lifecycle = new();
    lifecycle.Initialize(ownTime: 0, keepTime: 1);
    lifecycle.SetShimmered(true);

    Assert(
      WorldItemLifecycleQuery.IsActive(lifecycle, new ItemStackSnapshot(1, 1, 10, false, 0, 0, false, null)),
      "A live item with keep time should be active.");
    Assert(
      !WorldItemLifecycleQuery.CanBePickedUp(lifecycle, playerIndex: 2),
      "A shimmered item should not be picked up by the ordinary path.");
  }

  private static void TestLifecycleQueryHonorsReservations()
  {
    WorldItemLifecycleComponent lifecycle = new();
    lifecycle.Initialize(ownTime: 0, keepTime: 1);
    lifecycle.ReserveForPlayer(3);

    Assert(
      !WorldItemLifecycleQuery.CanBePickedUp(lifecycle, playerIndex: 2),
      "A reserved item should reject pickup by another player.");
    Assert(
      WorldItemLifecycleQuery.CanBePickedUp(lifecycle, playerIndex: 3),
      "A reserved item should accept pickup by its reserved player.");
  }

  private static void TestContainerProjectionIsVersionedAndDetached()
  {
    ChestSlotStorageComponent chest = new(capacity: 2);
    ItemStackSnapshot item = new(1, 2, 10, false, 0, 0, false, null);
    chest.TrySetSlot(0, item);

    ItemContainerNetworkSnapshot snapshot = ItemContainerNetworkProjection.Create(
      containerKey: "chest-1",
      chest);
    chest.TryClearSlot(0);

    Assert(snapshot.SchemaVersion == 1, "The network snapshot should carry a schema version.");
    Assert(snapshot.Slots[0]?.Stack == 2, "The projection must be detached from later mutations.");
  }

  private static void TestTransferPolicyPresetsCoverLegacySettings()
  {
    IReadOnlyList<string> presetNames = new[]
    {
      "GiftRecieved",
      "LootAllFromBank",
      "LootAllFromChest",
      "PickupItemFromWorld",
      "QuickTransferFromSlot",
      "ReturnItemFromSlot",
      "ReturnItemShowAsNew",
      "ItemCreatedFromItemUsage",
      "RefundConsumedItem",
      "ReturnItemShowAsNewNoCoinMerge"
    };

    foreach (string presetName in presetNames)
    {
      Assert(
        ItemTransferPolicyPresetCatalog.TryGet(presetName, out ItemTransferPolicyDefinition? _),
        $"The transfer preset '{presetName}' should be registered.");
    }

    Assert(
      ItemTransferPolicyPresetCatalog.ReturnItemShowAsNew.PostAction
        == ItemTransferPostAction.MakeNewAndShiny,
      "The show-as-new preset should preserve its explicit post action.");
    Assert(
      ItemTransferPolicyPresetCatalog.ReturnItemShowAsNewNoCoinMerge.NoCoinMerge,
      "The no-coin-merge preset should preserve its policy flag.");
  }

  private static void TestEquipmentAppearancePreservesAllSlots()
  {
    ItemEquipmentAppearanceComponent appearance = new(
      wornArmor: true,
      headSlot: 1,
      bodySlot: 2,
      legSlot: 3,
      social: true,
      vanity: true,
      newAndShiny: false,
      hasVanityEffects: true,
      handOnSlot: 4,
      handOffSlot: 5,
      backSlot: 6,
      frontSlot: 7,
      shoeSlot: 8,
      waistSlot: 9,
      wingSlot: 10,
      shieldSlot: 11,
      neckSlot: 12,
      faceSlot: 13,
      balloonSlot: 14,
      beardSlot: 15,
      voiceSlot: 16);

    ItemEquipmentAppearanceSnapshot snapshot =
      ItemEquipmentAppearanceQuery.CreateSnapshot(appearance);

    Assert(snapshot.HandOnSlot == 4 && snapshot.HandOffSlot == 5, "Hand slots should be snapshotted.");
    Assert(snapshot.WingSlot == 10 && snapshot.ShieldSlot == 11, "Wing and shield slots should be snapshotted.");
    Assert(snapshot.BeardSlot == 15 && snapshot.VoiceSlot == 16, "Voice and beard slots should be snapshotted.");
  }

  private static void TestWorldItemLifecycleTracksReservationAndIgnoreState()
  {
    WorldItemLifecycleComponent lifecycle = new();
    lifecycle.Initialize(ownTime: 0, keepTime: 4);
    lifecycle.SetOwnIgnore(2);
    lifecycle.ReserveForPlayer(3);

    WorldItemLifecycleSystem system = new();
    system.Update(lifecycle, new WorldItemLifecycleTick(2, 0.25f));

    Assert(lifecycle.OwnIgnore == 2, "The lifecycle should preserve the ignored player.");
    Assert(
      lifecycle.TimeSinceTheItemHasBeenReservedForSomeone == 2,
      "Reservation elapsed time should advance with the explicit tick.");
    Assert(
      !WorldItemLifecycleQuery.CanBePickedUp(lifecycle, playerIndex: 2),
      "The ignored player should not be allowed to pick up the item.");
    Assert(
      WorldItemLifecycleQuery.CanBePickedUp(lifecycle, playerIndex: 3),
      "The reserved player should be allowed to pick up the item.");
  }

  private static void TestQuickStackScratchIsResetAndPlanIsDetached()
  {
    ItemStackSnapshot sourceItem = new(1, 2, 10, false, 0, 0, false, null);
    QuickStackSourceSnapshot source = QuickStackSourceSnapshot.Create(
      "player",
      new InventoryPosition(0, 0),
      new QuickStackSourceSlotSnapshot(new InventorySlotReference("player", 0), sourceItem));
    QuickStackDestinationSnapshot blocked = QuickStackDestinationSnapshot.Create(
      "locked",
      locked: true,
      transferBlocked: false,
      new ItemStackSnapshot(1, 1, 10, false, 0, 0, false, null));
    QuickStackDestinationSnapshot compatible = QuickStackDestinationSnapshot.Create(
      "chest",
      locked: false,
      transferBlocked: false,
      new ItemStackSnapshot(1, 1, 10, false, 0, 0, false, null));
    QuickStackPlannerScratchState scratch = new();

    QuickStackPlan plan = QuickStackPlanQuery.CreatePlan(
      source,
      new[] { blocked, compatible },
      scratch);
    scratch.Reset();

    Assert(plan.Intents.Count == 1, "The scratch reset must not clear a completed plan.");
    Assert(plan.Intents[0].DestinationContainerKey == "chest", "The compatible destination should be selected.");
    Assert(plan.RemainingSources.Count == 0, "A fully planned source should not remain unhandled.");
  }

  private static void TestChestIconCatalogUsesDetachedArraysAndBounds()
  {
    int[] primaryIcons = new int[ChestCapacityPolicy.MaxChestTypes];
    primaryIcons[1] = 327;
    int[] secondaryIcons = new int[ChestCapacityPolicy.MaxChestTypes2];
    secondaryIcons[2] = 4714;
    int[] dresserIcons = new int[ChestCapacityPolicy.MaxDresserTypes];
    dresserIcons[3] = 5000;

    ChestIconCatalogAdapter catalog = new(primaryIcons, secondaryIcons, dresserIcons);
    primaryIcons[1] = 0;

    Assert(catalog.TryGetChestIcon(1, secondType: false, out int primaryIcon), "A primary chest icon should be found.");
    Assert(primaryIcon == 327, "The icon catalog should be detached from its input array.");
    Assert(!catalog.TryGetChestIcon(99, secondType: false, out _), "An out-of-range chest type should be rejected.");
    Assert(catalog.TryGetDresserIcon(3, out int dresserIcon) && dresserIcon == 5000, "Dresser icons should be indexed separately.");
  }

  private static void TestEmergencyStackQueriesAndCommitAreIdempotent()
  {
    ItemIdentityAndStackComponent sourceItem = new(1, 4, 10, false, 0, 0, false, null);
    ItemIdentityAndStackComponent destinationItem = new(1, 2, 10, false, 0, 0, false, null);
    EmergencyStackingTransferPlan plan = EmergencyStackingTransferPlan.Create(
      new EmergencyStackingCandidateSnapshot("world-a", sourceItem.CreateSnapshot(), 1, true, 1),
      new EmergencyStackingCandidateSnapshot("world-b", destinationItem.CreateSnapshot(), 2, true, 1),
      distanceOrder: 0,
      preservationOrder: 0,
      distance: 8);
    EmergencyStackingCommitCommand command = new(new EmergencyStackingComponentMutationPort());

    EmergencyStackingCommitResult result = command.Execute(
      plan,
      currentPlayerIndex: 1,
      operationKey: "emergency-1",
      sourceItem,
      destinationItem);
    EmergencyStackingCommitResult duplicate = command.Execute(
      plan,
      currentPlayerIndex: 1,
      operationKey: "emergency-1",
      sourceItem,
      destinationItem);

    Assert(result.Committed && result.Amount == 4, "A valid emergency transfer should commit its calculated amount.");
    Assert(duplicate.Duplicate, "A repeated emergency operation should be idempotently rejected.");
    Assert(sourceItem.Stack == 0 && destinationItem.Stack == 6, "The emergency commit should mutate both stacks once.");
  }

  private static void TestLifecycleProjectionsAreDetached()
  {
    WorldItemLifecycleComponent lifecycle = new();
    lifecycle.Initialize(ownTime: 3, keepTime: 9);
    lifecycle.ReserveForPlayer(4);
    lifecycle.SetShimmered(true);

    WorldItemLifecycleNetworkSnapshot network = WorldItemLifecycleNetworkProjection.Create(lifecycle);
    WorldItemLifecyclePersistenceSnapshot persistence = WorldItemLifecyclePersistenceProjection.Create(lifecycle);
    WorldItemLifecycleClientSnapshot client = WorldItemLifecycleClientProjection.Create(lifecycle);
    lifecycle.Advance(new WorldItemLifecycleTick(3, 1f));

    Assert(network.OwnTime == 3 && network.ReservedPlayerIndex == 4, "Network output should be a detached lifecycle snapshot.");
    Assert(persistence.KeepTime == 9, "Persistence output should preserve the committed keep time.");
    Assert(client.Shimmered, "Client output should preserve presentation lifecycle state.");
  }

  private static void TestQuickStackRetainsUntransferredSourceAmount()
  {
    ItemStackSnapshot sourceItem = new(1, 7, 10, false, 0, 0, false, null);
    QuickStackSourceSnapshot source = QuickStackSourceSnapshot.Create(
      "player",
      new InventoryPosition(0, 0),
      new QuickStackSourceSlotSnapshot(
        new InventorySlotReference("player", 0),
        sourceItem));
    QuickStackDestinationSnapshot destination = QuickStackDestinationSnapshot.Create(
      "chest",
      locked: false,
      transferBlocked: false,
      new ItemStackSnapshot(1, 8, 10, false, 0, 0, false, null));

    QuickStackPlan plan = QuickStackPlanQuery.CreatePlan(source, new[] { destination });

    Assert(plan.Intents.Count == 1, "A partially available destination should create one intent.");
    Assert(plan.Intents[0].Amount == 2, "The intent should be limited by destination capacity.");
    Assert(
      plan.RemainingSources.Count == 1
        && plan.RemainingSources[0] == new InventorySlotReference("player", 0),
      "The source slot should remain pending when its full stack was not planned.");
  }

  private static void TestChestSlotsAreDetachedFromInternalStorage()
  {
    ChestSlotStorageComponent chest = new(capacity: 1);
    ItemStackSnapshot original = new(1, 2, 10, false, 0, 0, false, null);
    ItemStackSnapshot replacement = new(2, 1, 10, false, 0, 0, false, null);
    chest.TrySetSlot(0, original);

    if (chest.Slots is ItemStackSnapshot?[] exposedSlots)
    {
      exposedSlots[0] = replacement;
    }

    Assert(
      chest.TryGetSlot(0, out ItemStackSnapshot? actual)
        && actual?.ContentType == original.ContentType,
      "A read-only slot view must not expose the mutable backing array.");
  }

  private static void TestItemRevisionDoesNotAdvanceForNoOpWrites()
  {
    ItemIdentityAndStackComponent item = new(1, 2, 10, false, 0, 0, false, null);
    long initialRevision = item.Revision;

    Assert(item.TrySetStack(2), "Setting the existing stack should remain valid.");
    Assert(item.TrySetMaxStack(10), "Setting the existing maximum should remain valid.");
    Assert(
      item.Revision == initialRevision,
      "No-op stack writes must not create a false revision conflict.");
  }

  private static void TestEmergencySchedulerReservesBothTransferItems()
  {
    EmergencyStackingTransferPlan plan = EmergencyStackingTransferPlan.Create(
      new EmergencyStackingCandidateSnapshot(
        "world-source",
        new ItemStackSnapshot(1, 2, 10, false, 0, 0, false, null),
        age: 1,
        isOnScreen: true,
        ownerPlayerIndex: 1),
      new EmergencyStackingCandidateSnapshot(
        "world-destination",
        new ItemStackSnapshot(1, 1, 10, false, 0, 0, false, null),
        age: 2,
        isOnScreen: true,
        ownerPlayerIndex: 1),
      distanceOrder: 0,
      preservationOrder: 0,
      distance: 1);
    EmergencyStackingSchedulerState scheduler = new();

    Assert(scheduler.TryAdd(plan), "The first emergency transfer should be accepted.");
    Assert(
      scheduler.HasPendingTransfer("world-source"),
      "The source item should be indexed as pending while the transfer is queued.");
  }

  private static void TestContainerPersistenceAndClientProjections()
  {
    ChestMetadataComponent metadata = new(
      x: 3,
      y: 4,
      index: 5,
      capacity: 2,
      bankChest: true,
      name: "Vault");
    ChestSlotStorageComponent storage = new(capacity: 2);
    storage.TrySetSlot(0, new ItemStackSnapshot(1, 3, 10, false, 0, 0, false, null));
    ChestPresentationStateComponent presentation = new();
    presentation.SetEatingAnimationTime(4);

    ItemContainerPersistenceSnapshot persistence = ItemContainerPersistenceProjection.Create(
      "chest-5",
      metadata,
      storage);
    ItemContainerClientSnapshot client = ItemContainerClientProjection.Create(
      ItemContainerNetworkProjection.Create("chest-5", storage),
      metadata,
      presentation);

    Assert(persistence.X == 3 && persistence.Capacity == 2, "Persistence should retain chest identity data.");
    Assert(persistence.Slots[0]?.Stack == 3, "Persistence should retain committed slot snapshots.");
    Assert(client.BankChest && client.EatingAnimationTime == 4, "Client projection should combine view state.");
  }

  private static void TestChestRegistryAndAccessAdaptersRejectDuplicates()
  {
    ChestRegistryAdapter registry = new();
    InventoryPosition position = new(1, 2);

    Assert(registry.TryAssign(position, 7), "The first chest assignment should succeed.");
    Assert(!registry.TryAssign(position, 8), "A coordinate must not map to two chest indices.");
    Assert(registry.TryFind(position, out int index) && index == 7, "The registry should return the assigned index.");

    ChestAccessTrackerAdapter access = new();
    Assert(access.TryOpen(7), "The first chest open should succeed.");
    Assert(!access.TryOpen(7), "A chest cannot be opened twice in the same access tracker.");
    Assert(access.Close(7) && !access.IsInUse(7), "Closing a chest should release its access marker.");
  }

  private static void TestQuickStackUsesExplicitReferenceSnapshots()
  {
    ItemStackSnapshot sourceItem = new(1, 7, 10, false, 0, 0, false, null);
    QuickStackSourceSnapshot source = QuickStackSourceSnapshot.Create(
      "player",
      new InventoryPosition(4, 5),
      new QuickStackSourceSlotSnapshot(
        new InventorySlotReference("player", 2),
        sourceItem));
    QuickStackDestinationSnapshot firstDestination = QuickStackDestinationSnapshot.Create(
      new InventorySlotReference("chest", 4),
      locked: false,
      transferBlocked: false,
      new ItemStackSnapshot(1, 8, 10, false, 0, 0, false, null));
    QuickStackDestinationSnapshot secondDestination = QuickStackDestinationSnapshot.Create(
      new InventorySlotReference("chest", 5),
      locked: false,
      transferBlocked: false,
      new ItemStackSnapshot(1, 8, 10, false, 0, 0, false, null));

    QuickStackPlan plan = QuickStackPlanQuery.CreatePlan(
      source,
      new[] { firstDestination, secondDestination });

    Assert(plan.Intents.Count == 2, "A source stack should be split across compatible destination slots.");
    Assert(
      plan.Intents[0].Destination == new InventorySlotReference("chest", 4)
        && plan.Intents[1].Destination == new InventorySlotReference("chest", 5),
      "Each transfer intent should retain its explicit destination slot.");
    Assert(
      source.PositionSnapshot.Position == new InventoryPosition(4, 5)
        && source.SlotReferences.References[0] == new InventorySlotReference("player", 2),
      "Source position and slot reference snapshots should be detached values.");
    Assert(
      firstDestination.Reference.Reference == new InventorySlotReference("chest", 4)
        && !firstDestination.Eligibility.Locked,
      "Destination reference and eligibility should be explicit snapshot boundaries.");
  }

  private static void Assert(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }
}
