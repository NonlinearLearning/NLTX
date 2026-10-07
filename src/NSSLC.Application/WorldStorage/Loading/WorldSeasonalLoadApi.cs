using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.seasonal.load", OwnerId, WorldFileSeasonalSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldSeasonalLoadApi :
    WorldSectionLoadApi<WorldFileSeasonalSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileSeasonalSection,
        WorldLoadSection<WorldFileSeasonalSection>> {
  public const string OwnerId = "world.seasonal";
  protected override string SectionId => WorldFileSeasonalSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileSeasonalSection section) {
    WorldSessionRestoreSystem.ApplySeasonal(owner.World,
        section.ForceHalloweenForToday,
        section.ForceChristmasForToday,
        section.CopperOreTier,
        section.IronOreTier,
        section.SilverOreTier,
        section.GoldOreTier);
  }
}
