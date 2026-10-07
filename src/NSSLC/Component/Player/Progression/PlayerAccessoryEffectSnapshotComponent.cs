namespace Terraria.Player.Progression;

// status: local-tick-core-verified; production-integration: unknown
// source-members: brokenMirrorBadLuck, flowerBoots, fairyBoots, hellfireTreads, moonLordLegs,
// deadMansSweater, arcticDivingGear, coolWhipBuff, cobWhipBuff, wearsRobe, magicCuffs,
// coldDash, sailDash, desertDash, desertBoots, eyeSpring, scope
/// <summary>
/// 保存玩家本轮饰品效果的能力快照。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：brokenMirrorBadLuck（第 1505 行）； flowerBoots（第 1522 行）； fairyBoots（第 1524 行）；
/// hellfireTreads（第 1526 行）； moonLordLegs（第 1528 行）； deadMansSweater（第 1530 行）；
/// arcticDivingGear（第 1532 行）； coolWhipBuff（第 1534 行）； cobWhipBuff（第 1536 行）； wearsRobe（第 1538
/// 行）； magicCuffs（第 1674 行）； coldDash（第 1676 行）； sailDash（第 1678 行）； desertDash（第 1680 行）；
/// desertBoots（第 1682 行）； eyeSpring（第 1684 行）； scope（第 1688 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 322 行。</para>
/// </remarks>
public sealed class PlayerAccessoryEffectSnapshotComponent
{
  public bool BrokenMirrorBadLuck { get; internal set; }

  public bool FlowerBoots { get; internal set; }

  public bool FairyBoots { get; internal set; }

  public bool HellfireTreads { get; internal set; }

  public bool MoonLordLegs { get; internal set; }

  public bool DeadMansSweater { get; internal set; }

  public bool ArcticDivingGear { get; internal set; }

  public bool CoolWhipBuff { get; internal set; }

  public bool CobWhipBuff { get; internal set; }

  public bool WearsRobe { get; internal set; }

  public bool MagicCuffs { get; internal set; }

  public bool ColdDash { get; internal set; }

  public bool SailDash { get; internal set; }

  public bool DesertDash { get; internal set; }

  public bool DesertBoots { get; internal set; }

  public bool EyeSpring { get; internal set; }

  public bool Scope { get; internal set; }
}
