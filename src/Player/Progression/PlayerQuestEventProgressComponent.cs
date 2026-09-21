namespace Terraria.Player.Progression;

public sealed class PlayerQuestEventProgressComponent
{
  internal const int MaximumGolferScore = 1_000_000_000;

  private readonly HashSet<Guid> _appliedCommandTokens = new();

  public int AnglerQuestsFinished { get; internal set; }

  public int GolferScoreAccumulated { get; internal set; }

  public bool DownedDd2EventAnyDifficulty { get; internal set; }

  internal bool HasApplied(PlayerProgressionCommandToken token)
  {
    return _appliedCommandTokens.Contains(token.Value);
  }

  internal bool TryMarkApplied(PlayerProgressionCommandToken token)
  {
    return _appliedCommandTokens.Add(token.Value);
  }
}
