using System;

namespace Terraria.WorldStorage;

/// <summary>
/// 保存液体更新预算、停滞统计和紧急处理状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Liquid。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Liquid.cs。</para>
/// <para>
/// 主要源成员：skipCount（第 18 行）； stuckCount（第 20 行）； stuckAmount（第 22 行）； quickFall（第 32 行）；
/// quickSettle（第 34 行）； wetCounter（第 36 行）； panicCounter（第 38 行）； panicMode（第 40 行）； panicY（第 42
/// 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 163 行。</para>
/// <para>
/// 用途：液体调度流程记录活跃工作量、跳过和停滞统计，决定是否使用快速下落、快速稳定或紧急处理策略。
/// </para>
/// <para>
/// 状态变化：Reset 从策略读取 EffectiveMaximumLiquid 并清空所有运行状态；ApplyTick 先要求各计数非负，再整体提交本轮结果。
/// </para>
/// <para>
/// 当前实现：保存状态并校验基础数值。停滞判定、紧急阈值、预算消费和液体移动算法由外部流程决定。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C208，L2（通用可选玩法）。分级理由：液体缓冲、排序、工作项、脏区块、预算和流动状态只在启用液体传播时需要。
/// </para>
/// </remarks>
public sealed class LiquidFlowBudgetAndPanicStateComponent
{
  public LiquidFlowBudgetAndPanicStateComponent(LiquidFlowBudgetPolicy policy)
  {
    ArgumentNullException.ThrowIfNull(policy);
    Reset(policy);
  }

  /// <summary>
  /// 策略给出的有效活跃液体数量上限。
  /// </summary>
  public int EffectiveMaximumLiquid { get; private set; }

  /// <summary>
  /// 本轮记录的活跃液体数量。
  /// </summary>
  public int ActiveLiquidCount { get; private set; }

  /// <summary>
  /// 液体工作被跳过的统计计数。
  /// </summary>
  public int SkipCount { get; private set; }

  /// <summary>
  /// 液体停滞相关计数。
  /// </summary>
  public int StuckCount { get; private set; }

  /// <summary>
  /// 液体停滞数量或程度输入。
  /// </summary>
  public int StuckAmount { get; private set; }

  /// <summary>
  /// 当前是否进入停滞处理状态。
  /// </summary>
  public bool IsStuck { get; private set; }

  /// <summary>
  /// 是否启用快速下落策略。
  /// </summary>
  public bool QuickFall { get; private set; }

  /// <summary>
  /// 是否启用快速稳定策略。
  /// </summary>
  public bool QuickSettle { get; private set; }

  /// <summary>
  /// 原版液体更新 wetCounter 计数。
  /// </summary>
  public int WetCounter { get; private set; }

  /// <summary>
  /// 紧急处理相关计数。
  /// </summary>
  public int PanicCounter { get; private set; }

  /// <summary>
  /// 是否进入紧急液体处理模式。
  /// </summary>
  public bool PanicMode { get; private set; }

  /// <summary>
  /// 紧急处理所用的纵向位置或游标。
  /// </summary>
  public int PanicY { get; private set; }

  /// <summary>
  /// 读取预算策略并清空全部液体运行状态。
  /// </summary>
  public void Reset(LiquidFlowBudgetPolicy policy)
  {
    ArgumentNullException.ThrowIfNull(policy);
    EffectiveMaximumLiquid = policy.EffectiveMaximumLiquid;
    ActiveLiquidCount = 0;
    SkipCount = 0;
    StuckCount = 0;
    StuckAmount = 0;
    IsStuck = false;
    QuickFall = false;
    QuickSettle = false;
    WetCounter = 0;
    PanicCounter = 0;
    PanicMode = false;
    PanicY = 0;
  }

  /// <summary>
  /// 校验各计数非负后整体提交本轮液体调度状态。
  /// </summary>
  internal void ApplyTick(
    int activeLiquidCount,
    int skipCount,
    int stuckCount,
    int stuckAmount,
    bool isStuck,
    bool quickFall,
    bool quickSettle,
    int wetCounter,
    int panicCounter,
    bool panicMode,
    int panicY)
  {
    ValidateNonNegative(activeLiquidCount, nameof(activeLiquidCount));
    ValidateNonNegative(skipCount, nameof(skipCount));
    ValidateNonNegative(stuckCount, nameof(stuckCount));
    ValidateNonNegative(stuckAmount, nameof(stuckAmount));
    ValidateNonNegative(wetCounter, nameof(wetCounter));
    ValidateNonNegative(panicCounter, nameof(panicCounter));
    ValidateNonNegative(panicY, nameof(panicY));

    ActiveLiquidCount = activeLiquidCount;
    SkipCount = skipCount;
    StuckCount = stuckCount;
    StuckAmount = stuckAmount;
    IsStuck = isStuck;
    QuickFall = quickFall;
    QuickSettle = quickSettle;
    WetCounter = wetCounter;
    PanicCounter = panicCounter;
    PanicMode = panicMode;
    PanicY = panicY;
  }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
