using System;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Queries;

public static class WorldSeedOptionCatalogQuery
{
  public static WorldSeedOptionSeedMatchResult FindBySeedText(
    WorldSeedOptionCatalogDefinition catalog,
    string? seedText,
    int translatedSeedValue)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    string normalizedSeedText = NormalizeSeedText(seedText);
    foreach (WorldSeedOptionDefinition option in catalog.Options)
    {
      foreach (int specialSeedValue in option.SpecialSeedValues)
      {
        if (translatedSeedValue == specialSeedValue)
        {
          return WorldSeedOptionSeedMatchResult.Matched(
            new WorldSeedOptionMatch(option.Id, normalizedSeedText, true));
        }
      }

      foreach (string specialSeedName in option.SpecialSeedNames)
      {
        if (StringComparer.Ordinal.Equals(normalizedSeedText, specialSeedName))
        {
          return WorldSeedOptionSeedMatchResult.Matched(
            new WorldSeedOptionMatch(option.Id, normalizedSeedText, false));
        }
      }
    }

    return WorldSeedOptionSeedMatchResult.NoMatch;
  }

  public static WorldSeedOptionAutoGenerationResult ParseServerConfiguration(
    WorldSeedOptionCatalogDefinition catalog,
    string? line)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    if (string.IsNullOrWhiteSpace(line))
    {
      return WorldSeedOptionAutoGenerationResult.NoMatch;
    }

    const string prefix = "seed_";
    string lowered = line.ToLowerInvariant();
    if (!lowered.StartsWith(prefix, StringComparison.Ordinal))
    {
      return WorldSeedOptionAutoGenerationResult.NoMatch;
    }

    string[] parts = line[prefix.Length..].Split('=');
    if (parts.Length != 2 ||
        !int.TryParse(parts[1].Trim(), out int value))
    {
      return WorldSeedOptionAutoGenerationResult.NoMatch;
    }

    string name = parts[0].Trim().ToLowerInvariant();
    int clampedValue = Math.Clamp(value, 0, 1);
    foreach (WorldSeedOptionDefinition option in catalog.Options)
    {
      if (option.ServerConfigName is not null &&
          StringComparer.Ordinal.Equals(option.ServerConfigName, name))
      {
        return WorldSeedOptionAutoGenerationResult.Matched(
          option.Id,
          clampedValue == 1);
      }
    }

    return WorldSeedOptionAutoGenerationResult.NoMatch;
  }

  public static string NormalizeSeedText(string? seedText)
  {
    if (string.IsNullOrWhiteSpace(seedText))
    {
      return string.Empty;
    }

    System.Text.StringBuilder normalized = new(seedText.Length);
    foreach (char character in seedText.ToLowerInvariant())
    {
      if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
      {
        normalized.Append(character);
      }
    }

    return normalized.ToString();
  }
}
