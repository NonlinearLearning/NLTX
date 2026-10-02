namespace Terraria.WorldSession.Runtime;

public sealed class ServerFrameworkAdapter
{
  private readonly HashSet<string> _componentServices = new(StringComparer.Ordinal);

  public IReadOnlyCollection<string> ComponentServices => _componentServices;

  public void RegisterComponentService(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("A component service name is required.", nameof(name));
    }

    _componentServices.Add(name);
  }
}
