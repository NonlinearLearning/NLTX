using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.spawn-points.load", OwnerId, WorldFileSpawnSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldSpawnLoadApi :
    WorldSectionLoadApi<WorldFileSpawnSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileSpawnSection,
        WorldLoadSection<WorldFileSpawnSection>> {
  public const string OwnerId = "world.spawn-points";
  protected override string SectionId => WorldFileSpawnSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileSpawnSection section) {
    owner.ManifestJson = section.WorldManifestJson;
    WorldSpawnConfigurationRestoreSystem.Apply(owner.World,
        section.ExtraSpawnPoints.Select(point => new TileCoordinate(point.X, point.Y)).ToArray(),
        section.DualDungeonsSeed, section.MoreLightningSeed, section.NoLightningSeed);
  }
}
