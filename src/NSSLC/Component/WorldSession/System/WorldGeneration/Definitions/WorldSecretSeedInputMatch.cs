namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSecretSeedInputMatch(
  WorldSecretSeedDefinition Definition,
  string NormalizedPlaintext,
  string OriginalUnlockText,
  bool MatchedKnownPlaintext);
