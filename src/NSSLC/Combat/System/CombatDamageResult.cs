using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>Associates a processed result with the source that submitted it.</summary>
public readonly record struct CombatDamageResult(
    DamageRequest Request,
    DamageProcessResult Result)
{
  public EntityRuntimeHandle Source => Request.Source;

  public long RequestId => Request.RequestId;
}
