namespace Terraria.Player;

// status: implemented-isolated-core
// crossSubsystemOwner: ItemUse/tile release ownership remains unresolved
/// <summary>
/// 保存玩家按键释放、重复输入和悬停控制状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：releaseJump（第 1245 行）； releaseUp（第 1247 行）； releaseLeft（第 1254 行）； releaseRight（第 1256
/// 行）； releaseDown（第 1260 行）； releaseDash（第 1264 行）； tryKeepingHoveringDown（第 1280 行）；
/// tryKeepingHoveringUp（第 1282 行）； leftTimer（第 1288 行）； rightTimer（第 1290 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P10-player-input-control-component-design.md。</para>
/// <para>依据位置：第 176 行。</para>
/// </remarks>
public struct PlayerReleaseAndRepeatStateComponent
{
  public bool ReleaseJump;
  public bool ReleaseUp;
  public bool ReleaseLeft;
  public bool ReleaseRight;
  public bool ReleaseDown;
  public bool ReleaseDash;
  public bool TryKeepingHoveringDown;
  public bool TryKeepingHoveringUp;
  public int LeftTimer;
  public int RightTimer;
}
