namespace Terraria.Combat;

public struct KnockbackPolicyComponent
{
  public KnockbackPolicyComponent(
    bool isImmune,
    float resistance)
  {
    IsImmune = isImmune;
    Resistance = resistance;
  }

  public bool IsImmune;
  public float Resistance;

  public bool CanReceiveKnockback => !IsImmune && Resistance > 0.0f;
}
