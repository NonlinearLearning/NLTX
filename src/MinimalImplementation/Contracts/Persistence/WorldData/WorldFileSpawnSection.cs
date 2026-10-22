using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Persisted extra spawn points and the post-spawn world seed/manifest fields.
/// </summary>
/// <remarks>
/// The manifest is retained as its original JSON text. Parsing it would require the legacy
/// serializer and would make the formal codec depend on an external library.
/// </remarks>
public sealed class WorldFileSpawnSection
{
  public const string SectionId = "world.spawn-points";

  public WorldFileSpawnSection(
    IReadOnlyList<WorldFileExtraSpawnPoint> extraSpawnPoints,
    bool dualDungeonsSeed,
    bool moreLightningSeed,
    bool noLightningSeed,
    string? worldManifestJson)
  {
    ArgumentNullException.ThrowIfNull(extraSpawnPoints);
    ExtraSpawnPoints = Array.AsReadOnly(
      new List<WorldFileExtraSpawnPoint>(extraSpawnPoints).ToArray());
    DualDungeonsSeed = dualDungeonsSeed;
    MoreLightningSeed = moreLightningSeed;
    NoLightningSeed = noLightningSeed;
    WorldManifestJson = worldManifestJson;
  }

  public IReadOnlyList<WorldFileExtraSpawnPoint> ExtraSpawnPoints { get; }

  public bool DualDungeonsSeed { get; }

  public bool MoreLightningSeed { get; }

  public bool NoLightningSeed { get; }

  public string? WorldManifestJson { get; }

  public static WorldFileSpawnSection Empty => new(
    Array.Empty<WorldFileExtraSpawnPoint>(),
    dualDungeonsSeed: false,
    moreLightningSeed: false,
    noLightningSeed: false,
    worldManifestJson: null);
}
