using System;
using System.Linq;
using System.Threading.Tasks;
using NSSLC.WorldGeneration.WorldBuilding;

namespace NSSLC.WorldGeneration;

public static partial class Main {
  private static bool _objectDataInitialized;
  public static bool onlyShimmerOceanWorldsGeneration;
  public static bool onlyShimmerOceanWorlds;
  public static bool Setting_UseReducedMaxLiquids;
  public static int starsHit;
  public static int mapTimeMax = 30;
  public static string worldName = "Generated World";
  public static NSSLC.WorldGeneration.Geometry.Vector2 screenPosition;
  public static bool ShouldShowInvisibleBlocksAndWalls() => true;

  internal static void ResetWorldStorage() {
    tile = new Tile[maxTilesX, maxTilesY];
    for (int x = 0; x < maxTilesX; x++) {
      for (int y = 0; y < maxTilesY; y++) {
        tile[x, y] = new Tile();
      }
    }
    chest = new Chest[8000];
    sign = new Sign[32000];
    npc = Enumerable.Range(0, 200).Select(index => new NPC { whoAmI = index }).ToArray();
    player = Enumerable.Range(0, 256).Select(index => new Player()).ToArray();
    projectile = Enumerable.Range(0, 1000).Select(index => new Projectile()).ToArray();
    item = Enumerable.Range(0, 400).Select(index => new WorldItem()).ToArray();
    dust = Enumerable.Range(0, 6001).Select(index => new Dust()).ToArray();
    gore = Enumerable.Range(0, 601).Select(index => new Gore()).ToArray();
    maxNPCs = npc.Length;
    liquid = Enumerable.Range(0, Liquid.maxLiquid).Select(index => new Liquid()).ToArray();
    liquidBuffer = Enumerable.Range(0, Liquid.maxLiquidBuffer)
      .Select(index => new LiquidBuffer()).ToArray();
    leftWorld = 0;
    topWorld = 0;
    rightWorld = maxTilesX * 16;
    bottomWorld = maxTilesY * 16;
    maxSectionsX = (maxTilesX + 199) / 200;
    maxSectionsY = (maxTilesY + 149) / 150;
    spawnTileX = maxTilesX / 2;
    spawnTileY = maxTilesY / 3;
    gameMenu = true;
    dedServ = true;
    netMode = 0;
    dayTime = true;
    time = 13500;
    hardMode = false;
    UnderworldLayer = maxTilesY - 200;
    InitializeTileRules();
    ID.TileID.Sets.PostSetupContent();
    Framing.Initialize();
    Minecart.Initialize();
    ItemCatalog.InitializePlacementDetails();
    if (!_objectDataInitialized) {
      ObjectData.TileObjectData.Initialize();
      _objectDataInitialized = true;
    }
    TileEntity.Clear();
    Chest.Clear();
  }

  public static void checkXMas() { }
  public static void checkHalloween() { }
  public static void UpdateTimeRate() { }
	public static void ClearWorldSeedFlags()
{
	
		getGoodWorld = false;
		drunkWorld = false;
		tenthAnniversaryWorld = false;
		dontStarveWorld = false;
		notTheBeesWorld = false;
		remixWorld = false;
		noTrapsWorld = false;
		zenithWorld = false;
		skyblockWorld = false;
		vampireSeed = false;
		infectedSeed = false;
		teamBasedSpawnsSeed = false;
		dualDungeonsSeed = false;
	
	}
  public static void ResetWindCounter(bool resetExtreme = false) {
    windSpeedCurrent = 0;
    windSpeedTarget = 0;
  }
  public static void ChangeRain(bool instant = false, float? strengthOverride = null) {
    raining = true;
    maxRaining = strengthOverride ?? 0.5f;
  }
  public static void AnglerQuestSwap() { }
  public static void QueueMainThreadAction(Action action) {
    action.Invoke();
  }
  public static Task RunOnMainThread(Action action) {
    action.Invoke();
    return Task.CompletedTask;
  }
  public static void FixUIScale() { }
  	public static double starGameMath(double value = 1.0)
{
	
		if (!starGame)
		{
			return 1.0;
		}
		double num = (double)starsHit / 200.0;
		if (num > 1.0)
		{
			num = 1.0;
		}
		return 1.0 + num * value;
	
	}
}
