using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class AnglerQuestStateComponent
{
  private static readonly ImmutableArray<int> DefaultQuestItemNetIds =
  [
    2450, 2451, 2452, 2453, 2454, 2455, 2456, 2457, 2458, 2459,
    2460, 2461, 2462, 2463, 2464, 2465, 2466, 2467, 2468, 2469,
    2470, 2471, 2472, 2473, 2474, 2475, 2476, 2477, 2478, 2479,
    2480, 2481, 2482, 2483, 2484, 2485, 2486, 2487, 2488, 4393,
    4394
  ];

  private readonly ImmutableHashSet<string> _playersWhoFinishedToday;

  public AnglerQuestStateComponent(
    IEnumerable<int> questItemNetIds,
    int currentQuestIndex = 0,
    bool questFinished = false,
    IEnumerable<string>? playersWhoFinishedToday = null)
  {
    ArgumentNullException.ThrowIfNull(questItemNetIds);
    ImmutableArray<int> questItems = questItemNetIds.ToImmutableArray();
    if (questItems.IsDefaultOrEmpty)
    {
      throw new ArgumentException(
        "The angler quest catalog cannot be empty.",
        nameof(questItemNetIds));
    }

    if (questItems.Any(itemNetId => itemNetId <= 0))
    {
      throw new ArgumentException(
        "Angler quest item net IDs must be positive.",
        nameof(questItemNetIds));
    }

    if ((uint)currentQuestIndex >= (uint)questItems.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(currentQuestIndex));
    }

    QuestItemNetIds = questItems;
    CurrentQuestIndex = currentQuestIndex;
    QuestFinished = questFinished;
    _playersWhoFinishedToday = (playersWhoFinishedToday ?? [])
      .Where(playerKey => !string.IsNullOrWhiteSpace(playerKey))
      .ToImmutableHashSet(StringComparer.Ordinal);
  }

  public static AnglerQuestStateComponent CreateDefault()
  {
    return new AnglerQuestStateComponent(DefaultQuestItemNetIds);
  }

  public ImmutableArray<int> QuestItemNetIds { get; }

  public int CurrentQuestIndex { get; }

  public bool QuestFinished { get; }

  public int CurrentQuestItemNetId => QuestItemNetIds[CurrentQuestIndex];

  public ImmutableArray<string> PlayersWhoFinishedToday =>
    _playersWhoFinishedToday.Order(StringComparer.Ordinal).ToImmutableArray();

  public bool HasPlayerFinished(string playerKey)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(playerKey);
    return _playersWhoFinishedToday.Contains(playerKey);
  }

  public AnglerQuestStateComponent WithPlayerCompletion(string playerKey)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(playerKey);
    return new AnglerQuestStateComponent(
      QuestItemNetIds,
      CurrentQuestIndex,
      QuestFinished,
      _playersWhoFinishedToday.Append(playerKey));
  }
}
