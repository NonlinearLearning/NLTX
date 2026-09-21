using System.Globalization;

namespace Terraria.NonAuthoritative.WorldSession;

public sealed record WorldDescriptorPersistenceSnapshot
{
  public WorldDescriptorPersistenceSnapshot(
    int worldId,
    Guid uniqueId,
    ulong worldGeneratorVersion,
    string seedText,
    DateTime creationTime,
    DateTime lastPlayed,
    int worldSizeX,
    int worldSizeY,
    int gameMode)
  {
    if (worldSizeX <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSizeX), "World width must be positive.");
    }

    if (worldSizeY <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSizeY), "World height must be positive.");
    }

    if (seedText is null)
    {
      throw new ArgumentNullException(nameof(seedText));
    }

    if (seedText.Length > MaxUserSeedTextLength)
    {
      throw new ArgumentOutOfRangeException(
        nameof(seedText),
        $"Seed text cannot exceed {MaxUserSeedTextLength} characters.");
    }

    WorldId = worldId;
    UniqueId = uniqueId;
    WorldGeneratorVersion = worldGeneratorVersion;
    SeedText = seedText;
    CreationTime = creationTime;
    LastPlayed = lastPlayed;
    WorldSizeX = worldSizeX;
    WorldSizeY = worldSizeY;
    GameMode = gameMode;
  }

  public const int MaxUserSeedTextLength = 40;

  public const ulong GuidInWorldFileVersion = 777389080577UL;

  public int WorldId { get; init; }

  public Guid UniqueId { get; init; }

  public ulong WorldGeneratorVersion { get; init; }

  public string SeedText { get; init; }

  public DateTime CreationTime { get; init; }

  public DateTime LastPlayed { get; init; }

  public int WorldSizeX { get; init; }

  public int WorldSizeY { get; init; }

  public int GameMode { get; init; }

  public bool UseGuidAsMapName => WorldGeneratorVersion >= GuidInWorldFileVersion;

  public string MapFileName => UseGuidAsMapName ? UniqueId.ToString() : WorldId.ToString(CultureInfo.InvariantCulture);

  public string WorldSizeName => (WorldSizeX, WorldSizeY) switch
  {
    (4200, 1200) => "Small",
    (6400, 1800) => "Medium",
    (8400, 2400) => "Large",
    _ => "Unknown"
  };

  public bool TryGetNumericSeed(out int seed)
  {
    return int.TryParse(SeedText, NumberStyles.Integer, CultureInfo.InvariantCulture, out seed);
  }
}
