using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界邪恶阵营、感染统计及生态变更状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>
/// 主要源成员：totalEvil（第 4119 行）； totalBlood（第 4121 行）； totalGood（第 4123 行）； totalSolid（第 4125 行）。
/// </para>
/// <para>
/// 重组说明：GenerationId、BiomeRevision、ConversionRevision、TilePresenceRevision 用于拆分后的版本或实例生命周期管理。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-version4-world-generation-and-ecology-component-code-draft.md。
/// </para>
/// <para>依据位置：第 799 行。</para>
/// </remarks>
public sealed class BiomeEcologyStateComponent
{
  private IReadOnlySet<string> _biomeTags = FrozenSet<string>.Empty;

  public BiomeEcologyStateComponent(
    long generationId,
    IReadOnlySet<string>? biomeTags = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    _biomeTags = CopyTags(biomeTags);
  }

  public long GenerationId { get; }

  public ulong BiomeRevision { get; init; }

  public IReadOnlySet<string> BiomeTags
  {
    get => _biomeTags;
    init => _biomeTags = CopyTags(value);
  }

  public bool? WorldIsInfected { get; init; }

  public WorldEvilType? WorldEvil { get; init; }

  public ulong ConversionRevision { get; init; }

  public int? TotalEvil { get; init; }

  public int? TotalBlood { get; init; }

  public int? TotalGood { get; init; }

  public int? TotalSolid { get; init; }

  public ulong? TilePresenceRevision { get; init; }

  private static IReadOnlySet<string> CopyTags(IReadOnlySet<string>? source)
  {
    if (source is null)
    {
      return FrozenSet<string>.Empty;
    }

    HashSet<string> copy = new(StringComparer.Ordinal);
    foreach (string tag in source)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(tag);
      copy.Add(tag);
    }

    return copy.ToFrozenSet(StringComparer.Ordinal);
  }
}
