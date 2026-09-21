namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public sealed class GoreVisualsComponent
{
  private readonly Dictionary<int, GoreVisualState> _gore = new();

  public int Count => _gore.Count;

  public void Set(int goreId, GoreVisualState state)
  {
    ValidateId(goreId);
    _gore[goreId] = state;
  }

  public bool TryGet(int goreId, out GoreVisualState state)
  {
    return _gore.TryGetValue(goreId, out state);
  }

  public bool Remove(int goreId)
  {
    return _gore.Remove(goreId);
  }

  public void Clear()
  {
    _gore.Clear();
  }

  internal IReadOnlyList<KeyValuePair<int, GoreVisualState>> Entries =>
    _gore.OrderBy(entry => entry.Key).ToArray();

  private static void ValidateId(int goreId)
  {
    if (goreId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(goreId));
    }
  }
}
