namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class VertexStripBuildSystem
{
  public void Build(
    VertexStripWorksetComponent component,
    IReadOnlyList<VertexStripPoint> points)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Replace(points);
  }
}
