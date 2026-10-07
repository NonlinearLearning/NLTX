namespace Terraria.Player.Progression;

/// <summary>
/// 保存玩家钓鱼任务、高尔夫和撒旦军队事件进度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：anglerQuestsFinished（第 1344 行）； golferScoreAccumulated（第 1346 行）；
/// downedDD2EventAnyDifficulty（第 1349 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 222 行。</para>
/// </remarks>
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
