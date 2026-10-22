using System;
using System.Collections.Generic;

namespace MinimalImplementation.Persistence;

/// <summary>Explicit inputs for creating a new Version4 world document candidate.</summary>
public sealed class WorldCreationRequest
{
  public WorldCreationRequest(
    string worldName,
    string? seedText,
    Guid uniqueId,
    int worldId,
    int maxTilesX,
    int maxTilesY,
    int spawnTileX,
    int spawnTileY,
    ReadOnlyMemory<byte> compressedTilePayload,
    IReadOnlyList<bool> frameImportant,
    int gameMode = 0,
    ulong? worldGeneratorVersion = null,
    DateTime? creationTime = null,
    DateTime? lastPlayed = null)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(worldName);
    if (uniqueId == Guid.Empty)
    {
      throw new ArgumentException(
        "A newly created world requires a nonzero persisted identifier.", nameof(uniqueId));
    }

    if (maxTilesX <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxTilesX));
    }

    if (maxTilesY <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxTilesY));
    }

    long tileCount = checked((long)maxTilesX * maxTilesY);
    if (tileCount > 25_000_000)
    {
      throw new ArgumentOutOfRangeException(
        nameof(maxTilesX), "A minimal world exceeds the bounded tile budget.");
    }

    if (spawnTileX < 0 || spawnTileX >= maxTilesX)
    {
      throw new ArgumentOutOfRangeException(nameof(spawnTileX));
    }

    if (spawnTileY < 0 || spawnTileY >= maxTilesY)
    {
      throw new ArgumentOutOfRangeException(nameof(spawnTileY));
    }

    if (gameMode < 0 || gameMode > 3)
    {
      throw new ArgumentOutOfRangeException(nameof(gameMode));
    }

    if (compressedTilePayload.IsEmpty)
    {
      throw new ArgumentException(
        "A newly created world requires a nonempty encoded tile payload.",
        nameof(compressedTilePayload));
    }

    ArgumentNullException.ThrowIfNull(frameImportant);
    if (frameImportant.Count == 0 || frameImportant.Count > ushort.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(frameImportant));
    }

    WorldName = worldName;
    SeedText = seedText;
    UniqueId = uniqueId;
    WorldId = worldId;
    MaxTilesX = maxTilesX;
    MaxTilesY = maxTilesY;
    SpawnTileX = spawnTileX;
    SpawnTileY = spawnTileY;
    CompressedTilePayload = compressedTilePayload.ToArray();
    FrameImportant = Array.AsReadOnly(new List<bool>(frameImportant).ToArray());
    GameMode = gameMode;
    WorldGeneratorVersion = worldGeneratorVersion;
    CreationTime = creationTime;
    LastPlayed = lastPlayed;
  }

  public string WorldName { get; }

  public string? SeedText { get; }

  public Guid UniqueId { get; }

  public int WorldId { get; }

  public int MaxTilesX { get; }

  public int MaxTilesY { get; }

  public int SpawnTileX { get; }

  public int SpawnTileY { get; }

  public ReadOnlyMemory<byte> CompressedTilePayload { get; }

  public IReadOnlyList<bool> FrameImportant { get; }

  public int GameMode { get; }

  public ulong? WorldGeneratorVersion { get; }

  public DateTime? CreationTime { get; }

  public DateTime? LastPlayed { get; }
}
