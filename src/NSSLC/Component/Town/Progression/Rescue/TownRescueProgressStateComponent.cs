using System;

namespace Terraria.Town.Progression.Rescue;

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
