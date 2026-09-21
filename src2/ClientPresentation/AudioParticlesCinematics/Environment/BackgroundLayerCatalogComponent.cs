using System.Collections.ObjectModel;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public sealed class BackgroundLayerCatalogComponent
{
  private readonly Dictionary<string, ReadOnlyCollection<int>> _layerSets =
    new(StringComparer.Ordinal);

  public float CloudAlpha { get; private set; }

  public float CloudActive { get; private set; }

  public IReadOnlyDictionary<string, ReadOnlyCollection<int>> LayerSets => _layerSets;

  public void SetCloudState(float cloudAlpha, float cloudActive)
  {
    CloudAlpha = ValidateUnitInterval(cloudAlpha, nameof(cloudAlpha));
    CloudActive = ValidateUnitInterval(cloudActive, nameof(cloudActive));
  }

  public void SetLayerSet(string key, IEnumerable<int> layerIds)
  {
    if (string.IsNullOrWhiteSpace(key))
    {
      throw new ArgumentException("A background layer key is required.", nameof(key));
    }

    ArgumentNullException.ThrowIfNull(layerIds);
    int[] values = layerIds.ToArray();
    if (values.Length == 0)
    {
      throw new ArgumentException("A background layer set cannot be empty.", nameof(layerIds));
    }

    _layerSets[key] = Array.AsReadOnly(values);
  }

  public bool TryGetLayerSet(string key, out IReadOnlyList<int>? layerIds)
  {
    if (_layerSets.TryGetValue(key, out ReadOnlyCollection<int>? values))
    {
      layerIds = values;
      return true;
    }

    layerIds = null;
    return false;
  }

  private static float ValidateUnitInterval(float value, string parameterName)
  {
    if (value is < 0 or > 1)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return value;
  }
}
