using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// The quest and town-NPC prefix that follows world progression in a WorldFile header.
/// </summary>
/// <remarks>
/// Version-specific defaults are applied by the decoder. This object only carries persisted
/// values and does not mutate NPC or world runtime state.
/// </remarks>
public sealed class WorldFileQuestSection
{
  public const string SectionId = "world.quests";

  public WorldFileQuestSection(
    IReadOnlyList<string> anglerWhoFinishedToday,
    bool savedAngler,
    int anglerQuest,
    bool savedStylist,
    bool savedTaxCollector,
    bool savedGolfer,
    int invasionSizeStart,
    int cultistDelay)
  {
    AnglerWhoFinishedToday = CopyStrings(anglerWhoFinishedToday);
    SavedAngler = savedAngler;
    AnglerQuest = anglerQuest;
    SavedStylist = savedStylist;
    SavedTaxCollector = savedTaxCollector;
    SavedGolfer = savedGolfer;
    InvasionSizeStart = invasionSizeStart;
    CultistDelay = cultistDelay;
  }

  public IReadOnlyList<string> AnglerWhoFinishedToday { get; }

  public bool SavedAngler { get; }

  public int AnglerQuest { get; }

  public bool SavedStylist { get; }

  public bool SavedTaxCollector { get; }

  public bool SavedGolfer { get; }

  public int InvasionSizeStart { get; }

  public int CultistDelay { get; }

  public static WorldFileQuestSection Empty => new(
    anglerWhoFinishedToday: Array.Empty<string>(),
    savedAngler: false,
    anglerQuest: 0,
    savedStylist: false,
    savedTaxCollector: false,
    savedGolfer: false,
    invasionSizeStart: 0,
    cultistDelay: 86400);

  private static IReadOnlyList<string> CopyStrings(IReadOnlyList<string> values)
  {
    ArgumentNullException.ThrowIfNull(values);
    var copy = new string[values.Count];
    for (int index = 0; index < copy.Length; index++)
    {
      copy[index] = values[index] ?? throw new ArgumentNullException(nameof(values));
    }

    return Array.AsReadOnly(copy);
  }
}
