namespace Terraria.WorldSession.Runtime;

public sealed class LaunchParameterAdapter
{
  private readonly IReadOnlyDictionary<string, string> _parameters;

  public LaunchParameterAdapter(IEnumerable<KeyValuePair<string, string>> parameters)
  {
    ArgumentNullException.ThrowIfNull(parameters);
    _parameters = new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);
  }

  public bool TryGet(string name, out string? value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    return _parameters.TryGetValue(name, out value);
  }

  public IReadOnlyDictionary<string, string> Snapshot()
  {
    return new Dictionary<string, string>(_parameters, StringComparer.OrdinalIgnoreCase);
  }
}
