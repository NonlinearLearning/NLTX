namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// World effects captured by the P03 collision adapter for one committed Arrow movement.
/// These facts are intentionally separate from tile displacement and contain no tile state.
/// </summary>
public sealed record ProjectileArrowWorldEffectFacts(
    bool SwitchTilesChanged,
    int SwitchTilesObjectType,
    bool CanCutTiles,
    bool CutTilesApplied)
{
  public bool IsValid => SwitchTilesObjectType >= 0;
}
