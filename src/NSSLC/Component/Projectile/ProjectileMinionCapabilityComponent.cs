namespace Terraria.Projectile;

/// <summary>
/// 保存射弹作为召唤物的槽位占用和排列位置。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：minion（第 186 行）； minionSlots（第 188 行）； minionPos（第 190 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
public struct ProjectileMinionCapabilityComponent
{
  public ProjectileMinionCapabilityComponent(
    float minionSlots = 0.0f,
    int minionPosition = 0,
    bool isMinion = false)
  {
    MinionSlots = minionSlots;
    MinionPosition = minionPosition;
    IsMinion = isMinion;
  }

  public float MinionSlots;
  public int MinionPosition;
  public bool IsMinion;
}
