using System;

namespace Terraria.Attributes;

/// <summary>
/// Explicit input for deterministic critical-roll calculation. It contains no hidden clock or
/// random dependency, so retrying the same attack produces the same result.
/// </summary>
public readonly record struct CriticalRollInput(
    ulong WorldSeed,
    long Tick,
    long SourceKey,
    long AttackDefinitionKey,
    long RequestId)
{
  public bool IsValid => Tick >= 0 && SourceKey >= 0 &&
      AttackDefinitionKey >= 0 && RequestId >= 0;

  public void Validate()
  {
    if (!IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(CriticalRollInput));
    }
  }
}
