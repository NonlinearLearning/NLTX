namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class FishingAttemptConditionSnapshot
{
  public FishingAttemptConditionSnapshot(
    FishingRollClassification rollClassification,
    FishingEnvironmentSnapshot environment,
    FishingPowerSnapshot power,
    FishingWorldPredicateSnapshot world,
    int questFishType,
    int rolledEnemySpawn,
    bool rolledCorruption = false,
    bool rolledCrimson = false,
    bool rolledJungle = false,
    bool rolledSnow = false,
    bool rolledDesert = false,
    bool rolledInfectedDesert = false,
    bool rolledRemixOcean = false,
    bool isHardMode = false,
    bool remixWorld = false,
    bool bloodMoon = false,
    bool combatBookWasUsed = false,
    bool zoneDungeon = false,
    bool zoneBeach = false,
    bool zoneHallow = false,
    bool zoneGlowshroom = false,
    bool zoneDesert = false,
    bool zoneSnow = false,
    bool downedBoss3 = false)
  {
    ArgumentNullException.ThrowIfNull(rollClassification);
    ArgumentNullException.ThrowIfNull(environment);
    ArgumentNullException.ThrowIfNull(power);
    ArgumentNullException.ThrowIfNull(world);
    if (rolledEnemySpawn < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(rolledEnemySpawn));
    }

    RollClassification = rollClassification;
    Environment = environment;
    Power = power;
    World = world;
    QuestFishType = questFishType;
    RolledEnemySpawn = rolledEnemySpawn;
    RolledCorruption = rolledCorruption;
    RolledCrimson = rolledCrimson;
    RolledJungle = rolledJungle;
    RolledSnow = rolledSnow;
    RolledDesert = rolledDesert;
    RolledInfectedDesert = rolledInfectedDesert;
    RolledRemixOcean = rolledRemixOcean;
    IsHardMode = isHardMode;
    RemixWorld = remixWorld;
    BloodMoon = bloodMoon;
    CombatBookWasUsed = combatBookWasUsed;
    ZoneDungeon = zoneDungeon;
    ZoneBeach = zoneBeach;
    ZoneHallow = zoneHallow;
    ZoneGlowshroom = zoneGlowshroom;
    ZoneDesert = zoneDesert;
    ZoneSnow = zoneSnow;
    DownedBoss3 = downedBoss3;
  }

  public FishingRollClassification RollClassification { get; }

  public FishingEnvironmentSnapshot Environment { get; }

  public FishingPowerSnapshot Power { get; }

  public FishingWorldPredicateSnapshot World { get; }

  public int QuestFishType { get; }

  public int RolledEnemySpawn { get; }

  public bool RolledCorruption { get; }

  public bool RolledCrimson { get; }

  public bool RolledJungle { get; }

  public bool RolledSnow { get; }

  public bool RolledDesert { get; }

  public bool RolledInfectedDesert { get; }

  public bool RolledRemixOcean { get; }

  public bool IsHardMode { get; }

  public bool RemixWorld { get; }

  public bool BloodMoon { get; }

  public bool CombatBookWasUsed { get; }

  public bool ZoneDungeon { get; }

  public bool ZoneBeach { get; }

  public bool ZoneHallow { get; }

  public bool ZoneGlowshroom { get; }

  public bool ZoneDesert { get; }

  public bool ZoneSnow { get; }

  public bool DownedBoss3 { get; }

  public FishingConditionEvaluationContext CreateEvaluationContext(
    IFishingRandomSource random)
  {
    ArgumentNullException.ThrowIfNull(random);
    return new FishingConditionEvaluationContext(
      random,
      common: RollClassification.Common,
      uncommon: RollClassification.Uncommon,
      rare: RollClassification.Rare,
      veryRare: RollClassification.VeryRare,
      legendary: RollClassification.Legendary,
      crate: RollClassification.Crate,
      junk: RollClassification.Junk,
      inLava: Environment.InLava,
      inHoney: Environment.InHoney,
      waterTilesCount: Environment.WaterTilesCount,
      waterNeededToFish: Environment.WaterNeededToFish,
      waterQuality: Environment.WaterQuality,
      chumsInWater: Environment.ChumsInWater,
      fishingLevel: Power.FishingLevel,
      canFishInLava: Power.CanFishInLava,
      atmosphericValue: World.AtmosphericValue,
      questFishType: QuestFishType,
      fishingY: World.FishingY,
      heightLevel: World.HeightLevel,
      rolledEnemySpawn: RolledEnemySpawn,
      rolledCorruption: RolledCorruption,
      rolledCrimson: RolledCrimson,
      rolledJungle: RolledJungle,
      rolledSnow: RolledSnow,
      rolledDesert: RolledDesert,
      rolledInfectedDesert: RolledInfectedDesert,
      rolledRemixOcean: RolledRemixOcean,
      isHardMode: IsHardMode,
      remixWorld: RemixWorld,
      bloodMoon: BloodMoon,
      combatBookWasUsed: CombatBookWasUsed,
      zoneDungeon: ZoneDungeon,
      zoneBeach: ZoneBeach,
      zoneHallow: ZoneHallow,
      zoneGlowshroom: ZoneGlowshroom,
      zoneDesert: ZoneDesert,
      zoneSnow: ZoneSnow,
      downedBoss3: DownedBoss3,
      rockLayerY: World.RockLayerY,
      originalOcean: World.IsOriginalOcean);
  }
}
