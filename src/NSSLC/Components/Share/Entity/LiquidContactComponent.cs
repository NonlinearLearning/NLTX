using System;

using EntityEcs.Components;

namespace Terraria.SpatialSimulation.Components;

// status: proposed
// componentId: SPATIAL.COMP.LIQUID_CONTACT
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存空间主体各类液体接触结果，并维护连续接触计数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>
/// 主要源成员：wet（第 26 行）； shimmerWet（第 28 行）； honeyWet（第 30 行）； wetCount（第 32 行）； lavaWet（第 34 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-spatial-simulation-component-design.md。</para>
/// <para>依据位置：第 411 行。</para>
/// <para>
/// 用途：外部空间求解器在每个游戏 tick 提交接触结果，其他环境规则读取结果和接触持续计数。
/// </para>
/// <para>
/// 状态变化：Replace 在相邻 tick 且前后都有接触时将 WetTickCount 加一，上限 255；新接触从 1 开始，无接触归零。同一 tick 的重复提交
/// 不算连续的一步。
/// </para>
/// <para>
/// 当前实现：校验 tick 非负和主要液体枚举值；Clear 清除全部结果。液体探测和环境伤害由外部系统执行。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C161，L2（通用可选玩法）。分级理由：保留水、熔岩等介质交互时才需要该模型。
/// </para>
/// </remarks>
public struct LiquidContactComponent
{
  /// <summary>
  /// 最近提交接触结果的游戏 tick；尚无结果时为空。
  /// </summary>
  public long? ResolvedAtTick;
  /// <summary>
  /// 是否具有 wet 液体接触标记。
  /// </summary>
  public bool IsWet;
  /// <summary>
  /// 是否接触熔岩。
  /// </summary>
  public bool IsLavaWet;
  /// <summary>
  /// 是否接触蜂蜜。
  /// </summary>
  public bool IsHoneyWet;
  /// <summary>
  /// 是否接触微光。
  /// </summary>
  public bool IsShimmerWet;
  /// <summary>
  /// 连续相邻 tick 的液体接触次数，最多 255。
  /// </summary>
  public byte WetTickCount;
  /// <summary>
  /// 有接触时记录的主要液体类别；无接触时清为空。
  /// </summary>
  public LiquidKind? DominantLiquidKind;

  /// <summary>
  /// 任一接触标记为 true 时返回 true。
  /// </summary>
  public bool HasAnyLiquidContact =>
    IsWet || IsLavaWet || IsHoneyWet || IsShimmerWet;

  /// <summary>
  /// 接触熔岩、蜂蜜或微光时返回 true。
  /// </summary>
  public bool HasNonWaterLiquidContact =>
    IsLavaWet || IsHoneyWet || IsShimmerWet;

  /// <summary>
  /// 提交指定 tick 的接触结果，并按相邻 tick 规则重算连续接触计数。
  /// </summary>
  public void Replace(
    long tick,
    bool isWet,
    bool isLavaWet,
    bool isHoneyWet,
    bool isShimmerWet,
    LiquidKind? dominantLiquidKind = null)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(tick),
        tick,
        "Tick must be non-negative.");
    }

    if (dominantLiquidKind.HasValue &&
      !Enum.IsDefined(dominantLiquidKind.Value))
    {
      throw new ArgumentOutOfRangeException(
        nameof(dominantLiquidKind),
        dominantLiquidKind,
        "Liquid kind must be a defined value.");
    }

    bool hadContinuousContact =
      HasAnyLiquidContact &&
      ResolvedAtTick.HasValue &&
      ResolvedAtTick.Value + 1 == tick;

    IsWet = isWet;
    IsLavaWet = isLavaWet;
    IsHoneyWet = isHoneyWet;
    IsShimmerWet = isShimmerWet;

    bool hasContact = HasAnyLiquidContact;
    DominantLiquidKind = hasContact ? dominantLiquidKind : null;
    WetTickCount = hasContact
      ? hadContinuousContact
        ? (byte)Math.Min(byte.MaxValue, WetTickCount + 1)
        : (byte)1
      : (byte)0;
    ResolvedAtTick = tick;
  }

  /// <summary>
  /// 清空接触标记、主要液体、计数和求解时刻。
  /// </summary>
  public void Clear()
  {
    ResolvedAtTick = null;
    IsWet = false;
    IsLavaWet = false;
    IsHoneyWet = false;
    IsShimmerWet = false;
    WetTickCount = 0;
    DominantLiquidKind = null;
  }
}
