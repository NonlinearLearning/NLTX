namespace Terraria.WorldSession.Components;

public readonly record struct SecretSeedFlags(
  bool DrunkWorld,
  bool GoodWorld,
  bool TenthAnniversaryWorld,
  bool DontStarveWorld,
  bool NotTheBeesWorld,
  bool RemixWorld,
  bool NoTrapsWorld,
  bool ZenithWorld,
  bool SkyblockWorld,
  bool VampireSeed,
  bool InfectedSeed,
  bool TeamBasedSpawnsSeed,
  bool DualDungeonsSeed);
