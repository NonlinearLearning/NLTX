using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.environment.load", OwnerId, WorldFileEnvironmentSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldEnvironmentLoadApi :
    WorldSectionLoadApi<WorldFileEnvironmentSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileEnvironmentSection,
        WorldLoadSection<WorldFileEnvironmentSection>> {
  public const string OwnerId = "world.environment";
  protected override string SectionId => WorldFileEnvironmentSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileEnvironmentSection section) {
    WorldSessionRestoreSystem.ApplyEnvironment(owner.World,
        section.MoonType,
        section.TreeX,
        section.TreeStyle,
        section.CaveBackX,
        section.CaveBackStyle,
        section.IceBackStyle,
        section.JungleBackStyle,
        section.HellBackStyle,
        section.SpawnTileX,
        section.SpawnTileY,
        section.WorldSurface,
        section.RockLayer,
        section.Time,
        section.DayTime,
        section.MoonPhase,
        section.BloodMoon,
        section.Eclipse,
        section.DungeonX,
        section.DungeonY,
        section.Crimson);
  }
}
