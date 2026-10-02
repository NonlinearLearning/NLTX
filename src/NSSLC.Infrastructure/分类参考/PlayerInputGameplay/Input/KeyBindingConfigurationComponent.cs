namespace NLTX.PlayerInputGameplay.Input;

public sealed class KeyBindingConfigurationComponent
{
  private readonly Dictionary<string, List<string>> _keyStatus = new(StringComparer.Ordinal);

  public IReadOnlyDictionary<string, IReadOnlyList<string>> KeyStatus =>
    _keyStatus.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<string>)pair.Value.ToArray(), StringComparer.Ordinal);

  public void Set(string actionName, IEnumerable<string> bindings)
  {
    if (string.IsNullOrWhiteSpace(actionName))
    {
      throw new ArgumentException("An action name is required.", nameof(actionName));
    }

    ArgumentNullException.ThrowIfNull(bindings);
    _keyStatus[actionName] = bindings.Where(static binding => !string.IsNullOrWhiteSpace(binding)).Distinct(StringComparer.Ordinal).ToList();
  }
}
