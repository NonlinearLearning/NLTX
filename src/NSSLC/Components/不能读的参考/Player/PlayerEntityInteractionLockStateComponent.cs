namespace Terraria.Player;

// status: implemented-isolated-core
// source-member: P10-828 isOperatingAnotherEntity
// crossSubsystemOwner: integration-review for entity interaction claim/release ownership
/// <summary>
/// 保存玩家是否正在操作另一个实体的交互占用标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：isOperatingAnotherEntity（第 1272 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P10-player-input-control-component-design.md。</para>
/// <para>依据位置：第 18 行。</para>
/// <para>
/// 用途：交互流程登记玩家对外部实体的操作占用，供后续交互资格检查读取。
/// </para>
/// <para>
/// 当前实现：只有一个可写布尔字段。它不保存被操作实体身份；占用建立、释放和冲突处理由外部交互流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C123，L2（通用可选玩法）。分级理由：挖掘、放置、相邻方块、范围和交互锁是可选建造玩法。
/// </para>
/// </remarks>
public struct PlayerEntityInteractionLockStateComponent
{
  /// <summary>
  /// 玩家是否正在操作另一个实体，默认 false。
  /// </summary>
  public bool IsOperatingAnotherEntity;
}
