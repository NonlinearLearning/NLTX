namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSecretSeedDerivedOptionsInput(
  bool BiggerAbandonedHousesEnabled,
  bool ErrorWorldEnabled,
  int? BiggerAbandonedHousesRandomRoll,
  bool RainbowStuffEnabled,
  bool TenthAnniversaryWorld);
