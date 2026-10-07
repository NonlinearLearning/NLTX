namespace Terraria.Player;

/// <summary>
/// 保存玩家防御装备、护盾和叶水晶能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：boneArmor（第 935 行）； frostArmor（第 937 行）； honey（第 939 行）； crystalLeaf（第 941 行）；
/// crystalLeafCooldown（第 943 行）； defendedByPaladin（第 956 行）； hasPaladinShield（第 958 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 478 行。</para>
/// </remarks>
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
