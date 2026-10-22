using System;

namespace Terraria.WorldSession.Calendar;

/// <summary>
/// 保存世界沙暴是否进行、剩余时长及当前和目标强度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.GameContent.Events.Sandstorm。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent.Events/Sandstorm.cs。</para>
/// <para>
/// 主要源成员：Happening（第 14 行）； TimeLeft（第 16 行）； Severity（第 18 行）； IntendedSeverity（第 20 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md。
/// </para>
/// <para>依据位置：第 1251 行。</para>
/// <para>
/// 用途：天气流程记录沙暴时长和强度，生成、环境与表现流程读取当前沙暴状态。
/// </para>
/// <para>
/// 当前实现：构造时调用 Validate，要求 TimeLeft 在 0 到 86400 之间且两种强度有限并在 0 到 1 之间。字段可直接修改，后续校验由调用方显式执
/// 行；计时和强度变化由外部天气流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C205，L2（通用可选玩法）。分级理由：沙尘暴是可选环境过程，时间推进不要求沙尘暴。
/// </para>
/// </remarks>
public sealed class SandstormStateComponent
{
  /// <summary>
  /// 允许的最大沙暴时长 86400 tick。
  /// </summary>
  public const int MaximumDurationTicks = 86400;

  public SandstormStateComponent(
    bool happening = false,
    int timeLeft = 0,
    float severity = 0.0f,
    float intendedSeverity = 0.0f)
  {
    Happening = happening;
    TimeLeft = timeLeft;
    Severity = severity;
    IntendedSeverity = intendedSeverity;
    Validate();
  }

  /// <summary>
  /// 沙暴当前是否正在发生。
  /// </summary>
  public bool Happening;
  /// <summary>
  /// 沙暴剩余 tick 数。
  /// </summary>
  public int TimeLeft;
  /// <summary>
  /// 当前沙暴强度，规则范围 0 到 1。
  /// </summary>
  public float Severity;
  /// <summary>
  /// 外部天气流程希望达到的沙暴强度，规则范围 0 到 1。
  /// </summary>
  public float IntendedSeverity;

  /// <summary>
  /// 检查时长与两种强度的范围，不推进沙暴状态。
  /// </summary>
  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(TimeLeft);
    if (TimeLeft > MaximumDurationTicks)
    {
      throw new ArgumentOutOfRangeException(nameof(TimeLeft));
    }

    ValidateSeverity(Severity, nameof(Severity));
    ValidateSeverity(IntendedSeverity, nameof(IntendedSeverity));
  }

  private static void ValidateSeverity(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0.0f || value > 1.0f)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
