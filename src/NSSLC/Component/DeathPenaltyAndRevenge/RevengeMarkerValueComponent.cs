using System;

namespace Terraria.DeathPenaltyAndRevenge;

/// <summary>
/// 保存复仇标记对应的 NPC 基础价值和遗失金币价值。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.CoinLossRevengeSystem.RevengeMarker。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/CoinLossRevengeSystem.cs。</para>
/// <para>主要源成员：_baseValue（第 44 行）； _coinsValue（第 46 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 198 行。</para>
/// </remarks>
public sealed class RevengeMarkerValueComponent
{
  public RevengeMarkerValueComponent(float baseValue, int coinsValue)
  {
    if (!float.IsFinite(baseValue))
    {
      throw new ArgumentOutOfRangeException(
        nameof(baseValue),
        baseValue,
        "Base value must be finite.");
    }

    BaseValue = baseValue;
    CoinsValue = coinsValue;
  }

  public float BaseValue { get; }

  public int CoinsValue { get; }
}
