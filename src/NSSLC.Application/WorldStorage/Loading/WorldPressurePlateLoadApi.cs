using System.IO;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.pressure-plates.load", OwnerId, WorldFilePressurePlateSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldPressurePlateLoadApi :
    WorldSectionLoadApi<WorldFilePressurePlateSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFilePressurePlateSection,
        WorldLoadSection<WorldFilePressurePlateSection>> {
  public const string OwnerId = "world.pressure-plates";
  protected override string SectionId => WorldFilePressurePlateSection.SectionId;

  protected override void Validate(WorldFilePressurePlateSection section) {
    if (section.Plates.Select(plate => (plate.X, plate.Y)).Distinct().Count() !=
        section.Plates.Count) {
      throw new InvalidDataException("Duplicate pressure plate anchors.");
    }
  }

  protected override void Apply(LoadedWorldSession owner, WorldFilePressurePlateSection section) {
    owner.Storage.PressurePlates.Replace(section.Plates.Select(plate =>
        new TileCoordinate(plate.X, plate.Y)).ToArray());
  }
}
