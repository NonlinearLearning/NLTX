using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.sandstorm.load", OwnerId, WorldFileSandstormSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldSandstormLoadApi :
    WorldSectionLoadApi<WorldFileSandstormSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileSandstormSection,
        WorldLoadSection<WorldFileSandstormSection>> {
  public const string OwnerId = "world.sandstorm";
  protected override string SectionId => WorldFileSandstormSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileSandstormSection section) {
    WorldSessionRestoreSystem.ApplySandstorm(owner.World,
        section.Happening,
        section.TimeLeft,
        section.Severity,
        section.IntendedSeverity);
  }
}
