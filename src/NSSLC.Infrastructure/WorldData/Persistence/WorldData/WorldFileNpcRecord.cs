using System;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// A persisted NPC record without a dependency on the runtime NPC object.
/// </summary>
public sealed class WorldFileNpcRecord
{
  public WorldFileNpcRecord(
    int? netId,
    string? legacyTypeName,
    bool isTownNpc,
    string name,
    float positionX,
    float positionY,
    bool homeless,
    int homeTileX,
    int homeTileY,
    int? townNpcVariationIndex,
    bool homelessDespawn)
  {
    if (netId is < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(netId));
    }

    if (netId is null && string.IsNullOrEmpty(legacyTypeName))
    {
      throw new ArgumentException(
        "An NPC requires a numeric type or a legacy type name.",
        nameof(legacyTypeName));
    }

    ArgumentNullException.ThrowIfNull(name);
    if (!float.IsFinite(positionX) || !float.IsFinite(positionY))
    {
      throw new ArgumentOutOfRangeException(nameof(positionX));
    }

    NetId = netId;
    LegacyTypeName = legacyTypeName;
    IsTownNpc = isTownNpc;
    Name = name;
    PositionX = positionX;
    PositionY = positionY;
    Homeless = homeless;
    HomeTileX = homeTileX;
    HomeTileY = homeTileY;
    TownNpcVariationIndex = townNpcVariationIndex;
    HomelessDespawn = homelessDespawn;
  }

  public int? NetId { get; }

  public string? LegacyTypeName { get; }

  public bool IsTownNpc { get; }

  public string Name { get; }

  public float PositionX { get; }

  public float PositionY { get; }

  public bool Homeless { get; }

  public int HomeTileX { get; }

  public int HomeTileY { get; }

  public int? TownNpcVariationIndex { get; }

  public bool HomelessDespawn { get; }
}
