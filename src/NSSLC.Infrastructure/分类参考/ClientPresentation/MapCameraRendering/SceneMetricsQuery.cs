namespace NLTX.ClientPresentation.MapCameraRendering;

public static class SceneMetricsQuery
{
  public static SceneMetricsAggregateValue Read(
    SceneMetricsAggregateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.Snapshot();
  }
}
