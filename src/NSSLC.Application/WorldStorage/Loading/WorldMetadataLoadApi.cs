using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.metadata.load", OwnerId, WorldFileMetadataSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldMetadataLoadApi :
    WorldSectionLoadApi<WorldFileMetadataSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileMetadataSection,
        WorldLoadSection<WorldFileMetadataSection>> {
  public const string OwnerId = "world.metadata";
  protected override string SectionId => WorldFileMetadataSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileMetadataSection section) {
    owner.Metadata = section;
  }
}
