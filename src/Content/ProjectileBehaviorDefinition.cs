namespace Terraria.Content;

public sealed record ProjectileBehaviorDefinition(
  int AiStyle,
  int ExtraUpdates,
  int DefaultTimeLeft,
  int NumUpdates = 0)
{
  public bool DecidesManualFallThrough { get; init; }

  public bool ShouldFallThrough { get; init; }
}
