namespace Terraria.Player.Grapple;

// status: implemented-isolated-core
// crossSubsystemOwner: Equipment, Jump, Flight, Carpet, Spatial, effects, and persistence
/// <summary>
/// 保存火箭推进剩余时间、释放和冷却状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：rocketTime（第 2140 行）； rocketTimeMax（第 2142 行）； rocketDelay（第 2144 行）； rocketRelease（第
/// 2150 行）； canRocket（第 2158 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 925 行。</para>
/// </remarks>
public sealed class PlayerRocketStateComponent
{
  public int RocketTime { get; set; }

  public int RocketTimeMax { get; set; } = 7;

  public int RocketDelay { get; set; }

  public int RocketEffectDelay { get; set; }

  public bool RocketRelease { get; set; }

  public bool CanRocket { get; set; }
}
