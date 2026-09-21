namespace NLTX.PlayerInputGameplay.Input;

public sealed class InputTriggerFrameComponent
{
  private readonly Dictionary<string, bool> _current = new(StringComparer.Ordinal);
  private readonly Dictionary<string, bool> _old = new(StringComparer.Ordinal);

  public int HotbarScrollCooldown { get; private set; }

  public int HotbarHoldTime { get; private set; }

  public bool UsedMovementKey { get; private set; }

  public IReadOnlyDictionary<string, bool> Current => new Dictionary<string, bool>(_current, StringComparer.Ordinal);

  public IReadOnlyDictionary<string, bool> Old => new Dictionary<string, bool>(_old, StringComparer.Ordinal);

  public IReadOnlyCollection<string> JustPressed => Edges(true);

  public IReadOnlyCollection<string> JustReleased => Edges(false);

  public void Capture(IEnumerable<string> pressedActions, bool usedMovementKey)
  {
    ArgumentNullException.ThrowIfNull(pressedActions);
    _old.Clear();
    foreach (var pair in _current)
    {
      _old[pair.Key] = pair.Value;
    }

    _current.Clear();
    foreach (var action in pressedActions.Where(static action => !string.IsNullOrWhiteSpace(action)).Distinct(StringComparer.Ordinal))
    {
      _current[action] = true;
    }

    UsedMovementKey = usedMovementKey;
    if (HotbarScrollCooldown > 0)
    {
      HotbarScrollCooldown--;
    }

    HotbarHoldTime = _current.ContainsKey("Hotbar") ? checked(HotbarHoldTime + 1) : 0;
  }

  public void SetHotbarScrollCooldown(int ticks)
  {
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    HotbarScrollCooldown = ticks;
  }

  private IReadOnlyCollection<string> Edges(bool pressed)
  {
    return _current.Keys
      .Concat(_old.Keys)
      .Distinct(StringComparer.Ordinal)
      .Where(action => (_current.ContainsKey(action) == pressed) && (_current.ContainsKey(action) != _old.ContainsKey(action)))
      .ToArray();
  }
}
