namespace Terraria.Player;

/// <summary>
/// 保存玩家武器与弹药类别的伤害倍率及箭类加法叠加输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：meleeDamage（第 1857 行）； magicDamage（第 1859 行）； rangedDamage（第 1861 行）； rangedMultDamage（第
/// 1863 行）； arrowDamageAdditiveStack（第 1865 行）； arrowDamage（第 1867 行）； bulletDamage（第 1869 行）；
/// rocketDamage（第 1871 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 997 行。</para>
/// <para>
/// 用途：装备和状态效果汇总近战、魔法、远程以及箭、子弹、火箭修正，攻击流程按武器和弹药类别组合这些输入。
/// </para>
/// <para>
/// 当前实现：各倍率基础值为 1，ArrowDamageAdditiveStack 基础值为 0。ResetEffects 恢复基础值；具体乘法顺序、加法叠加和最终伤害由外
/// 部战斗流程决定。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C150，L2（通用可选玩法）。分级理由：穿甲、减伤、暴击、武器倍率、攻速、招架及装备闪避是基础扣血之外的规则。
/// </para>
/// </remarks>
public sealed class PlayerWeaponDamageModifierComponent
{
  /// <summary>
  /// 近战伤害倍率输入，基础值 1。
  /// </summary>
  public float MeleeDamage { get; internal set; } = 1f;

  /// <summary>
  /// 魔法伤害倍率输入，基础值 1。
  /// </summary>
  public float MagicDamage { get; internal set; } = 1f;

  /// <summary>
  /// 远程伤害倍率输入，基础值 1。
  /// </summary>
  public float RangedDamage { get; internal set; } = 1f;

  /// <summary>
  /// 远程额外乘法修正输入，基础值 1。
  /// </summary>
  public float RangedMultDamage { get; internal set; } = 1f;

  /// <summary>
  /// 箭类伤害的加法叠加输入，基础值 0。
  /// </summary>
  public float ArrowDamageAdditiveStack { get; internal set; }

  /// <summary>
  /// 箭类伤害倍率输入，基础值 1。
  /// </summary>
  public float ArrowDamage { get; internal set; } = 1f;

  /// <summary>
  /// 子弹伤害倍率输入，基础值 1。
  /// </summary>
  public float BulletDamage { get; internal set; } = 1f;

  /// <summary>
  /// 火箭伤害倍率输入，基础值 1。
  /// </summary>
  public float RocketDamage { get; internal set; } = 1f;

  /// <summary>
  /// 将伤害倍率恢复为 1，并清零箭类加法叠加输入。
  /// </summary>
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
