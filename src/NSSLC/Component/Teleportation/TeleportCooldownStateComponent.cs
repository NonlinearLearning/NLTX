namespace Terraria.Teleportation;

/// <summary>
/// 保存主体通用传送冷却及其来源和开始时刻。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Player.Teleport 的旅行流程及 NLTX 通用传送冷却模型重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>重组说明：通用冷却通道、来源与开始时刻是 NLTX 新增状态，不来自单一原版冷却字段。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-teleportation-and-traversal-code-component-draft.md。
/// </para>
/// <para>依据位置：第 315 行。</para>
/// </remarks>
public struct TeleportCooldownStateComponent
{
  public bool IsOnCooldown => RemainingTicks > 0;
  public int RemainingTicks;
  public TeleportSource Source;
  public long? StartedAtTick;
}
