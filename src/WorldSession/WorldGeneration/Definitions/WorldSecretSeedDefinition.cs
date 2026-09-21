using System;

namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSecretSeedDefinition
{
  public WorldSecretSeedDefinition(
    string variant,
    string localizationKey,
    string opaqueCode,
    string sourceAnchor)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(variant);
    ArgumentException.ThrowIfNullOrWhiteSpace(localizationKey);
    ArgumentException.ThrowIfNullOrWhiteSpace(opaqueCode);
    ArgumentException.ThrowIfNullOrWhiteSpace(sourceAnchor);

    Variant = variant;
    LocalizationKey = localizationKey;
    _opaqueCode = opaqueCode;
    SourceAnchor = sourceAnchor;
  }

  public string Variant { get; }

  public string LocalizationKey { get; }

  public string SourceAnchor { get; }

  internal string OpaqueCode => _opaqueCode;

  private readonly string _opaqueCode;
}
