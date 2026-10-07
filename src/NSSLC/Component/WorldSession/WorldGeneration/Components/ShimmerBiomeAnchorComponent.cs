using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成的微光群落锚点。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>主要源成员：shimmerPosition（第 278 行）。</para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 1663 行。</para>
/// </remarks>
public sealed class ShimmerBiomeAnchorComponent
{
  public ShimmerBiomeAnchorComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public bool HasAnchor { get; private set; }

  public ShimmerBiomeAnchorPoint Position { get; private set; }

  public ShimmerBiomeAnchorSnapshot CreateSnapshot()
  {
    return new ShimmerBiomeAnchorSnapshot(
      GenerationId,
      HasAnchor,
      Position);
  }

  internal void Publish(ShimmerBiomeAnchorPoint position)
  {
    Position = position;
    HasAnchor = true;
  }

  internal void Clear()
  {
    Position = default;
    HasAnchor = false;
  }
}
