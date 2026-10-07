using System;

namespace Terraria.WorldProgression.Components;

/// <summary>
/// 保存世界暗影珠、祭坛和相关进度事件。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>
/// 主要源成员：shadowOrbSmashed（第 4153 行）； shadowOrbCount（第 4155 行）； altarCount（第 4157 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P16-world-lifecycle-housing-metrics-component-design.md。
/// </para>
/// <para>依据位置：第 172 行。</para>
/// </remarks>
public sealed class WorldProgressionEventStateComponent
{
  public bool ShadowOrbSmashed { get; private set; }

  public int ShadowOrbCount { get; private set; }

  public int AltarCount { get; private set; }

  public void Replace(bool shadowOrbSmashed, int shadowOrbCount, int altarCount)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(shadowOrbCount);
    ArgumentOutOfRangeException.ThrowIfNegative(altarCount);

    ShadowOrbSmashed = shadowOrbSmashed;
    ShadowOrbCount = shadowOrbCount;
    AltarCount = altarCount;
  }

  public void Reset()
  {
    ShadowOrbSmashed = false;
    ShadowOrbCount = 0;
    AltarCount = 0;
  }
}
