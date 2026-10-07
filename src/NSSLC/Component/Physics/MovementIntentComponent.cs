using System.Numerics;

namespace Terraria.Physics;

/// <summary>
/// 保存移动方向、跳跃和穿透平台的请求。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 Player 的方向、跳跃和落台控制输入重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>重组说明：请求来源、序号和发出时刻是统一移动输入模型新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P10-player-input-control-component-design.md。</para>
/// <para>依据位置：第 56 行。</para>
/// </remarks>
public struct MovementIntentComponent
{
  public Vector2 DesiredDirection;
  public long? IssuedAtTick;
  public MovementIntentSource Source;
  public uint Sequence;
  public bool WantsDropThroughPlatforms;
  public bool WantsJump;

  public bool HasDirectionalInput => DesiredDirection != Vector2.Zero;

}
