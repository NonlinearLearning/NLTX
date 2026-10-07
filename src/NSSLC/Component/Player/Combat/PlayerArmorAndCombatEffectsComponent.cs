namespace Terraria.Player.Combat;

// status: implemented
// componentId: PLAYER.COMP.ARMOR_AND_COMBAT_EFFECTS
// source-members: P08-1275..P08-1281
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存反伤、套装和吸血鬼日照等战斗效果。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：thorns（第 2194 行）； turtleArmor（第 2196 行）； turtleThorns（第 2198 行）； cactusThorns（第 2200 行）；
/// spiderArmor（第 2202 行）； anglerSetSpawnReduction（第 2204 行）； vampireBurningInSunlight（第 2206 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P08-player-environment-armor-component-design.md。</para>
/// <para>依据位置：第 90 行。</para>
/// </remarks>
public sealed class PlayerArmorAndCombatEffectsComponent
{
  public float Thorns { get; internal set; }

  public bool TurtleArmor { get; internal set; }

  public bool TurtleThorns { get; internal set; }

  public bool CactusThorns { get; internal set; }

  public bool SpiderArmor { get; internal set; }

  public bool AnglerSetSpawnReduction { get; internal set; }

  public bool VampireBurningInSunlight { get; internal set; }
}
