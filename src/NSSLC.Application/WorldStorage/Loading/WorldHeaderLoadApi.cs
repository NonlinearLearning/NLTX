using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

[WorldLoadApi("world.header.load", OwnerId, WorldFileHeaderSection.SectionId,
    319, 319, WorldLoadSectionRequirement.Required)]
public sealed class WorldHeaderLoadApi :
    WorldSectionLoadApi<WorldFileHeaderSection>,
    IWorldLoadApi<LoadedWorldSession, WorldFileHeaderSection,
        WorldLoadSection<WorldFileHeaderSection>> {
  public const string OwnerId = "world.header";
  protected override string SectionId => WorldFileHeaderSection.SectionId;

  protected override void Apply(LoadedWorldSession owner, WorldFileHeaderSection section) {
    WorldSecretSeedFlags flags = WorldSecretSeedFlags.None;
    if (section.DrunkWorld) { flags |= WorldSecretSeedFlags.Drunk; }
    if (section.GetGoodWorld) { flags |= WorldSecretSeedFlags.ForTheWorthy; }
    if (section.TenthAnniversaryWorld) { flags |= WorldSecretSeedFlags.TenthAnniversary; }
    if (section.DontStarveWorld) { flags |= WorldSecretSeedFlags.DontStarve; }
    if (section.NotTheBeesWorld) { flags |= WorldSecretSeedFlags.NotTheBees; }
    if (section.RemixWorld) { flags |= WorldSecretSeedFlags.Remix; }
    if (section.NoTrapsWorld) { flags |= WorldSecretSeedFlags.NoTraps; }
    if (section.ZenithWorld) { flags |= WorldSecretSeedFlags.Zenith; }
    if (section.SkyblockWorld) { flags |= WorldSecretSeedFlags.Skyblock; }
    WorldSessionRestoreSystem.ApplyHeader(owner.World, new WorldDescriptorSnapshotValue(
        section.WorldId, section.UniqueId ?? Guid.Empty, section.WorldName,
        section.SeedText ?? "", section.WorldGeneratorVersion ?? 0,
        section.MaxTilesX, section.MaxTilesY,
        new WorldBounds(section.LeftWorld, section.TopWorld,
            section.RightWorld, section.BottomWorld),
        0, 0, 0, 0, 0, 0), (WorldGameMode)section.GameMode, flags);
    owner.DimensionCompatibility.Capture(section.MaxTilesX, section.MaxTilesY);
    owner.CreationTime = section.CreationTime;
    owner.LastPlayed = section.LastPlayed;
  }
}
