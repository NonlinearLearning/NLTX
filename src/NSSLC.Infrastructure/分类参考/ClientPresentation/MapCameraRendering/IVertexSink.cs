namespace NLTX.ClientPresentation.MapCameraRendering;

public interface IVertexSink
{
  void Submit(
    IReadOnlyList<VertexStripPoint> vertices,
    IReadOnlyList<short> indices);
}
