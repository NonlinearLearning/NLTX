namespace EntityEcs.Components;

/// <summary>
/// 保存实体各类液体接触标记及求解时刻。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>主要源成员：wet（第 26 行）； shimmerWet（第 28 行）； honeyWet（第 30 行）； lavaWet（第 34 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 1499 行。</para>
/// </remarks>
public struct LiquidContactStateComponent
{
  public long? ResolvedAtTick;
  public bool IsWet;
  public bool IsLavaWet;
  public bool IsHoneyWet;
  public bool IsShimmerWet;
  public int WetTickCount;
  public LiquidKind? DominantLiquidType;

  public bool HasAnyLiquidContact => IsWet || IsLavaWet ||
    IsHoneyWet || IsShimmerWet;
}
