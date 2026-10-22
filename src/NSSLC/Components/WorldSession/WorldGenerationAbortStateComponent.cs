namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成是否已经登记终止请求。
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成控制器的待中止请求。</para>
/// <para>拆分来源：Terraria.WorldBuilding.WorldGenerator.Controller。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/WorldGenerator.cs。</para>
/// <para>主要源成员：QueuedAbort（第 80 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 366 行。</para>
/// <para>
/// 用途：生成控制流程提交终止意图，执行流程在允许停止的位置读取该意图。
/// </para>
/// <para>
/// 当前实现：AbortQueued 只在构造时赋值，默认 false，本文件没有后续修改方法。终止请求的消费、执行停止和资源清理由外部生成流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C186，L2（通用可选玩法）。分级理由：生成器的运行阶段、步骤计划、随机流、选择与取消仅在服务端生成世界时需要，预置地图可省去。
/// </para>
/// </remarks>
public sealed class WorldGenerationAbortStateComponent
{
  public WorldGenerationAbortStateComponent(bool abortQueued = false)
  {
    AbortQueued = abortQueued;
  }

  /// <summary>
  /// 是否已登记世界生成终止请求。
  /// </summary>
  public bool AbortQueued { get; private set; }
}
