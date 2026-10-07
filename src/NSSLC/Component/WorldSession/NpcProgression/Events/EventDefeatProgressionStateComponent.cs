using System;

namespace Terraria.WorldSession.NpcProgression.Events;

/// <summary>
/// 保存世界入侵、季节事件和天界塔的击败进度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：downedGoblins（第 6221 行）； downedFrost（第 6223 行）； downedPirates（第 6225 行）； downedClown（第
/// 6227 行）； downedMartians（第 6233 行）； downedHalloweenTree（第 6237 行）； downedHalloweenKing（第 6239
/// 行）； downedChristmasIceQueen（第 6241 行）； downedChristmasTree（第 6243 行）； downedChristmasSantank（第
/// 6245 行）； downedTowerSolar（第 6251 行）； downedTowerVortex（第 6253 行）； downedTowerNebula（第 6255 行）；
/// downedTowerStardust（第 6257 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 137 行。</para>
/// </remarks>
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
