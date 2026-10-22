using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// A bounded progression and weather prefix from a pointer-based WorldFile header.
/// </summary>
/// <remarks>
/// The values are persisted facts. This DTO does not apply derived fixes, update runtime
/// globals, or rebuild any owner store. Fields that did not exist in an older file version use
/// their documented default values during decoding.
/// </remarks>
public sealed class WorldFileProgressionSection
{
  public const string SectionId = "world.progression";

  public WorldFileProgressionSection(
    bool downedBoss1,
    bool downedBoss2,
    bool downedBoss3,
    bool downedQueenBee,
    bool downedMechBoss1,
    bool downedMechBoss2,
    bool downedMechBoss3,
    bool downedMechBossAny,
    bool downedPlantBoss,
    bool downedGolemBoss,
    bool downedSlimeKing,
    bool savedGoblin,
    bool savedWizard,
    bool savedMech,
    bool downedGoblins,
    bool downedClown,
    bool downedFrost,
    bool downedPirates,
    bool shadowOrbSmashed,
    bool spawnMeteor,
    byte shadowOrbCount,
    int altarCount,
    bool hardMode,
    bool afterPartyOfDoom,
    int invasionDelay,
    int invasionSize,
    int invasionType,
    double invasionX,
    double slimeRainTime,
    byte sundialCooldown,
    bool raining,
    int rainTime,
    float maxRain,
    int cobaltOreTier,
    int mythrilOreTier,
    int adamantiteOreTier,
    IReadOnlyList<byte> backgroundStyles,
    int cloudBackgroundActive,
    short cloudCount,
    float windSpeedTarget)
  {
    BackgroundStyles = CopyFixed(backgroundStyles, 8, nameof(backgroundStyles));
    if (!double.IsFinite(invasionX))
    {
      throw new ArgumentOutOfRangeException(nameof(invasionX));
    }

    if (!double.IsFinite(slimeRainTime))
    {
      throw new ArgumentOutOfRangeException(nameof(slimeRainTime));
    }

    if (!float.IsFinite(maxRain))
    {
      throw new ArgumentOutOfRangeException(nameof(maxRain));
    }

    if (!float.IsFinite(windSpeedTarget))
    {
      throw new ArgumentOutOfRangeException(nameof(windSpeedTarget));
    }

    DownedBoss1 = downedBoss1;
    DownedBoss2 = downedBoss2;
    DownedBoss3 = downedBoss3;
    DownedQueenBee = downedQueenBee;
    DownedMechBoss1 = downedMechBoss1;
    DownedMechBoss2 = downedMechBoss2;
    DownedMechBoss3 = downedMechBoss3;
    DownedMechBossAny = downedMechBossAny;
    DownedPlantBoss = downedPlantBoss;
    DownedGolemBoss = downedGolemBoss;
    DownedSlimeKing = downedSlimeKing;
    SavedGoblin = savedGoblin;
    SavedWizard = savedWizard;
    SavedMech = savedMech;
    DownedGoblins = downedGoblins;
    DownedClown = downedClown;
    DownedFrost = downedFrost;
    DownedPirates = downedPirates;
    ShadowOrbSmashed = shadowOrbSmashed;
    SpawnMeteor = spawnMeteor;
    ShadowOrbCount = shadowOrbCount;
    AltarCount = altarCount;
    HardMode = hardMode;
    AfterPartyOfDoom = afterPartyOfDoom;
    InvasionDelay = invasionDelay;
    InvasionSize = invasionSize;
    InvasionType = invasionType;
    InvasionX = invasionX;
    SlimeRainTime = slimeRainTime;
    SundialCooldown = sundialCooldown;
    Raining = raining;
    RainTime = rainTime;
    MaxRain = maxRain;
    CobaltOreTier = cobaltOreTier;
    MythrilOreTier = mythrilOreTier;
    AdamantiteOreTier = adamantiteOreTier;
    CloudBackgroundActive = cloudBackgroundActive;
    CloudCount = cloudCount;
    WindSpeedTarget = windSpeedTarget;
  }

  public bool DownedBoss1 { get; }

  public bool DownedBoss2 { get; }

  public bool DownedBoss3 { get; }

  public bool DownedQueenBee { get; }

  public bool DownedMechBoss1 { get; }

  public bool DownedMechBoss2 { get; }

  public bool DownedMechBoss3 { get; }

  public bool DownedMechBossAny { get; }

  public bool DownedPlantBoss { get; }

  public bool DownedGolemBoss { get; }

  public bool DownedSlimeKing { get; }

  public bool SavedGoblin { get; }

  public bool SavedWizard { get; }

  public bool SavedMech { get; }

  public bool DownedGoblins { get; }

  public bool DownedClown { get; }

  public bool DownedFrost { get; }

  public bool DownedPirates { get; }

  public bool ShadowOrbSmashed { get; }

  public bool SpawnMeteor { get; }

  public byte ShadowOrbCount { get; }

  public int AltarCount { get; }

  public bool HardMode { get; }

  public bool AfterPartyOfDoom { get; }

  public int InvasionDelay { get; }

  public int InvasionSize { get; }

  public int InvasionType { get; }

  public double InvasionX { get; }

  public double SlimeRainTime { get; }

  public byte SundialCooldown { get; }

  public bool Raining { get; }

  public int RainTime { get; }

  public float MaxRain { get; }

  public int CobaltOreTier { get; }

  public int MythrilOreTier { get; }

  public int AdamantiteOreTier { get; }

  public IReadOnlyList<byte> BackgroundStyles { get; }

  public int CloudBackgroundActive { get; }

  public short CloudCount { get; }

  public float WindSpeedTarget { get; }

  public static WorldFileProgressionSection Empty => new(
    downedBoss1: false,
    downedBoss2: false,
    downedBoss3: false,
    downedQueenBee: false,
    downedMechBoss1: false,
    downedMechBoss2: false,
    downedMechBoss3: false,
    downedMechBossAny: false,
    downedPlantBoss: false,
    downedGolemBoss: false,
    downedSlimeKing: false,
    savedGoblin: false,
    savedWizard: false,
    savedMech: false,
    downedGoblins: false,
    downedClown: false,
    downedFrost: false,
    downedPirates: false,
    shadowOrbSmashed: false,
    spawnMeteor: false,
    shadowOrbCount: 0,
    altarCount: 0,
    hardMode: false,
    afterPartyOfDoom: false,
    invasionDelay: 0,
    invasionSize: 0,
    invasionType: 0,
    invasionX: 0,
    slimeRainTime: 0,
    sundialCooldown: 0,
    raining: false,
    rainTime: 0,
    maxRain: 0,
    cobaltOreTier: -1,
    mythrilOreTier: -1,
    adamantiteOreTier: -1,
    backgroundStyles: new byte[8],
    cloudBackgroundActive: 0,
    cloudCount: 0,
    windSpeedTarget: 0);

  private static IReadOnlyList<byte> CopyFixed(
    IReadOnlyList<byte> values,
    int expectedCount,
    string parameterName)
  {
    ArgumentNullException.ThrowIfNull(values);
    if (values.Count != expectedCount)
    {
      throw new ArgumentException(
        $"The progression section requires exactly {expectedCount} values.",
        parameterName);
    }

    return Array.AsReadOnly(new List<byte>(values).ToArray());
  }
}
