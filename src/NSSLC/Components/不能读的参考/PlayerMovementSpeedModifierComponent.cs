namespace Terraria.Player;

/// <summary>
/// 保存玩家移动速度的效果修正输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：moveSpeed（第 1883 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 1078 行。</para>
/// <para>
/// 用途：装备和状态效果汇总 MoveSpeed，移动流程再结合基础移动参数计算实际运动。
/// </para>
/// <para>
/// 当前实现：基础值为 1，ResetEffects 恢复到 1。具体倍率公式、速度上限和位置更新由外部移动流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C136，L2（通用可选玩法）。分级理由：基础重力和速度由 MovementPhysics／MovementState 提供。Mo
/// bilitySystem 使用跳跃修正，装备增益和额外动作可以关闭；速度倍率类型未发现消费者。
/// </para>
/// </remarks>
public sealed class PlayerMovementSpeedModifierComponent
{
  /// <summary>
  /// 移动速度修正输入，基础值 1。
  /// </summary>
  public float MoveSpeed { get; internal set; } = 1f;

  /// <summary>
  /// 清除本轮临时效果输入并恢复基础值，供外部效果流程重新汇总。
  /// </summary>
  internal void ResetEffects()
  {
    MoveSpeed = 1f;
  }
}
