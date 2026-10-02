using System;
using System.Numerics;

using EntityEcs.Components;

namespace Terraria.SpatialSimulation;

// status: proposed
// snapshotId: SPATIAL.SNAPSHOT.TILE
// crossSubsystemOwner: integration-review
public readonly record struct SpatialTileSnapshot
{
  public const int TileSize = 16;

  public SpatialTileSnapshot(
    int x,
    int y,
    bool exists,
    bool isActive,
    bool blocksMovement,
    bool isSolid,
    bool isSolidTop,
    bool isHalfBrick,
    byte slope,
    byte liquidAmount,
    LiquidKind liquidKind = LiquidKind.Nano)
  {
    if (slope > 5)
    {
      throw new ArgumentOutOfRangeException(
        nameof(slope),
        slope,
        "Slope must be in the supported tile range.");
    }

    if (!Enum.IsDefined(liquidKind))
    {
      throw new ArgumentOutOfRangeException(
        nameof(liquidKind),
        liquidKind,
        "Liquid kind must be defined.");
    }

    X = x;
    Y = y;
    Exists = exists;
    IsActive = isActive;
    BlocksMovement = blocksMovement;
    IsSolid = isSolid;
    IsSolidTop = isSolidTop;
    IsHalfBrick = isHalfBrick;
    Slope = slope;
    LiquidAmount = liquidAmount;
    LiquidKind = liquidKind;
  }

  public int X { get; }

  public int Y { get; }

  public bool Exists { get; }

  public bool IsActive { get; }

  public bool BlocksMovement { get; }

  public bool IsSolid { get; }

  public bool IsSolidTop { get; }

  public bool IsHalfBrick { get; }

  public byte Slope { get; }

  public byte LiquidAmount { get; }

  public LiquidKind LiquidKind { get; }

  public bool HasLiquid => LiquidAmount > 0;

  public bool HasUnsupportedSlopeFacts => Slope != 0;

  public SpatialGeometrySnapshot CollisionGeometry(long revision)
  {
    float height = IsHalfBrick ? 8.0f : TileSize;
    Vector2 position = new(
      X * TileSize,
      Y * TileSize + (IsHalfBrick ? 8.0f : 0.0f));
    return new SpatialGeometrySnapshot(
      revision,
      position,
      new Vector2(TileSize, height));
  }

  public SpatialGeometrySnapshot LiquidGeometry(long revision)
  {
    float liquidOffset = (256.0f - LiquidAmount) / 32.0f;
    float height = TileSize - (int)(liquidOffset * 2.0f);
    Vector2 position = new(
      X * TileSize,
      Y * TileSize + liquidOffset * 2.0f);
    return new SpatialGeometrySnapshot(
      revision,
      position,
      new Vector2(TileSize, height));
  }
}
