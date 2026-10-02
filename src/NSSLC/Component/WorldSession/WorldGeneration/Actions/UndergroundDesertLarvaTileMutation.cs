using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Actions;

/// <summary>
/// Describes one tile mutation in the Version4 underground-desert larva footprint.
/// </summary>
public readonly record struct UndergroundDesertLarvaTileMutation
{
  public enum OperationKind : byte
  {
    DeactivateTile,
    ConfigureFoundationTile,
    PlaceLarvaObject,
  }

  private UndergroundDesertLarvaTileMutation(
    OperationKind kind,
    TilePosition target,
    bool isActive,
    ushort tileType,
    int slope,
    bool halfBrick,
    bool mute)
  {
    Kind = kind;
    Target = target;
    IsActive = isActive;
    TileType = tileType;
    Slope = slope;
    HalfBrick = halfBrick;
    Mute = mute;
  }

  public OperationKind Kind { get; }

  public TilePosition Target { get; }

  public bool IsActive { get; }

  public ushort TileType { get; }

  public int Slope { get; }

  public bool HalfBrick { get; }

  public bool Mute { get; }

  public static UndergroundDesertLarvaTileMutation DeactivateTile(
    TilePosition target)
  {
    return new UndergroundDesertLarvaTileMutation(
      OperationKind.DeactivateTile,
      target,
      isActive: false,
      tileType: 0,
      slope: 0,
      halfBrick: false,
      mute: false);
  }

  public static UndergroundDesertLarvaTileMutation ConfigureFoundationTile(
    TilePosition target)
  {
    return new UndergroundDesertLarvaTileMutation(
      OperationKind.ConfigureFoundationTile,
      target,
      isActive: true,
      tileType: 225,
      slope: 0,
      halfBrick: false,
      mute: false);
  }

  public static UndergroundDesertLarvaTileMutation PlaceLarvaObject(
    TilePosition target)
  {
    return new UndergroundDesertLarvaTileMutation(
      OperationKind.PlaceLarvaObject,
      target,
      isActive: true,
      tileType: 231,
      slope: 0,
      halfBrick: false,
      mute: true);
  }

  public bool IsWellFormed =>
    Kind switch
    {
      OperationKind.DeactivateTile =>
        !IsActive && TileType == 0 && Slope == 0 && !HalfBrick && !Mute,
      OperationKind.ConfigureFoundationTile =>
        IsActive && TileType == 225 && Slope == 0 && !HalfBrick && !Mute,
      OperationKind.PlaceLarvaObject =>
        IsActive && TileType == 231 && Slope == 0 && !HalfBrick && Mute,
      _ => false,
    };

  public void Validate()
  {
    if (!IsWellFormed)
    {
      throw new ArgumentException(
        "The underground-desert larva tile mutation is not well formed.",
        nameof(Kind));
    }
  }
}
