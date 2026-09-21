namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class AnimationAdvanceSystem
{
  public void Advance(SpriteAnimationComponent component, int elapsedTicks)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Advance(elapsedTicks);
  }
}
