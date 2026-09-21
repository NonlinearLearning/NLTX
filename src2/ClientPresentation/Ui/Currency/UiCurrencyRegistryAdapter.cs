namespace Terraria.ClientPresentation.Ui.Currency;

public sealed class UiCurrencyRegistryAdapter
{
  private readonly Dictionary<RegistrationKey, Definition> _definitions = [];
  private readonly Dictionary<string, RegistrationKey> _keysByExternalName =
    new(StringComparer.Ordinal);
  private int _nextKey = 1;

  public bool TryRegister(Definition definition, out RegistrationKey key)
  {
    if (!definition.IsValid || _keysByExternalName.ContainsKey(definition.ExternalName))
    {
      key = RegistrationKey.Invalid;
      return false;
    }

    key = new RegistrationKey(_nextKey++);
    _definitions.Add(key, definition);
    _keysByExternalName.Add(definition.ExternalName, key);
    return true;
  }

  public bool TryGet(RegistrationKey key, out Definition definition)
  {
    return _definitions.TryGetValue(key, out definition);
  }

  public void Reset()
  {
    _definitions.Clear();
    _keysByExternalName.Clear();
    _nextKey = 1;
  }

  public readonly record struct RegistrationKey(int Value)
  {
    public static RegistrationKey Invalid => new(0);

    public bool IsValid => Value > 0;
  }

  public readonly record struct Definition(
    string ExternalName,
    string LocalizationKey,
    int Denomination,
    long MaxBalance,
    uint Color,
    string IconResourceKey)
  {
    public bool IsValid => !string.IsNullOrWhiteSpace(ExternalName)
      && !string.IsNullOrWhiteSpace(LocalizationKey)
      && Denomination > 0
      && MaxBalance >= 0
      && !string.IsNullOrWhiteSpace(IconResourceKey);
  }
}
