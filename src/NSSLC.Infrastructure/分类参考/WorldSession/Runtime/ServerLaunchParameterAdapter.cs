namespace Terraria.WorldSession.Runtime;

public sealed class ServerLaunchParameterAdapter
{
  private readonly LaunchParameterAdapter _inner;

  public ServerLaunchParameterAdapter(IEnumerable<KeyValuePair<string, string>> parameters)
  {
    _inner = new LaunchParameterAdapter(parameters);
  }

  public bool TryGet(string name, out string? value)
  {
    return _inner.TryGet(name, out value);
  }
}
