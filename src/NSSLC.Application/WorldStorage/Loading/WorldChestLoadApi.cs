using System.IO;
using Terraria.Items;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.chests.load", OwnerId, WorldFileChestSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldChestLoadApi :
    WorldSectionLoadApi<WorldFileChestSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileChestSection,
        WorldLoadSection<WorldFileChestSection>> {
  public const string OwnerId = "world.chests";
  protected override string SectionId => WorldFileChestSection.SectionId;

  protected override void Validate(WorldFileChestSection section) {
    if (section.Chests.Select(chest => (chest.X, chest.Y)).Distinct().Count() !=
        section.Chests.Count) {
      throw new InvalidDataException("Duplicate chest anchors.");
    }
    if (section.Chests.Any(chest => chest.Items.Any(item =>
        item.Stack < short.MinValue || item.Stack > short.MaxValue ||
        (item.Stack > 0 && item.Type == 0)))) {
      throw new InvalidDataException("A chest contains an invalid item.");
    }
  }

  protected override void Apply(LoadedWorldSession owner, WorldFileChestSection section) {
    WorldContainerRestoreSystem.Apply(owner.Storage.WorldContainers, section.Chests
        .Select(chest => new WorldChestSnapshot(new TileCoordinate(chest.X, chest.Y),
            chest.Name, chest.Items.Select(item =>
                new ItemState(item.Type, item.Prefix, item.Stack)).ToArray())).ToArray());
  }
}
