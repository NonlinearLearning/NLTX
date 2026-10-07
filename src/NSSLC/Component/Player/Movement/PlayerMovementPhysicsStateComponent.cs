namespace Terraria.Player.Movement;

// status: implemented-partial
// componentId: PLAYER.COMP.MOVEMENT_PHYSICS_STATE
// source-members: P08-1147..P08-1151
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存玩家重力、最大落速、跑速及加减速度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：gravity（第 1927 行）； maxFallSpeed（第 1929 行）； maxRunSpeed（第 1931 行）； runAcceleration（第 1933
/// 行）； runSlowdown（第 1935 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P08-player-environment-armor-component-design.md。</para>
/// <para>依据位置：第 75 行。</para>
/// </remarks>
public sealed class PlayerMovementPhysicsStateComponent
{
  // This baseline mirrors the Version4 instance initialization; the default owner remains under review.
  public float Gravity { get; internal set; } = 0.4f;

  public float MaxFallSpeed { get; internal set; } = 10f;

  public float MaxRunSpeed { get; internal set; } = 3f;

  public float RunAcceleration { get; internal set; } = 0.08f;

  public float RunSlowdown { get; internal set; } = 0.2f;
}
