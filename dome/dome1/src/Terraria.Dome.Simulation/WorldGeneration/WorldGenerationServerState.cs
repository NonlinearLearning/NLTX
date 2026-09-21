using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

/// <summary>
/// Server-owned WorldGen values that must survive pass boundaries.
/// Presentation, transport, and thread-local values are intentionally absent.
/// </summary>
public sealed record WorldGenerationServerState
{
  public WorldGenerationServerState(
    WorldRuleSnapshotComponent rules,
    SavedOreTierDefaults oreTiers,
    TilePresenceScanResult? tilePresence = null,
    TileCountEnvironmentCounters? environmentCounters = null,
    TileCountSchedulingState? tileCountSchedule = null,
    int totalEvil = 0,
    int totalBlood = 0,
    int totalGood = 0,
    int totalSolid = 0,
    int roomTiles = 0,
    int maxRoomTiles = 0,
    int maxRoomSize = 0,
    int roomX1 = 0,
    int roomX2 = 0,
    int roomY1 = 0,
    int roomY2 = 0,
    bool canSpawn = false,
    bool roomTorch = false,
    bool roomDoor = false,
    bool roomChair = false,
    bool roomTable = false,
    bool roomHasStinkbug = false,
    bool roomHasEchoStinkbug = false,
    int shadowOrbCount = 0,
    int altarCount = 0,
    bool spawnEye = false,
    bool spawnHardBoss = false,
    bool spawnMeteor = false,
    int meteorShowerCount = 0,
    bool worldCleared = false,
    bool loadFailed = false,
    IReadOnlyList<int>? countedTiles = null)
  {
    if (totalEvil < 0 || totalBlood < 0 || totalGood < 0 || totalSolid < 0 ||
        roomTiles < 0 || maxRoomTiles < 0 || maxRoomSize < 0 || shadowOrbCount < 0 ||
        altarCount < 0 || meteorShowerCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(totalEvil));
    }

    if (roomX1 > roomX2 || roomY1 > roomY2)
    {
      throw new ArgumentException("Room bounds must be ordered.", nameof(roomX1));
    }

    Rules = rules;
    OreTiers = oreTiers;
    TilePresence = tilePresence;
    EnvironmentCounters = environmentCounters ?? TileCountEnvironmentCounters.Empty;
    TileCountSchedule = tileCountSchedule ?? TileCountSchedulingState.Initial;
    TotalEvil = totalEvil;
    TotalBlood = totalBlood;
    TotalGood = totalGood;
    TotalSolid = totalSolid;
    RoomTiles = roomTiles;
    MaxRoomTiles = maxRoomTiles;
    MaxRoomSize = maxRoomSize;
    RoomX1 = roomX1;
    RoomX2 = roomX2;
    RoomY1 = roomY1;
    RoomY2 = roomY2;
    CanSpawn = canSpawn;
    RoomTorch = roomTorch;
    RoomDoor = roomDoor;
    RoomChair = roomChair;
    RoomTable = roomTable;
    RoomHasStinkbug = roomHasStinkbug;
    RoomHasEchoStinkbug = roomHasEchoStinkbug;
    ShadowOrbCount = shadowOrbCount;
    AltarCount = altarCount;
    SpawnEye = spawnEye;
    SpawnHardBoss = spawnHardBoss;
    SpawnMeteor = spawnMeteor;
    MeteorShowerCount = meteorShowerCount;
    WorldCleared = worldCleared;
    LoadFailed = loadFailed;
    CountedTiles = countedTiles is null
      ? Array.Empty<int>()
      : new List<int>(countedTiles).AsReadOnly();
  }

  public WorldRuleSnapshotComponent Rules { get; }
  public SavedOreTierDefaults OreTiers { get; }
  public TilePresenceScanResult? TilePresence { get; }
  public TileCountEnvironmentCounters EnvironmentCounters { get; }
  public TileCountSchedulingState TileCountSchedule { get; }
  public int TotalEvil { get; }
  public int TotalBlood { get; }
  public int TotalGood { get; }
  public int TotalSolid { get; }
  public int RoomTiles { get; }
  public int MaxRoomTiles { get; }
  public int MaxRoomSize { get; }
  public int RoomX1 { get; }
  public int RoomX2 { get; }
  public int RoomY1 { get; }
  public int RoomY2 { get; }
  public bool CanSpawn { get; }
  public bool RoomTorch { get; }
  public bool RoomDoor { get; }
  public bool RoomChair { get; }
  public bool RoomTable { get; }
  public bool RoomHasStinkbug { get; }
  public bool RoomHasEchoStinkbug { get; }
  public int ShadowOrbCount { get; }
  public int AltarCount { get; }
  public bool SpawnEye { get; }
  public bool SpawnHardBoss { get; }
  public bool SpawnMeteor { get; }
  public int MeteorShowerCount { get; }
  public bool WorldCleared { get; }
  public bool LoadFailed { get; }
  public IReadOnlyList<int> CountedTiles { get; }

  public CrimsonHeartPositionSnapshot HeartPositions { get; init; } =
    new CrimsonHeartPositionSnapshot(Array.Empty<(int X, int Y)>());

  public WorldGenerationLegacyServerFields LegacyFields { get; init; } =
    new WorldGenerationLegacyServerFields();

  public WorldGenerationTerrainState Terrain { get; init; }

  public WorldGenerationProgressionState Progression { get; init; }

  public WorldGenerationHousingState Housing { get; init; }
}
