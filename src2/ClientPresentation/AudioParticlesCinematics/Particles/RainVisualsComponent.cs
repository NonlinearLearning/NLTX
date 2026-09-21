namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public sealed class RainVisualsComponent
{
  private readonly Dictionary<int, RainVisualState> _rain = new();

  public int Count => _rain.Count;

  public void Set(int rainId, RainVisualState state)
  {
    ValidateId(rainId);
    _rain[rainId] = state;
  }

  public bool TryGet(int rainId, out RainVisualState state)
  {
    return _rain.TryGetValue(rainId, out state);
  }

  public bool Remove(int rainId)
  {
    return _rain.Remove(rainId);
  }

  public void Clear()
  {
    _rain.Clear();
  }

  internal IReadOnlyList<KeyValuePair<int, RainVisualState>> Entries =>
    _rain.OrderBy(entry => entry.Key).ToArray();

  private static void ValidateId(int rainId)
  {
    if (rainId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(rainId));
    }
  }
}
