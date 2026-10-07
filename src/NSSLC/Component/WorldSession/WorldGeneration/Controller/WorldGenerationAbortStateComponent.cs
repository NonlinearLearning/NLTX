namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores a queued terminal abort request for the current generation run.
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
/// </remarks>
public sealed class WorldGenerationAbortStateComponent
{
  public WorldGenerationAbortStateComponent(bool abortQueued = false)
  {
    AbortQueued = abortQueued;
  }

  public bool AbortQueued { get; private set; }
}
