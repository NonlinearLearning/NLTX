using MinimalImplementation.Contracts;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Modules.Events;

public sealed record InvasionSnapshot(
    long WorldScope, CommitVersion Version, int Type, int Size, int SizeStart,
    int Delay, double X, int WarningCountdown) {
  public WorldFileProgressionSection ToPersistenceSection(WorldFileProgressionSection existing) {
    ArgumentNullException.ThrowIfNull(existing);
    return new WorldFileProgressionSection(
        downedBoss1: existing.DownedBoss1,
        downedBoss2: existing.DownedBoss2,
        downedBoss3: existing.DownedBoss3,
        downedQueenBee: existing.DownedQueenBee,
        downedMechBoss1: existing.DownedMechBoss1,
        downedMechBoss2: existing.DownedMechBoss2,
        downedMechBoss3: existing.DownedMechBoss3,
        downedMechBossAny: existing.DownedMechBossAny,
        downedPlantBoss: existing.DownedPlantBoss,
        downedGolemBoss: existing.DownedGolemBoss,
        downedSlimeKing: existing.DownedSlimeKing,
        savedGoblin: existing.SavedGoblin,
        savedWizard: existing.SavedWizard,
        savedMech: existing.SavedMech,
        downedGoblins: existing.DownedGoblins,
        downedClown: existing.DownedClown,
        downedFrost: existing.DownedFrost,
        downedPirates: existing.DownedPirates,
        shadowOrbSmashed: existing.ShadowOrbSmashed,
        spawnMeteor: existing.SpawnMeteor,
        shadowOrbCount: existing.ShadowOrbCount,
        altarCount: existing.AltarCount,
        hardMode: existing.HardMode,
        afterPartyOfDoom: existing.AfterPartyOfDoom,
        invasionDelay: Delay,
        invasionSize: Size,
        invasionType: Type,
        invasionX: X,
        slimeRainTime: existing.SlimeRainTime,
        sundialCooldown: existing.SundialCooldown,
        raining: existing.Raining,
        rainTime: existing.RainTime,
        maxRain: existing.MaxRain,
        cobaltOreTier: existing.CobaltOreTier,
        mythrilOreTier: existing.MythrilOreTier,
        adamantiteOreTier: existing.AdamantiteOreTier,
        backgroundStyles: existing.BackgroundStyles,
        cloudBackgroundActive: existing.CloudBackgroundActive,
        cloudCount: existing.CloudCount,
        windSpeedTarget: existing.WindSpeedTarget);
  }

  public WorldFileQuestSection ToPersistenceSection(WorldFileQuestSection existing) {
    ArgumentNullException.ThrowIfNull(existing);
    return new WorldFileQuestSection(
        anglerWhoFinishedToday: existing.AnglerWhoFinishedToday,
        savedAngler: existing.SavedAngler,
        anglerQuest: existing.AnglerQuest,
        savedStylist: existing.SavedStylist,
        savedTaxCollector: existing.SavedTaxCollector,
        savedGolfer: existing.SavedGolfer,
        invasionSizeStart: SizeStart,
        cultistDelay: existing.CultistDelay);
  }
}
