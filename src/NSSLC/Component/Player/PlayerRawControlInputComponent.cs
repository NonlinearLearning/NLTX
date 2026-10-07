namespace Terraria.Player;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for protocol and scheduler integration
/// <summary>
/// 保存玩家方向、跳跃、冲刺和火把的原始控制输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：controlLeft（第 1219 行）； controlRight（第 1221 行）； controlUp（第 1223 行）； controlDown（第 1225
/// 行）； controlJump（第 1227 行）； controlTorch（第 1235 行）； controlDash（第 1241 行）； controlDownHold（第
/// 1270 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P10-player-input-control-component-design.md。</para>
/// <para>依据位置：第 117 行。</para>
/// </remarks>
public struct PlayerRawControlInputComponent
{
  public bool ControlLeft;
  public bool ControlRight;
  public bool ControlUp;
  public bool ControlDown;
  public bool ControlJump;
  public bool ControlTorch;
  public bool ControlDash;
  public bool ControlDownHold;
}
