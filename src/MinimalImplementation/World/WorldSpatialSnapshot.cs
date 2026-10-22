using System.Numerics;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>
/// Detached bounded tile capture. Queries never read the live owner. An uncaptured in-world
/// tile is an integration error, rather than an invented empty tile or an impassable border.
/// </summary>
public sealed class WorldSpatialSnapshot {
  public const int MaximumCaptureTiles = 262_144;

  private readonly WorldTile[] _tiles;

  public WorldHeaderSnapshot Header { get; }
  public WorldFacts Facts { get; }
  public ulong WorldEpoch { get; }
  public CommitVersion Version { get; }
  public WorldSectionBounds Bounds { get; }

  internal WorldSpatialSnapshot(
      WorldHeaderSnapshot header, WorldFacts facts, ulong worldEpoch, CommitVersion version,
      WorldSectionBounds bounds, WorldTile[] ownedTiles) {
    Header = header;
    Facts = facts;
    WorldEpoch = worldEpoch;
    Version = version;
    Bounds = bounds;
    _tiles = ownedTiles;
  }

  public WorldTile GetTile(TileCoordinate coordinate) {
    if (!Header.Geometry.Contains(coordinate)) {
      throw new ArgumentOutOfRangeException(nameof(coordinate));
    }
    RequireCoverage(new WorldSectionBounds(coordinate.X, coordinate.Y, 1, 1));
    return _tiles[(coordinate.Y - Bounds.OriginY) * Bounds.Width +
        coordinate.X - Bounds.OriginX];
  }

  /// <summary>Includes TileCollision's hoik neighbors and WetCollision's preceding row.</summary>
  public static WorldSectionBounds GetQueryBounds(
      WorldGeometry geometry, Vector2 position, int width, int height) {
    ValidateHitbox(position, width, height);
    int left = (int)Math.Clamp(Math.Truncate(position.X / 16d) - 2, 0, geometry.Width);
    int top = (int)Math.Clamp(Math.Truncate(position.Y / 16d) - 2, 0, geometry.Height);
    int right = (int)Math.Clamp(Math.Truncate((position.X + (double)width) / 16) + 3,
        0, geometry.Width);
    int bottom = (int)Math.Clamp(Math.Truncate((position.Y + (double)height) / 16) + 2,
        0, geometry.Height);
    return new WorldSectionBounds(left, top, right - left, bottom - top);
  }

  /// <summary>WorldGen.SolidTile2(int,int), including platform top slopes.</summary>
  public bool IsSolidTile2(int x, int y) {
    if (!Header.Geometry.Contains(new TileCoordinate(x, y))) {
      return false;
    }
    WorldTile tile = GetTile(new TileCoordinate(x, y));
    bool platform = tile.Type is 19 or 427 or 435 or 436 or 437 or 438 or 439;
    return tile.Active && !tile.Inactive && Facts.Solid[tile.Type] && !tile.HalfBlock &&
        (tile.Slope == 0 || (platform && tile.Slope is 1 or 2));
  }

  public bool HasLiquid(int x, int y, int? liquidType = null) {
    if (liquidType.HasValue && (uint)liquidType.Value > 3) {
      throw new ArgumentOutOfRangeException(nameof(liquidType));
    }
    if (!Header.Geometry.Contains(new TileCoordinate(x, y))) {
      return false;
    }
    WorldTile tile = GetTile(new TileCoordinate(x, y));
    return tile.LiquidAmount > 0 &&
        (!liquidType.HasValue || (int)tile.LiquidType == liquidType.Value);
  }

  /// <summary>Scans active tiles, including pylons that have no registered tile entity.</summary>
  public bool ContainsActiveTile(WorldSectionBounds rectangle, ushort tileType) {
    WorldSectionBounds scan = Clip(rectangle);
    RequireCoverage(scan);
    for (int x = scan.OriginX; x < scan.OriginX + scan.Width; x++) {
      for (int y = scan.OriginY; y < scan.OriginY + scan.Height; y++) {
        WorldTile tile = GetTile(new TileCoordinate(x, y));
        if (tile.Active && tile.Type == tileType) {
          return true;
        }
      }
    }
    return false;
  }

  /// <summary>Collision.SolidCollision's three-argument overload (slopes use tile AABBs).</summary>
  public bool HasSolidTileCollision(Vector2 position, int width, int height) {
    WorldSectionBounds scan = PrepareCollisionScan(position, width, height);
    for (int x = scan.OriginX; x < scan.OriginX + scan.Width; x++) {
      for (int y = scan.OriginY; y < scan.OriginY + scan.Height; y++) {
        WorldTile tile = GetTile(new TileCoordinate(x, y));
        if (tile.Active && !tile.Inactive && Facts.Solid[tile.Type] &&
            !Facts.SolidTop[tile.Type] && Intersects(position, width, height,
                x * 16f, y * 16f + (tile.HalfBlock ? 8 : 0),
                16, tile.HalfBlock ? 8 : 16)) {
          return true;
        }
      }
    }
    return false;
  }

  public WorldLiquidContact GetLiquidContact(Vector2 position, int width, int height) {
    WorldSectionBounds scan = PrepareCollisionScan(position, width, height);
    int probeWidth = Math.Min(10, width);
    int probeHeight = height / 2;
    var probe = new Vector2(position.X + width / 2 - probeWidth / 2,
        position.Y + height / 2 - probeHeight / 2);
    bool wet = false;
    bool honey = false;
    bool shimmer = false;
    bool lava = false;
    for (int x = scan.OriginX; x < scan.OriginX + scan.Width; x++) {
      for (int y = scan.OriginY; y < scan.OriginY + scan.Height; y++) {
        WorldTile tile = GetTile(new TileCoordinate(x, y));
        if (tile.LiquidAmount > 0) {
          float surfaceOffset = (256 - tile.LiquidAmount) / 16f;
          float surface = y * 16f + surfaceOffset;
          int liquidHeight = 16 - (int)surfaceOffset;
          if (!wet && Intersects(probe, probeWidth, probeHeight,
              x * 16f, surface, 16, liquidHeight)) {
            wet = true;
            honey = tile.LiquidType == LiquidType.Honey;
            shimmer = tile.LiquidType == LiquidType.Shimmer;
          }
          if (tile.LiquidType == LiquidType.Lava && Intersects(position, width, height,
              x * 16f, surface, 16, liquidHeight)) {
            lava = true;
          }
        } else if (!wet && tile.Active && tile.Slope != 0 && y > 0) {
          WorldTile above = GetTile(new TileCoordinate(x, y - 1));
          if (above.LiquidAmount > 0 && Intersects(probe, probeWidth, probeHeight,
              x * 16f, y * 16f, 16, 16)) {
            wet = true;
            honey = above.LiquidType == LiquidType.Honey;
            shimmer = above.LiquidType == LiquidType.Shimmer;
          }
        }
      }
    }
    return new WorldLiquidContact(wet, honey, shimmer, lava);
  }

  /// <summary>
  /// Collision.TileCollision with doors/platforms enabled. Resolves displacement without
  /// changing tiles, NPCs, liquid state, or the original static up/down flags.
  /// </summary>
  public WorldMotionResult Move(
      Vector2 position, Vector2 velocity, int width, int height,
      bool fallThrough = false, bool fall2 = false, int gravityDirection = 1, bool hoik = true) {
    ValidateHitbox(velocity, 1, 1);
    if (gravityDirection is not (1 or -1)) {
      throw new ArgumentOutOfRangeException(nameof(gravityDirection));
    }
    WorldSectionBounds scan = PrepareCollisionScan(position, width, height);
    Vector2 result = velocity;
    Vector2 target = position + velocity;
    ValidateHitbox(target, width, height);
    int sideX = -1;
    int sideY = -1;
    int verticalX = -1;
    int verticalY = -1;
    float nearestFloor = (scan.OriginY + scan.Height + 3) * 16f;
    for (int x = scan.OriginX; x < scan.OriginX + scan.Width; x++) {
      for (int y = scan.OriginY; y < scan.OriginY + scan.Height; y++) {
        WorldTile tile = GetTile(new TileCoordinate(x, y));
        bool platform = Facts.SolidTop[tile.Type];
        if (!tile.Active || tile.Inactive ||
            !(Facts.Solid[tile.Type] || (platform && tile.FrameY == 0))) {
          continue;
        }
        float tileX = x * 16f;
        float tileY = y * 16f + (tile.HalfBlock ? 8 : 0);
        int tileHeight = tile.HalfBlock ? 8 : 16;
        if (!Intersects(target, width, height, tileX, tileY, 16, tileHeight)) {
          continue;
        }
        bool topSlope = tile.Slope is 1 or 2;
        bool passSlope = tile.Slope switch {
          1 => position.Y + height - Math.Abs(velocity.X) <= tileY + tileHeight &&
              position.X >= tileX,
          2 => position.Y + height - Math.Abs(velocity.X) <= tileY + tileHeight &&
              position.X + width <= tileX + 16,
          3 => position.Y + Math.Abs(velocity.X) >= tileY && position.X >= tileX,
          4 => position.Y + Math.Abs(velocity.X) >= tileY && position.X + width <= tileX + 16,
          _ => false
        };
        if (passSlope) {
          continue;
        }
        if (position.Y + height <= tileY) {
          if (!(platform && fallThrough && (velocity.Y <= 1f || fall2)) &&
              nearestFloor > tileY) {
            verticalX = x;
            verticalY = y + (tile.HalfBlock ? 1 : 0);
            if (verticalX != sideX && !topSlope) {
              result.Y = tileY - (position.Y + height) +
                  (gravityDirection == -1 ? -0.01f : 0);
              nearestFloor = tileY;
            }
          }
        } else if (position.X + width <= tileX && !platform) {
          byte slope = x > 0 ? GetTile(new TileCoordinate(x - 1, y)).Slope : (byte)0;
          if (!hoik || slope is not (2 or 4)) {
            sideX = x;
            sideY = y;
            if (sideY != verticalY) {
              result.X = tileX - (position.X + width);
            }
            if (verticalX == sideX) {
              result.Y = velocity.Y;
            }
          }
        } else if (position.X >= tileX + 16 && !platform) {
          byte slope = x + 1 < Header.Geometry.Width
              ? GetTile(new TileCoordinate(x + 1, y)).Slope
              : (byte)0;
          if (!hoik || slope is not (1 or 3)) {
            sideX = x;
            sideY = y;
            if (sideY != verticalY) {
              result.X = tileX + 16 - position.X;
            }
            if (verticalX == sideX) {
              result.Y = velocity.Y;
            }
          }
        } else if (position.Y >= tileY + tileHeight && !platform) {
          verticalX = x;
          verticalY = y;
          result.Y = tileY + tileHeight - position.Y +
              (gravityDirection == 1 ? 0.01f : 0);
          if (verticalY == sideY) {
            result.X = velocity.X;
          }
        }
      }
    }
    return new WorldMotionResult(position + result, result,
        result.X != velocity.X, result.Y != velocity.Y);
  }

  /// <summary>Fits a resized collider using the current tile movement rules, feet first.</summary>
  public bool TryResizeFromBottomCenter(
      Vector2 position, int width, int height, int targetWidth, int targetHeight,
      out Vector2 resizedPosition) {
    ValidateHitbox(position, width, height);
    ValidateHitbox(position, targetWidth, targetHeight);
    resizedPosition = position;
    int heightGrowth = targetHeight - height;
    if (heightGrowth > 0) {
      int down = GetClearTravel(position, Vector2.UnitY, heightGrowth, width, height);
      int up = GetClearTravel(position, -Vector2.UnitY, heightGrowth, width, height);
      if (up + down < heightGrowth) {
        return false;
      }

      resizedPosition.Y -= Math.Min(heightGrowth, up);
    } else {
      resizedPosition.Y -= heightGrowth;
    }

    int widthGrowth = targetWidth - width;
    if (widthGrowth > 0) {
      int right = GetClearTravel(resizedPosition, Vector2.UnitX,
        widthGrowth, width, targetHeight);
      int left = GetClearTravel(resizedPosition, -Vector2.UnitX,
        widthGrowth, width, targetHeight);
      if (left + right < widthGrowth) {
        resizedPosition = position;
        return false;
      }

      int shared = Math.Min(widthGrowth / 2, Math.Min(left, right));
      int leftGrowth = shared + Math.Min(widthGrowth - 2 * shared, left - shared);
      resizedPosition.X -= leftGrowth;
    } else {
      resizedPosition.X += widthGrowth / 2;
    }

    return true;
  }

  private int GetClearTravel(
      Vector2 position, Vector2 direction, int distance, int width, int height) {
    const int stepSize = 16;
    for (int travelled = 0; travelled < distance; travelled += stepSize) {
      int step = Math.Min(stepSize, distance - travelled);
      Vector2 requested = direction * step;
      WorldMotionResult motion = Move(position + direction * travelled,
        requested, width, height);
      if (motion.Velocity != requested) {
        return travelled + (int)motion.Velocity.Length();
      }
    }

    return distance;
  }

  internal void RequireCoverage(WorldSectionBounds rectangle) {
    if (rectangle.Width > 0 && rectangle.Height > 0 &&
        (rectangle.OriginX < Bounds.OriginX || rectangle.OriginY < Bounds.OriginY ||
        (long)rectangle.OriginX + rectangle.Width > (long)Bounds.OriginX + Bounds.Width ||
        (long)rectangle.OriginY + rectangle.Height > (long)Bounds.OriginY + Bounds.Height)) {
      throw new InvalidOperationException("The query extends beyond the captured world region.");
    }
  }

  internal WorldSectionBounds Clip(WorldSectionBounds rectangle) {
    if (rectangle.Width < 0 || rectangle.Height < 0) {
      throw new ArgumentOutOfRangeException(nameof(rectangle));
    }
    WorldGeometry geometry = Header.Geometry;
    int left = Math.Clamp(rectangle.OriginX, 0, geometry.Width);
    int top = Math.Clamp(rectangle.OriginY, 0, geometry.Height);
    int right = (int)Math.Clamp((long)rectangle.OriginX + rectangle.Width, left, geometry.Width);
    int bottom = (int)Math.Clamp((long)rectangle.OriginY + rectangle.Height, top, geometry.Height);
    return new WorldSectionBounds(left, top, right - left, bottom - top);
  }

  private WorldSectionBounds PrepareCollisionScan(Vector2 position, int width, int height) {
    WorldGeometry geometry = Header.Geometry;
    RequireCoverage(GetQueryBounds(geometry, position, width, height));
    // Version4 collision intentionally excludes the last column and bottom 40 rows.
    int maxX = geometry.Width - 1;
    int maxY = Math.Max(0, geometry.Height - 40);
    int left = (int)Math.Clamp(Math.Truncate(position.X / 16d) - 1, 0, maxX);
    int top = (int)Math.Clamp(Math.Truncate(position.Y / 16d) - 1, 0, maxY);
    int right = (int)Math.Clamp(Math.Truncate((position.X + (double)width) / 16) + 2, 0, maxX);
    int bottom = (int)Math.Clamp(Math.Truncate((position.Y + (double)height) / 16) + 2, 0, maxY);
    return new WorldSectionBounds(left, top, right - left, bottom - top);
  }

  private static bool Intersects(
      Vector2 position, int width, int height, float x, float y, int otherWidth, int otherHeight) {
    return position.X + width > x && position.X < x + otherWidth &&
        position.Y + height > y && position.Y < y + otherHeight;
  }

  private static void ValidateHitbox(Vector2 position, int width, int height) {
    if (!float.IsFinite(position.X) || !float.IsFinite(position.Y) || width <= 0 || height <= 0) {
      throw new ArgumentOutOfRangeException(nameof(position));
    }
  }
}
