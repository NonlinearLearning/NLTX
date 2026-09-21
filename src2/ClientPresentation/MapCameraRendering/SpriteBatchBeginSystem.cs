namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class SpriteBatchBeginSystem
{
  public void Begin(
    uint frameLease,
    SpriteBatchStateComponent component,
    SpriteBatchStateInput state)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Begin(frameLease, state);
  }
}
