namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class FishingConditionEvaluationContext
{
  public FishingConditionEvaluationContext(
    IFishingRandomSource random,
    bool common = false,
    bool uncommon = false,
    bool rare = false,
    bool veryRare = false,
    bool legendary = false,
    bool crate = false,
    bool junk = false,
    bool inLava = false,
    bool inHoney = false,
    int waterTilesCount = 0,
    int waterNeededToFish = 0,
    float waterQuality = 0,
    int chumsInWater = 0,
    int fishingLevel = 0,
    bool canFishInLava = false,
    float atmosphericValue = 0,
    int questFishType = 0,
    int fishingY = 0,
    int heightLevel = 0,
    int rolledEnemySpawn = 0,
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
    bool downedBoss3 = false,
    double? rockLayerY = null,
    bool? originalOcean = null)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (waterTilesCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(waterTilesCount));
    }

    if (waterNeededToFish < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(waterNeededToFish));
    }

    if (chumsInWater < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(chumsInWater));
    }

    if (fishingLevel < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(fishingLevel));
    }

    if (rolledEnemySpawn < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(rolledEnemySpawn));
    }

    Random = random;
    Common = common;
    Uncommon = uncommon;
    Rare = rare;
    VeryRare = veryRare;
    Legendary = legendary;
    Crate = crate;
    Junk = junk;
    InLava = inLava;
    InHoney = inHoney;
    WaterTilesCount = waterTilesCount;
    WaterNeededToFish = waterNeededToFish;
    WaterQuality = waterQuality;
    ChumsInWater = chumsInWater;
    FishingLevel = fishingLevel;
    CanFishInLava = canFishInLava;
    AtmosphericValue = atmosphericValue;
    QuestFishType = questFishType;
    FishingY = fishingY;
    HeightLevel = heightLevel;
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
    RockLayerY = rockLayerY;
    IsOriginalOcean = originalOcean;
  }

  public IFishingRandomSource Random { get; }

  public bool Common { get; }

  public bool Uncommon { get; }

  public bool Rare { get; }

  public bool VeryRare { get; }

  public bool Legendary { get; }

  public bool Crate { get; }

  public bool Junk { get; }

  public bool InLava { get; }

  public bool InHoney { get; }

  public int WaterTilesCount { get; }

  public int WaterNeededToFish { get; }

  public float WaterQuality { get; }

  public int ChumsInWater { get; }

  public int FishingLevel { get; }

  public bool CanFishInLava { get; }

  public float AtmosphericValue { get; }

  public int QuestFishType { get; }

  public int FishingY { get; }

  public int HeightLevel { get; }

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

  public double? RockLayerY { get; }

  public bool? IsOriginalOcean { get; }
}
