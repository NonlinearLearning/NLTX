namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public sealed class StarVisualsComponent
{
  private readonly Dictionary<int, StarVisualState> _stars = new();

  public int Count => _stars.Count;

  public void Set(int starId, StarVisualState state)
  {
    ValidateId(starId);
    _stars[starId] = state;
  }

  public bool TryGet(int starId, out StarVisualState state)
  {
    return _stars.TryGetValue(starId, out state);
  }

  public bool Remove(int starId)
  {
    return _stars.Remove(starId);
  }

  public void Clear()
  {
    _stars.Clear();
  }

  internal IReadOnlyList<KeyValuePair<int, StarVisualState>> Entries =>
    _stars.OrderBy(entry => entry.Key).ToArray();

  private static void ValidateId(int starId)
  {
    if (starId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(starId));
    }
  }
}
