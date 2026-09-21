namespace Terraria.Dome.Simulation.WorldGeneration;

/// <summary>
/// Named server-side WorldGen fields retained during the legacy migration.
/// </summary>
public sealed record WorldGenerationLegacyServerFields
{
  public bool NoAltars { get; init; }
  public bool NoDungeon { get; init; }
  public bool NoTemple { get; init; }
  public bool NoHellstone { get; init; }
  public bool NoFossils { get; init; }
  public bool NoLifeCrystals { get; init; }
  public bool NoHellforge { get; init; }
  public bool LowTiles { get; init; }
  public bool DenyFloatingIslands { get; init; }
  public bool DenyAllGeneration { get; init; }
  public bool DenySomeGeneration { get; init; }
  public bool AllowedToSpreadInfections { get; init; } = true;
  public bool DestroyObject { get; init; }
  public bool MergeUp { get; init; }
  public bool MergeDown { get; init; }
  public bool MergeLeft { get; init; }
  public bool MergeRight { get; init; }
  public bool StopDrops { get; init; }
  public bool PlacingTraps { get; init; }
  public bool FossilBreak { get; init; }
  public bool HardModeWorldUpdates { get; init; }
  public bool GrowGrassUnderground { get; init; }
  public bool IsRainingBoulders { get; init; }
  public int TileReframeCount { get; init; }
  public int NumTileCount { get; init; }
  public int MaxTileCount { get; init; } = 3500;
  public int MaxWallOut { get; init; } = 5000;
  public int LavaCount { get; init; }
  public int IceCount { get; init; }
  public int SandCount { get; init; }
  public int RockCount { get; init; }
  public int ShroomCount { get; init; }
  public int TotalEvil2 { get; init; }
  public int TotalBlood2 { get; init; }
  public int TotalGood2 { get; init; }
  public int TotalSolid2 { get; init; }
  public byte ColumnEvil { get; init; }
  public byte ColumnBlood { get; init; }
  public byte ColumnGood { get; init; }
  public int TotalX { get; init; }
  public int TotalD { get; init; }
  public int LastMaxTilesX { get; init; }
  public int LastMaxTilesY { get; init; }
  public int MaxRoomTiles { get; init; } = 750;
  public int MaxRoomSize { get; init; } = 100;
  public int NumberRoomTiles { get; init; }
  public int RoomX1 { get; init; }
  public int RoomX2 { get; init; }
  public int RoomY1 { get; init; }
  public int RoomY2 { get; init; }
  public bool CanSpawn { get; init; }
  public int BestX { get; init; }
  public int BestY { get; init; }
  public int HighScore { get; init; }
  public bool RoomTorch { get; init; }
  public bool RoomDoor { get; init; }
  public bool RoomChair { get; init; }
  public bool RoomTable { get; init; }
  public bool RoomHasStinkbug { get; init; }
  public bool RoomHasEchoStinkbug { get; init; }
  public bool CurrentlyTryingAlternateHousingSpot { get; init; }
  public int SharedRoomX { get; init; }
  public int RoomCheckFailureReason { get; init; }
  public int NpcSpawnDelay { get; init; }
  public int NpcSpawnPeriod { get; init; }
  public int PrioritizedTownNpcType { get; init; }
  public bool SpawnEye { get; init; }
  public int SpawnHardBoss { get; init; }
  public bool ShadowOrbSmashed { get; init; }
  public int ShadowOrbCount { get; init; }
  public int AltarCount { get; init; }
  public bool SpawnMeteor { get; init; }
  public bool WorldCleared { get; init; }
  public bool LoadFailed { get; init; }
  public bool WorldBackupAvailable { get; init; }
  public int MeteorShowerCount { get; init; }
  public bool GeneratingWorld { get; init; }
  public bool GeneratingWorldOrLoading { get; init; }
  public bool RemixWorldGeneration { get; init; }
  public bool EverythingWorldGeneration { get; init; }
  public bool NoTrapsWorldGeneration { get; init; }
  public bool DrunkWorldGeneration { get; init; }
  public bool GoodWorldGeneration { get; init; }
  public bool TenthAnniversaryWorldGeneration { get; init; }
  public bool DontStarveWorldGeneration { get; init; }
  public bool NotTheBeesWorld { get; init; }
  public bool SkyblockWorldGeneration { get; init; }
  public bool PlacingWorldStructures { get; init; }
  public int WorldGenParamEvil { get; init; } = -1;
  public int GenerationCursor { get; init; }
  public int GenerationPassVersion { get; init; } = 1;
  public int SurfaceY { get; init; }
  public int RockLayerY { get; init; }
  public int UnderworldLayerY { get; init; }
  public int OceanDistance { get; init; } = 250;
  public int BeachDistance { get; init; } = 380;
  public int ShimmerSafetyDistance { get; init; } = 150;
  public int CactusWaterWidth { get; init; } = 50;
  public int CactusWaterHeight { get; init; } = 25;
  public int CactusWaterLimit { get; init; } = 25;
  public int GrassSpread { get; init; }
  public int HeartCount { get; init; }
  public int SmallConsecutivesFound { get; init; }
  public int SmallConsecutivesEliminated { get; init; }
  public int NeonMossType { get; init; }
  public int TrapDiagonalCount { get; init; }
  public bool GemSelectionComplete { get; init; }
  public bool MossSelectionComplete { get; init; }
  public bool PreventInfiniteRopeFraming { get; init; }
  public bool BuiltHouseWithNoFurniture { get; init; }
  public bool BuiltHouseWithNoLight { get; init; }
  public bool ExtraLiquidGeneration { get; init; }
  public bool NoInfectionGeneration { get; init; }
  public bool WorldIsInfected { get; init; }
  public bool SurfaceIsDesert { get; init; }
  public bool SurfaceIsMushrooms { get; init; }
  public bool SurfaceIsInSpace { get; init; }
  public bool HallowOnTheSurface { get; init; }
  public bool NoSpiderCaves { get; init; }
  public bool DigExtraHoles { get; init; }
  public bool RoundLandmasses { get; init; }
  public bool ExtraLivingTrees { get; init; }
  public bool ExtraFloatingIslands { get; init; }
  public bool BiggerAbandonedHouses { get; init; }
  public bool AddTeleporters { get; init; }
  public bool PortalGunInChests { get; init; }
  public bool ActuallyNoTraps { get; init; }
  public bool PooEverywhere { get; init; }
  public bool RainbowStuff { get; init; }
  public bool HalloweenGeneration { get; init; }
  public bool TeamBasedSpawns { get; init; }
  public bool DualDungeons { get; init; }
  public bool Vampirism { get; init; }
}
