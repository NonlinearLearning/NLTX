namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record SecretSeedInputResult(
  bool HasNormalizedInput,
  bool IsMatch,
  string NormalizedInput,
  string DisplayInput,
  int MatchedCandidateIndex,
  bool SecretTransformDeferred);
