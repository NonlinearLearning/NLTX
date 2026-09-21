namespace Terraria.Player;

public sealed class PlayerDefenseCapabilityComponent
{
  public bool BoneArmor { get; internal set; }

  public bool FrostArmor { get; internal set; }

  public bool Honey { get; internal set; }

  public bool CrystalLeaf { get; internal set; }

  public int CrystalLeafCooldown { get; internal set; }

  public bool DefendedByPaladin { get; internal set; }

  public bool HasPaladinShield { get; internal set; }

  internal void ResetEffects()
  {
    BoneArmor = false;
    FrostArmor = false;
    Honey = false;
    CrystalLeaf = false;
    CrystalLeafCooldown = 0;
    DefendedByPaladin = false;
    HasPaladinShield = false;
  }
}
