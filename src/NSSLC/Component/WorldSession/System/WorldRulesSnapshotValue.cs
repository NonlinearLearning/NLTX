namespace Terraria.WorldSession.Components;

public readonly record struct WorldRulesSnapshotValue(
  WorldGameMode GameMode,
  bool HardMode,
  WorldSecretSeedFlags SecretSeeds,
  WorldEvilType WorldEvil,
  OreTierState SavedOreTiers);
