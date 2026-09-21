using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Commerce;

public sealed class ShopBiomeModifierCatalog
{
  public ShopBiomeModifierCatalog(IEnumerable<string> biomeKeys)
  {
    ArgumentNullException.ThrowIfNull(biomeKeys);
    ImmutableArray<string> snapshot = biomeKeys
      .Select(key => key ?? throw new ArgumentException(
        "Biome keys cannot be null.",
        nameof(biomeKeys)))
      .ToImmutableArray();
    if (snapshot.Any(string.IsNullOrWhiteSpace))
    {
      throw new ArgumentException(
        "Biome keys cannot be empty.",
        nameof(biomeKeys));
    }

    if (snapshot.Distinct(StringComparer.Ordinal).Count() != snapshot.Length)
    {
      throw new ArgumentException(
        "Biome keys must be unique.",
        nameof(biomeKeys));
    }

    BiomeKeys = snapshot;
  }

  public ImmutableArray<string> BiomeKeys { get; }

  public bool Contains(string biomeKey)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(biomeKey);
    return BiomeKeys.Contains(biomeKey, StringComparer.Ordinal);
  }
}
