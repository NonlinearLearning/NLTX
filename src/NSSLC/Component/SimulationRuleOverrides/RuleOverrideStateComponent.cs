using System;

namespace Terraria.SimulationRuleOverrides;

/// <summary>
/// 保存世界时间、难度、天气和生态规则的规范化覆盖输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 CreativePowers 的开关、滑块和权限状态重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.Creative/CreativePowers.cs。</para>
/// <para>重组说明：原 Power 内部状态按世界、玩家和权限主体分别重组；版本字段是新增状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 1717 行。</para>
/// </remarks>
public readonly record struct RuleOverrideStateComponent
{
  public RuleOverrideStateComponent(
    float timeRateNormalized = 0.0f,
    float difficultyNormalized = 0.0f,
    float windDirectionAndStrengthNormalized = 0.0f,
    float rainStrengthNormalized = 0.0f,
    bool freezeTimeEnabled = false,
    bool freezeWindEnabled = false,
    bool freezeRainEnabled = false,
    bool stopBiomeSpreadEnabled = false,
    long? authorityRevision = null)
  {
    ValidateNormalized(timeRateNormalized, nameof(timeRateNormalized));
    ValidateNormalized(difficultyNormalized, nameof(difficultyNormalized));
    ValidateNormalized(
      windDirectionAndStrengthNormalized,
      nameof(windDirectionAndStrengthNormalized));
    ValidateNormalized(rainStrengthNormalized, nameof(rainStrengthNormalized));
    ValidateRevision(authorityRevision, nameof(authorityRevision));

    TimeRateNormalized = timeRateNormalized;
    DifficultyNormalized = difficultyNormalized;
    WindDirectionAndStrengthNormalized = windDirectionAndStrengthNormalized;
    RainStrengthNormalized = rainStrengthNormalized;
    FreezeTimeEnabled = freezeTimeEnabled;
    FreezeWindEnabled = freezeWindEnabled;
    FreezeRainEnabled = freezeRainEnabled;
    StopBiomeSpreadEnabled = stopBiomeSpreadEnabled;
    AuthorityRevision = authorityRevision;
  }

  public float TimeRateNormalized { get; }

  public float DifficultyNormalized { get; }

  public float WindDirectionAndStrengthNormalized { get; }

  public float RainStrengthNormalized { get; }

  public bool FreezeTimeEnabled { get; }

  public bool FreezeWindEnabled { get; }

  public bool FreezeRainEnabled { get; }

  public bool StopBiomeSpreadEnabled { get; }

  public long? AuthorityRevision { get; }

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

  private static void ValidateRevision(long? value, string parameterName)
  {
    if (value is < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The revision must be non-negative when present.");
    }
  }
}
