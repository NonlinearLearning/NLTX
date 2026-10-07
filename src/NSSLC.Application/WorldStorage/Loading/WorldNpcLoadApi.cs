using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.npcs.load", OwnerId, WorldFileNpcSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldNpcLoadApi :
    WorldSectionLoadApi<WorldFileNpcSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileNpcSection,
        WorldLoadSection<WorldFileNpcSection>> {
  public const string OwnerId = "world.npcs";
  protected override string SectionId => WorldFileNpcSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileNpcSection section) {
    WorldNpcRestoreSystem.Apply(owner.Storage.Npcs,
        section.TownNpcs.Concat(section.SavedNpcs).Select(npc => new WorldNpcState(
            npc.NetId, npc.LegacyTypeName, npc.IsTownNpc, npc.Name,
            npc.PositionX, npc.PositionY, npc.Homeless,
            new TileCoordinate(npc.HomeTileX, npc.HomeTileY), npc.TownNpcVariationIndex,
            npc.HomelessDespawn)).ToArray());
    WorldSessionRestoreSystem.ApplyShimmeredTownNpcs(owner.World.History,
        section.ShimmeredTownNpcIds);
  }
}
