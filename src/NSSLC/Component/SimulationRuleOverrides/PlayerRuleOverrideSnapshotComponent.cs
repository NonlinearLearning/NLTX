using System;

namespace Terraria.SimulationRuleOverrides;

/// <summary>
/// 保存本轮生效的玩家创造模式规则快照。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 CreativePowers 的规则读取与生效流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.Creative/CreativePowers.cs。</para>
/// <para>重组说明：生效快照、Tick 和来源版本是拆分时新增的派生结果。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-07-version4-second-round-review-merged.md。</para>
/// <para>依据位置：第 1720 行。</para>
/// </remarks>
public readonly record struct PlayerRuleOverrideSnapshotComponent
{
  public PlayerRuleOverrideSnapshotComponent(
    long tick,
    bool godmodeEnabled,
    bool farPlacementRangeEnabled,
    float spawnRateMultiplier,
    bool disableSpawns,
    long? sourceRevision = null)
  {
    ValidateTick(tick, nameof(tick));
    ValidateRevision(sourceRevision, nameof(sourceRevision));
    ValidateRange(spawnRateMultiplier, 0.1f, 10.0f, nameof(spawnRateMultiplier));

    Tick = tick;
    SourceRevision = sourceRevision;
    GodmodeEnabled = godmodeEnabled;
    FarPlacementRangeEnabled = farPlacementRangeEnabled;
    SpawnRateMultiplier = spawnRateMultiplier;
    DisableSpawns = disableSpawns;
  }

  public long Tick { get; }

  public long? SourceRevision { get; }

  public bool GodmodeEnabled { get; }

  public bool FarPlacementRangeEnabled { get; }

  public float SpawnRateMultiplier { get; }

  public bool DisableSpawns { get; }

  private static void ValidateTick(long value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The tick must be non-negative.");
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

  private static void ValidateRange(
    float value,
    float minimum,
    float maximum,
    string parameterName)
  {
    if (float.IsNaN(value)
      || float.IsInfinity(value)
      || value < minimum
      || value > maximum)
    {
      throw new ArgumentOutOfRangeException(
        parameterName,
        value,
        "The value must be finite and within its candidate range.");
    }
  }
}
