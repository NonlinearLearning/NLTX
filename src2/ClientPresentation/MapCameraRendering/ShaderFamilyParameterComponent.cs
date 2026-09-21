namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class ShaderFamilyParameterComponent
{
  private readonly Dictionary<string, ShaderParameterValue> _staged =
    new(StringComparer.Ordinal);

  public ShaderFamilyParameterComponent(string family)
  {
    if (string.IsNullOrWhiteSpace(family))
    {
      throw new ArgumentException("A shader family is required.", nameof(family));
    }

    Family = family;
  }

  public string Family { get; }

  public int StagedCount => _staged.Count;

  public uint Revision { get; private set; }

  internal void Stage(string name, ShaderParameterValue value)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("A shader family parameter name is required.", nameof(name));
    }

    _staged[name] = value;
    Revision++;
  }

  internal IReadOnlyDictionary<string, ShaderParameterValue> Snapshot()
  {
    return new Dictionary<string, ShaderParameterValue>(_staged, StringComparer.Ordinal);
  }

  internal void ClearStaged()
  {
    _staged.Clear();
    Revision++;
  }
}
