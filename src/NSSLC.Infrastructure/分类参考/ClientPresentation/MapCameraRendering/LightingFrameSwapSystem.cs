namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class LightingFrameSwapSystem
{
  public void Prepare(
    LightingCoordinatorComponent component,
    LightingFrameSnapshot frame)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Prepare(frame);
  }

  public void CommitPrepared(LightingCoordinatorComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.CommitPrepared();
  }

  public void Commit(
    LightingCoordinatorComponent component,
    LightingFrameSnapshot frame)
  {
    Prepare(component, frame);
    CommitPrepared(component);
  }
}
