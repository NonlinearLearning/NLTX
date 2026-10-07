using System.Numerics;

namespace Terraria.Player.Progression;

/// <summary>
/// 保存玩家矿车轨道、翻转、坡道和加速状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：onWrongGround（第 1540 行）； onTrack（第 1542 行）； cartRampTime（第 1544 行）； cartFlip（第 1546 行）；
/// trackBoost（第 1548 行）； lastBoost（第 1550 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 315 行。</para>
/// </remarks>
public sealed class PlayerMountVehicleIntegrationComponent
{
  public bool OnWrongGround { get; internal set; }

  public bool OnTrack { get; internal set; }

  public int CartRampTime { get; internal set; }

  public bool CartFlip { get; internal set; }

  public float TrackBoost { get; internal set; }

  public Vector2 LastBoost { get; internal set; }
}
