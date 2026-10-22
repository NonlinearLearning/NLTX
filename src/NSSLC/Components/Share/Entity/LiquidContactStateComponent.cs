namespace EntityEcs.Components;

/// <summary>
/// 保存实体的液体接触标记、计数和最近求解时刻。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>主要源成员：wet（第 26 行）； shimmerWet（第 28 行）； honeyWet（第 30 行）； lavaWet（第 34 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 1499 行。</para>
/// <para>
/// 用途：空间系统写入接触事实后，环境与移动流程读取水、熔岩、蜂蜜和微光接触结果。
/// </para>
/// <para>
/// 当前实现：所有状态由外部写入，HasAnyLiquidContact 只合并四个布尔标记。WetTickCount 是 int，当前组件没有自动增长、清零或范围限制方法
/// 。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C159，L2（通用可选玩法）。分级理由：只有保留液体交互时需要这些接触状态，简单固体世界可关闭。
/// </para>
/// </remarks>
public struct LiquidContactStateComponent
{
  /// <summary>
  /// 最近一次接触求解的游戏 tick，可为空。
  /// </summary>
  public long? ResolvedAtTick;
  /// <summary>
  /// 是否具有 wet 液体接触标记。
  /// </summary>
  public bool IsWet;
  /// <summary>
  /// 是否接触熔岩。
  /// </summary>
  public bool IsLavaWet;
  /// <summary>
  /// 是否接触蜂蜜。
  /// </summary>
  public bool IsHoneyWet;
  /// <summary>
  /// 是否接触微光。
  /// </summary>
  public bool IsShimmerWet;
  /// <summary>
  /// 外部流程维护的接触计数。
  /// </summary>
  public int WetTickCount;
  /// <summary>
  /// 主要接触液体类别，可为空。
  /// </summary>
  public LiquidKind? DominantLiquidType;

  /// <summary>
  /// 任一液体接触标记为 true 时返回 true。
  /// </summary>
  public bool HasAnyLiquidContact => IsWet || IsLavaWet ||
    IsHoneyWet || IsShimmerWet;
}
