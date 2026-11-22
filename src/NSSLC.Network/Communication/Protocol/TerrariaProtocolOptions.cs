namespace Terraria.Network;

using Terraria.World;

/// <summary>Small authoritative facts needed by the Version4 join handshake.</summary>
public sealed class TerrariaProtocolOptions
{
  public string ProtocolVersion { get; init; } = "Terraria319";

  /// <summary>
  /// Accepts newer Terraria build labels while the bounded Version4 join layout remains shared.
  /// This is required for clients such as Terraria326 connecting to the Terraria319-compatible
  /// generated packet profile.
  /// </summary>
  public bool AcceptCompatibleTerrariaVersions { get; init; } = true;

  public int WorldWidth { get; init; } = 256;

  public int WorldHeight { get; init; } = 128;

  public int SpawnTileX { get; init; } = 128;

  public int SpawnTileY { get; init; } = 64;

  public string WorldName { get; init; } = "NSSLC";

  public int WorldId { get; init; } = 1;

  public Guid WorldGuid { get; init; } = Guid.Empty;

  public ulong WorldGeneratorVersion { get; init; }

  public int GameMode { get; init; }

  public short WorldSurface { get; init; }

  public short RockLayer { get; init; }

  public int WorldTime { get; init; }

  public bool DayTime { get; init; } = true;

  public byte MoonPhase { get; init; }

  public IReadOnlyList<WorldTileState>? WorldTiles { get; init; }

  public IReadOnlyList<bool>? FrameImportant { get; init; }

  public void Validate()
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(ProtocolVersion);
    ArgumentException.ThrowIfNullOrWhiteSpace(WorldName);
    if (WorldWidth is <= 0 or > short.MaxValue ||
        WorldHeight is <= 0 or > short.MaxValue ||
        SpawnTileX < 0 || SpawnTileX >= WorldWidth ||
        SpawnTileY < 0 || SpawnTileY >= WorldHeight)
    {
      throw new ArgumentOutOfRangeException(nameof(WorldWidth));
    }

    if (WorldTiles is not null && WorldTiles.Count != checked(WorldWidth * WorldHeight))
    {
      throw new ArgumentException(
          "The world tile payload does not match the configured dimensions.",
          nameof(WorldTiles));
    }

    if (FrameImportant is not null && FrameImportant.Count == 0)
    {
      throw new ArgumentException("The frame importance table cannot be empty.", nameof(FrameImportant));
    }
  }
}
