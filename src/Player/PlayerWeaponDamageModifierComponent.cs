namespace Terraria.Player;

public sealed class PlayerWeaponDamageModifierComponent
{
  public float MeleeDamage { get; internal set; } = 1f;

  public float MagicDamage { get; internal set; } = 1f;

  public float RangedDamage { get; internal set; } = 1f;

  public float RangedMultDamage { get; internal set; } = 1f;

  public float ArrowDamageAdditiveStack { get; internal set; }

  public float ArrowDamage { get; internal set; } = 1f;

  public float BulletDamage { get; internal set; } = 1f;

  public float RocketDamage { get; internal set; } = 1f;

  internal void ResetEffects()
  {
    MeleeDamage = 1f;
    MagicDamage = 1f;
    RangedDamage = 1f;
    RangedMultDamage = 1f;
    ArrowDamageAdditiveStack = 0f;
    ArrowDamage = 1f;
    BulletDamage = 1f;
    RocketDamage = 1f;
  }
}
