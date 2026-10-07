using System.Numerics;

namespace Terraria.Player.Armor;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Spatial and presentation output
/// <summary>
/// 保存日耀盾的位置和速度序列。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：solarShieldPos（第 582 行）； solarShieldVel（第 584 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 275 行。</para>
/// </remarks>
public sealed class PlayerSolarShieldKinematicsComponent
{
  public const int ShieldCapacity = 3;

  public Vector2[] Positions { get; } = new Vector2[ShieldCapacity];

  public Vector2[] Velocities { get; } = new Vector2[ShieldCapacity];
}
