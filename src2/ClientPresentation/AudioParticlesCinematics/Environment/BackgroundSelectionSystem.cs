namespace NLTX.ClientPresentation.AudioParticlesCinematics.Environment;

public sealed class BackgroundSelectionSystem
{
  public BackgroundSelectionResult Apply(
    BackgroundParallaxVisualsComponent state,
    BackgroundLayerCatalogComponent catalog,
    string layerSetKey,
    float cameraPosition)
  {
    ArgumentNullException.ThrowIfNull(state);
    BackgroundSelectionResult selection = BackgroundSelectionQuery.Select(
      catalog,
      layerSetKey,
      cameraPosition,
      state.EssScale);
    return selection;
  }
}
