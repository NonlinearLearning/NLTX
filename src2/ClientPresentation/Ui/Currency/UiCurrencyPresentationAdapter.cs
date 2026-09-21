namespace Terraria.ClientPresentation.Ui.Currency;

public sealed class UiCurrencyPresentationAdapter
{
  public string FormatAmount(long amount, long maxBalance)
  {
    if (amount < 0 || maxBalance < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    if (maxBalance > 0 && amount >= maxBalance)
    {
      return "MAX";
    }

    return amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
  }
}
