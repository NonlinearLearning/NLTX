namespace Terraria.WorldGeneration.Actions;

public readonly record struct ClearSecretSeedsCommand(
  long GenerationId,
  ulong RuntimeVersion,
  string IdempotencyKey)
{
  public bool IsWellFormed =>
    GenerationId >= 0 &&
    RuntimeVersion > 0 &&
    !string.IsNullOrWhiteSpace(IdempotencyKey);
}
