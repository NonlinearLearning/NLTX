namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-727, P09-728, P09-729, P09-730, P09-731, P09-732, P09-733, P09-734, P09-735, P09-737, P09-738
// crossSubsystemOwner: effect rebuild order and renderer consumption remain integration-review
/// <summary>
/// 保存玩家装备提供的阴影、轮廓和其他表现效果。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：armorEffectDrawShadow（第 1051 行）； armorEffectDrawShadowSubtle（第 1053 行）；
/// armorEffectDrawOutlines（第 1055 行）； armorEffectDrawShadowLokis（第 1057 行）；
/// armorEffectDrawShadowBasilisk（第 1059 行）； armorEffectDrawOutlinesForbidden（第 1061 行）；
/// armorEffectDrawShadowEOCShield（第 1063 行）； socialShadowRocketBoots（第 1065 行）； socialGhost（第
/// 1067 行）； ashWoodBonus（第 1071 行）； socialIgnoreLight（第 1073 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 14 行。</para>
/// </remarks>
public sealed class PlayerEquipmentEffectStateComponent
{
  public bool ArmorEffectDrawShadow { get; internal set; }

  public bool ArmorEffectDrawShadowSubtle { get; internal set; }

  public bool ArmorEffectDrawOutlines { get; internal set; }

  public bool ArmorEffectDrawShadowLokis { get; internal set; }

  public bool ArmorEffectDrawShadowBasilisk { get; internal set; }

  public bool ArmorEffectDrawOutlinesForbidden { get; internal set; }

  public bool ArmorEffectDrawShadowEocShield { get; internal set; }

  public bool SocialShadowRocketBoots { get; internal set; }

  public bool SocialGhost { get; internal set; }

  public bool AshWoodBonus { get; internal set; }

  public bool SocialIgnoreLight { get; internal set; }
}
