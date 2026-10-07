namespace Terraria.Player.Grapple;

// status: implemented-isolated-core
// crossSubsystemOwner: Equipment definition, vanity presentation, effects, and persistence
/// <summary>
/// 保存火箭靴能力与外观等级。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：rocketBoots（第 2154 行）； vanityRocketBoots（第 2156 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 932 行。</para>
/// </remarks>
public sealed class PlayerRocketCapabilityComponent
{
  public int BootLevel { get; set; }

  public int VanityBootLevel { get; set; }
}
