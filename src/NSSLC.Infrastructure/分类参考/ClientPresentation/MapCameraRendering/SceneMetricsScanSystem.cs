namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class SceneMetricsScanSystem
{
  public void BeginScan(
    SceneMetricsAggregateComponent component,
    uint scanRevision,
    uint scanTime,
    SceneScanRectangle tileCenter,
    Guid? perspectiveEntityId)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Reset(scanRevision, scanTime, tileCenter, perspectiveEntityId);
  }

  public void AddTile(
    SceneMetricsAggregateComponent component,
    SceneMetricKind kind,
    int amount = 1)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.AddMetric(kind, amount);
  }

  public void AddLiquid(
    SceneMetricsAggregateComponent component,
    int liquidIndex,
    int amount = 1)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.AddLiquid(liquidIndex, amount);
  }
}
