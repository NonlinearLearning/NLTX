namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores whether the current pause was caused by a generation hash mismatch.
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成是否因哈希差异暂停。</para>
/// <para>拆分来源：Terraria.WorldBuilding.WorldGenerator.Controller。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/WorldGenerator.cs。</para>
/// <para>主要源成员：PausedDueToHashMismatch（第 56 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 363 行。</para>
/// </remarks>
public sealed class WorldGenerationHashMismatchStateComponent
{
  public WorldGenerationHashMismatchStateComponent(bool pausedDueToHashMismatch = false)
  {
    PausedDueToHashMismatch = pausedDueToHashMismatch;
  }

  public bool PausedDueToHashMismatch { get; private set; }
}
