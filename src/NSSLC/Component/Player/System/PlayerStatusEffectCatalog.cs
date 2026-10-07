namespace Terraria.Player;

public sealed class PlayerStatusEffectCatalog
{
  private readonly IReadOnlyDictionary<int, PlayerStatusEffectDefinition> _definitions;

  public PlayerStatusEffectCatalog(
    IEnumerable<PlayerStatusEffectDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);

    Dictionary<int, PlayerStatusEffectDefinition> registered = [];
    foreach (PlayerStatusEffectDefinition definition in definitions)
    {
      ArgumentNullException.ThrowIfNull(definition);
      if (!registered.TryAdd(definition.EffectType.Value, definition))
      {
        throw new ArgumentException(
          $"The status effect type {definition.EffectType.Value} is registered twice.",
          nameof(definitions));
      }
    }

    _definitions = registered;
  }

  public bool TryGet(
    ContentId<BuffDefinition> effectType,
    out PlayerStatusEffectDefinition definition)
  {
    return _definitions.TryGetValue(effectType.Value, out definition!);
  }
}
