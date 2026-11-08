using System;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// The portion of a Terraria world file header that can be decoded without a runtime world.
/// </summary>
public sealed class WorldFileHeaderSection
{
  public const string SectionId = "world.header";

  public WorldFileHeaderSection(
    string worldName,
    string? seedText,
    ulong? worldGeneratorVersion,
    Guid? uniqueId,
    int worldId,
    int leftWorld,
    int rightWorld,
    int topWorld,
    int bottomWorld,
    int maxTilesX,
    int maxTilesY,
    int gameMode,
    bool drunkWorld,
    bool getGoodWorld,
    bool tenthAnniversaryWorld,
    bool dontStarveWorld,
    bool notTheBeesWorld,
    bool remixWorld,
    bool noTrapsWorld,
    bool zenithWorld,
    bool skyblockWorld,
    DateTime? creationTime,
    DateTime? lastPlayed)
  {
    ArgumentNullException.ThrowIfNull(worldName);
    WorldName = worldName;
    SeedText = seedText;
    WorldGeneratorVersion = worldGeneratorVersion;
    UniqueId = uniqueId;
    WorldId = worldId;
    LeftWorld = leftWorld;
    RightWorld = rightWorld;
    TopWorld = topWorld;
    BottomWorld = bottomWorld;
    MaxTilesX = maxTilesX;
    MaxTilesY = maxTilesY;
    GameMode = gameMode;
    DrunkWorld = drunkWorld;
    GetGoodWorld = getGoodWorld;
    TenthAnniversaryWorld = tenthAnniversaryWorld;
    DontStarveWorld = dontStarveWorld;
    NotTheBeesWorld = notTheBeesWorld;
    RemixWorld = remixWorld;
    NoTrapsWorld = noTrapsWorld;
    ZenithWorld = zenithWorld;
    SkyblockWorld = skyblockWorld;
    CreationTime = creationTime;
    LastPlayed = lastPlayed;
  }

  public string WorldName { get; }

  public string? SeedText { get; }

  public ulong? WorldGeneratorVersion { get; }

  public Guid? UniqueId { get; }

  public int WorldId { get; }

  public int LeftWorld { get; }

  public int RightWorld { get; }

  public int TopWorld { get; }

  public int BottomWorld { get; }

  public int MaxTilesX { get; }

  public int MaxTilesY { get; }

  public int GameMode { get; }

  public bool DrunkWorld { get; }

  public bool GetGoodWorld { get; }

  public bool TenthAnniversaryWorld { get; }

  public bool DontStarveWorld { get; }

  public bool NotTheBeesWorld { get; }

  public bool RemixWorld { get; }

  public bool NoTrapsWorld { get; }

  public bool ZenithWorld { get; }

  public bool SkyblockWorld { get; }

  public DateTime? CreationTime { get; }

  public DateTime? LastPlayed { get; }
}
