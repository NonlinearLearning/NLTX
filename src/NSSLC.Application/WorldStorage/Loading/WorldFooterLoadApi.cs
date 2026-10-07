using System.IO;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.footer.load", OwnerId, WorldFileFooterSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Required,
    "world.backgrounds.load", "world.banners.load", "world.bestiary.load",
    "world.boss-progression.load", "world.chests.load", "world.creative-powers.load",
    "world.defender-event.load", "world.environment.load", "world.event-flags.load",
    "world.header.load", "world.metadata.load", "world.npcs.load", "world.npc-unlocks.load",
    "world.party.load", "world.pressure-plates.load", "world.progression.load", "world.quests.load",
    "world.sandstorm.load", "world.seasonal.load", "world.signs.load", "world.spawn-points.load",
    "world.tile-entities.load", "world.tiles.load", "world.time-policy.load",
    "world.town-manager.load",
    "world-generation.tree-tops.load")]
public sealed class WorldFooterLoadApi :
    WorldSectionLoadApi<WorldFileFooterSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileFooterSection,
        WorldLoadSection<WorldFileFooterSection>> {
  public const string OwnerId = "world.footer";
  protected override string SectionId => WorldFileFooterSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileFooterSection section) {
    if (!section.IsComplete || owner.World.Descriptor.WorldId != section.WorldId ||
        owner.World.Descriptor.Name != section.WorldName) {
      throw new InvalidDataException("The footer does not match the committed world identity.");
    }
    owner.IsComplete = true;
  }
}
