using System.IO;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.creative-powers.load", OwnerId, WorldFileCreativePowersSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldCreativePowersLoadApi :
    WorldSectionLoadApi<WorldFileCreativePowersSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileCreativePowersSection,
        WorldLoadSection<WorldFileCreativePowersSection>> {
  public const string OwnerId = "world.creative-powers";
  protected override string SectionId => WorldFileCreativePowersSection.SectionId;

  protected override void Validate(WorldFileCreativePowersSection section) {
    if (section.SerializedPayload.Length != 1 || section.SerializedPayload.Span[0] != 0) {
      throw new InvalidDataException(
          "This load composition supports the empty CreativePowers state of generated worlds.");
    }
  }

  protected override void Apply(LoadedWorldSession owner, WorldFileCreativePowersSection section) {
    owner.HasCreativePowers = false;
  }
}
