using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the diagnostic and integrity data committed for one generation pass.
/// </summary>
/// <remarks>
/// <para>职责：保存单个世界生成步骤的耗时、哈希、跳过和随机结果。</para>
/// <para>拆分来源：Terraria.WorldBuilding.GenPassResult。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenPassResult.cs。</para>
/// <para>主要源成员：DurationMs（第 6 行）； Hash（第 8 行）； Skipped（第 10 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 506 行。</para>
/// </remarks>
public sealed class WorldGenerationPassResultComponent
{
  public WorldGenerationPassResultComponent(
    long generationId,
    int passIndex,
    string passId,
    int durationMs = 0,
    uint? hash = null,
    bool skipped = false,
    int? randomNextValue = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (passIndex < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(passIndex));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(passId);
    if (durationMs < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(durationMs));
    }

    GenerationId = generationId;
    PassIndex = passIndex;
    PassId = passId;
    DurationMs = durationMs;
    Hash = hash;
    Skipped = skipped;
    RandomNextValue = randomNextValue;
  }

  public long GenerationId { get; }

  public int PassIndex { get; }

  public string PassId { get; }

  public int DurationMs { get; private set; }

  public uint? Hash { get; private set; }

  public bool Skipped { get; private set; }

  public int? RandomNextValue { get; }
}
