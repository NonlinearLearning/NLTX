using System.Numerics;
using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

RunAll();
Console.WriteLine("P18 focused verifier passed.");

static void RunAll()
{
  VerifySecretSeedCatalog();
  VerifyRuntimeRegistryTransitions();
  VerifySkyblockRulesAndSnapshotIsolation();
  VerifySkyblockPolicy();
  VerifyDerivedVariations();
  VerifyWorldSeedCatalogMatching();
  VerifyLandmassAndTreeProfiles();
}

static void VerifySecretSeedCatalog()
{
  WorldSecretSeedRegistryDefinitionsProjection catalog =
    WorldSecretSeedRegistryDefinitionsProjection.Version4;
  Assert(catalog.Definitions.Count == 35, "The Version4 secret-seed catalog must contain 35 entries.");
  Assert(
    catalog.Definitions[0].Variant == "paint-everything-gray" &&
    catalog.Definitions[^1].Variant == "dual-dungeons",
    "The Version4 secret-seed registration order changed.");

  WorldSecretSeedVisualAndSurfaceRulesDefinition visual =
    WorldSecretSeedVisualAndSurfaceRulesDefinition.Create(catalog);
  WorldSecretSeedTerrainAndStructureRulesDefinition terrain =
    WorldSecretSeedTerrainAndStructureRulesDefinition.Create(catalog);
  WorldSecretSeedProgressionAndInfectionRulesDefinition progression =
    WorldSecretSeedProgressionAndInfectionRulesDefinition.Create(catalog);

  Assert(visual.PaintEverythingGray.Variant == "paint-everything-gray", "Visual rule mapping failed.");
  Assert(terrain.ExtraFloatingIslands.Variant == "extra-floating-islands", "Terrain rule mapping failed.");
  Assert(progression.SurfaceIsDesert.Variant == "surface-is-desert", "Progression rule mapping failed.");
}

static void VerifyRuntimeRegistryTransitions()
{
  WorldSecretSeedRuntimeRegistryComponent component =
    new(generationId: 7, runtimeVersion: 3);

  WorldSecretSeedRuntimeRegistryCommitResult applied =
    WorldSecretSeedRuntimeRegistryCommitSystem.Commit(
      component,
      new EnableSecretSeedCommand(7, 3, "no-surface", "enable-1"));
  Assert(applied.Status == WorldSecretSeedRuntimeRegistryCommitStatus.Applied, "Enable must apply.");
  Assert(component.ActiveSecretSeedCount == 1, "Enable must update the derived active count.");

  WorldSecretSeedRuntimeRegistryCommitResult repeated =
    WorldSecretSeedRuntimeRegistryCommitSystem.Commit(
      component,
      new EnableSecretSeedCommand(7, 3, "no-surface", "enable-1"));
  Assert(repeated.Status == WorldSecretSeedRuntimeRegistryCommitStatus.Idempotent, "Repeated enable must be idempotent.");

  WorldSecretSeedRuntimeRegistryCommitResult conflict =
    WorldSecretSeedRuntimeRegistryCommitSystem.Commit(
      component,
      new EnableSecretSeedCommand(7, 3, "world-is-frozen", "enable-1"));
  Assert(
    conflict.Status == WorldSecretSeedRuntimeRegistryCommitStatus.RejectedIdempotencyConflict,
    "An idempotency-key reuse with a different operation must be rejected.");

  WorldSecretSeedRuntimeRegistryCommitResult stale =
    WorldSecretSeedRuntimeRegistryCommitSystem.Commit(
      component,
      new DisableSecretSeedCommand(7, 4, "no-surface", "disable-stale"));
  Assert(stale.Status == WorldSecretSeedRuntimeRegistryCommitStatus.RejectedStaleVersion, "A stale runtime version must be rejected.");

  WorldSecretSeedRuntimeRegistryCommitResult disabled =
    WorldSecretSeedRuntimeRegistryCommitSystem.Commit(
      component,
      new DisableSecretSeedCommand(7, 3, "no-surface", "disable-1"));
  Assert(disabled.Status == WorldSecretSeedRuntimeRegistryCommitStatus.Applied, "Disable must apply.");
  Assert(component.ActiveSecretSeedCount == 0, "Disable must restore the derived count.");

  WorldSecretSeedRuntimeRegistrySnapshot closed =
    WorldSecretSeedRuntimeRegistryCommitSystem.Close(component);
  Assert(!closed.IsActive && closed.ActiveSecretSeedCount == 0, "Close must clear and close the registry.");
}

static void VerifySkyblockRulesAndSnapshotIsolation()
{
  HashSet<ushort> activeTileTypes = new() { 26, 58, 12, 77, 226, 404 };
  HashSet<ushort> wallTypes = new() { 87 };
  WorldSkyblockGenerationScanSnapshot snapshot = new(
    generationId: 11,
    scanVersion: 2,
    worldTileCount: 100,
    currentActiveTiles: 9,
    activeTileTypes,
    wallTypes);
  activeTileTypes.Clear();
  wallTypes.Clear();

  Assert(snapshot.HasTile(26) && snapshot.HasWall(87), "A scan snapshot must not alias caller-owned sets.");

  WorldSkyblockGenerationRulesInput input = new(
    snapshot,
    new HashSet<ushort> { 999 },
    new HashSet<ushort> { 998 },
    skyblockWorld: true);
  WorldSkyblockGenerationRulesSelection selection =
    WorldSkyblockGenerationRulesQuery.Evaluate(input);
  Assert(!selection.NoAltars, "An altar tile must prevent NoAltars.");
  Assert(!selection.NoTemple, "A temple tile or wall must prevent NoTemple.");
  Assert(!selection.NoHellstone && !selection.NoLifeCrystals, "Observed special tiles must be preserved.");
  Assert(selection.LowTiles, "A scan below the strict 10% threshold must set LowTiles.");
  Assert(selection.NoDungeon, "Absent configured dungeon tiles and walls must set NoDungeon.");

  WorldSkyblockGenerationRulesSelection nonSkyblock =
    WorldSkyblockGenerationRulesQuery.Evaluate(
      new WorldSkyblockGenerationRulesInput(
        snapshot,
        new HashSet<ushort> { 26, 58, 12, 77, 226, 404 },
        new HashSet<ushort> { 87 },
        skyblockWorld: false));
  Assert(
    !nonSkyblock.NoAltars && !nonSkyblock.NoDungeon && !nonSkyblock.NoTemple &&
    !nonSkyblock.NoHellstone && !nonSkyblock.NoFossils && !nonSkyblock.NoLifeCrystals &&
    !nonSkyblock.NoHellforge && !nonSkyblock.LowTiles,
    "Non-Skyblock worlds must not expose Skyblock restrictions.");
}

static void VerifySkyblockPolicy()
{
  WorldSecretSeedRuntimeRegistryComponent component =
    new(generationId: 1, runtimeVersion: 1);
  WorldSecretSeedRegistryDefinitionsProjection catalog =
    WorldSecretSeedRegistryDefinitionsProjection.Version4;
  WorldSecretSeedVisualAndSurfaceRulesDefinition visual =
    WorldSecretSeedVisualAndSurfaceRulesDefinition.Create(catalog);
  WorldSecretSeedTerrainAndStructureRulesDefinition terrain =
    WorldSecretSeedTerrainAndStructureRulesDefinition.Create(catalog);
  WorldSecretSeedProgressionAndInfectionRulesDefinition progression =
    WorldSecretSeedProgressionAndInfectionRulesDefinition.Create(catalog);

  WorldSkyblockGenerationPolicySelection skyblock =
    WorldSkyblockGenerationPolicyQuery.Evaluate(
      true,
      component.CreateSnapshot(),
      visual,
      terrain,
      progression);
  Assert(skyblock.DenyFloatingIslands && skyblock.DenyAllGeneration && skyblock.DenySomeGeneration, "Empty Skyblock policy must deny generation.");

  WorldSkyblockGenerationPolicySelection normal =
    WorldSkyblockGenerationPolicyQuery.Evaluate(
      false,
      component.CreateSnapshot(),
      visual,
      terrain,
      progression);
  Assert(!normal.DenyFloatingIslands && !normal.DenyAllGeneration && !normal.DenySomeGeneration, "Non-Skyblock policy must allow all three outputs.");
}

static void VerifyDerivedVariations()
{
  WorldSecretSeedRegistryDefinitionsProjection catalog =
    WorldSecretSeedRegistryDefinitionsProjection.Version4;
  WorldSecretSeedRuntimeRegistryComponent component =
    new(generationId: 2, runtimeVersion: 5);
  WorldSecretSeedDerivedVariationsSelection selection =
    WorldSecretSeedDerivedVariationsQuery.Evaluate(
      new WorldSecretSeedDerivedVariationsInput(
        component.CreateSnapshot(),
        WorldSecretSeedVisualAndSurfaceRulesDefinition.Create(catalog),
        WorldSecretSeedTerrainAndStructureRulesDefinition.Create(catalog),
        WorldSecretSeedProgressionAndInfectionRulesDefinition.Create(catalog),
        SkyblockWorld: false));
  Assert(selection.ActiveSecretSeedCount == 0, "Derived variation count must come from the runtime snapshot.");
  Assert(!selection.PaintEverythingGrayJustTreasure && !selection.ExtraFloatingIslandsNormalAmount, "Empty runtime state must produce no enabled variations.");
}

static void VerifyWorldSeedCatalogMatching()
{
  WorldSeedOptionCatalogDefinition catalog = WorldSeedOptionCatalogDefinition.Version4;
  Assert(catalog.Options.Count == 10, "World-seed catalog must contain ten options.");
  WorldSeedOptionSeedMatchResult remix =
    WorldSeedOptionCatalogQuery.FindBySeedText(catalog, "Don't Dig Up", 0);
  Assert(remix.IsMatch && remix.Match.OptionId == WorldSeedOptionId.Remix, "Remix special seed matching failed.");

  WorldSeedOptionSeedMatchResult everything =
    WorldSeedOptionCatalogQuery.FindBySeedText(catalog, "getfixedboi", 0);
  Assert(everything.IsMatch && everything.Match.OptionId == WorldSeedOptionId.Everything, "Everything special seed matching failed.");

  WorldSeedOptionAutoGenerationResult skyblock =
    WorldSeedOptionCatalogQuery.ParseServerConfiguration(catalog, "seed_skyblock=1");
  Assert(
    skyblock.IsMatch &&
    skyblock.OptionId == WorldSeedOptionId.Skyblock &&
    skyblock.AutoGenerationEnabled,
    "Skyblock server configuration parsing failed.");
}

static void VerifyLandmassAndTreeProfiles()
{
  WorldLandmassDefinition landmass = new(
    WorldLandmassDataType.RoundLandmass,
    new Vector2(10f, 20f),
    radiusOrHalfSize: 4,
    style: 2);
  Vector2 top = landmass.Top;
  landmass.Top = top;
  Assert(landmass.Position == new Vector2(10f, 20f), "Landmass Top round-trip changed Position.");
  Assert(WorldTreeProfileCatalog.Version4.Profiles.Count == 10, "Tree profile catalog must contain ten profiles.");
  Assert(
    WorldTreeProfileCatalogQuery.TryGetFromTreeId(
      WorldTreeProfileCatalog.Version4,
      587,
      out WorldTreeProfileDefinition ruby) &&
    ruby.Id == WorldTreeProfileId.GemTreeRuby,
    "Tree tile lookup failed.");
  Assert(
    !WorldTreeProfileCatalogQuery.TryGetFromTreeId(
      WorldTreeProfileCatalog.Version4,
      -1,
      out _),
    "Out-of-range tree tile lookup must reject the value.");
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
