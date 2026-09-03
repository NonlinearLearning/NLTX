using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Dome.Simulation.Items.Definitions;

public sealed class CustomCurrencyDefinitionRegistry
{
  private const int DefenderMedalCurrencyId = 0;
  private const ushort DefenderMedalItemType = 3817;
  private const long DefenderMedalCurrencyCap = 999L;
  private readonly IReadOnlyDictionary<int, CustomCurrencyDefinition> _definitions;

  public CustomCurrencyDefinitionRegistry(
    IEnumerable<CustomCurrencyDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    Dictionary<int, CustomCurrencyDefinition> indexed = new();
    foreach (CustomCurrencyDefinition definition in definitions)
    {
      definition.Validate();
      if (!indexed.TryAdd(definition.CurrencyId, definition))
      {
        throw new ArgumentException(
          "Custom currency definitions must have unique currency IDs.",
          nameof(definitions));
      }
    }

    _definitions = new ReadOnlyDictionary<int, CustomCurrencyDefinition>(indexed);
  }

  public int Count => _definitions.Count;

  public IReadOnlyDictionary<int, CustomCurrencyDefinition> Definitions => _definitions;

  public static CustomCurrencyDefinitionRegistry CreateDefault()
  {
    return new([
      new CustomCurrencyDefinition(
        DefenderMedalCurrencyId,
        DefenderMedalItemType,
        DefenderMedalCurrencyCap)]);
  }

  public bool TryGet(int currencyId, out CustomCurrencyDefinition definition)
  {
    return _definitions.TryGetValue(currencyId, out definition);
  }
}
