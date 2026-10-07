using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.boss-progression.load", OwnerId, WorldFileBossProgressionSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldBossProgressionLoadApi :
    WorldSectionLoadApi<WorldFileBossProgressionSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileBossProgressionSection,
        WorldLoadSection<WorldFileBossProgressionSection>> {
  public const string OwnerId = "world.boss-progression";
  protected override string SectionId => WorldFileBossProgressionSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileBossProgressionSection section) {
    WorldSessionRestoreSystem.ApplyBossProgression(owner.World,
        section.FastForwardTimeToDawn,
        section.DownedFishron,
        section.DownedMartians,
        section.DownedAncientCultist,
        section.DownedMoonlord,
        section.DownedHalloweenKing,
        section.DownedHalloweenTree,
        section.DownedChristmasIceQueen,
        section.DownedChristmasSantank,
        section.DownedChristmasTree,
        section.DownedTowerSolar,
        section.DownedTowerVortex,
        section.DownedTowerNebula,
        section.DownedTowerStardust,
        section.TowerActiveSolar,
        section.TowerActiveVortex,
        section.TowerActiveNebula,
        section.TowerActiveStardust,
        section.LunarApocalypseIsUp);
  }
}
