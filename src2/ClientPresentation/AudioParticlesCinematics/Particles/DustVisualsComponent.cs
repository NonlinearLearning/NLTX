namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public sealed class DustVisualsComponent
{
  private readonly Dictionary<int, DustVisualState> _dust = new();

  public int Count => _dust.Count;

  public void Set(int dustId, DustVisualState state)
  {
    ValidateId(dustId);
    _dust[dustId] = state;
  }

  public bool TryGet(int dustId, out DustVisualState state)
  {
    return _dust.TryGetValue(dustId, out state);
  }

  public bool Remove(int dustId)
  {
    return _dust.Remove(dustId);
  }

  public void Clear()
  {
    _dust.Clear();
  }

  internal IReadOnlyList<KeyValuePair<int, DustVisualState>> Entries =>
    _dust.OrderBy(entry => entry.Key).ToArray();

  private static void ValidateId(int dustId)
  {
    if (dustId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(dustId));
    }
  }
}
