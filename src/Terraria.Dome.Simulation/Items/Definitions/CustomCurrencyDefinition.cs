using System;

namespace Terraria.Dome.Simulation.Items.Definitions;

public readonly record struct CustomCurrencyDefinition(
  int CurrencyId,
  ushort ItemType,
  long CurrencyCap)
{
  public void Validate()
  {
    if (CurrencyId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(CurrencyId));
    }

    if (ItemType == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ItemType));
    }

    if (CurrencyCap <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(CurrencyCap));
    }
  }
}
