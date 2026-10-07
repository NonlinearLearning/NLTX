using System.IO;
using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.bestiary.load", OwnerId, WorldFileBestiarySection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldBestiaryLoadApi :
    WorldSectionLoadApi<WorldFileBestiarySection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileBestiarySection,
        WorldLoadSection<WorldFileBestiarySection>> {
  public const string OwnerId = "world.bestiary";
  protected override string SectionId => WorldFileBestiarySection.SectionId;

  protected override void Validate(WorldFileBestiarySection section) {
    if (section.KillCounts.Any(record => record.Count < 0) ||
        section.KillCounts.Select(record => record.PersistentId).Distinct().Count() !=
        section.KillCounts.Count) {
      throw new InvalidDataException("Invalid bestiary kill counts.");
    }
  }

  protected override void Apply(LoadedWorldSession owner, WorldFileBestiarySection section) {
    WorldSessionRestoreSystem.ApplyBestiary(owner.World,
        section.KillCounts.ToDictionary(record => record.PersistentId, record => record.Count),
        section.SeenNpcIds, section.ChattedNpcIds);
  }
}
