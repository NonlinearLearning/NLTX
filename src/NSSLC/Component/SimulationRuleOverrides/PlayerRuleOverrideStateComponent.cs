using System;

namespace Terraria.SimulationRuleOverrides;

/// <summary>
/// 保存玩家创造模式开关及生成倍率的规范化输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 CreativePowers 的开关、滑块和权限状态重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.Creative/CreativePowers.cs。</para>
/// <para>重组说明：原 Power 内部状态按世界、玩家和权限主体分别重组；版本字段是新增状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 1719 行。</para>
/// </remarks>
public readonly record struct PlayerRuleOverrideStateComponent
{
  public PlayerRuleOverrideStateComponent()
    : this(false, true, 0.5f)
  {
  }

  public PlayerRuleOverrideStateComponent(
    bool godmodeEnabled,
    bool farPlacementRangeEnabled,
    float spawnRateNormalized)
  {
    ValidateNormalized(spawnRateNormalized, nameof(spawnRateNormalized));

    GodmodeEnabled = godmodeEnabled;
    FarPlacementRangeEnabled = farPlacementRangeEnabled;
    SpawnRateNormalized = spawnRateNormalized;
  }

  public bool GodmodeEnabled { get; }

  public bool FarPlacementRangeEnabled { get; }

  public float SpawnRateNormalized { get; }

  private static void ValidateNormalized(float value, string parameterName)
  {
    if (float.IsNaN(value) || float.IsInfinity(value) || value < 0.0f || value > 1.0f)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The normalized value must be finite and within [0, 1].");
    }
  }
}
