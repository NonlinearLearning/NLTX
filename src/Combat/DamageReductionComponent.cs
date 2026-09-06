namespace Terraria.Combat;

public struct DamageReductionComponent
{
  public DamageReductionComponent(float endurance)
  {
    Endurance = endurance;
  }

  public float Endurance;

  public bool HasReduction => Endurance > 0.0f;
}
