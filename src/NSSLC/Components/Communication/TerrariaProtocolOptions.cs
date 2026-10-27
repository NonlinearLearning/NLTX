namespace Terraria.Communication;

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
  }
}
