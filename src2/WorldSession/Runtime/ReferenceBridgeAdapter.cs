namespace Terraria.WorldSession.Runtime;

public sealed class ReferenceBridgeAdapter<T>
  where T : class
{
  private T? _value;

  public T? Value => _value;

  public void Set(T value)
  {
    _value = value ?? throw new ArgumentNullException(nameof(value));
  }

  public void Clear()
  {
    _value = null;
  }
}
