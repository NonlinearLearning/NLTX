namespace Terraria.WorldSession.Runtime;

public sealed class ServerServiceContainerAdapter
{
  private readonly Dictionary<Type, object> _services = new();

  public void Register<T>(T service)
    where T : class
  {
    _services[typeof(T)] = service ?? throw new ArgumentNullException(nameof(service));
  }

  public bool TryResolve<T>(out T? service)
    where T : class
  {
    if (_services.TryGetValue(typeof(T), out object? value))
    {
      service = (T)value;
      return true;
    }

    service = null;
    return false;
  }
}
