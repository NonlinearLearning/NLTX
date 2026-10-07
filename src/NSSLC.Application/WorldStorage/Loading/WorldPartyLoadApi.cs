using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.party.load", OwnerId, WorldFilePartySection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldPartyLoadApi :
    WorldSectionLoadApi<WorldFilePartySection>,
    IWorldLoadApi<LoadedWorldSession, WorldFilePartySection,
        WorldLoadSection<WorldFilePartySection>> {
  public const string OwnerId = "world.party";
  protected override string SectionId => WorldFilePartySection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFilePartySection section) {
    WorldSessionRestoreSystem.ApplyParty(owner.World,
        section.Manual,
        section.Genuine,
        section.Cooldown,
        section.CelebratingNpcIds);
  }
}
