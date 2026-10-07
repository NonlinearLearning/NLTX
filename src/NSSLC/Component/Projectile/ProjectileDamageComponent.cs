namespace Terraria.Projectile;

/// <summary>
/// 保存射弹当前与原始伤害、击退、穿甲和暴击参数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>
/// 主要源成员：damage（第 142 行）； originalDamage（第 144 行）； knockBack（第 152 行）； armorPenetration（第 266 行）；
/// bonusCritChance（第 268 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/baseline/。</para>
/// <para>拆分依据文件：战斗状态与归因系统组件拆分报告.md。</para>
/// <para>依据位置：第 71 行。</para>
/// </remarks>
public struct ProjectileDamageComponent
{
  public ProjectileDamageComponent(
    int current,
    int original,
    float knockback,
    int armorPenetration,
    int criticalChance)
  {
    Current = current;
    Original = original;
    Knockback = knockback;
    ArmorPenetration = armorPenetration;
    CriticalChance = criticalChance;
  }

  public int Current;
  public int Original;
  public float Knockback;
  public int ArmorPenetration;
  public int CriticalChance;
}
