namespace Terraria.ClientPresentation.Ui.Currency;

public sealed class UiCurrencyDefinitionQuery
{
  private readonly UiCurrencyRegistryAdapter _registry;

  public UiCurrencyDefinitionQuery(UiCurrencyRegistryAdapter registry)
  {
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
  }

  public bool TryGet(
    UiCurrencyRegistryAdapter.RegistrationKey key,
    out Snapshot snapshot)
  {
    if (!_registry.TryGet(
      key,
      out UiCurrencyRegistryAdapter.Definition definition))
    {
      snapshot = default;
      return false;
    }

    snapshot = new Snapshot(
      key,
      definition.ExternalName,
      definition.LocalizationKey,
      definition.Denomination,
      definition.MaxBalance,
      definition.Color,
      definition.IconResourceKey);
    return true;
  }

  public readonly record struct Snapshot(
    UiCurrencyRegistryAdapter.RegistrationKey RegistrationKey,
    string ExternalName,
    string LocalizationKey,
    int Denomination,
    long MaxBalance,
    uint Color,
    string IconResourceKey);
}
