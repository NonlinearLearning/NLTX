using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.backgrounds.load", OwnerId, WorldFileBackgroundSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldBackgroundLoadApi :
    WorldSectionLoadApi<WorldFileBackgroundSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileBackgroundSection,
        WorldLoadSection<WorldFileBackgroundSection>> {
  public const string OwnerId = "world.backgrounds";
  protected override string SectionId => WorldFileBackgroundSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileBackgroundSection section) {
    WorldSessionRestoreSystem.ApplyBackground(owner.World,
        section.Styles);
  }
}
