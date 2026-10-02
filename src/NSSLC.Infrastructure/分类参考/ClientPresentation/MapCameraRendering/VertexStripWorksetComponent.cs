namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class VertexStripWorksetComponent
{
  private readonly int _maxVertices;
  private readonly List<VertexStripPoint> _vertices = new();
  private readonly List<short> _indices = new();

  public VertexStripWorksetComponent(int maxVertices)
  {
    if (maxVertices <= 0 || maxVertices > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(maxVertices));
    }

    _maxVertices = maxVertices;
  }

  public int VertexCount => _vertices.Count;

  public int IndexCount => _indices.Count;

  public uint Revision { get; private set; }

  internal void Replace(IReadOnlyList<VertexStripPoint> vertices)
  {
    ArgumentNullException.ThrowIfNull(vertices);
    if (vertices.Count > _maxVertices)
    {
      throw new ArgumentException("The vertex strip exceeds its configured capacity.", nameof(vertices));
    }

    _vertices.Clear();
    _indices.Clear();
    for (int index = 0; index < vertices.Count; index++)
    {
      VertexStripPoint point = vertices[index];
      if (!float.IsFinite(point.Position.X) ||
        !float.IsFinite(point.Position.Y) ||
        !float.IsFinite(point.TexCoord.X) ||
        !float.IsFinite(point.TexCoord.Y) ||
        !float.IsFinite(point.TexCoord.Z))
      {
        throw new ArgumentOutOfRangeException(nameof(vertices));
      }

      _vertices.Add(point);
      if (index >= 2)
      {
        _indices.Add((short)(index - 2));
        _indices.Add((short)(index - 1));
        _indices.Add((short)index);
      }
    }

    Revision++;
  }

  internal IReadOnlyList<VertexStripPoint> ReadVertices()
  {
    return _vertices.ToArray();
  }

  internal IReadOnlyList<short> ReadIndices()
  {
    return _indices.ToArray();
  }
}
