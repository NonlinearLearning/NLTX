using System;
using System.Collections.Generic;
using Terraria.Npc;

internal static class NpcSpawnBranchSelectionCoreChecks
{
  public static void Run()
  {
    VerifyLegacyWallClassification();
    VerifyTowerBranchDispatchPrecedence();
    VerifySkyMobBranchPrecedesInvasion();
    VerifyUnknownInvasionDispatchEarlyReturn();
    VerifyUnmodeledBranchBarrier();
    VerifyGraveyardBranchPrecedesType244();
    VerifyType244BranchDispatchAndHandledWithoutRequest();
    VerifyUnknownInvasionTypeEarlyReturn();
    VerifyMartianSaucerPositionAndSolidArea();
    VerifyPirateCaptainPriorityAndStartIndex();
    VerifySkyMobWaterCandleReroll();
    VerifyStatueMimicRequestAndEffectOrder();
    VerifyDualDungeonFallbackAfterStatueMimicGate();
    VerifyWaterCritterRequest();
    VerifyUndergroundCritterCanBeSuppressed();
    VerifyGnomeRequestCarriesPostSpawnMultiplier();
    Console.WriteLine("PASS: NPC spawn branch selection core cases");
  }

  public static void RunDispatchCore()
  {
    VerifyLegacyWallClassification();
    VerifyTowerBranchDispatchPrecedence();
    VerifySkyMobBranchPrecedesInvasion();
    VerifyUnknownInvasionDispatchEarlyReturn();
    VerifyUnmodeledBranchBarrier();
    VerifyGraveyardBranchPrecedesType244();
    VerifyType244BranchDispatchAndHandledWithoutRequest();
    Console.WriteLine("PASS: NPC spawn branch dispatch core cases");
  }

  private static void VerifyLegacyWallClassification()
  {
    NpcSpawnLegacyWallClassificationFacts facts = new(
      WallAboveSpawnTile: 17,
      WallTwoTilesAboveSpawnTile: 244,
      WallOnSpawnTile: 31);

    int wallType = NpcSpawnLegacyWallClassificationQuery.ResolveLegacyWallType(in facts);

    Assert(wallType == 244, "A living-tree wall two tiles above must override legacy wall type.");
    facts = facts with { WallTwoTilesAboveSpawnTile = 18, WallOnSpawnTile = 244 };
    wallType = NpcSpawnLegacyWallClassificationQuery.ResolveLegacyWallType(in facts);
    Assert(wallType == 244, "A living-tree wall on the spawn tile must override legacy wall type.");
    facts = facts with { WallOnSpawnTile = 31 };
    wallType = NpcSpawnLegacyWallClassificationQuery.ResolveLegacyWallType(in facts);
    Assert(wallType == 17, "Otherwise legacy wall type must come from one tile above.");
  }

  private static void VerifyTowerBranchDispatchPrecedence()
  {
    RecordingPort port = new(nextValues: [0]);
    port.ActiveNpcCount = 0;
    NpcSpawnBranchSelectionInputs inputs = CreateDispatchInputs(
      towerNebula: true,
      skyMob: true,
      invaders: true,
      invasionType: 5,
      earlierBranches: NpcSpawnEarlierLegacyBranchDisposition.NoBranchMatched);

    NpcSpawnBranchSelectionResult result = NpcSpawnBranchSelectionSystem.Select(
      in inputs,
      port,
      port,
      port,
      port,
      port);

    Assert(result.HasRequest, "Tower precedence must produce the first selected request.");
    Assert(result.SelectedBranch == NpcSpawnBranchKind.Tower,
      "Tower zones must run before SkyMob and Invasion selectors.");
    Assert(result.SpawnRequest!.Value.Type == new NpcTypeId(424),
      "The tower result must preserve the selected tower NPC type.");
    Assert(port.Calls.SequenceEqual(["Next(11)", "CountNPCS(424)"]),
      "Tower selection must short-circuit all lower-priority selector effects.");
  }

  private static void VerifyUnknownInvasionDispatchEarlyReturn()
  {
    RecordingPort port = new();
    NpcSpawnBranchSelectionInputs inputs = CreateDispatchInputs(
      invaders: true,
      invasionType: 5,
      earlierBranches: NpcSpawnEarlierLegacyBranchDisposition.NoBranchMatched,
      legacyWallFacts: new NpcSpawnLegacyWallClassificationFacts(1, 244, 1),
      critter: CreateCritterInputs(spawnTileY: 101));

    NpcSpawnBranchSelectionResult result = NpcSpawnBranchSelectionSystem.Select(
      in inputs,
      port,
      port,
      port,
      port,
      port);

    Assert(
      result.Status == NpcSpawnBranchSelectionStatus.UnknownInvasionTypeEarlyReturn &&
        result.SelectedBranch == NpcSpawnBranchKind.Invasion,
      "Unknown invasion types must preserve the early return through branch dispatch.");
    Assert(port.Calls.Count == 0,
      "An unknown invasion must stop before graveyard or type 244 selector effects.");
  }

  private static void VerifySkyMobBranchPrecedesInvasion()
  {
    RecordingPort port = new();
    NpcSpawnBranchSelectionInputs inputs = CreateDispatchInputs(
      skyMob: true,
      invaders: true,
      invasionType: 4,
      earlierBranches: NpcSpawnEarlierLegacyBranchDisposition.NoBranchMatched);

    NpcSpawnBranchSelectionResult result = NpcSpawnBranchSelectionSystem.Select(
      in inputs,
      port,
      port,
      port,
      port,
      port);

    Assert(result.HasRequest && result.SelectedBranch == NpcSpawnBranchKind.SkyMob,
      "The sky branch must be selected before the invasion selector.");
    Assert(result.SpawnRequest!.Value.Type == new NpcTypeId(388),
      "The sky invasion branch must retain NPC type 388.");
    Assert(port.Calls.Count == 0,
      "The sky invasion branch must not fall through to the pirate invasion selector.");
  }

  private static void VerifyUnmodeledBranchBarrier()
  {
    RecordingPort port = new(nextValues: [0]);
    NpcSpawnBranchSelectionInputs inputs = CreateDispatchInputs(
      legacyWallFacts: new NpcSpawnLegacyWallClassificationFacts(1, 244, 1),
      critter: CreateCritterInputs(spawnTileY: 101));

    NpcSpawnBranchSelectionResult result = NpcSpawnBranchSelectionSystem.Select(
      in inputs,
      port,
      port,
      port,
      port,
      port);

    Assert(
      result.Status == NpcSpawnBranchSelectionStatus.EarlierUnmodeledBranchesUnresolved &&
        result.RequiresLegacyContinuation,
      "The tail must remain unresolved until earlier legacy branches are evaluated.");
    Assert(port.Calls.Count == 0,
      "The unresolved precedence barrier must not consume lower-priority effects.");
  }

  private static void VerifyType244BranchDispatchAndHandledWithoutRequest()
  {
    RecordingPort port = new(nextValues: [1, 1, 0]) { LuckRollValue = 1 };
    NpcSpawnBranchSelectionInputs inputs = CreateDispatchInputs(
      earlierBranches: NpcSpawnEarlierLegacyBranchDisposition.NoBranchMatched,
      legacyWallFacts: new NpcSpawnLegacyWallClassificationFacts(17, 18, 244),
      critter: CreateCritterInputs(spawnTileY: 101));

    NpcSpawnBranchSelectionResult result = NpcSpawnBranchSelectionSystem.Select(
      in inputs,
      port,
      port,
      port,
      port,
      port);

    string diagnostic =
      $"Actual result: {result.Status}/{result.SelectedBranch}; {string.Join(";", port.Calls)}.";
    Assert(
      result.Status == NpcSpawnBranchSelectionStatus.HandledWithoutRequest &&
        result.SelectedBranch == NpcSpawnBranchKind.Type244Critter &&
        !result.HasRequest,
      "A selected type 244 branch with no critter roll must stop the modeled chain without a " +
        diagnostic);
    Assert(
      port.Calls.SequenceEqual(["Next(3)", "Next(2)", "RollLuck(7)", "Next(3)"]),
      "Type 244 dispatch must preserve the underground roll order and short-circuit.");
  }

  private static void VerifyGraveyardBranchPrecedesType244()
  {
    RecordingPort port = new();
    NpcSpawnBranchSelectionInputs inputs = CreateDispatchInputs(
      earlierBranches: NpcSpawnEarlierLegacyBranchDisposition.NoBranchMatched,
      legacyWallFacts: new NpcSpawnLegacyWallClassificationFacts(17, 18, 244),
      downedBoss3: true,
      zoneGraveyard: true);

    NpcSpawnBranchSelectionResult result = NpcSpawnBranchSelectionSystem.Select(
      in inputs,
      port,
      port,
      port,
      port,
      port);

    Assert(result.HasRequest && result.SelectedBranch == NpcSpawnBranchKind.StatueMimic,
      "The graveyard branch must take precedence over a later type 244 match.");
    Assert(result.SpawnRequest!.Value.Type == new NpcTypeId(690),
      "The graveyard branch must preserve the Statue Mimic request.");
    Assert(port.Calls.SequenceEqual(
      ["RollBadLuckExtreme(25)", "AnyNPCs(690)", "IsGoodPlaceForStatueMimic(100,200)"]),
      "A successful Statue Mimic branch must stop before dual-dungeon and type 244 effects.");
  }

  private static NpcSpawnBranchSelectionInputs CreateDispatchInputs(
    bool towerNebula = false,
    bool skyMob = false,
    bool invaders = false,
    int invasionType = 0,
    NpcSpawnEarlierLegacyBranchDisposition earlierBranches =
      NpcSpawnEarlierLegacyBranchDisposition.Unknown,
    NpcSpawnLegacyWallClassificationFacts legacyWallFacts = default,
    NpcSpawnCritterSelectionInputs? critter = null,
    bool downedBoss3 = false,
    bool zoneGraveyard = false,
    bool noWorms = false,
    bool trespassingDualDungeon = false,
    bool hardMode = false)
  {
    NpcSpawnEventAndTowerEligibilitySnapshot towerSnapshot = default;
    towerSnapshot = towerSnapshot with { ZoneTowerNebula = towerNebula };
    NpcSpawnAcceptedCandidate candidate = CreateCandidate(
      tileX: 100,
      tileY: 200,
      downedBoss3: downedBoss3,
      zoneGraveyard: zoneGraveyard,
      noWorms: noWorms,
      trespassingDualDungeon: trespassingDualDungeon,
      hardMode: hardMode);
    NpcSpawnTileSearchResult tile = candidate.TileSearchResult with { SkyMob = skyMob };
    candidate = candidate with { TileSearchResult = tile };

    return new NpcSpawnBranchSelectionInputs(
      Candidate: candidate,
      Tower: new NpcSpawnTowerSelectionInputs(towerSnapshot, 0, 0),
      SkyMob: new NpcSpawnSkyMobSelectionInputs(
        IsSkyMob: false,
        SpawnTileX: 0,
        SpawnTileY: 0,
        MaxTilesX: 2000,
        SkyBehindPlayer: false,
        ZoneWaterCandle: false,
        Invaders: false,
        InvasionType: 0,
        HardMode: false,
        DownedGolemBoss: false,
        DownedMartians: false,
        NoWorms: true,
        UnlockedSlimePurpleSpawn: true),
      Invasion: new NpcSpawnInvasionSelectionInputs(
        Invaders: invaders,
        InvasionType: invasionType,
        SpawnTileX: 0,
        SpawnTileY: 0,
        HardMode: false,
        InvasionSize: 0,
        InvasionSizeStart: 100),
      LegacyWallFacts: legacyWallFacts,
      Critter: critter ?? CreateCritterInputs(),
      EarlierUnmodeledBranches: earlierBranches);
  }

  private static NpcSpawnCritterSelectionInputs CreateCritterInputs(int spawnTileY = 200)
  {
    return new NpcSpawnCritterSelectionInputs(
      RemixWorld: false,
      WaterTile: false,
      SpawnTileX: 0,
      SpawnTileY: spawnTileY,
      WorldSurface: 100,
      GoldCritterChance: 7,
      GnomeChance: 0,
      Halloween: false,
      Christmas: false,
      BirthdayPartyActive: false);
  }

  private static void VerifyUnknownInvasionTypeEarlyReturn()
  {
    RecordingPort port = new();
    NpcSpawnInvasionSelectionInputs inputs = new(
      Invaders: true,
      InvasionType: 5,
      SpawnTileX: 100,
      SpawnTileY: 200,
      HardMode: false,
      InvasionSize: 0,
      InvasionSizeStart: 100);

    NpcSpawnInvasionSelectionResult result =
      NpcSpawnInvasionSelectionSystem.Select(in inputs, port);

    Assert(
      result.Status == NpcSpawnInvasionSelectionStatus.UnknownInvasionTypeEarlyReturn,
      "Unknown invasion type must preserve the early return.");
    Assert(port.Calls.Count == 0, "Unknown invasion types must not consume random or query ports.");
  }

  private static void VerifyMartianSaucerPositionAndSolidArea()
  {
    RecordingPort port = new(nextValues: [0]);
    NpcSpawnInvasionSelectionInputs inputs = new(
      Invaders: true,
      InvasionType: 3,
      SpawnTileX: 100,
      SpawnTileY: 200,
      HardMode: false,
      InvasionSize: 10,
      InvasionSizeStart: 100);

    NpcSpawnInvasionSelectionResult result =
      NpcSpawnInvasionSelectionSystem.Select(in inputs, port);

    Assert(result.HasRequest, "The eligible Martian Saucer branch must produce a request.");
    NpcSpawnEntityRequest request = result.SpawnRequest!.Value;
    Assert(request.Type == new NpcTypeId(491), "The request must select Martian Saucer type 491.");
    Assert(request.PositionX == 1608, "The Martian Saucer X position must retain its tile center.");
    Assert(request.PositionY == 3040, "The Martian Saucer must spawn 10 tiles above the candidate.");
    Assert(
      port.Calls.SequenceEqual(
      [
        "Next(20)",
        "AnyNPCs(491)",
        "SolidTiles(80,120,160,190)",
      ]),
      "The Martian Saucer gates and inclusive SolidTiles bounds must retain source order.");
  }

  private static void VerifyPirateCaptainPriorityAndStartIndex()
  {
    RecordingPort port = new(nextValues: [6, 0]);
    NpcSpawnInvasionSelectionInputs inputs = new(
      Invaders: true,
      InvasionType: 4,
      SpawnTileX: 30,
      SpawnTileY: 40,
      HardMode: false,
      InvasionSize: 60,
      InvasionSizeStart: 100);

    NpcSpawnInvasionSelectionResult result =
      NpcSpawnInvasionSelectionSystem.Select(in inputs, port);

    Assert(result.HasRequest, "The Pirate Captain branch must produce a request.");
    NpcSpawnEntityRequest request = result.SpawnRequest!.Value;
    Assert(request.Type == new NpcTypeId(395), "The eligible branch must select Pirate Captain.");
    Assert(request.StartIndex == 1, "Pirate invasion requests must preserve StartIndex 1.");
    Assert(
      port.Calls.SequenceEqual(["Next(7)", "AnyNPCs(395)", "Next(45)"]),
      "Pirate Captain presence and priority rolls must retain source order.");
  }

  private static void VerifySkyMobWaterCandleReroll()
  {
    RecordingPort port = new(nextValues: [1, 0]);
    NpcSpawnSkyMobSelectionInputs inputs = new(
      IsSkyMob: true,
      SpawnTileX: 1500,
      SpawnTileY: 700,
      MaxTilesX: 2000,
      SkyBehindPlayer: true,
      ZoneWaterCandle: true,
      Invaders: false,
      InvasionType: 0,
      HardMode: true,
      DownedGolemBoss: true,
      DownedMartians: true,
      NoWorms: false,
      UnlockedSlimePurpleSpawn: true);

    NpcSpawnSkyMobSelectionResult result =
      NpcSpawnSkyMobSelectionSystem.Select(in inputs, port);

    Assert(result.HasRequest, "The second Water Candle roll must be allowed to select the saucer.");
    Assert(
      result.SpawnRequest!.Value.Type == new NpcTypeId(399),
      "The retried Martian branch must select the saucer.");
    Assert(
      port.Calls.SequenceEqual(["AnyDanger", "Next(10)", "Next(10)", "AnyNPCs(399)"]),
      "Water Candle duplicate conditions must keep their second random draw.");
  }

  private static void VerifyStatueMimicRequestAndEffectOrder()
  {
    RecordingPort port = new();
    NpcSpawnAcceptedCandidate candidate = CreateCandidate(
      tileX: 100,
      tileY: 200,
      downedBoss3: true,
      zoneGraveyard: true,
      noWorms: false,
      trespassingDualDungeon: true,
      hardMode: false);

    NpcSpawnGraveyardDualDungeonSelectionResult result =
      NpcSpawnGraveyardDualDungeonSelectionSystem.Select(in candidate, port);

    Assert(result.HasRequest, "The eligible graveyard branch must produce a request.");
    NpcSpawnEntityRequest request = result.SpawnRequest!.Value;
    Assert(request.Type == new NpcTypeId(690), "The graveyard branch must select Statue Mimic.");
    Assert(request.PositionX == 1602, "The Statue Mimic request must retain its two-pixel offset.");
    Assert(request.PositionY == 3200, "The Statue Mimic request must use the candidate tile Y.");
    Assert(
      port.Calls.SequenceEqual(
      [
        "RollBadLuckExtreme(25)",
        "AnyNPCs(690)",
        "IsGoodPlaceForStatueMimic(100,200)",
      ]),
      "The Statue Mimic gates must preserve source order and short-circuit the next branch.");
  }

  private static void VerifyDualDungeonFallbackAfterStatueMimicGate()
  {
    RecordingPort port = new();
    port.ActiveNpcTypes.Add(690);
    NpcSpawnAcceptedCandidate candidate = CreateCandidate(
      tileX: 100,
      tileY: 200,
      downedBoss3: true,
      zoneGraveyard: true,
      noWorms: false,
      trespassingDualDungeon: true,
      hardMode: true);

    NpcSpawnGraveyardDualDungeonSelectionResult result =
      NpcSpawnGraveyardDualDungeonSelectionSystem.Select(in candidate, port);

    Assert(result.HasRequest, "The dual-dungeon branch must follow a rejected Statue Mimic gate.");
    Assert(
      result.SpawnRequest!.Value.Type == new NpcTypeId(82),
      "Hardmode dual-dungeon requests must select NPC type 82.");
    Assert(
      port.Calls.SequenceEqual(
      ["RollBadLuckExtreme(25)", "AnyNPCs(690)", "RollBadLuck(15)"]),
      "The dual-dungeon roll must run after the Statue Mimic presence gate.");
  }

  private static void VerifyWaterCritterRequest()
  {
    RecordingPort port = new() { LuckRollValue = 0 };
    NpcSpawnCritterSelectionInputs inputs = new(
      RemixWorld: false,
      WaterTile: true,
      SpawnTileX: 10,
      SpawnTileY: 20,
      WorldSurface: 100,
      GoldCritterChance: 8,
      GnomeChance: 0,
      Halloween: false,
      Christmas: false,
      BirthdayPartyActive: false);

    NpcSpawnCritterSelectionResult result = NpcSpawnCritterSelectionSystem.Select(
      in inputs,
      port);

    Assert(result.HasRequest, "A water critter attempt must produce a request.");
    NpcSpawnEntityRequest request = result.SpawnRequest!.Value;
    Assert(request.Type == new NpcTypeId(592), "A successful water gold roll must select type 592.");
    Assert(request.PositionX == 168 && request.PositionY == 320,
      "Water critters must use the centered tile X and unshifted tile Y.");
    Assert(request.StartIndex == 0 && request.Target == 255,
      "Water critter requests must preserve SpawnNPC's optional argument defaults.");
    Assert(port.Calls.SequenceEqual(["RollLuck(8)"]),
      "The water branch must consume only its gold critter roll.");
  }

  private static void VerifyUndergroundCritterCanBeSuppressed()
  {
    RecordingPort port = new(nextValues: [1, 1, 0]) { LuckRollValue = 1 };
    NpcSpawnCritterSelectionInputs inputs = new(
      RemixWorld: false,
      WaterTile: false,
      SpawnTileX: 10,
      SpawnTileY: 101,
      WorldSurface: 100,
      GoldCritterChance: 7,
      GnomeChance: 0,
      Halloween: false,
      Christmas: false,
      BirthdayPartyActive: false);

    NpcSpawnCritterSelectionResult result = NpcSpawnCritterSelectionSystem.Select(
      in inputs,
      port);

    Assert(
      result.Status == NpcSpawnCritterSelectionStatus.HandledWithoutRequest &&
        !result.HasRequest,
      "The final underground random failure must preserve the legacy no-spawn outcome.");
    Assert(
      port.Calls.SequenceEqual(["Next(3)", "Next(2)", "RollLuck(7)", "Next(3)"]),
      "The underground path must preserve its ordered short-circuit draws.");
  }

  private static void VerifyGnomeRequestCarriesPostSpawnMultiplier()
  {
    RecordingPort port = new() { LuckRollValue = 0 };
    NpcSpawnCritterSelectionInputs inputs = new(
      RemixWorld: false,
      WaterTile: false,
      SpawnTileX: 10,
      SpawnTileY: 50,
      WorldSurface: 100,
      GoldCritterChance: 7,
      GnomeChance: 29,
      Halloween: true,
      Christmas: true,
      BirthdayPartyActive: true);

    NpcSpawnCritterSelectionResult result = NpcSpawnCritterSelectionSystem.Select(
      in inputs,
      port);

    Assert(result.HasRequest, "A successful gnome roll must produce a request.");
    Assert(
      result.SpawnRequest!.Value.Type == new NpcTypeId(624),
      "The gnome branch must request NPC type 624.");
    Assert(
      result.PostSpawnTimeLeftMultiplier == 10,
      "The gnome result must carry the legacy post-spawn timeLeft multiplier.");
    Assert(port.Calls.SequenceEqual(["RollLuck(3)"]),
      "A successful gnome roll must short-circuit all later critter choices.");
  }

  private static NpcSpawnAcceptedCandidate CreateCandidate(
    int tileX,
    int tileY,
    bool downedBoss3,
    bool zoneGraveyard,
    bool noWorms,
    bool trespassingDualDungeon,
    bool hardMode)
  {
    NpcSpawnRateWorldInputs worldInputs = default;
    worldInputs = worldInputs with
    {
      DownedBoss3 = downedBoss3,
      HardMode = hardMode,
    };

    NpcSpawnBiomeZoneEligibilitySnapshot biomeZones = default;
    biomeZones = biomeZones with { ZoneGraveyard = zoneGraveyard };

    NpcSpawnBiomeAndDungeonEligibilitySnapshot biomeAndDungeon = default;
    biomeAndDungeon = biomeAndDungeon with
    {
      TresspassingDualDungeon = trespassingDualDungeon,
    };

    NpcSpawnRateInputs rateInputs = default;
    rateInputs = rateInputs with
    {
      World = worldInputs,
      BiomeZones = biomeZones,
      BiomeAndDungeon = biomeAndDungeon,
    };

    NpcSpawnChosenTileFlagsResult chosenTileFlags = default;
    chosenTileFlags = chosenTileFlags with { NoWorms = noWorms };

    return new NpcSpawnAcceptedCandidate(
      PlayerIndex: 0,
      RateInputs: rateInputs,
      RateResult: default,
      TileSearchResult: new NpcSpawnTileSearchResult(
        Found: true,
        TileX: tileX,
        TileY: tileY,
        XRange: false,
        SkyMob: false,
        Area: default),
      PostCheckInputs: default,
      ChosenTileFlags: chosenTileFlags);
  }

  private static void Assert(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private sealed class RecordingPort :
    INpcSpawnTowerSelectionPort,
    INpcSpawnInvasionSelectionPort,
    INpcSpawnSkyMobSelectionPort,
    INpcSpawnGraveyardDualDungeonSelectionPort,
    INpcSpawnCritterSelectionPort
  {
    private readonly Queue<int> _nextValues;

    public RecordingPort(IReadOnlyList<int>? nextValues = null)
    {
      _nextValues = new Queue<int>(nextValues ?? Array.Empty<int>());
    }

    public List<string> Calls { get; } = [];

    public bool AnyDangerValue { get; set; }

    public bool HasSolidTilesValue { get; set; }

    public int LuckRollValue { get; set; }

    public int BadLuckExtremeValue { get; set; }

    public int BadLuckValue { get; set; }

    public bool StatueMimicGoodPlaceValue { get; set; } = true;

    public int ActiveNpcCount { get; set; }

    public HashSet<int> ActiveNpcTypes { get; } = [];

    public int Next(int exclusiveUpperBound)
    {
      Calls.Add($"Next({exclusiveUpperBound})");
      if (_nextValues.Count == 0)
      {
        throw new InvalidOperationException("No random value was configured for the request.");
      }

      return _nextValues.Dequeue();
    }

    public int CountActiveNpcs(int npcTypeId)
    {
      Calls.Add($"CountNPCS({npcTypeId})");
      return ActiveNpcCount;
    }

    public bool AnyDanger()
    {
      Calls.Add("AnyDanger");
      return AnyDangerValue;
    }

    public bool HasActiveNpc(int npcTypeId)
    {
      Calls.Add($"AnyNPCs({npcTypeId})");
      return ActiveNpcTypes.Contains(npcTypeId);
    }

    public bool HasAnySolidTiles(
      int startXInclusive,
      int endXInclusive,
      int startYInclusive,
      int endYInclusive)
    {
      Calls.Add(
        $"SolidTiles({startXInclusive},{endXInclusive},{startYInclusive},{endYInclusive})");
      return HasSolidTilesValue;
    }

    public int RollLuck(int range)
    {
      Calls.Add($"RollLuck({range})");
      return LuckRollValue;
    }

    public int RollBadLuckExtreme(int denominator)
    {
      Calls.Add($"RollBadLuckExtreme({denominator})");
      return BadLuckExtremeValue;
    }

    public bool IsGoodPlaceForStatueMimic(int tileX, int tileY)
    {
      Calls.Add($"IsGoodPlaceForStatueMimic({tileX},{tileY})");
      return StatueMimicGoodPlaceValue;
    }

    public int RollBadLuck(int denominator)
    {
      Calls.Add($"RollBadLuck({denominator})");
      return BadLuckValue;
    }
  }
}
