namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores immutable run context needed by deterministic generation execution.
/// </summary>
/// <remarks>
/// <para>职责：保存本次世界生成器运行的随机种子。</para>
/// <para>拆分来源：Terraria.WorldBuilding.WorldGenerator。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/WorldGenerator.cs。</para>
/// <para>主要源成员：_seed（第 264 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 19 行。</para>
/// </remarks>
public sealed class WorldGenerationGeneratorExecutionStateComponent
{
  public WorldGenerationGeneratorExecutionStateComponent(int seed)
  {
    Seed = seed;
  }

  public int Seed { get; }
}
