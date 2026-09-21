namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSeedOptionMatch(
  WorldSeedOptionId OptionId,
  string NormalizedSeedText,
  bool MatchedTranslatedValue);
