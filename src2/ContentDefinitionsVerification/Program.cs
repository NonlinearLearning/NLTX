using Terraria.NonAuthoritative.ContentDefinitions;

TestContentAbilityCatalog();
TestServiceReferences();
TestContentSampleIndex();
TestSetArrayFactory();
TestColorsAndShaders();
TestTileObjectDefinitions();
TestTileObjectGeometry();
TestTileObjectPlacement();
TestTilePlacementHooks();
TestTileStyles();
TestContentValidation();
TestPresentationCatalog();
TestItemVariants();
TestLegacyPrefixes();
TestArmorSetBonuses();
TestWingStats();
TestStaticItemCapabilities();

Console.WriteLine("P12 content definitions verifier passed.");

static void TestContentAbilityCatalog()
{
  ContentAbilityCatalog catalog =
    ContentAbilityRegistrationSystem.CreateCatalog(
      new ContentSize(1920, 1200),
      projectileCount: 4,
      buffCount: 3);

  ContentAbilityRegistrationSystem.RegisterProjectile(
    catalog,
    projectileType: 1,
    isHostile: true,
    isHook: false);
  ContentAbilityRegistrationSystem.RegisterBuff(
    catalog,
    buffType: 2,
    new BuffAbilityFlags(
      IsPvp: true,
      IsPersistent: true,
      IsVanityPet: false,
      IsLightPet: false,
      IsMelee: true,
      IsDebuff: false,
      DoNotSave: false,
      HideTimeDisplay: true));
  ContentAbilityRegistrationSystem.SetMusicPitch(catalog, 0.25f);

  ContentAbilitySnapshot snapshot = ContentAbilityQuery.Snapshot(catalog);
  Require(snapshot.MaxWorldViewSize == new ContentSize(1920, 1200),
    "Maximum world view size must be retained.");
  Require(snapshot.IsProjectileHostile(1), "Projectile capability must be queryable.");
  Require(snapshot.IsPvpBuff(2) && snapshot.IsPersistentBuff(2),
    "Buff capabilities must be queryable.");
  Require(snapshot.HideBuffTimeDisplay(2),
    "Buff display capability must be queryable.");
  Require(snapshot.MusicPitch == 0.25f, "Music pitch must be retained.");

  IList<bool> exposed = snapshot.ProjectileHostile as IList<bool> ??
    throw new InvalidOperationException();
  AssertThrows<NotSupportedException>(
    () => exposed[1] = false,
    "Catalog snapshots must reject mutations through list interfaces.");
  Require(snapshot.IsProjectileHostile(1),
    "Catalog snapshots must not expose mutable backing arrays.");
}

static void TestServiceReferences()
{
  MainContentServiceReferenceBoundary boundary =
    MainContentServiceBootstrapSystem.Create();
  TestServicePort port = new("bestiary");
  MainContentServiceBootstrapSystem.Attach(
    boundary,
    ContentServiceKind.Bestiary,
    port);

  Require(boundary.TryGet(ContentServiceKind.Bestiary, out IContentServicePort? actual),
    "Typed service reference must be discoverable.");
  Require(ReferenceEquals(port, actual), "Boundary must retain the typed port.");
  MainContentServiceBootstrapSystem.Clear(boundary);
  Require(!boundary.TryGet(ContentServiceKind.Bestiary, out _),
    "Clearing service references must remove handles.");
}

static void TestContentSampleIndex()
{
  ContentSampleIndexCatalog catalog = new();
  ContentSampleIndexBuildSystem.Rebuild(
    catalog,
    new[]
    {
      new ContentSampleReference(
        ContentDefinitionKind.Item,
        new ContentIdentity(4, 104, 204),
        "Item.Test"),
      new ContentSampleReference(
        ContentDefinitionKind.Npc,
        new ContentIdentity(5, 105, 205),
        "Npc.Test")
    },
    new[] { new CreativeItemOrder(4, 2, 1) });

  Require(catalog.TryGetByPersistentId(
      ContentDefinitionKind.Item,
      104,
      out ContentSampleReference item) && item.Identity.LocalType == 4,
    "Persistent IDs must map to sample references.");
  Require(catalog.TryGetByNetworkId(
      ContentDefinitionKind.Npc,
      205,
      out ContentSampleReference npc) && npc.Identity.LocalType == 5,
    "Network IDs must remain separate from persistent IDs.");
  Require(catalog.CreativeItems.Count == 1, "Creative ordering must be retained.");
  ContentSampleIndexSnapshot snapshot = ContentSampleIndexQuery.Snapshot(catalog);
  Require(snapshot.Version == 1 && snapshot.Samples.Count == 2,
    "Sample snapshots must capture a stable rebuild version.");
  ContentSampleIndexBuildSystem.Clear(catalog);
  Require(!catalog.IsInitialized && catalog.Samples.Count == 0,
    "Sample index clear must remove derived state.");
}

static void TestSetArrayFactory()
{
  SetArrayFactoryBoundary factory = new(4);
  bool[] bools = factory.CreateBoolSet(false, 1, 3);
  Require(bools[1] && bools[3] && !bools[0], "Boolean sets must apply sparse values.");
  int[] ints = factory.CreateIntSet(-1, 0, 7, 2, 9);
  Require(ints[0] == 7 && ints[2] == 9, "Integer sets must apply value pairs.");
  factory.Recycle(bools);
  factory.Recycle(ints);
  bool[] reset = factory.CreateBoolSet();
  int[] resetInts = factory.CreateIntSet();
  Require(!reset.Any(value => value), "Reused arrays must be reset before exposure.");
  Require(resetInts.All(value => value == -1),
    "Reused integer arrays must be reset before exposure.");
  AssertThrows<ArgumentException>(
    () => factory.CreateIntSet(0, 1),
    "Odd set input must be rejected.");
}

static void TestColorsAndShaders()
{
  ColorAndShaderCatalog catalog = ColorCatalogLoadSystem.CreateDefault();
  ColorValue color = LiquidColorQuery.CurrentLiquidColor(
    catalog,
    new[] { 1f, 0.5f });
  Require(color != catalog.LiquidColors[0],
    "Multiple liquid layers must be blended explicitly.");

  ShaderIndexAdapter shaders = new();
  shaders.Set("team", 7);
  ShaderIndexProjection.Apply(catalog, shaders);
  Require(catalog.TeamDyeShaderIndex == 7, "Shader indexes must cross an adapter seam.");
}

static void TestTileObjectDefinitions()
{
  TileObjectDefinitionCatalog catalog = new();
  TileObjectInheritanceDefinition parent = new(1);
  TileObjectInheritanceDefinition child = new(2);
  TileObjectDefinitionRegistrationSystem.Register(catalog, parent);
  TileObjectDefinitionRegistrationSystem.Register(catalog, child);
  TileObjectDefinitionRegistrationSystem.CopyFrom(child, parent);
  TileObjectDefinitionRegistrationSystem.AddAlternate(parent, 2);
  TileObjectDefinitionRegistrationSystem.Freeze(catalog);

  Require(child.ParentTileType == 1 && parent.AlternatesCount == 1,
    "Tile inheritance and alternates must be retained.");
  AssertThrows<InvalidOperationException>(
    () => TileObjectDefinitionRegistrationSystem.AddAlternate(parent, 3),
    "Frozen tile definitions must reject writes.");
}

static void TestTileObjectGeometry()
{
  TileObjectGeometryDefinition geometry = new(
    tileType: 1,
    width: 2,
    height: 2,
    origin: new PointValue(0, 1),
    direction: TileObjectDirection.Right,
    coordinateHeights: new[] { 16, 18 },
    coordinateWidth: 16,
    coordinatePadding: 2);
  TileGeometryRegistrationSystem.Calculate(geometry);
  TileGeometrySnapshot snapshot = TileGeometryQuery.Snapshot(geometry);
  Require(snapshot.CoordinateFullWidth == 34 && snapshot.CoordinateFullHeight == 36,
    "Tile geometry must calculate full frame dimensions.");
  Require(TileDrawProjection.Frame(snapshot, 1, 2).Width > 0,
    "Tile draw projection must expose a frame rectangle.");
}

static void TestTileObjectPlacement()
{
  TileObjectPlacementDefinition definition = new(1)
  {
    AnchorBottom = new TileAnchor(AnchorKind.Bottom, 1),
    AnchorValidTiles = new[] { 10 },
    WaterDeath = false,
    WaterPlacement = LiquidPlacementMode.Allowed
  };
  TileReadSnapshot tile = new(
    TileType: 10,
    WallType: 0,
    HasSolidAnchor: true,
    HasWallAnchor: false,
    Liquid: LiquidKind.Water);
  PlacementEligibilityResult result = TilePlacementRuleQuery.Evaluate(definition, tile);
  Require(result.IsAllowed, "Valid anchor and liquid rules must allow placement.");
}

static void TestTilePlacementHooks()
{
  TileObjectPlacementHookDefinition definition = new(1);
  TilePlacementHookRegistrationSystem.RegisterHook(
    definition,
    PlacementHookKind.CheckIfCanPlace,
    "check");
  TilePlacementHookRegistrationSystem.RegisterSubTile(definition, 2);
  TilePlacementCommand command = new(1, new PointValue(4, 5), 0);
  RecordingWorldWriter writer = new();
  TilePlacementCommandAdapter adapter = new(writer);
  PlacementCommitResult result = adapter.Submit(command);
  Require(result.Committed && writer.Commands.Count == 1,
    "Placement must emit one command to the world writer.");
  Require(definition.SubTiles.Contains(2), "Subtile definitions must be retained.");
  Require(definition.HookKeys[PlacementHookKind.CheckIfCanPlace] == "check",
    "Placement hook keys must remain explicit registration data.");
}

static void TestTileStyles()
{
  TileObjectStyleDefinitionAndSelection catalog = new();
  TileStyleRegistrationSystem.Register(
    catalog,
    new TileStyleDefinition(style: 2, width: 1, height: 1, step: 1));
  TileStyleRegistrationSystem.SetOverride(catalog, 4, 2);
  int selected = TileStyleSelectionQuery.Select(
    catalog,
    new TileStyleSelectionInput(4, 3, null, new FixedRandomSource(0)));
  Require(selected == 2, "Explicit style overrides must take precedence.");
  TileStyleRegistrationSystem.Freeze(catalog);
  AssertThrows<InvalidOperationException>(
    () => TileStyleRegistrationSystem.SetOverride(catalog, 5, 2),
    "Frozen tile styles must reject writes.");
}

static void TestContentValidation()
{
  ContentValidationBoundary boundary = new();
  ContentValidationLoadSystem.Allow(boundary, "Sprites/Test");
  ContentValidationLoadSystem.SetMetadata(
    boundary,
    "Sprites/Test",
    new TextureMetaData(16, 16));
  ContentValidationResult result = ContentValidationQuery.Validate(
    boundary,
    "Sprites/Test",
    new TextureSize(16, 16));
  Require(result.IsAccepted, "Matching texture metadata must be accepted.");
  Require(!ContentValidationQuery.Validate(
      boundary,
      "Sprites/Test",
      new TextureSize(8, 8)).IsAccepted,
    "Mismatched texture metadata must be rejected.");
}

static void TestPresentationCatalog()
{
  ContentPresentationCatalog catalog = new();
  PresentationCatalogLoadSystem.RegisterFont(catalog, new FontDefinition("UI.Default"));
  PresentationCatalogLoadSystem.RegisterHairstyle(
    catalog,
    new HairstyleUnlockDefinition(3, "Progression.Hair"));
  Require(catalog.TryGetFont("UI.Default", out _), "Fonts must use stable keys.");
  Require(catalog.TryGetHairstyle(3, out HairstyleUnlockDefinition hairstyle) &&
    hairstyle.ConfigKey == "Progression.Hair",
    "Presentation definitions must preserve config keys.");
  PresentationCatalogLoadSystem.Clear(catalog);
  Require(!catalog.IsLoaded && catalog.Fonts.Count == 0,
    "Presentation catalog clear must remove derived definitions.");
}

static void TestItemVariants()
{
  ItemVariantCatalog catalog = new();
  ItemVariantRegistrationSystem.Register(
    catalog,
    new ItemVariantDefinition(10, 2, "Hardmode", new RequiredWorldRuleCondition("hardmode")));
  ItemVariantSelectionResult result = ItemVariantSelectionQuery.Select(
    catalog,
    10,
    new WorldRuleSnapshot(new Dictionary<string, bool> { ["hardmode"] = true }));
  Require(result.IsSelected && result.VariantId == 2,
    "Variant selection must use explicit world-rule input.");
  Require(!ItemVariantSelectionQuery.Select(
      catalog,
      10,
      new WorldRuleSnapshot(new Dictionary<string, bool> { ["hardmode"] = false })).IsSelected,
    "Variant selection must not read implicit global world state.");
}

static void TestLegacyPrefixes()
{
  LegacyItemPrefixCatalog catalog = new(itemTypeCount: 8, prefixCount: 4);
  LegacyPrefixRegistrationSystem.SetCategoryMask(
    catalog,
    ItemPrefixCategory.Melee,
    new[] { 1, 2 });
  Require(catalog.IsAllowed(ItemPrefixCategory.Melee, 2),
    "Prefix category masks must be queryable.");
  Require(!catalog.IsAllowed(ItemPrefixCategory.Melee, 3),
    "Prefix category masks must reject unrelated prefixes.");
}

static void TestArmorSetBonuses()
{
  ArmorSetBonusBuilder builder = new("Armor.Test", ArmorSetPartType.Head);
  builder.Set(10, 20, 30);
  ArmorSetBonusDefinition definition = builder.Build();
  ArmorSetQueryResult result = ArmorSetQualificationQuery.Evaluate(
    definition,
    new ArmorSetQueryContext(10, 20, 30));
  Require(result.Complete && result.ItemsFound == 3,
    "Armor qualification must count matching parts without applying effects.");

  ArmorSetBonusLookupCatalog lookup = new();
  ArmorSetBonusLookupBuildSystem.Rebuild(lookup, new[] { definition });
  Require(lookup.GetSetsContaining(10).Count == 1,
    "Armor lookup must index definitions by item type.");
  Require(lookup.TryGetCompleteSet(
      new ArmorSetQueryContext(10, 20, 30),
      out ArmorSetBonusDefinition? complete) && ReferenceEquals(complete, definition),
    "Armor lookup must expose qualification without applying effects.");
}

static void TestWingStats()
{
  WingStatsDefinition stats = WingStatsRegistrationSystem.Create(
    wingType: 1,
    flyTime: 100,
    speedOverride: 2,
    accelerationMultiplier: 1.5f,
    hasDownHoverStats: true,
    downHoverSpeedOverride: 0.5f,
    downHoverAccelerationMultiplier: 0.75f);
  Require(stats.HasDownHoverStats && stats.FlyTime == 100,
    "Wing stats must retain hover-gated values.");
  Require(WingStatsQuery.GetDownHoverSpeed(stats) == 0.5f,
    "Down-hover values must be gated by their definition flag.");
  Require(!WingStatsDefinition.Default.HasDownHoverStats,
    "Wing defaults must keep down-hover behavior disabled.");
}

static void TestStaticItemCapabilities()
{
  ItemStaticCapabilityCatalog catalog = new(itemTypeCount: 8, armorSlotCount: 4);
  ItemStaticCapabilityRegistrationSystem.Register(
    catalog,
    itemType: 3,
    headType: 1,
    bodyType: 0,
    legType: 0,
    isStaff: true,
    isClaw: false);
  Require(catalog.IsStaff(3) && catalog.GetHeadItem(1) == 3,
    "Static item masks and reverse indexes must be queryable.");

  ItemSpawnCacheAdapter.RecordSpawn(catalog, 3);
  Require(catalog.GetSpawnCount(3) == 1, "Spawn counts must be explicit derived cache state.");
  catalog.InvalidateDerivedCaches();
  Require(catalog.GetSpawnCount(3) == 0, "Derived cache invalidation must clear counts.");
  ColorValue phaseColor = PhaseColorProjection.GetPhaseColor(
    catalog,
    new[] { new ColorValue(10, 20, 30) },
    0);
  Require(phaseColor == new ColorValue(10, 20, 30),
    "Phase colors must cross a read-only projection seam.");
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertThrows<TException>(Action action, string message)
  where TException : Exception
{
  try
  {
    action();
  }
  catch (TException)
  {
    return;
  }

  throw new InvalidOperationException(message);
}

sealed class TestServicePort : IContentServicePort
{
  public TestServicePort(string name)
  {
    Name = name;
  }

  public string Name { get; }
}

sealed class RecordingWorldWriter : IWorldTilePlacementWriter
{
  public List<TilePlacementCommand> Commands { get; } = new();

  public PlacementCommitResult Commit(TilePlacementCommand command)
  {
    Commands.Add(command);
    return new PlacementCommitResult(true, null);
  }
}

sealed class FixedRandomSource : IStyleRandomSource
{
  private readonly int _value;

  public FixedRandomSource(int value)
  {
    _value = value;
  }

  public int Next(int exclusiveMax)
  {
    return Math.Clamp(_value, 0, exclusiveMax - 1);
  }
}
