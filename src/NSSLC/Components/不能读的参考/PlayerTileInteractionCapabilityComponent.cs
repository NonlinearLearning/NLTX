namespace Terraria.Player;

// Holds the current-tick tile interaction capabilities derived from equipment.
/// <summary>
/// 保存装备提供的方块交互范围、放置速度和放墙速度能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：equippedAnyTileRangeAcc（第 2410 行）； equippedAnyTileSpeedAcc（第 2412 行）；
/// equippedAnyWallSpeedAcc（第 2414 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 496 行。</para>
/// <para>
/// 用途：装备效果流程写入当前轮能力，方块交互流程读取是否具备对应饰品加成。
/// </para>
/// <para>
/// 当前实现：只有可写布尔属性。具体范围数值、速度修正和每轮重建由外部装备与交互流程处理。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C147，L2（通用可选玩法）。分级理由：挖掘、放置、相邻方块、范围和交互锁是可选建造玩法。
/// </para>
/// </remarks>
public sealed class PlayerTileInteractionCapabilityComponent
{
  /// <summary>
  /// 是否具有提供方块交互范围加成的饰品能力。
  /// </summary>
  public bool HasTileRangeAccessory { get; set; }

  /// <summary>
  /// 是否具有提供方块放置速度加成的饰品能力。
  /// </summary>
  public bool HasTileSpeedAccessory { get; set; }

  /// <summary>
  /// 是否具有提供放墙速度加成的饰品能力。
  /// </summary>
  public bool HasWallSpeedAccessory { get; set; }
}
