using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.banners.load", OwnerId, WorldFileBannerSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldBannerLoadApi :
    WorldSectionLoadApi<WorldFileBannerSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileBannerSection,
        WorldLoadSection<WorldFileBannerSection>> {
  public const string OwnerId = "world.banners";
  protected override string SectionId => WorldFileBannerSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileBannerSection section) {
    WorldSessionRestoreSystem.ApplyBanner(owner.World,
        section.KillCounts,
        section.ClaimableCounts);
  }
}
