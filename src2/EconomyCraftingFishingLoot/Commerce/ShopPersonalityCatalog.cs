using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Commerce;

public sealed class ShopPersonalityCatalog
{
  public ShopPersonalityCatalog(IEnumerable<string> personalityKeys)
  {
    ArgumentNullException.ThrowIfNull(personalityKeys);
    ImmutableArray<string> snapshot = personalityKeys
      .Select(key => key ?? throw new ArgumentException(
        "Personality keys cannot be null.",
        nameof(personalityKeys)))
      .ToImmutableArray();
    if (snapshot.Any(string.IsNullOrWhiteSpace))
    {
      throw new ArgumentException(
        "Personality keys cannot be empty.",
        nameof(personalityKeys));
    }

    if (snapshot.Distinct(StringComparer.Ordinal).Count() != snapshot.Length)
    {
      throw new ArgumentException(
        "Personality keys must be unique.",
        nameof(personalityKeys));
    }

    PersonalityKeys = snapshot;
  }

  public ImmutableArray<string> PersonalityKeys { get; }

  public bool Contains(string personalityKey)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(personalityKey);
    return PersonalityKeys.Contains(personalityKey, StringComparer.Ordinal);
  }
}
