using Terraria.NpcTownBestiary;

var failures = new List<string>();

Run("town room assignment and eviction", () =>
{
  var registry = new TownRoomRegistryComponent();
  var system = new TownRoomAssignmentSystem();
  var assigned = system.Assign(registry, new AssignTownRoomCommand(22, new TownRoomTilePoint(10, 20), 0));
  Assert(assigned.Applied, "initial room assignment should apply");
  Assert(registry.Revision == 1, "assignment should advance revision");
  Assert(TownRoomQuery.TryGetRoom(registry, 22, out var point) && point == new TownRoomTilePoint(10, 20), "room query should return assigned point");

  var evicted = system.Evict(registry, new EvictTownResidentCommand(22, 1));
  Assert(evicted.Applied, "eviction should apply at the expected revision");
  Assert(!TownRoomQuery.HasRoom(registry, 22), "eviction should remove the room");
  Assert(!system.Evict(registry, new EvictTownResidentCommand(22, 1)).Applied, "stale eviction should be rejected");
});

Run("bestiary discovery and collection projection", () =>
{
  var kills = new BestiaryKillCountStateComponent();
  var sights = new BestiarySightDiscoveryStateComponent();
  var chats = new BestiaryChatDiscoveryStateComponent();
  var discovery = new BestiaryDiscoverySystem();
  var credit = new BestiaryCreditId("Guide");

  Assert(discovery.RegisterKill(kills, credit, 3).Applied, "kill should apply");
  Assert(discovery.RegisterKill(kills, credit, 999999999).Applied, "kill count should clamp rather than overflow");
  Assert(kills.GetCount(credit) == BestiaryKillCountPolicy.PositiveCap, "kill count should respect the cap");
  Assert(discovery.RegisterSight(sights, credit).Applied, "first sight should apply");
  Assert(!discovery.RegisterSight(sights, credit).Changed, "second sight should be idempotent");
  Assert(discovery.RegisterChat(chats, credit).Applied, "first chat should apply");

  var policy = new BestiaryCommonEnemyUnlockPolicy(false, 10);
  var state = BestiaryCollectionUnlockQuery.GetCommonEnemyState(kills, credit, policy);
  Assert(state == BestiaryEntryUnlockState.CanShowDropsWithDropRates, "full kill threshold should fully unlock entry");
  var critter = BestiaryCollectionUnlockQuery.GetCritterState(sights, credit);
  var townNpc = BestiaryCollectionUnlockQuery.GetTownNpcState(chats, credit);
  Assert(critter == BestiaryEntryUnlockState.CanShowDropsWithDropRates, "sight should unlock critter entry");
  Assert(townNpc == BestiaryEntryUnlockState.CanShowDropsWithDropRates, "chat should unlock town NPC entry");
});

Run("catalog, filters, and personality query", () =>
{
  var catalog = new NpcPersonalityCatalogBuilder()
    .Add(22, new NpcPersonalityDefinition(new[]
    {
      new NpcBiomePreference(NpcAffectionLevel.Like, new ShoppingBiomeDefinition("Forest")),
      new NpcBiomePreference(NpcAffectionLevel.Dislike, new ShoppingBiomeDefinition("Ocean"))
    }))
    .Build();
  var result = NpcPersonalityQuery.Evaluate(catalog, 22, new PersonalityEvaluationContext(
    new NpcEntityId(1),
    new PlayerEntityId(2),
    Array.Empty<NpcReadView>(),
    new[] { "Forest" }));
  Assert(result.Found && result.BestAffection == NpcAffectionLevel.Like, "personality query should select the best active biome preference");

  var bestiary = new BestiaryCatalogRegistrationSystem();
  var entry = bestiary.Register(new BestiaryEntryDefinition(
    new BestiaryEntryKey("guide"),
    new NpcNetId(22),
    creditId: new BestiaryCreditId("Guide"),
    providerKind: BestiaryProviderKind.TownNpc));
  Assert(bestiary.Catalog.FindByNetId(new NpcNetId(22)) == entry, "catalog should index entries by network ID");
});

Run("conditional dialogue consumption and cooldown", () =>
{
  var catalog = new ConditionalDialogueCatalog();
  catalog.Register(new ConditionalDialogueDefinition(
    new ConditionalDialogueKey("free-cake"),
    new NpcTypeId(208),
    _ => true,
    showIndicator: true));
  var dialogue = new ConditionalDialogueConsumeSystem(catalog);
  var query = new ConditionalDialogueQuery(catalog);
  var eligibility = query.Evaluate(new NpcDialogueReadView(new NpcTypeId(208), 0));
  Assert(eligibility.IsEligible, "eligible dialogue should be queryable");
  var consumed = dialogue.Consume(new ConsumeConditionalDialogueCommand(
    new NpcTypeId(208),
    new ConditionalDialogueKey("free-cake"),
    expectedRevision: 0));
  Assert(consumed.Applied, "eligible dialogue should be consumed once");
  Assert(!dialogue.Consume(new ConsumeConditionalDialogueCommand(
    new NpcTypeId(208),
    new ConditionalDialogueKey("free-cake"),
    expectedRevision: 0)).Applied, "consumed dialogue should not be consumed twice");

  var cooldown = new LucyAxeCooldownComponent();
  cooldown.Start(LucyMessageSource.Tree, 2);
  var cooldownSystem = new LucyAxeCooldownSystem();
  cooldownSystem.Tick(cooldown);
  Assert(cooldown.GetRemaining(LucyMessageSource.Tree) == 1, "cooldown should decrement by one tick");
  cooldownSystem.Tick(cooldown);
  cooldownSystem.Tick(cooldown);
  Assert(cooldown.GetRemaining(LucyMessageSource.Tree) == 0, "cooldown should not go below zero");
});

Run("bestiary persistence, network, and sight scan boundaries", () =>
{
  var kills = new BestiaryKillCountStateComponent();
  var sights = new BestiarySightDiscoveryStateComponent();
  var chats = new BestiaryChatDiscoveryStateComponent();
  var discovery = new BestiaryDiscoverySystem();
  var credit = new BestiaryCreditId("Slime");
  discovery.RegisterKill(kills, credit, 7);
  discovery.RegisterSight(sights, credit);
  discovery.RegisterChat(chats, credit);

  var buffer = new BestiarySightScanBuffer();
  var scan = new BestiarySightScanSystem();
  var candidate = new BestiarySightCandidate(
    new NpcNetId(1),
    credit,
    new BestiaryPlayerBounds(5, 5, 2, 2));
  var scanResult = scan.Scan(
    buffer,
    new[] { new BestiaryPlayerBounds(0, 0, 10, 10) },
    new[] { candidate },
    sights,
    discovery);
  Assert(scanResult.DiscoveredCount == 0, "scan should not double-count an already discovered sight");
  Assert(buffer.SeenNpcNetIds.Contains(new NpcNetId(1)), "scan should retain per-scan network dedupe state");
  buffer.Reset();
  Assert(buffer.SeenNpcNetIds.Count == 0 && buffer.PlayerBounds.Count == 0, "scan reset should clear transient state");

  var network = new BestiaryDiscoveryNetworkAdapter();
  var payload = network.CreateKillPayload(new NpcNetId(1), int.MaxValue);
  Assert(payload.KillCount == BestiaryKillCountPolicy.PositiveCap, "network kill payload should clamp count");
  var receivedKills = new BestiaryKillCountStateComponent();
  var received = network.Apply(
    payload,
    credit,
    discovery,
    receivedKills,
    new BestiarySightDiscoveryStateComponent(),
    new BestiaryChatDiscoveryStateComponent());
  Assert(received.Applied && receivedKills.GetCount(credit) == BestiaryKillCountPolicy.PositiveCap,
    "network kill payload should apply through the discovery writer");

  var adapter = new BestiaryUnlockPersistenceAdapter();
  using var stream = new MemoryStream();
  using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    adapter.Save(kills, sights, chats, writer);
  }

  stream.Position = 0;
  var restoredKills = new BestiaryKillCountStateComponent();
  var restoredSights = new BestiarySightDiscoveryStateComponent();
  var restoredChats = new BestiaryChatDiscoveryStateComponent();
  using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true))
  {
    adapter.Load(restoredKills, restoredSights, restoredChats, reader);
  }

  Assert(restoredKills.GetCount(credit) == 7, "persistence should restore kill counts");
  Assert(restoredSights.Contains(credit), "persistence should restore sight discoveries");
  Assert(restoredChats.Contains(credit), "persistence should restore chat discoveries");
});

Run("bestiary catalog, filters, sorting, and collection projection", () =>
{
  var catalog = new BestiaryCatalogRegistrationSystem();
  var entry = catalog.Register(new BestiaryEntryDefinition(
    new BestiaryEntryKey("zombie"),
    new NpcNetId(3),
    new BestiaryCreditId("Zombie"),
    BestiaryProviderKind.CommonEnemy,
    displayName: "Zombie"));
  entry.SetPresentation(rarityLevel: 2, sortingId: 4, isBoss: false);
  var other = catalog.Register(new BestiaryEntryDefinition(
    new BestiaryEntryKey("guide"),
    new NpcNetId(2),
    new BestiaryCreditId("Guide"),
    BestiaryProviderKind.TownNpc,
    displayName: "Guide"));
  other.SetPresentation(rarityLevel: 0, sortingId: 1, isBoss: false);

  var drops = new BestiaryDropMergeAdapter();
  var merge = drops.Merge(
    catalog,
    new[]
    {
      new BestiaryDropRegistration(new NpcNetId(3), new BestiaryDropRateView(1, 0.5f, 1, 1)),
      new BestiaryDropRegistration(new NpcNetId(99), new BestiaryDropRateView(2, 0.1f, 1, 1))
    });
  Assert(merge.AppliedCount == 1 && merge.MissingEntryCount == 1, "drop merge should report applied and missing entries");
  Assert(entry.InfoElements.Count == 1, "drop merge should append a UI-neutral info element");

  var filterCatalog = new BestiaryFilterCatalog();
  var search = filterCatalog.Definitions.Single(
    definition => definition.Kind == BestiaryFilterKind.Search);
  Assert(BestiaryFilterQuery.Matches(search, new BestiaryFilterContext(
    entry,
    BestiaryEntryUnlockState.NotKnownAtAll,
    new HashSet<string>(),
    "zom")), "search filter should normalize case-insensitively");
  Assert(BestiaryFilterMetadataQuery.GetForcedDisplay(search) == true,
    "search filter should be forced visible");

  var views = new[]
  {
    new BestiarySortView(entry, BestiaryEntryUnlockState.CanShowPortraitOnly, 20),
    new BestiarySortView(other, BestiaryEntryUnlockState.CanShowDropsWithDropRates, 10)
  };
  var sorted = BestiarySortQuery.Sort(views, BestiarySortKind.NetId);
  Assert(sorted[0].Entry == other, "net ID sort should use the explicit network ID");
  var unlockSorted = BestiarySortQuery.Sort(views, BestiarySortKind.UnlockState);
  Assert(unlockSorted[0].Entry == other, "unlock sort should order the highest unlock state first");

  var providerDefinitions = new BestiaryCollectionProviderDefinitions();
  providerDefinitions.Register(
    entry.Key,
    new BestiaryCollectionProviderDefinition(
      BestiaryProviderKind.CommonEnemy,
      entry.CreditId,
      fullKillCountNeeded: 10));
  var kills = new BestiaryKillCountStateComponent();
  new BestiaryDiscoverySystem().RegisterKill(kills, entry.CreditId, 10);
  var providerDefinitionsFound = providerDefinitions.TryGet(entry.Key, out var provider);
  Assert(providerDefinitionsFound, "collection provider definition should be addressable by entry key");
  var collection = BestiaryUICollectionProjection.Create(
    entry,
    provider,
    kills,
    new BestiarySightDiscoveryStateComponent(),
    new BestiaryChatDiscoveryStateComponent(),
    5);
  Assert(collection.OwnerEntryKey == entry.Key &&
    collection.UnlockState == BestiaryEntryUnlockState.CanShowDropsWithDropRates,
    "collection projection should derive unlock state without writing discovery state");
});

Run("profile assets, info elements, and item groups", () =>
{
  var profile = new TownNpcProfileDefinition(
    new NpcTypeId(22),
    "Images/TownNPCs/Guide",
    "Images/TownNPCs/Shimmered/Guide",
    new[] { 1, 2 });
  var assetAdapter = new TownNpcProfileAssetAdapter();
  Assert(assetAdapter.GetRoot(profile, false) == "Images/TownNPCs/Guide",
    "profile adapter should expose the default asset root");
  Assert(assetAdapter.GetVariantAssetKey(profile, true, "Party") ==
    "Images/TownNPCs/Shimmered/Guide_Party",
    "profile adapter should isolate shimmered variant asset keys");

  var statsAdapter = new BestiaryStatsRefreshAdapter();
  var stats = statsAdapter.Refresh(
    new NpcReadView(new NpcEntityId(1), new NpcTypeId(22), Damage: 10, LifeMax: 100),
    new NpcNetId(22));
  Assert(stats.NpcNetId == new NpcNetId(22) && stats.Damage == 10 && stats.LifeMax == 100,
    "stats adapter should preserve the explicit network identity");

  var groups = new ConditionalDialogueItemGroupAdapter();
  groups.RegisterStatic("Bars", new[] { 1, 2 });
  groups.ReplaceDynamic("Whips", new[] { 3 });
  var snapshot = groups.Snapshot();
  Assert(snapshot["Bars"].Contains(2) && snapshot["Whips"].Contains(3),
    "item group adapter should expose static and dynamic snapshots");
});

if (failures.Count > 0)
{
  Console.Error.WriteLine(string.Join(Environment.NewLine, failures));
  return 1;
}

Console.WriteLine("P11 focused verifier passed: 8 scenarios");
return 0;

void Run(string name, Action action)
{
  try
  {
    action();
  }
  catch (Exception exception)
  {
    failures.Add($"{name}: {exception.Message}");
  }
}

void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
