using Terraria.WorldGeneration.Terrain;
using Terraria.WorldGeneration.Terrain.TreeTops;
using Terraria.WorldGeneration.Metrics;
using Terraria.WorldGeneration.Passes;

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

WorldFossilShatterScope scope = new();
Require(!scope.IsActive, "fossil scope starts inactive");
Require(!scope.TryEnter(403), "non-fossil tiles do not enter fossil scope");
Require(
  scope.TryEnter(WorldFossilShatterScope.FossilTileType),
  "fossil tile enters fossil scope");
Require(scope.IsActive, "fossil scope reports active after entry");
Require(
  !scope.TryEnter(WorldFossilShatterScope.FossilTileType),
  "nested fossil entry is rejected");

scope.Dispose();
Require(!scope.IsActive, "fossil scope releases after dispose");

WorldFossilShatterScope exceptionScope = new();
try
{
  using (exceptionScope)
  {
    Require(
      exceptionScope.TryEnter(WorldFossilShatterScope.FossilTileType),
      "exception scope enters fossil tile");
    throw new InvalidOperationException("synthetic fossil effect failure");
  }
}
catch (InvalidOperationException exception) when (
  exception.Message == "synthetic fossil effect failure")
{
}

Require(!exceptionScope.IsActive, "exception scope releases on using exit");
Console.WriteLine("WorldFossilShatterScope C12 focused verifier passed.");

WorldBoulderRainState boulderState = new();
WorldBoulderRainTransition started = boulderState.Advance(
  worldSurfaceAvailable: true,
  drunkWorld: true,
  goodWorld: true,
  remixWorld: false,
  storming: true);
Require(started.IsRaining, "boulder rain starts under the confirmed seed and storm conditions");
Require(!started.ShouldNotifyProgression, "boulder rain start does not notify progression");

WorldBoulderRainTransition ended = boulderState.Advance(
  worldSurfaceAvailable: true,
  drunkWorld: true,
  goodWorld: true,
  remixWorld: false,
  storming: false);
Require(!ended.IsRaining, "boulder rain ends when storming stops");
Require(ended.ShouldNotifyProgression, "boulder rain end emits a progression intent");

boulderState.Advance(
  worldSurfaceAvailable: true,
  drunkWorld: true,
  goodWorld: true,
  remixWorld: false,
  storming: true);
WorldBoulderRainTransition unavailableSurface = boulderState.Advance(
  worldSurfaceAvailable: false,
  drunkWorld: false,
  goodWorld: false,
  remixWorld: true,
  storming: false);
Require(unavailableSurface.IsRaining, "missing world surface preserves the prior boulder state");
Require(
  !unavailableSurface.ShouldNotifyProgression,
  "missing world surface does not advance or notify boulder state");

boulderState.Reset();
Require(!boulderState.IsRaining, "boulder state reset clears the world event state");
Console.WriteLine("WorldBoulderRainState C12 focused verifier passed.");

WorldTreeTopsStateComponent treeTops = new();
Require(
  treeTops.AreaCount == WorldTreeTopsStateComponent.TreeTopsAreaCount,
  "tree tops state exposes the confirmed 13 areas");
treeTops.SetTreeStyle(0, 4);
treeTops.SetTreeStyle(12, 42);
Require(treeTops.GetTreeStyle(0) == 4, "tree tops state reads a changed forest style");
Require(treeTops.GetTreeStyle(12) == 42, "tree tops state reads the underworld style");

WorldTreeTopsStateSnapshot treeTopsSnapshot = treeTops.CreateSnapshot();
treeTops.SetTreeStyle(0, 9);
Require(
  treeTopsSnapshot.Variations[0] == 4,
  "tree tops snapshot is isolated from later state mutations");
Require(
  treeTopsSnapshot.Variations[12] == 42,
  "tree tops snapshot preserves all area values");

try
{
  treeTops.GetTreeStyle(13);
  throw new InvalidOperationException("tree tops accepted an out-of-range area");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  treeTops.SetTreeStyle(-1, 0);
  throw new InvalidOperationException("tree tops accepted a negative area");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  ((IList<int>)treeTopsSnapshot.Variations)[0] = 7;
  throw new InvalidOperationException("tree tops snapshot exposed a writable list");
}
catch (NotSupportedException)
{
}

Console.WriteLine("WorldTreeTopsStateComponent C12 focused verifier passed.");

WorldBackgroundFlashCacheProjection backgroundCache = new();
int[] backgroundStyles = new int[WorldBackgroundFlashCacheProjection.BackgroundAreaCount];
backgroundStyles[0] = 3;
backgroundStyles[12] = 7;
backgroundCache.UpdateCache(backgroundStyles, isGameMenu: false);
Require(
  backgroundCache.GetVariation(0) == 3,
  "background cache stores a changed forest style");
Require(
  backgroundCache.GetVariation(12) == 7,
  "background cache stores a changed underworld style");
Require(
  backgroundCache.GetFlashPower(0) == 1f,
  "background style change starts a full flash outside the menu");

backgroundCache.UpdateFlashValues();
Require(
  Math.Abs(backgroundCache.GetFlashPower(0) - 0.95f) < 0.0001f,
  "background flash decays by the confirmed tick amount");
backgroundCache.UpdateCache(backgroundStyles, isGameMenu: false);
Require(
  Math.Abs(backgroundCache.GetFlashPower(0) - 0.95f) < 0.0001f,
  "unchanged background styles do not restart the flash");

backgroundStyles[0] = 4;
backgroundCache.UpdateCache(backgroundStyles, isGameMenu: true);
Require(
  Math.Abs(backgroundCache.GetFlashPower(0) - 0.95f) < 0.0001f,
  "background style changes in the menu do not start a flash");

backgroundStyles[0] = 5;
backgroundCache.UpdateCache(backgroundStyles, isGameMenu: false);
backgroundCache.UpdateFlashValues();
for (int i = 0; i < 30; i++)
{
  backgroundCache.UpdateFlashValues();
}

Require(
  backgroundCache.GetFlashPower(0) == 0f,
  "background flash decay clamps at zero");

try
{
  backgroundCache.GetFlashPower(WorldBackgroundFlashCacheProjection.BackgroundAreaCount);
  throw new InvalidOperationException("background cache accepted an out-of-range area");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  backgroundCache.UpdateCache(new int[12], isGameMenu: false);
  throw new InvalidOperationException("background cache accepted the wrong area count");
}
catch (ArgumentException)
{
}

Console.WriteLine("WorldBackgroundFlashCacheProjection C12 focused verifier passed.");

WorldStormSafeSpotScratch safeSpots = new();
safeSpots.Add(new WorldStormSafeSpot(10, 20, 24, 24));
Require(safeSpots.Count == 1, "storm safe spot scratch accepts one spot");
Require(
  safeSpots.Get(0).Contains(10, 20),
  "storm safe spot contains its inclusive top-left boundary");
Require(
  !safeSpots.Get(0).Contains(34, 44),
  "storm safe spot uses exclusive right and bottom boundaries");
safeSpots.Clear();
Require(safeSpots.Count == 0, "storm safe spot scratch clears between updates");

try
{
  safeSpots.Add(new WorldStormSafeSpot(0, 0, -1, 24));
  throw new InvalidOperationException("negative storm safe spot width was accepted");
}
catch (ArgumentOutOfRangeException)
{
}

Console.WriteLine("WorldStormSafeSpotScratch C12 focused verifier passed.");

WorldMeteorSpawnGeometry surfaceMeteor = WorldMeteorGeometryQuery.Calculate(
  maxTilesX: 4200,
  maxTilesY: 1200,
  worldSurface: 300.0,
  rockLayer: 500.0,
  underworldLayer: 1100,
  spawnTileX: 2100,
  spawnUnderGround: false);
Require(
  surfaceMeteor.VerticalMinInclusive == 90,
  "surface meteor start preserves Version4 cast order");
Require(
  surfaceMeteor.VerticalMaxExclusive == 1200,
  "surface meteor scan ends at maxTilesY");
Require(
  surfaceMeteor.HorizontalMinInclusive == 150 &&
  surfaceMeteor.HorizontalMaxExclusive == 4050,
  "meteor horizontal candidates preserve the Version4 half-open range");
Require(
  surfaceMeteor.IsHorizontalCandidate(150) &&
  !surfaceMeteor.IsHorizontalCandidate(4050),
  "meteor horizontal bounds are left-inclusive and right-exclusive");
Require(
  surfaceMeteor.IsSpawnExcluded(2100) &&
  !surfaceMeteor.IsSpawnExcluded(2100 - 336) &&
  !surfaceMeteor.IsSpawnExcluded(2100 + 336),
  "spawn exclusion preserves strict Version4 boundaries");

WorldMeteorSpawnGeometry undergroundMeteor = WorldMeteorGeometryQuery.Calculate(
  maxTilesX: 8400,
  maxTilesY: 2400,
  worldSurface: 400.0,
  rockLayer: 800.0,
  underworldLayer: 2200,
  spawnTileX: 4200,
  spawnUnderGround: true);
Require(
  undergroundMeteor.VerticalMinInclusive == 600,
  "underground meteor start preserves Version4 integer division order");
Require(
  undergroundMeteor.VerticalMaxExclusive == 2400,
  "underground meteor scan ends at maxTilesY");

try
{
  WorldMeteorGeometryQuery.Calculate(
    maxTilesX: 300,
    maxTilesY: 1200,
    worldSurface: 300.0,
    rockLayer: 500.0,
    underworldLayer: 1100,
    spawnTileX: 150,
    spawnUnderGround: false);
  throw new InvalidOperationException("meteor query accepted an empty horizontal range");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  WorldMeteorGeometryQuery.Calculate(
    maxTilesX: 4200,
    maxTilesY: 1200,
    worldSurface: double.NaN,
    rockLayer: 500.0,
    underworldLayer: 1100,
    spawnTileX: 2100,
    spawnUnderGround: false);
  throw new InvalidOperationException("meteor query accepted a non-finite surface");
}
catch (ArgumentOutOfRangeException)
{
}

Console.WriteLine("WorldMeteorGeometryQuery C10 focused verifier passed.");
