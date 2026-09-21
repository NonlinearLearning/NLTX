namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public sealed class ParticleLayerRegistryAdapterState
{
  private readonly Dictionary<string, object> _layers = new(StringComparer.Ordinal);

  public int Count => _layers.Count;

  public void Attach(string layerKey, object layerHandle)
  {
    if (string.IsNullOrWhiteSpace(layerKey))
    {
      throw new ArgumentException("A layer key is required.", nameof(layerKey));
    }

    ArgumentNullException.ThrowIfNull(layerHandle);
    _layers[layerKey] = layerHandle;
  }

  public bool TryGet(string layerKey, out object? layerHandle)
  {
    return _layers.TryGetValue(layerKey, out layerHandle);
  }

  public bool Detach(string layerKey)
  {
    return _layers.Remove(layerKey);
  }

  public void Clear()
  {
    _layers.Clear();
  }
}
