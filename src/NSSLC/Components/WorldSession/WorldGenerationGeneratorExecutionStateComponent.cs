namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存一次世界生成执行使用的整数种子。
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
/// <para>
/// 用途：生成执行器读取 Seed，初始化相应随机流程并确定本次生成输入。
/// </para>
/// <para>
/// 当前实现：构造时直接保存种子，只读属性保持该值。随机算法状态、取样和生成步骤推进由外部生成器负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C190，L2（通用可选玩法）。分级理由：生成器的运行阶段、步骤计划、随机流、选择与取消仅在服务端生成世界时需要，预置地图可省去。
/// </para>
/// </remarks>
public sealed class WorldGenerationGeneratorExecutionStateComponent
{
  public WorldGenerationGeneratorExecutionStateComponent(int seed)
  {
    Seed = seed;
  }

  /// <summary>
  /// 本次生成执行使用的整数种子。
  /// </summary>
  public int Seed { get; }
}
