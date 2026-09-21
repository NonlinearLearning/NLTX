namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public static class BackgroundSelectionQuery
{
  public static BackgroundSelectionResult Select(
    BackgroundLayerCatalogComponent catalog,
    string layerSetKey,
    float cameraPosition,
    float parallaxScale)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    if (parallaxScale < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(parallaxScale));
    }

    if (!catalog.TryGetLayerSet(layerSetKey, out IReadOnlyList<int>? layerIds) || layerIds is null)
    {
      throw new KeyNotFoundException($"Background layer set '{layerSetKey}' was not found.");
    }

    return new BackgroundSelectionResult(layerIds, cameraPosition * parallaxScale);
  }
}
