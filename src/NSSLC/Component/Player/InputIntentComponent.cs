namespace Terraria.Player;

/// <summary>
/// 保存玩家移动、跳跃和使用动作的输入意图。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：controlLeft（第 1219 行）； controlRight（第 1221 行）； controlUp（第 1223 行）； controlDown（第 1225
/// 行）； controlJump（第 1227 行）； controlUseItem（第 1229 行）； controlUseTile（第 1231 行）； controlTorch（第
/// 1235 行）； controlDash（第 1241 行）； releaseJump（第 1245 行）； releaseUp（第 1247 行）； releaseLeft（第 1254
/// 行）； releaseRight（第 1256 行）； releaseDown（第 1260 行）； releaseDash（第 1264 行）； controlDownHold（第
/// 1270 行）。
/// </para>
/// <para>重组说明：输入来源、序号和发出时刻是输入意图模型新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P10-player-input-control-component-design.md。</para>
/// <para>依据位置：第 130 行。</para>
/// </remarks>
public struct InputIntentComponent
{
  public InputIntentComponent(
    bool moveLeft,
    bool moveRight,
    bool moveUp,
    bool moveDown,
    bool jump,
    bool useItem)
  {
    MoveLeft = moveLeft;
    MoveRight = moveRight;
    MoveUp = moveUp;
    MoveDown = moveDown;
    Jump = jump;
    UseItem = useItem;
    UseTile = false;
    ControlTorch = false;
    ControlDash = false;
    ControlDownHold = false;
    ReleaseJump = false;
    ReleaseUp = false;
    ReleaseLeft = false;
    ReleaseRight = false;
    ReleaseDown = false;
    ReleaseDash = false;
    AlternateUseMode = ItemUseMode.None;
    RequestedDirection = DirectionKind.None;
    IssuedAtTick = null;
    Sequence = 0;
    Source = InputIntentSource.None;
  }

  public bool MoveLeft;
  public bool MoveRight;
  public bool MoveUp;
  public bool MoveDown;
  public bool Jump;
  public bool UseItem;
  public bool UseTile;
  public bool ControlTorch;
  public bool ControlDash;
  public bool ControlDownHold;
  public bool ReleaseJump;
  public bool ReleaseUp;
  public bool ReleaseLeft;
  public bool ReleaseRight;
  public bool ReleaseDown;
  public bool ReleaseDash;
  public ItemUseMode AlternateUseMode;
  public DirectionKind RequestedDirection;
  public long? IssuedAtTick;
  public uint Sequence;
  public InputIntentSource Source;

  public bool WantsDropThroughPlatforms => MoveDown;
  public bool HasDirectionalInput => MoveLeft || MoveRight || MoveUp || MoveDown;
}
