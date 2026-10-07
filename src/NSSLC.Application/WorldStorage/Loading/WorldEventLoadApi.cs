using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.event-flags.load", OwnerId, WorldFileEventSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldEventLoadApi :
    WorldSectionLoadApi<WorldFileEventSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileEventSection,
        WorldLoadSection<WorldFileEventSection>> {
  public const string OwnerId = "world.event-flags";
  protected override string SectionId => WorldFileEventSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileEventSection section) {
    WorldSessionRestoreSystem.ApplyEvent(owner.World,
        section.CombatBookWasUsed,
        section.LanternNightCooldown,
        section.LanternNightGenuine,
        section.LanternNightManual,
        section.LanternNightNextNightIsGenuine);
  }
}
