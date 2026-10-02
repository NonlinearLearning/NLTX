namespace Terraria.NpcTownBestiary;

public sealed class LucyAxeCooldownComponent
{
  private readonly Dictionary<LucyMessageSource, int> _remainingBySource = new();

  public int GetRemaining(LucyMessageSource source)
  {
    return _remainingBySource.TryGetValue(source, out int remaining) ? remaining : 0;
  }

  public void Start(LucyMessageSource source, int duration)
  {
    if (duration < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(duration));
    }

    _remainingBySource[source] = duration;
  }

  internal void Tick()
  {
    foreach (LucyMessageSource source in _remainingBySource.Keys.ToArray())
    {
      int remaining = GetRemaining(source);
      if (remaining > 0)
      {
        _remainingBySource[source] = remaining - 1;
      }
    }
  }
}
