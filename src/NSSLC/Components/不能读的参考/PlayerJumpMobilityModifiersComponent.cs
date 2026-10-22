namespace Terraria.Player.Jump;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Equipment, Spatial, Collision, and persistence
/// <summary>
/// 保存玩家跳跃加成、自动跳跃和下冲相关状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：downDashTime（第 2058 行）； autoJump（第 2116 行）； justJumped（第 2118 行）； jumpSpeedBoost（第 2120
/// 行）； extraFall（第 2122 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 861 行。</para>
/// <para>
/// 用途：移动与输入流程读取跳跃速度加成、自动跳跃许可和本次起跳标记，并维护下冲与额外坠落状态。
/// </para>
/// <para>
/// 当前实现：只有可写属性。JustJumped 的消费时机、计时推进和实际速度计算由外部跳跃流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C103，L2（通用可选玩法）。分级理由：基础重力和速度由 MovementPhysics／MovementState 提供。Mo
/// bilitySystem 使用跳跃修正，装备增益和额外动作可以关闭；速度倍率类型未发现消费者。
/// </para>
/// </remarks>
public sealed class PlayerJumpMobilityModifiersComponent
{
  /// <summary>
  /// 向下冲刺相关计时。
  /// </summary>
  public int DownDashTime { get; set; }

  /// <summary>
  /// 是否允许持续输入触发自动跳跃。
  /// </summary>
  public bool AutoJump { get; set; }

  /// <summary>
  /// 本轮是否刚刚发生起跳。
  /// </summary>
  public bool JustJumped { get; set; }

  /// <summary>
  /// 跳跃速度的附加输入。
  /// </summary>
  public float JumpSpeedBoost { get; set; }

  /// <summary>
  /// 额外坠落相关输入，具体单位和公式由移动流程定义。
  /// </summary>
  public int ExtraFall { get; set; }
}
