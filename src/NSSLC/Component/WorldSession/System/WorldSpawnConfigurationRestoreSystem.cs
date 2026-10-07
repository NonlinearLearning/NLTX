using Terraria.WorldStorage;

namespace Terraria.WorldSession.Components;

public static class WorldSpawnConfigurationRestoreSystem {
  public static void Apply(WorldSessionRestoreState owner,
      IReadOnlyList<TileCoordinate> extraSpawnPoints, bool dualDungeons, bool moreLightning,
      bool noLightning) {
    if (moreLightning || noLightning) {
      throw new ArgumentException("WorldFile 319 does not support the later lightning flags.");
    }
    owner.Descriptor.ExtraSpawnPoints = Array.AsReadOnly(extraSpawnPoints.ToArray());
    owner.Rules.SecretSeeds = dualDungeons
        ? owner.Rules.SecretSeeds | WorldSecretSeedFlags.DualDungeons
        : owner.Rules.SecretSeeds & ~WorldSecretSeedFlags.DualDungeons;
  }
}
