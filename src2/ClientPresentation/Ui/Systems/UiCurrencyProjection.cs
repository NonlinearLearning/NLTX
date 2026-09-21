using Terraria.ClientPresentation.Ui.Currency;

namespace Terraria.ClientPresentation.Ui.Systems;

public sealed class UiCurrencyProjection
{
  private readonly UiCurrencyDefinitionQuery _definitions;
  private readonly UiCurrencyPresentationAdapter _presentation;

  public UiCurrencyProjection(
    UiCurrencyDefinitionQuery definitions,
    UiCurrencyPresentationAdapter presentation)
  {
    _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
    _presentation = presentation
      ?? throw new ArgumentNullException(nameof(presentation));
  }

  public bool TryProject(
    UiCurrencyVisualsComponent visuals,
    long balance,
    long price,
    out Projection projection)
  {
    ArgumentNullException.ThrowIfNull(visuals);
    if (balance < 0 || price < 0
      || !_definitions.TryGet(visuals.RegistrationKey, out UiCurrencyDefinitionQuery.Snapshot definition))
    {
      projection = default;
      return false;
    }

    projection = new Projection(
      visuals.RegistrationKey,
      visuals.TextKey,
      _presentation.FormatAmount(balance, definition.MaxBalance),
      _presentation.FormatAmount(price, definition.MaxBalance),
      visuals.DrawScale,
      visuals.Color,
      visuals.IconResourceKey,
      definition.Denomination,
      definition.MaxBalance);
    return true;
  }

  public readonly record struct Projection(
    UiCurrencyRegistryAdapter.RegistrationKey RegistrationKey,
    string TextKey,
    string BalanceText,
    string PriceText,
    float DrawScale,
    uint Color,
    string IconResourceKey,
    int Denomination,
    long MaxBalance);
}
