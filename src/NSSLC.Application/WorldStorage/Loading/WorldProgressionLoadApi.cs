using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.progression.load", OwnerId, WorldFileProgressionSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldProgressionLoadApi :
    WorldSectionLoadApi<WorldFileProgressionSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileProgressionSection,
        WorldLoadSection<WorldFileProgressionSection>> {
  public const string OwnerId = "world.progression";
  protected override string SectionId => WorldFileProgressionSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileProgressionSection section) {
    WorldSessionRestoreSystem.ApplyProgression(owner.World,
        section.DownedBoss1,
        section.DownedBoss2,
        section.DownedBoss3,
        section.DownedQueenBee,
        section.DownedMechBoss1,
        section.DownedMechBoss2,
        section.DownedMechBoss3,
        section.DownedMechBossAny,
        section.DownedPlantBoss,
        section.DownedGolemBoss,
        section.DownedSlimeKing,
        section.SavedGoblin,
        section.SavedWizard,
        section.SavedMech,
        section.DownedGoblins,
        section.DownedClown,
        section.DownedFrost,
        section.DownedPirates,
        section.ShadowOrbSmashed,
        section.SpawnMeteor,
        section.ShadowOrbCount,
        section.AltarCount,
        section.HardMode,
        section.AfterPartyOfDoom,
        section.InvasionDelay,
        section.InvasionSize,
        section.InvasionType,
        section.InvasionX,
        section.SlimeRainTime,
        section.SundialCooldown,
        section.Raining,
        section.RainTime,
        section.MaxRain,
        section.CobaltOreTier,
        section.MythrilOreTier,
        section.AdamantiteOreTier,
        section.BackgroundStyles,
        section.CloudBackgroundActive,
        section.CloudCount,
        section.WindSpeedTarget);
  }
}
