using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.time-policy.load", OwnerId, WorldFileTimePolicySection.SectionId,
    319, 319, WorldLoadSectionRequirement.Optional, "world.header.load")]
public sealed class WorldTimePolicyLoadApi :
    WorldSectionLoadApi<WorldFileTimePolicySection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileTimePolicySection,
        WorldLoadSection<WorldFileTimePolicySection>> {
  public const string OwnerId = "world.time-policy";
  protected override string SectionId => WorldFileTimePolicySection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileTimePolicySection section) {
    WorldSessionRestoreSystem.ApplyTimePolicy(owner.World,
        section.FastForwardTimeToDusk,
        section.MoondialCooldown,
        section.ForceHalloweenForever,
        section.ForceChristmasForever,
        section.VampireSeed,
        section.InfectedSeed,
        section.MeteorShowerCount,
        section.CoinRain,
        section.TeamBasedSpawnsSeed);
  }
}
