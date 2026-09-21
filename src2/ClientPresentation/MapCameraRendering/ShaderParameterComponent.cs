namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class ShaderParameterComponent
{
  private readonly Dictionary<string, ShaderParameterValue> _staged =
    new(StringComparer.Ordinal);

  public int StagedCount => _staged.Count;

  public bool Disabled { get; private set; }

  public uint Revision { get; private set; }

  internal void Stage(string name, ShaderParameterValue value)
  {
    ValidateName(name);
    _staged[name] = value;
    Revision++;
  }

  internal void SetDisabled(bool disabled)
  {
    Disabled = disabled;
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

  private static void ValidateName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("A shader parameter name is required.", nameof(name));
    }
  }
}
