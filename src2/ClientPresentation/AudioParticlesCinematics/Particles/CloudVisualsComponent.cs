namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public sealed class CloudVisualsComponent
{
  private readonly Dictionary<int, CloudVisualState> _clouds = new();

  public int Count => _clouds.Count;

  public void Set(int cloudId, CloudVisualState state)
  {
    ValidateId(cloudId);
    _clouds[cloudId] = state;
  }

  public bool TryGet(int cloudId, out CloudVisualState state)
  {
    return _clouds.TryGetValue(cloudId, out state);
  }

  public bool Remove(int cloudId)
  {
    return _clouds.Remove(cloudId);
  }

  public void Clear()
  {
    _clouds.Clear();
  }

  internal IReadOnlyList<KeyValuePair<int, CloudVisualState>> Entries =>
    _clouds.OrderBy(entry => entry.Key).ToArray();

  private static void ValidateId(int cloudId)
  {
    if (cloudId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cloudId));
    }
  }
}
