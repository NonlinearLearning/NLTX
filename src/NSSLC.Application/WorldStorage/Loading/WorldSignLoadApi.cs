using System.IO;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.signs.load", OwnerId, WorldFileSignSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldSignLoadApi :
    WorldSectionLoadApi<WorldFileSignSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileSignSection,
        WorldLoadSection<WorldFileSignSection>> {
  public const string OwnerId = "world.signs";
  protected override string SectionId => WorldFileSignSection.SectionId;

  protected override void Validate(WorldFileSignSection section) {
    if (section.Signs.Select(sign => (sign.X, sign.Y)).Distinct().Count() != section.Signs.Count) {
      throw new InvalidDataException("Duplicate sign anchors.");
    }
  }

  protected override void Apply(LoadedWorldSession owner, WorldFileSignSection section) {
    WorldSignRestoreSystem.Apply(owner.Storage.WorldSigns, section.Signs.Select(sign =>
        new WorldSignSnapshot(new TileCoordinate(sign.X, sign.Y), sign.Text)).ToArray());
  }
}
