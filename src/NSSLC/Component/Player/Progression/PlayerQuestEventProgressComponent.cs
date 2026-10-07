namespace Terraria.Player.Progression;

public sealed class PlayerQuestEventProgressComponent
{
  internal const int MaximumGolferScore = 1_000_000_000;

  private readonly HashSet<PlayerProgressionCommandToken> _appliedCommandTokens = new();

  public int AnglerQuestsFinished { get; internal set; }

  public int GolferScoreAccumulated { get; internal set; }

  /// <summary>Replaces the two absolute counters received from packet 76.</summary>
  public bool ApplyNetworkCounts(int anglerQuestsFinished, int golferScoreAccumulated)
  {
    if (anglerQuestsFinished < 0 ||
      golferScoreAccumulated < 0 ||
      golferScoreAccumulated > MaximumGolferScore)
    {
      return false;
    }

    AnglerQuestsFinished = anglerQuestsFinished;
    GolferScoreAccumulated = golferScoreAccumulated;
    return true;
  }

  public bool DownedDd2EventAnyDifficulty { get; internal set; }

  internal bool HasApplied(PlayerProgressionCommandToken token)
  {
    return token.IsValid && _appliedCommandTokens.Contains(token);
  }

  internal bool TryMarkApplied(PlayerProgressionCommandToken token)
  {
    return token.IsValid && _appliedCommandTokens.Add(token);
  }
}
