using System;

namespace Terraria.WorldSession.NpcProgression.Events;

public sealed class EventDefeatProgressionStateComponent
{
  public bool DownedGoblins { get; private set; }

  public bool DownedFrost { get; private set; }

  public bool DownedPirates { get; private set; }

  public bool DownedClown { get; private set; }

  public bool DownedMartians { get; private set; }

  public bool DownedHalloweenTree { get; private set; }

  public bool DownedHalloweenKing { get; private set; }

  public bool DownedChristmasIceQueen { get; private set; }

  public bool DownedChristmasTree { get; private set; }

  public bool DownedChristmasSantank { get; private set; }

  public bool DownedTowerSolar { get; private set; }

  public bool DownedTowerVortex { get; private set; }

  public bool DownedTowerNebula { get; private set; }

  public bool DownedTowerStardust { get; private set; }

  public bool MarkDefeated(EventDefeatProgressionKind eventKind)
  {
    if (IsDefeated(eventKind))
    {
      return false;
    }

    switch (eventKind)
    {
      case EventDefeatProgressionKind.Goblins:
        DownedGoblins = true;
        break;
      case EventDefeatProgressionKind.Frost:
        DownedFrost = true;
        break;
      case EventDefeatProgressionKind.Pirates:
        DownedPirates = true;
        break;
      case EventDefeatProgressionKind.Clown:
        DownedClown = true;
        break;
      case EventDefeatProgressionKind.Martians:
        DownedMartians = true;
        break;
      case EventDefeatProgressionKind.HalloweenTree:
        DownedHalloweenTree = true;
        break;
      case EventDefeatProgressionKind.HalloweenKing:
        DownedHalloweenKing = true;
        break;
      case EventDefeatProgressionKind.ChristmasIceQueen:
        DownedChristmasIceQueen = true;
        break;
      case EventDefeatProgressionKind.ChristmasTree:
        DownedChristmasTree = true;
        break;
      case EventDefeatProgressionKind.ChristmasSantank:
        DownedChristmasSantank = true;
        break;
      case EventDefeatProgressionKind.TowerSolar:
        DownedTowerSolar = true;
        break;
      case EventDefeatProgressionKind.TowerVortex:
        DownedTowerVortex = true;
        break;
      case EventDefeatProgressionKind.TowerNebula:
        DownedTowerNebula = true;
        break;
      case EventDefeatProgressionKind.TowerStardust:
        DownedTowerStardust = true;
        break;
      default:
        throw new ArgumentOutOfRangeException(
          nameof(eventKind),
          eventKind,
          "Unknown event defeat progression kind.");
    }

    return true;
  }

  public bool IsDefeated(EventDefeatProgressionKind eventKind)
  {
    return eventKind switch
    {
      EventDefeatProgressionKind.Goblins => DownedGoblins,
      EventDefeatProgressionKind.Frost => DownedFrost,
      EventDefeatProgressionKind.Pirates => DownedPirates,
      EventDefeatProgressionKind.Clown => DownedClown,
      EventDefeatProgressionKind.Martians => DownedMartians,
      EventDefeatProgressionKind.HalloweenTree => DownedHalloweenTree,
      EventDefeatProgressionKind.HalloweenKing => DownedHalloweenKing,
      EventDefeatProgressionKind.ChristmasIceQueen => DownedChristmasIceQueen,
      EventDefeatProgressionKind.ChristmasTree => DownedChristmasTree,
      EventDefeatProgressionKind.ChristmasSantank => DownedChristmasSantank,
      EventDefeatProgressionKind.TowerSolar => DownedTowerSolar,
      EventDefeatProgressionKind.TowerVortex => DownedTowerVortex,
      EventDefeatProgressionKind.TowerNebula => DownedTowerNebula,
      EventDefeatProgressionKind.TowerStardust => DownedTowerStardust,
      _ => throw new ArgumentOutOfRangeException(
        nameof(eventKind),
        eventKind,
        "Unknown event defeat progression kind."),
    };
  }

  public void Reset()
  {
    DownedGoblins = false;
    DownedFrost = false;
    DownedPirates = false;
    DownedClown = false;
    DownedMartians = false;
    DownedHalloweenTree = false;
    DownedHalloweenKing = false;
    DownedChristmasIceQueen = false;
    DownedChristmasTree = false;
    DownedChristmasSantank = false;
    DownedTowerSolar = false;
    DownedTowerVortex = false;
    DownedTowerNebula = false;
    DownedTowerStardust = false;
  }
}
