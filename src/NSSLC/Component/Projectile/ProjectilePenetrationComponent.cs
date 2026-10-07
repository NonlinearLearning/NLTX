namespace Terraria.Projectile;

/// <summary>
/// 保存射弹剩余和最大穿透次数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>主要源成员：penetrate（第 156 行）； maxPenetrate（第 166 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/baseline/。</para>
/// <para>拆分依据文件：战斗状态与归因系统组件拆分报告.md。</para>
/// <para>依据位置：第 71 行。</para>
/// </remarks>
public struct ProjectilePenetrationComponent
{
  public ProjectilePenetrationComponent(
    int remainingHits,
    int maximumHits,
    bool stopsDealingDamageWhenDepleted)
  {
    RemainingHits = remainingHits;
    MaximumHits = maximumHits;
    StopsDealingDamageWhenDepleted = stopsDealingDamageWhenDepleted;
  }

  public int RemainingHits;
  public int MaximumHits;
  public bool StopsDealingDamageWhenDepleted;
}
