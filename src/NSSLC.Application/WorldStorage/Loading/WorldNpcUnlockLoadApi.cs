using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.npc-unlocks.load", OwnerId, WorldFileNpcUnlockSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldNpcUnlockLoadApi :
    WorldSectionLoadApi<WorldFileNpcUnlockSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileNpcUnlockSection,
        WorldLoadSection<WorldFileNpcUnlockSection>> {
  public const string OwnerId = "world.npc-unlocks";
  protected override string SectionId => WorldFileNpcUnlockSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileNpcUnlockSection section) {
    WorldSessionRestoreSystem.ApplyNpcUnlock(owner.World,
        section.BoughtCat,
        section.BoughtDog,
        section.BoughtBunny,
        section.DownedEmpressOfLight,
        section.DownedQueenSlime,
        section.DownedDeerclops,
        section.UnlockedSlimeBlueSpawn,
        section.UnlockedMerchantSpawn,
        section.UnlockedDemolitionistSpawn,
        section.UnlockedPartyGirlSpawn,
        section.UnlockedDyeTraderSpawn,
        section.UnlockedTruffleSpawn,
        section.UnlockedArmsDealerSpawn,
        section.UnlockedNurseSpawn,
        section.UnlockedPrincessSpawn,
        section.CombatBookVolumeTwoWasUsed,
        section.PeddlersSatchelWasUsed,
        section.UnlockedSlimeGreenSpawn,
        section.UnlockedSlimeOldSpawn,
        section.UnlockedSlimePurpleSpawn,
        section.UnlockedSlimeRainbowSpawn,
        section.UnlockedSlimeRedSpawn,
        section.UnlockedSlimeYellowSpawn,
        section.UnlockedSlimeCopperSpawn);
  }
}
