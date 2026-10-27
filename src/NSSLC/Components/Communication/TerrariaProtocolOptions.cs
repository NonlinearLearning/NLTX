namespace Terraria.Communication;

/// <summary>Small authoritative facts needed by the Version4 join handshake.</summary>
public sealed class TerrariaProtocolOptions
{
  public string ProtocolVersion { get; init; } = "Terraria319";

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
