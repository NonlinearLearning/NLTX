namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores whether a generation run pauses when a committed pass hash differs.
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成遇到哈希差异时的暂停策略。</para>
/// <para>拆分来源：Terraria.WorldBuilding.WorldGenerator.Controller。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/WorldGenerator.cs。</para>
/// <para>主要源成员：PauseOnHashMismatch（第 54 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 362 行。</para>
/// </remarks>
public sealed class WorldGenerationHashMismatchPolicyComponent
{
  public WorldGenerationHashMismatchPolicyComponent(bool pauseOnHashMismatch = true)
  {
    PauseOnHashMismatch = pauseOnHashMismatch;
  }

  public bool PauseOnHashMismatch { get; private set; }
}
