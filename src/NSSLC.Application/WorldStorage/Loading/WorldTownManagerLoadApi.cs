using System.Collections.Generic;
using System.IO;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.town-manager.load", OwnerId, WorldFileTownManagerSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldTownManagerLoadApi :
    WorldSectionLoadApi<WorldFileTownManagerSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileTownManagerSection,
        WorldLoadSection<WorldFileTownManagerSection>> {
  public const string OwnerId = "world.town-manager";
  protected override string SectionId => WorldFileTownManagerSection.SectionId;

  protected override void Validate(WorldFileTownManagerSection section) {
    var residents = new HashSet<int>();
    foreach (WorldFileTownRoomRecord room in section.Rooms) {
      if (!residents.Add(room.NpcType)) {
        throw new InvalidDataException("Duplicate town housing resident keys.");
      }
    }
  }

  protected override void Apply(LoadedWorldSession owner, WorldFileTownManagerSection section) {
    var assignments = new List<KeyValuePair<TownHousingResidentKey, TilePosition>>(section.Rooms.Count);
    foreach (WorldFileTownRoomRecord room in section.Rooms) {
      assignments.Add(new KeyValuePair<TownHousingResidentKey, TilePosition>(
          new TownHousingResidentKey(room.NpcType), new TilePosition(room.TileX, room.TileY)));
    }

    TownHousingRegistrySystem.ReplaceRoomAssignments(owner.TownHousing, assignments);
  }
}
