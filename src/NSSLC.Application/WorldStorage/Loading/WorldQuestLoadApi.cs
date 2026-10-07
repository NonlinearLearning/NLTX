using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.quests.load", OwnerId, WorldFileQuestSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldQuestLoadApi :
    WorldSectionLoadApi<WorldFileQuestSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileQuestSection,
        WorldLoadSection<WorldFileQuestSection>> {
  public const string OwnerId = "world.quests";
  protected override string SectionId => WorldFileQuestSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileQuestSection section) {
    WorldSessionRestoreSystem.ApplyQuest(owner.World,
        section.AnglerWhoFinishedToday,
        section.SavedAngler,
        section.AnglerQuest,
        section.SavedStylist,
        section.SavedTaxCollector,
        section.SavedGolfer,
        section.InvasionSizeStart,
        section.CultistDelay);
  }
}
