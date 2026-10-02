namespace NLTX.ClientPresentation.MapCameraRendering;

public static class SceneAudioProjectionQuery
{
  public static SceneAudioProjection Read(SceneVisualProjectionComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return new SceneAudioProjection(
      component.StrongBlizzardSound,
      component.InsideBlizzardSound,
      component.Revision);
  }
}
