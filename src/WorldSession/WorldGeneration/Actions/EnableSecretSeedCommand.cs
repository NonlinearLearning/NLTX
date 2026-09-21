namespace Terraria.WorldGeneration.Actions;

public readonly record struct EnableSecretSeedCommand(
  long GenerationId,
  ulong RuntimeVersion,
  string Variant,
  string IdempotencyKey)
{
  public bool IsWellFormed =>
    GenerationId >= 0 &&
    RuntimeVersion > 0 &&
    !string.IsNullOrWhiteSpace(Variant) &&
    !string.IsNullOrWhiteSpace(IdempotencyKey);
}
