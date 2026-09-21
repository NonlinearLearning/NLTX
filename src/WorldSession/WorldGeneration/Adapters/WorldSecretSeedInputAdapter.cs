using System;
using System.Collections.Generic;
using System.Text;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Adapters;

public sealed class WorldSecretSeedInputAdapter
{
  private readonly IWorldSecretSeedCodeTransformer _codeTransformer;
  private readonly WorldSecretSeedRegistryDefinitionsProjection _definitions;

  public WorldSecretSeedInputAdapter(
    WorldSecretSeedRegistryDefinitionsProjection definitions,
    IWorldSecretSeedCodeTransformer codeTransformer)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(codeTransformer);
    _definitions = definitions;
    _codeTransformer = codeTransformer;
  }

  public WorldSecretSeedInputResult Match(
    string? rawInput,
    IReadOnlyDictionary<string, string>? knownPlaintexts = null)
  {
    string normalizedInput = NormalizeSeedText(rawInput);
    if (normalizedInput.Length == 0)
    {
      return WorldSecretSeedInputResult.Rejected(
        WorldSecretSeedInputRejectionReason.EmptyInput);
    }

    string originalUnlockText = NormalizeUnlockText(rawInput ?? string.Empty);
    if (knownPlaintexts is not null)
    {
      foreach (WorldSecretSeedDefinition definition in _definitions.Definitions)
      {
        if (knownPlaintexts.TryGetValue(definition.Variant, out string? knownPlaintext) &&
            StringComparer.Ordinal.Equals(knownPlaintext, normalizedInput))
        {
          return WorldSecretSeedInputResult.Matched(new WorldSecretSeedInputMatch(
            definition,
            normalizedInput,
            originalUnlockText,
            MatchedKnownPlaintext: true));
        }
      }
    }

    string transformedCode = _codeTransformer.Transform(normalizedInput);
    if (!string.IsNullOrWhiteSpace(transformedCode))
    {
      foreach (WorldSecretSeedDefinition definition in _definitions.Definitions)
      {
        if (StringComparer.Ordinal.Equals(definition.OpaqueCode, transformedCode))
        {
          return WorldSecretSeedInputResult.Matched(new WorldSecretSeedInputMatch(
            definition,
            normalizedInput,
            originalUnlockText,
            MatchedKnownPlaintext: false));
        }
      }
    }

    return WorldSecretSeedInputResult.Rejected(
      WorldSecretSeedInputRejectionReason.NoMatch);
  }

  public WorldSecretSeedInputResult MatchAndPlaySound(
    string? rawInput,
    IWorldSecretSeedSoundPort soundPort,
    bool playSound,
    IReadOnlyDictionary<string, string>? knownPlaintexts = null)
  {
    ArgumentNullException.ThrowIfNull(soundPort);
    WorldSecretSeedInputResult result = Match(rawInput, knownPlaintexts);
    if (result.IsMatch && playSound)
    {
      soundPort.Play(result.Match.Definition);
    }

    return result;
  }

  public static string NormalizeSeedText(string? rawInput)
  {
    if (string.IsNullOrWhiteSpace(rawInput))
    {
      return string.Empty;
    }

    StringBuilder normalized = new(rawInput.Length);
    foreach (char character in rawInput.ToLowerInvariant())
    {
      if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
      {
        normalized.Append(character);
      }
    }

    return normalized.ToString();
  }

  public static string NormalizeUnlockText(string rawInput)
  {
    StringBuilder normalized = new(rawInput.Length);
    foreach (char character in rawInput)
    {
      if (character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' ||
          character == ' ')
      {
        normalized.Append(character);
      }
    }

    return normalized.ToString();
  }
}
