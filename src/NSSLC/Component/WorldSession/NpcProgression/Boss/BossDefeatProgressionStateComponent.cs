using System;

namespace Terraria.WorldSession.NpcProgression.Boss;

/// <summary>
/// 保存世界各首领的击败进度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：downedBoss1（第 6211 行）； downedBoss2（第 6213 行）； downedBoss3（第 6215 行）； downedQueenBee（第
/// 6217 行）； downedSlimeKing（第 6219 行）； downedPlantBoss（第 6229 行）； downedGolemBoss（第 6231 行）；
/// downedFishron（第 6235 行）； downedAncientCultist（第 6247 行）； downedMoonlord（第 6249 行）；
/// downedEmpressOfLight（第 6259 行）； downedQueenSlime（第 6261 行）； downedDeerclops（第 6263 行）；
/// downedMechBossAny（第 6285 行）； downedMechBoss1（第 6287 行）； downedMechBoss2（第 6289 行）；
/// downedMechBoss3（第 6291 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 136 行。</para>
/// </remarks>
public sealed class BossDefeatProgressionStateComponent
{
  public bool DownedBoss1 { get; private set; }

  public bool DownedBoss2 { get; private set; }

  public bool DownedBoss3 { get; private set; }

  public bool DownedQueenBee { get; private set; }

  public bool DownedSlimeKing { get; private set; }

  public bool DownedPlantBoss { get; private set; }

  public bool DownedGolemBoss { get; private set; }

  public bool DownedFishron { get; private set; }

  public bool DownedAncientCultist { get; private set; }

  public bool DownedMoonlord { get; private set; }

  public bool DownedEmpressOfLight { get; private set; }

  public bool DownedQueenSlime { get; private set; }

  public bool DownedDeerclops { get; private set; }

  public bool DownedMechBossAny { get; private set; }

  public bool DownedMechBoss1 { get; private set; }

  public bool DownedMechBoss2 { get; private set; }

  public bool DownedMechBoss3 { get; private set; }

  public bool MarkDefeated(BossDefeatProgressionKind bossKind)
  {
    if (IsDefeated(bossKind))
    {
      return false;
    }

    switch (bossKind)
    {
      case BossDefeatProgressionKind.Boss1:
        DownedBoss1 = true;
        break;
      case BossDefeatProgressionKind.Boss2:
        DownedBoss2 = true;
        break;
      case BossDefeatProgressionKind.Boss3:
        DownedBoss3 = true;
        break;
      case BossDefeatProgressionKind.QueenBee:
        DownedQueenBee = true;
        break;
      case BossDefeatProgressionKind.SlimeKing:
        DownedSlimeKing = true;
        break;
      case BossDefeatProgressionKind.PlantBoss:
        DownedPlantBoss = true;
        break;
      case BossDefeatProgressionKind.GolemBoss:
        DownedGolemBoss = true;
        break;
      case BossDefeatProgressionKind.Fishron:
        DownedFishron = true;
        break;
      case BossDefeatProgressionKind.AncientCultist:
        DownedAncientCultist = true;
        break;
      case BossDefeatProgressionKind.Moonlord:
        DownedMoonlord = true;
        break;
      case BossDefeatProgressionKind.EmpressOfLight:
        DownedEmpressOfLight = true;
        break;
      case BossDefeatProgressionKind.QueenSlime:
        DownedQueenSlime = true;
        break;
      case BossDefeatProgressionKind.Deerclops:
        DownedDeerclops = true;
        break;
      case BossDefeatProgressionKind.MechBossAny:
        DownedMechBossAny = true;
        break;
      case BossDefeatProgressionKind.MechBoss1:
        DownedMechBoss1 = true;
        DownedMechBossAny = true;
        break;
      case BossDefeatProgressionKind.MechBoss2:
        DownedMechBoss2 = true;
        DownedMechBossAny = true;
        break;
      case BossDefeatProgressionKind.MechBoss3:
        DownedMechBoss3 = true;
        DownedMechBossAny = true;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(bossKind), bossKind, "Unknown boss defeat progression kind.");
    }

    return true;
  }

  public bool IsDefeated(BossDefeatProgressionKind bossKind)
  {
    return bossKind switch
    {
      BossDefeatProgressionKind.Boss1 => DownedBoss1,
      BossDefeatProgressionKind.Boss2 => DownedBoss2,
      BossDefeatProgressionKind.Boss3 => DownedBoss3,
      BossDefeatProgressionKind.QueenBee => DownedQueenBee,
      BossDefeatProgressionKind.SlimeKing => DownedSlimeKing,
      BossDefeatProgressionKind.PlantBoss => DownedPlantBoss,
      BossDefeatProgressionKind.GolemBoss => DownedGolemBoss,
      BossDefeatProgressionKind.Fishron => DownedFishron,
      BossDefeatProgressionKind.AncientCultist => DownedAncientCultist,
      BossDefeatProgressionKind.Moonlord => DownedMoonlord,
      BossDefeatProgressionKind.EmpressOfLight => DownedEmpressOfLight,
      BossDefeatProgressionKind.QueenSlime => DownedQueenSlime,
      BossDefeatProgressionKind.Deerclops => DownedDeerclops,
      BossDefeatProgressionKind.MechBossAny => DownedMechBossAny,
      BossDefeatProgressionKind.MechBoss1 => DownedMechBoss1,
      BossDefeatProgressionKind.MechBoss2 => DownedMechBoss2,
      BossDefeatProgressionKind.MechBoss3 => DownedMechBoss3,
      _ => throw new ArgumentOutOfRangeException(nameof(bossKind), bossKind, "Unknown boss defeat progression kind."),
    };
  }

  public void Reset()
  {
    DownedBoss1 = false;
    DownedBoss2 = false;
    DownedBoss3 = false;
    DownedQueenBee = false;
    DownedSlimeKing = false;
    DownedPlantBoss = false;
    DownedGolemBoss = false;
    DownedFishron = false;
    DownedAncientCultist = false;
    DownedMoonlord = false;
    DownedEmpressOfLight = false;
    DownedQueenSlime = false;
    DownedDeerclops = false;
    DownedMechBossAny = false;
    DownedMechBoss1 = false;
    DownedMechBoss2 = false;
    DownedMechBoss3 = false;
  }
}
