namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class ShaderRegistryComponent
{
  private readonly Dictionary<string, ShaderDefinition> _definitions =
    new(StringComparer.Ordinal);

  public int Count => _definitions.Count;

  public uint Revision { get; private set; }

  internal ShaderHandle Register(ShaderDefinition definition)
  {
    ValidateDefinition(definition);
    if (_definitions.ContainsKey(definition.Key))
    {
      throw new InvalidOperationException(
        $"The shader key '{definition.Key}' is already registered.");
    }

    Revision++;
    _definitions.Add(definition.Key, definition);
    return new ShaderHandle(definition.Key, Revision);
  }

  internal void Rebuild(
    uint contentRevision,
    IReadOnlyList<ShaderDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    if (contentRevision == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(contentRevision));
    }

    Dictionary<string, ShaderDefinition> replacement =
      new(StringComparer.Ordinal);
    foreach (ShaderDefinition definition in definitions)
    {
      ValidateDefinition(definition);
      if (!replacement.TryAdd(definition.Key, definition))
      {
        throw new InvalidOperationException(
          $"The shader key '{definition.Key}' is duplicated in the registry.");
      }
    }

    _definitions.Clear();
    foreach (KeyValuePair<string, ShaderDefinition> definition in replacement)
    {
      _definitions.Add(definition.Key, definition.Value);
    }

    Revision = contentRevision;
  }

  public bool TryFind(string key, out ShaderHandle handle)
  {
    if (string.IsNullOrWhiteSpace(key) || !_definitions.ContainsKey(key) || Revision == 0)
    {
      handle = default;
      return false;
    }

    handle = new ShaderHandle(key, Revision);
    return true;
  }

  private static void ValidateDefinition(ShaderDefinition definition)
  {
    if (string.IsNullOrWhiteSpace(definition.Key) ||
      string.IsNullOrWhiteSpace(definition.Family))
    {
      throw new ArgumentException("A shader definition requires a key and family.", nameof(definition));
    }
  }
}
