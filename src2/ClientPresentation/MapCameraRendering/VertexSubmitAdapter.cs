namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class VertexSubmitAdapter
{
  private readonly IVertexSink _sink;

  public VertexSubmitAdapter(IVertexSink sink)
  {
    _sink = sink ?? throw new ArgumentNullException(nameof(sink));
  }

  public void Submit(VertexStripWorksetComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    _sink.Submit(component.ReadVertices(), component.ReadIndices());
  }
}
