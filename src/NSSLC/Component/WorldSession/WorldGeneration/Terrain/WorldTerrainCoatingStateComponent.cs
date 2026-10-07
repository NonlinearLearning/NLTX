using System;
using System.Collections.Generic;
using Terraria.Content;

namespace Terraria.WorldGeneration.Terrain;

/// <summary>
/// Owns the current coating colors without exposing mutable storage.
/// </summary>
/// <remarks>
/// <para>职责：保存世界地形当前使用的涂层颜色集合。</para>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：_coatingColors（第 4332 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-design.md。
/// </para>
/// <para>依据位置：第 996 行。</para>
/// </remarks>
public sealed class WorldTerrainCoatingStateComponent
{
  private ColorRgba[] _colors = Array.Empty<ColorRgba>();
  private IReadOnlyList<ColorRgba> _colorsView = Array.Empty<ColorRgba>();

  public int Count => _colors.Length;

  public IReadOnlyList<ColorRgba> Colors => _colorsView;

  public void Replace(IReadOnlyList<ColorRgba> colors)
  {
    ArgumentNullException.ThrowIfNull(colors);

    ColorRgba[] copy = new ColorRgba[colors.Count];
    for (int index = 0; index < colors.Count; index++)
    {
      copy[index] = colors[index];
    }

    _colors = copy;
    _colorsView = Array.AsReadOnly(copy);
  }

  public void Clear()
  {
    _colors = Array.Empty<ColorRgba>();
    _colorsView = Array.Empty<ColorRgba>();
  }
}
