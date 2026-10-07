using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the snapshot cadence selected for the current generation run.
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成快照的采样频率。</para>
/// <para>拆分来源：Terraria.WorldBuilding.WorldGenerator.Controller。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/WorldGenerator.cs。</para>
/// <para>主要源成员：SnapshotFrequency（第 58 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 364 行。</para>
/// </remarks>
public sealed class WorldGenerationSnapshotPolicyComponent
{
  public enum Frequency : sbyte
  {
    None = -1,
    Manual,
    Automatic,
    Always,
  }

  public WorldGenerationSnapshotPolicyComponent(
    Frequency snapshotFrequency = Frequency.None)
  {
    if (!Enum.IsDefined(snapshotFrequency))
    {
      throw new ArgumentOutOfRangeException(nameof(snapshotFrequency));
    }

    SnapshotFrequency = snapshotFrequency;
  }

  public Frequency SnapshotFrequency { get; private set; }
}
