using System.Numerics;
using Terraria.Content;
using Terraria.Npc;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldSession.Components;
using Terraria.WorldStorage;
using RuntimeMain = NSSLC.WorldGeneration.Main;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class RuntimeNpcNaturalSpawnPass
{
  private readonly NpcSpawnSystem _spawnSystem;
  private readonly RuntimeNpcSpawnPassPort _port;

  public RuntimeNpcNaturalSpawnPass(
    LoadedWorldSession session,
    RuntimePlayerStore players,
    RuntimeNpcStore npcs,
    ContentCatalog content)
  {
    _port = new RuntimeNpcSpawnPassPort(session, players, npcs, content);
    _spawnSystem = new NpcSpawnSystem(_port);
  }

  public IReadOnlyList<int> CreatedNetIds => _port.CreatedNetIds;

  public void Update(long tickNumber)
  {
    if (!NpcAiAuthorityGate.IsAuthoritative(RuntimeMain.netMode))
    {
      return;
    }

    _port.BeginTick(tickNumber);
    _ = _spawnSystem.ProcessNaturalSpawnPassDetailed();
  }
}

internal sealed class RuntimeNpcSpawnPassPort : INpcSpawnObservedPassPort
{
  private const int ScreenWidthPixels = 1920;
  private const int ScreenHeightPixels = 1080;
  private const int DefaultSpawnRate = 600;
  private const int DefaultMaxSpawns = 5;
  private const int SlimeRainSpawnPeriod = 120;
  private const ushort ActiveTileFlag = 0x20;
  private const ushort InactiveTileFlag = 0x40;

  private readonly LoadedWorldSession _session;
  private readonly RuntimePlayerStore _players;
  private readonly RuntimeNpcStore _npcs;
  private readonly ContentCatalog _content;
  private readonly List<int> _createdNetIds = new();
  private readonly HashSet<int> _slimeRainPlayersSpawnedThisTick = new();

  public RuntimeNpcSpawnPassPort(
    LoadedWorldSession session,
    RuntimePlayerStore players,
    RuntimeNpcStore npcs,
    ContentCatalog content)
  {
    _session = session ?? throw new ArgumentNullException(nameof(session));
    _players = players ?? throw new ArgumentNullException(nameof(players));
    _npcs = npcs ?? throw new ArgumentNullException(nameof(npcs));
    _content = content ?? throw new ArgumentNullException(nameof(content));
  }

  public long TickNumber { get; private set; }

  public void BeginTick(long tickNumber)
  {
    TickNumber = tickNumber;
    _slimeRainPlayersSpawnedThisTick.Clear();
  }

  public IReadOnlyList<int> CreatedNetIds => _createdNetIds.AsReadOnly();

  public bool IsSlimeRainActive => _session.World.TimeWeather.SlimeRain;

  public NpcSpawnPlayerEligibilitySnapshot CapturePlayerEligibility(int playerIndex)
  {
    RuntimePlayerEntity? player = GetPlayer(playerIndex);
    return new NpcSpawnPlayerEligibilitySnapshot(
      IsPlayerActive: player is not null,
      IsPlayerDead: player?.Lifecycle.IsDead ?? true,
      IsJourneyMode: _session.World.Rules.GameMode == WorldGameMode.Journey,
      IsSpawnRatePowerUnlocked: false,
      DoesSpawnRatePowerDisablePlayer: false,
      IsNearMoonLord: false);
  }

  public IReadOnlyList<NpcSpawnScreenPlayerSnapshot> CaptureScreenPlayers()
  {
    return _players.Players.Select(static player => new NpcSpawnScreenPlayerSnapshot(
      IsActive: !player.Lifecycle.IsDead,
      CenterXInPixels: player.Movement.Position.X + 10f,
      CenterYInPixels: player.Movement.Position.Y + 21f,
      InsideUnbreakableWalls: false)).ToArray();
  }

  public NpcSpawnPostCheckInputs CapturePostCheckInputs(
    int playerIndex,
    in NpcSpawnTileSearchResult tileSearchResult)
  {
    TileCellState spawnTile = ReadCell(tileSearchResult.TileX, tileSearchResult.TileY);
    TileCellState above = ReadCell(tileSearchResult.TileX, tileSearchResult.TileY - 1);
    TileCellState twoAbove = ReadCell(tileSearchResult.TileX, tileSearchResult.TileY - 2);
    WorldTimeWeatherState time = _session.World.TimeWeather;
    return new NpcSpawnPostCheckInputs(
      SpawnTileType: spawnTile.Type,
      SpawnWallType: spawnTile.Wall,
      ZoneDungeon: false,
      IsDungeonTile: false,
      DualDungeonsSeed: HasSeed(WorldSecretSeedFlags.DualDungeons),
      HasLiquidAtTileAbove: above.LiquidAmount > 0,
      HasLiquidAtTwoTilesAbove: twoAbove.LiquidAmount > 0,
      TileAboveIsLava: above.LiquidType == 1,
      TileAboveIsShimmer: above.LiquidType == 3,
      TileAboveIsHoney: above.LiquidType == 2,
      BloodMoon: time.BloodMoon,
      Eclipse: time.Eclipse,
      InvasionType: (int)_session.World.Progression.Invasion.Type,
      PumpkinMoon: time.PumpkinMoon,
      SnowMoon: time.SnowMoon,
      SlimeRain: time.SlimeRain);
  }

  public int Next(int exclusiveUpperBound) => GetRandom().Next(exclusiveUpperBound);

  public int Next(int minimumInclusive, int maximumExclusive)
  {
    return minimumInclusive == maximumExclusive
      ? minimumInclusive
      : GetRandom().Next(minimumInclusive, maximumExclusive);
  }

  public int RollOnlyBadLuckExtreme(float luck, int range)
  {
    if (!float.IsFinite(luck))
    {
      throw new ArgumentOutOfRangeException(nameof(luck));
    }
    return Next(range);
  }

  public NpcSpawnAreaInputs CaptureSpawnAreaInputs(int playerIndex)
  {
    RuntimePlayerEntity player = RequirePlayer(playerIndex);
    int tileX = (int)MathF.Floor((player.Movement.Position.X + 10f) / 16f);
    int tileY = (int)MathF.Floor((player.Movement.Position.Y + 21f) / 16f);
    double surface = _session.World.Descriptor.SurfaceLayer;
    return new NpcSpawnAreaInputs(
      ScreenWidthPixels,
      ScreenHeightPixels,
      tileX,
      tileY,
      SelectedItemType: 0,
      PlayerScope: false,
      DualDungeonsSeed: HasSeed(WorldSecretSeedFlags.DualDungeons),
      ZoneOverworldHeight: tileY <= surface,
      ZoneSkyHeight: tileY < surface * 0.35,
      MaxTilesX: _session.Storage.TileMap.Width,
      MaxTilesY: _session.Storage.TileMap.Height);
  }

  public bool IsActiveSolidTile(int tileX, int tileY)
  {
    if (!IsInWorld(tileX, tileY))
    {
      return true;
    }
    TileCellState tile = _session.Storage.TileMap.GetTile(tileX, tileY);
    if ((tile.TileHeader & ActiveTileFlag) == 0 ||
        (tile.TileHeader & InactiveTileFlag) != 0 ||
        tile.Type >= RuntimeMain.tileSolid.Length ||
        tile.Type >= RuntimeMain.tileSolidTop.Length)
    {
      return false;
    }
    return RuntimeMain.tileSolid[tile.Type] && !RuntimeMain.tileSolidTop[tile.Type];
  }

  public bool IsHouseWallTile(int tileX, int tileY)
  {
    if (!IsInWorld(tileX, tileY))
    {
      return true;
    }
    ushort wall = _session.Storage.TileMap.GetTile(tileX, tileY).Wall;
    return wall < RuntimeMain.wallHouse.Length && RuntimeMain.wallHouse[wall];
  }

  public NpcSpawnTileSpaceFacts CaptureTileSpaceFacts(int tileX, int tileY)
  {
    if (!IsInWorld(tileX, tileY))
    {
      return new NpcSpawnTileSpaceFacts(IsActive: true, IsSolid: true, HasAnyLava: true);
    }
    TileCellState tile = _session.Storage.TileMap.GetTile(tileX, tileY);
    bool active = (tile.TileHeader & (ActiveTileFlag | InactiveTileFlag)) == ActiveTileFlag;
    bool solid = tile.Type < RuntimeMain.tileSolid.Length &&
      tile.Type < RuntimeMain.tileSolidTop.Length &&
      RuntimeMain.tileSolid[tile.Type] && !RuntimeMain.tileSolidTop[tile.Type];
    return new NpcSpawnTileSpaceFacts(
      active,
      solid,
      tile.LiquidAmount > 0 && tile.LiquidType == 1);
  }

  public NpcSpawnPerPlayerFlagsPrelude CapturePerPlayerFlagsPrelude(int playerIndex)
  {
    NpcSpawnRateInputs rate = CreateBaseRateInputs(playerIndex);
    return new NpcSpawnPerPlayerFlagsPrelude(
      rate.Context,
      rate.BiomeZones,
      rate.EventAndTower,
      _session.World.Progression.Bosses.PlantBoss,
      _session.World.Rules.HardMode,
      HasSeed(WorldSecretSeedFlags.DualDungeons),
      PlayerInsideUnbreakableWalls: false,
      DungeonProgressCanSafelyMatch: 0,
      DungeonProgressPlayerNeedsToMatch: 0);
  }

  public NpcSpawnInvasionInputs CaptureInvasionInputs(int playerIndex)
  {
    RuntimePlayerEntity player = RequirePlayer(playerIndex);
    InvasionRuntimeState invasion = _session.World.Progression.Invasion;
    return new NpcSpawnInvasionInputs(
      (int)invasion.Type,
      invasion.Delay,
      invasion.Size,
      player.Movement.Position.X,
      player.Movement.Position.Y,
      _session.World.Descriptor.SurfaceLayer,
      ScreenHeightPixels,
      (int)MathF.Floor((player.Movement.Position.Y + 21f) / 16f),
      invasion.PositionX,
      _session.Storage.TileMap.Width,
      200);
  }

  public bool IsTownNpcSlot(int npcIndex)
  {
    return _npcs.CreateSpawnSnapshots().Any(npc =>
      npc.SlotIndex == npcIndex && npc.IsTownNpc);
  }

  public float GetNpcCenterX(int npcIndex)
  {
    return _npcs.CreateSpawnSnapshots()
      .Where(npc => npc.SlotIndex == npcIndex)
      .Select(static npc => npc.Center.X)
      .DefaultIfEmpty(0f)
      .First();
  }

  public NpcSpawnPerPlayerFlagsPostlude CapturePerPlayerFlagsPostlude(int playerIndex)
  {
    RuntimePlayerEntity player = RequirePlayer(playerIndex);
    int tileX = (int)MathF.Floor((player.Movement.Position.X + 10f) / 16f);
    int tileY = (int)MathF.Floor((player.Movement.Position.Y + 21f) / 16f);
    TileCellState tile = ReadCell(tileX, tileY);
    return new NpcSpawnPerPlayerFlagsPostlude(
      PlayerTownNpcCount: CountNearbyTownNpcs(player),
      PlayerTileIsInWorld: IsInWorld(tileX, tileY),
      PlayerTileHasHouseWall: tile.Wall < RuntimeMain.wallHouse.Length &&
        RuntimeMain.wallHouse[tile.Wall],
      PlayerAfkCounter: 0,
      AfkTimeNeededForNoWormSpawns: 600,
      PlayerTileHasLightWall: false,
      PlayerTileWallType: tile.Wall,
      RemixWorld: HasSeed(WorldSecretSeedFlags.Remix),
      PlayerCenterX: player.Movement.Position.X + 10f,
      MaxTilesX: _session.Storage.TileMap.Width,
      ArmorSlot0ItemType: 0,
      ArmorSlot1ItemType: 0,
      PlayerMaximumLife: player.Vitals.EffectiveLifeMaximum);
  }

  public NpcSpawnRateInputs CaptureSpawnRateInputs(int playerIndex)
  {
    return CreateBaseRateInputs(playerIndex);
  }

  public NpcSpawnChosenTileWorldInputs CaptureChosenTileWorldInputs(
    int spawnTileX,
    int spawnTileY,
    int spawnTileType)
  {
    TileCellState tile = ReadCell(spawnTileX, spawnTileY);
    return new NpcSpawnChosenTileWorldInputs(
      DontStarveWorld: HasSeed(WorldSecretSeedFlags.DontStarve),
      WindSpeedTarget: _session.World.TimeWeather.WindTarget,
      OceanDistance: 250,
      BeachDistance: 250,
      SpawnTileIsSand: tile.Type is 53 or 112 or 116 or 234 or 397 or 398);
  }

  public NpcSpawnTileFacts ReadTile(int tileX, int tileY)
  {
    TileCellState tile = ReadCell(tileX, tileY);
    return new NpcSpawnTileFacts(
      tile.Type,
      tile.Wall,
      tile.LiquidAmount,
      tile.LiquidType);
  }

  public bool AllowsUndergroundDesertEnemiesToSpawn(int wallType) => false;

  public bool IsOceanDepths(int tileX, int tileY)
  {
    int width = _session.Storage.TileMap.Width;
    return (tileX < 250 || tileX >= width - 250) &&
      tileY > _session.World.Descriptor.RockLayer;
  }

  public void SpawnSlimeRainForPlayer(int playerIndex)
  {
    if (TickNumber % SlimeRainSpawnPeriod != 0 ||
        !_slimeRainPlayersSpawnedThisTick.Add(playerIndex))
    {
      return;
    }

    RuntimePlayerEntity? player = GetPlayer(playerIndex);
    if (player is null || player.Lifecycle.IsDead)
    {
      return;
    }

    float side = Next(2) == 0 ? -1f : 1f;
    TryCreateNpc(
      netId: SimulationContentSupportManifest.GreenSlimeNetId,
      player.Movement.Position + new Vector2(side * 160f, -24f));
  }

  public void ContinueSpawnAttempt(in NpcSpawnAcceptedCandidate candidate)
  {
    _ = ContinueSpawnAttemptObserved(in candidate);
  }

  public NpcSpawnCreationObservation ContinueSpawnAttemptObserved(
    in NpcSpawnAcceptedCandidate candidate)
  {
    if (candidate.RateResult.SpawnFriendly)
    {
      return NpcSpawnCreationObservation.NotCreated;
    }

    int netId = candidate.TileSearchResult.SkyMob
      ? SimulationContentSupportManifest.DemonEyeNetId
      : candidate.ChosenTileFlags.DayTime
        ? SimulationContentSupportManifest.GreenSlimeNetId
        : SimulationContentSupportManifest.ZombieNetId;
    bool created = TryCreateNpc(
      netId,
      new Vector2(
        candidate.TileSearchResult.TileX * 16f,
        candidate.TileSearchResult.TileY * 16f));
    return created
      ? NpcSpawnCreationObservation.Created
      : NpcSpawnCreationObservation.NotCreated;
  }

  private NpcSpawnRateInputs CreateBaseRateInputs(int playerIndex)
  {
    RuntimePlayerEntity player = RequirePlayer(playerIndex);
    int tileX = (int)MathF.Floor((player.Movement.Position.X + 10f) / 16f);
    int tileY = (int)MathF.Floor((player.Movement.Position.Y + 21f) / 16f);
    WorldTimeWeatherState time = _session.World.TimeWeather;
    WorldDescriptorState descriptor = _session.World.Descriptor;
    WorldRulesState rules = _session.World.Rules;
    bool remix = HasSeed(WorldSecretSeedFlags.Remix);
    bool journey = rules.GameMode == WorldGameMode.Journey;
    int activePlayers = _players.Players.Count(static current => !current.Lifecycle.IsDead);
    NpcSpawnContextSnapshot context = new(
      SpawnSpaceX: 3,
      SpawnSpaceY: 3,
      FairyLog: false,
      NumberOfActivePlayers: activePlayers,
      ReachedInvasionBossCap: false,
      PlayerTileX: tileX,
      PlayerTileY: tileY,
      Luck: 0f,
      DayTime: time.DayTime,
      Raining: time.Raining);
    var playerInputs = new NpcSpawnRatePlayerInputs(
      player.Movement.Position.Y,
      player.Movement.Position.Y + 21f,
      CountNearbyHostileNpcs(player),
      ZoneUndergroundDesert: false,
      IsInvisible: false,
      IsCalmed: false,
      HasSunflower: false,
      HasAnglerSetSpawnReduction: false,
      EnemySpawnsEnabled: false,
      HasNearbyFairy: false);
    var worldInputs = new NpcSpawnRateWorldInputs(
      DefaultSpawnRate,
      DefaultMaxSpawns,
      rules.HardMode,
      remix,
      descriptor.SurfaceLayer,
      descriptor.RockLayer,
      Math.Max(1, descriptor.SizeY - 200),
      ScreenHeightPixels,
      time.BloodMoon,
      time.PumpkinMoon,
      time.SnowMoon,
      time.Eclipse,
      time.RainStrength,
      HasSeed(WorldSecretSeedFlags.Drunk),
      PlayerTileHasDrunkWorldWall: false,
      WallOfFleshPresent: false,
      HasSeed(WorldSecretSeedFlags.ForTheWorthy),
      journey,
      SpawnRatePowerUnlocked: false,
      HasRemappedJourneySpawnRate: false,
      JourneySpawnRateValue: 1f,
      _session.World.Progression.Dd2.Ongoing,
      _session.World.Progression.Bosses.Boss3,
      SkyblockLowTiles: false,
      HasSeed(WorldSecretSeedFlags.Infected),
      rules.GameMode is WorldGameMode.Expert or WorldGameMode.Master);
    return new NpcSpawnRateInputs(
      context,
      default,
      new NpcSpawnSpatialEligibilitySnapshot(
        SurfaceSpawn: tileY <= descriptor.SurfaceLayer,
        SpawnUndergroundDesert: false,
        HardDungeon: false,
        DeeperThanRockLayer: tileY >= descriptor.RockLayer,
        UnderGround: tileY > descriptor.SurfaceLayer,
        IsOcean: false,
        IsBeach: false,
        SkyBehindPlayer: false,
        LivingTree: false,
        InRemixStartingArea: false),
      default,
      default,
      new NpcSpawnEventAndTowerEligibilitySnapshot(
        ZoneTowerSolar: _session.World.Progression.Lunar.SolarTowerActive,
        ZoneTowerVortex: _session.World.Progression.Lunar.VortexTowerActive,
        ZoneTowerNebula: _session.World.Progression.Lunar.NebulaTowerActive,
        ZoneTowerStardust: _session.World.Progression.Lunar.StardustTowerActive,
        ZoneOldOneArmy: _session.World.Progression.Dd2.Ongoing,
        ZoneWaterCandle: false,
        ZonePeaceCandle: false,
        ZoneShadowCandle: false),
      playerInputs,
      worldInputs);
  }

  private int CountNearbyHostileNpcs(RuntimePlayerEntity player)
  {
    const float radius = 800f;
    float radiusSquared = radius * radius;
    Vector2 center = player.Movement.Position + new Vector2(10f, 21f);
    return _npcs.CreateSpawnSnapshots().Count(npc =>
      npc.IsHostile && Vector2.DistanceSquared(center, npc.Center) <= radiusSquared);
  }

  private int CountNearbyTownNpcs(RuntimePlayerEntity player)
  {
    const float radius = 3000f;
    float radiusSquared = radius * radius;
    Vector2 center = player.Movement.Position + new Vector2(10f, 21f);
    return _npcs.CreateSpawnSnapshots().Count(npc =>
      npc.IsTownNpc && Vector2.DistanceSquared(center, npc.Center) <= radiusSquared);
  }

  private bool TryCreateNpc(int netId, Vector2 position)
  {
    if (!_content.Npcs.TryGetByNetId(netId, out _) ||
        !_npcs.TrySpawn(
          netId,
          position,
          _content,
          _session.World.Descriptor.WorldId,
          out _,
          isNaturallySpawned: true))
    {
      return false;
    }
    _createdNetIds.Add(netId);
    return true;
  }

  private bool HasSeed(WorldSecretSeedFlags seed)
  {
    return (_session.World.Rules.SecretSeeds & seed) != 0;
  }

  private RuntimePlayerEntity? GetPlayer(int playerIndex)
  {
    return _players.TryGetPlayerAtSlot(playerIndex, out RuntimePlayerEntity? player)
      ? player
      : null;
  }

  private RuntimePlayerEntity RequirePlayer(int playerIndex)
  {
    return GetPlayer(playerIndex) ??
      throw new ArgumentOutOfRangeException(nameof(playerIndex));
  }

  private TileCellState ReadCell(int x, int y)
  {
    return IsInWorld(x, y)
      ? _session.Storage.TileMap.GetTile(x, y)
      : default;
  }

  private bool IsInWorld(int x, int y)
  {
    return (uint)x < (uint)_session.Storage.TileMap.Width &&
      (uint)y < (uint)_session.Storage.TileMap.Height;
  }

  private static NSSLC.WorldGeneration.Utilities.UnifiedRandom GetRandom()
  {
    return RuntimeMain.rand ??
      throw new InvalidOperationException("The simulation random source was not initialized.");
  }
}
