using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成的独立随机流及当前步骤消费游标。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：genRand（第 4370 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-component-execution.md。
/// </para>
/// <para>依据位置：第 375 行。</para>
/// </remarks>
public readonly record struct WorldGenerationRandomStateComponent
{
  public WorldGenerationRandomStateComponent(
    long generationId,
    uint state,
    int streamVersion,
    string? passId = null,
    long cursor = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (streamVersion <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(streamVersion));
    }

    if (cursor < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cursor));
    }

    if (passId is not null)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(passId);
    }

    GenerationId = generationId;
    State = state;
    StreamVersion = streamVersion;
    PassId = passId;
    Cursor = cursor;
  }

  public long GenerationId { get; }

  public uint State { get; }

  public int StreamVersion { get; }

  public string? PassId { get; }

  public long Cursor { get; }
}
