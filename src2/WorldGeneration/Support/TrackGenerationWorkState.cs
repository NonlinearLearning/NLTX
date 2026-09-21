using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Support;

public sealed class TrackGenerationWorkState
{
  private readonly TrackHistoryEntry[] _history;
  private readonly TrackHistoryEntry[] _rewriteHistory;
  private int _historyCount;
  private int _rewriteHistoryCount;

  public TrackGenerationWorkState(int historyCapacity = 4096, int rewriteHistoryCapacity = 25)
  {
    if (historyCapacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(historyCapacity));
    }

    if (rewriteHistoryCapacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(rewriteHistoryCapacity));
    }

    _history = new TrackHistoryEntry[historyCapacity];
    _rewriteHistory = new TrackHistoryEntry[rewriteHistoryCapacity];
  }

  public int HistoryCapacity => _history.Length;

  public int RewriteHistoryCapacity => _rewriteHistory.Length;

  public int HistoryCount => _historyCount;

  public int RewriteHistoryCount => _rewriteHistoryCount;

  public IReadOnlyList<TrackHistoryEntry> History => CreateSnapshot(_history, _historyCount);

  public IReadOnlyList<TrackHistoryEntry> RewriteHistory =>
    CreateSnapshot(_rewriteHistory, _rewriteHistoryCount);

  public void AddHistory(TrackHistoryEntry entry)
  {
    if (!TryAddHistory(entry))
    {
      throw new InvalidOperationException("Track history capacity has been reached.");
    }
  }

  public bool TryAddHistory(TrackHistoryEntry entry)
  {
    if (_historyCount == _history.Length)
    {
      return false;
    }

    _history[_historyCount++] = entry;
    return true;
  }

  public void AddRewriteHistory(TrackHistoryEntry entry)
  {
    if (_rewriteHistoryCount == _rewriteHistory.Length)
    {
      throw new InvalidOperationException("Track rewrite history capacity has been reached.");
    }

    _rewriteHistory[_rewriteHistoryCount++] = entry;
  }

  public void Clear()
  {
    Array.Clear(_history);
    Array.Clear(_rewriteHistory);
    _historyCount = 0;
    _rewriteHistoryCount = 0;
  }

  private static IReadOnlyList<TrackHistoryEntry> CreateSnapshot(
    TrackHistoryEntry[] source,
    int count)
  {
    var copy = new TrackHistoryEntry[count];
    Array.Copy(source, copy, count);
    return new ReadOnlyCollection<TrackHistoryEntry>(copy);
  }
}
