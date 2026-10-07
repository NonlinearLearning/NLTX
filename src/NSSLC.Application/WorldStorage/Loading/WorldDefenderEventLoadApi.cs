using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.defender-event.load", OwnerId, WorldFileDefenderEventSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldDefenderEventLoadApi :
    WorldSectionLoadApi<WorldFileDefenderEventSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileDefenderEventSection,
        WorldLoadSection<WorldFileDefenderEventSection>> {
  public const string OwnerId = "world.defender-event";
  protected override string SectionId => WorldFileDefenderEventSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileDefenderEventSection section) {
    WorldSessionRestoreSystem.ApplyDefenderEvent(owner.World,
        section.SavedBartender,
        section.DownedInvasionTier1,
        section.DownedInvasionTier2,
        section.DownedInvasionTier3);
  }
}
