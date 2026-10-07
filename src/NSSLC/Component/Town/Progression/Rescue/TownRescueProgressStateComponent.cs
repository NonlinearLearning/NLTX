using System;

namespace Terraria.Town.Progression.Rescue;

/// <summary>
/// 保存城镇 NPC 的救援解锁进度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：savedTaxCollector（第 6151 行）； savedGoblin（第 6153 行）； savedWizard（第 6155 行）； savedMech（第
/// 6157 行）； savedAngler（第 6159 行）； savedStylist（第 6161 行）； savedBartender（第 6163 行）；
/// savedGolfer（第 6165 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 130 行。</para>
/// </remarks>
public sealed class TownRescueProgressStateComponent
{
  public bool SavedTaxCollector { get; private set; }

  public bool SavedGoblin { get; private set; }

  public bool SavedWizard { get; private set; }

  public bool SavedMechanic { get; private set; }

  public bool SavedAngler { get; private set; }

  public bool SavedStylist { get; private set; }

  public bool SavedBartender { get; private set; }

  public bool SavedGolfer { get; private set; }

  public bool MarkRescued(TownRescueKind rescueKind)
  {
    bool wasNew = !IsRescued(rescueKind);
    if (!wasNew)
    {
      return false;
    }

    switch (rescueKind)
    {
      case TownRescueKind.TaxCollector:
        SavedTaxCollector = true;
        break;
      case TownRescueKind.Goblin:
        SavedGoblin = true;
        break;
      case TownRescueKind.Wizard:
        SavedWizard = true;
        break;
      case TownRescueKind.Mechanic:
        SavedMechanic = true;
        break;
      case TownRescueKind.Angler:
        SavedAngler = true;
        break;
      case TownRescueKind.Stylist:
        SavedStylist = true;
        break;
      case TownRescueKind.Bartender:
        SavedBartender = true;
        break;
      case TownRescueKind.Golfer:
        SavedGolfer = true;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(rescueKind), rescueKind, "Unknown town rescue kind.");
    }

    return true;
  }

  public bool IsRescued(TownRescueKind rescueKind)
  {
    return rescueKind switch
    {
      TownRescueKind.TaxCollector => SavedTaxCollector,
      TownRescueKind.Goblin => SavedGoblin,
      TownRescueKind.Wizard => SavedWizard,
      TownRescueKind.Mechanic => SavedMechanic,
      TownRescueKind.Angler => SavedAngler,
      TownRescueKind.Stylist => SavedStylist,
      TownRescueKind.Bartender => SavedBartender,
      TownRescueKind.Golfer => SavedGolfer,
      _ => throw new ArgumentOutOfRangeException(nameof(rescueKind), rescueKind, "Unknown town rescue kind."),
    };
  }

  public void Reset()
  {
    SavedTaxCollector = false;
    SavedGoblin = false;
    SavedWizard = false;
    SavedMechanic = false;
    SavedAngler = false;
    SavedStylist = false;
    SavedBartender = false;
    SavedGolfer = false;
  }
}
