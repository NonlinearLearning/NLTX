using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria.Unshared.Session;
using Terraria.Share.Liquid;
using Terraria.Unshared.World;

namespace Terraria.Unshared.EntitySpawn;

/// <summary>
/// Port of Version4 <c>Spawner.GetSpawnArea</c>, <c>FindSpawnTile</c>, <c>HasTileSpawnSpace</c>,
/// <c>CanSpawnInTile</c> and <c>CheckNotSpawningOnScreen</c>.
/// </summary>
/// <remarks>
/// The reference works on the client screen size, so the numbers below are expressed in tiles of
/// that screen. It approximates solidity with <c>Main.tileSolid</c>; the server tile grid has no
/// solidity table yet, so an active, unactuated tile is treated as solid, the same approximation
/// the item and tile motion solvers already make. House walls are not modelled, which matches the
/// reference behaviour with <c>ignoreSafeWalls</c> set.
/// </remarks>
public static class EntitySpawnAreaRules
{
  /// <summary>Reference screen width in pixels (1920x1080 client).</summary>
  public const float ScreenWidthPixels = 1920f;

  /// <summary>Reference screen height in pixels.</summary>
  public const float ScreenHeightPixels = 1080f;

  /// <summary>Reference <c>sWidth</c> / <c>sHeight</c> expressed in tiles.</summary>
  public const float ScreenTileWidth = ScreenWidthPixels / 16f;

  public const float ScreenTileHeight = ScreenHeightPixels / 16f;

  /// <summary>Version4 uses 0.7 of the screen for the search area.</summary>
  public const float SpawnHalfExtentFactor = 0.7f;

  /// <summary>Version4 uses 0.52 of the screen for the forbidden safe area.</summary>
  public const float SafeHalfExtentFactor = 0.52f;

  /// <summary>Version4 tries 50 random tiles before giving up for this player.</summary>
  public const int MaximumAttempts = 50;

  public const int SpawnSpaceWidth = 2;

  public const int SpawnSpaceHeight = 3;

  public static EntitySpawnArea GetSpawnArea(
      Vector2 playerPosition,
      float tileSize,
      int worldWidth,
      int worldHeight)
  {
    if (tileSize <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(tileSize));
    }

    int playerTileX = (int)(playerPosition.X / tileSize);
    int playerTileY = (int)(playerPosition.Y / tileSize);
    int spawnHalfExtentX = (int)(ScreenTileWidth * SpawnHalfExtentFactor);
    int spawnHalfExtentY = (int)(ScreenTileHeight * SpawnHalfExtentFactor);
    int safeHalfExtentX = (int)(ScreenTileWidth * SafeHalfExtentFactor);
    int safeHalfExtentY = (int)(ScreenTileHeight * SafeHalfExtentFactor);
    return new EntitySpawnArea(
        WorldTileArea
            .Centered(playerTileX, playerTileY, spawnHalfExtentX * 2, spawnHalfExtentY * 2)
            .ClampToWorld(worldWidth, worldHeight),
        WorldTileArea.Centered(playerTileX, playerTileY, safeHalfExtentX * 2, safeHalfExtentY * 2),
        safeHalfExtentX,
        safeHalfExtentY);
  }

  /// <summary>
  /// Picks one spawn tile inside <see cref="EntitySpawnArea.Spawn"/>: a random column, then the first
  /// ground below it, outside the safe area and with room for the NPC body.
  /// </summary>
  public static bool TryFindSpawnTile(
      in EntitySpawnArea area,
      IWorldTileGrid tiles,
      ref EntitySpawnRandom random,
      out WorldTileCoordinate spawnTile)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    spawnTile = default;
    if (!area.IsValid)
    {
      return false;
    }

    for (int attempt = 0; attempt < MaximumAttempts; attempt++)
    {
      int x = random.Next(area.Spawn.Left, area.Spawn.Right);
      int y = random.Next(area.Spawn.Top, area.Spawn.Bottom);
      if (IsSolid(tiles, x, y))
      {
        continue;
      }

      while (y < area.Spawn.Bottom && !IsSolid(tiles, x, y))
      {
        y++;
      }

      if (y >= area.Spawn.Bottom)
      {
        continue;
      }

      if (area.Safe.Contains(x, y) || !HasSpawnSpace(tiles, x, y))
      {
        continue;
      }

      spawnTile = new WorldTileCoordinate(x, y);
      return true;
    }

    return false;
  }

  /// <summary>Version4 <c>CanSpawnInTile</c>: free of solid tiles and free of lava.</summary>
  public static bool CanSpawnInTile(IWorldTileGrid tiles, int x, int y)
  {
    if (!IsInsideWorld(tiles, x, y))
    {
      return false;
    }

    WorldTileState tile = tiles.ReadTile(new WorldTileCoordinate(x, y));
    if (tile.Active && !tile.Inactive)
    {
      return false;
    }

    return !(tile.Liquid == LiquidContactFlags.Lava && tile.LiquidAmount > 0);
  }

  /// <summary>Version4 <c>HasTileSpawnSpace</c>: a 2x3 box of spawnable tiles above the ground.</summary>
  public static bool HasSpawnSpace(IWorldTileGrid tiles, int groundTileX, int groundTileY)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    int left = groundTileX - SpawnSpaceWidth / 2;
    int top = groundTileY - SpawnSpaceHeight;
    for (int x = left; x < left + SpawnSpaceWidth; x++)
    {
      for (int y = top; y < top + SpawnSpaceHeight; y++)
      {
        if (!CanSpawnInTile(tiles, x, y))
        {
          return false;
        }
      }
    }

    return true;
  }

  /// <summary>
  /// Version4 <c>CheckNotSpawningOnScreen</c>: false when the tile falls inside any player's view
  /// plus their safe margin.
  /// </summary>
  /// <remarks>
  /// The reference mixes units here: the screen term is in pixels while the safe extent keeps the
  /// tile count it was computed with. Porting the mixed form keeps the observable distance
  /// identical to the client.
  /// </remarks>
  public static bool IsVisibleToAnyPlayer(
      int tileX,
      int tileY,
      float tileSize,
      IReadOnlyList<SessionPlayer> players,
      int safeHalfExtentX,
      int safeHalfExtentY)
  {
    ArgumentNullException.ThrowIfNull(players);
    int screenHalfWidth = (int)(ScreenWidthPixels * 0.5f);
    int screenHalfHeight = (int)(ScreenHeightPixels * 0.5f);
    int tileLeft = (int)(tileX * tileSize);
    int tileTop = (int)(tileY * tileSize);
    int tileRight = tileLeft + (int)tileSize;
    int tileBottom = tileTop + (int)tileSize;
    for (int index = 0; index < players.Count; index++)
    {
      SessionPlayer player = players[index];
      if (!player.IsAlive)
      {
        continue;
      }

      int centerX = (int)(player.Position.X + player.Size.X / 2f);
      int centerY = (int)(player.Position.Y + player.Size.Y / 2f);
      int viewLeft = centerX - screenHalfWidth - safeHalfExtentX;
      int viewTop = centerY - screenHalfHeight - safeHalfExtentY;
      int viewRight = viewLeft + (int)ScreenWidthPixels + safeHalfExtentX * 2;
      int viewBottom = viewTop + (int)ScreenHeightPixels + safeHalfExtentY * 2;
      if (tileLeft < viewRight && viewLeft < tileRight &&
          tileTop < viewBottom && viewTop < tileBottom)
      {
        return true;
      }
    }

    return false;
  }

  private static bool IsSolid(IWorldTileGrid tiles, int x, int y)
  {
    if (!IsInsideWorld(tiles, x, y))
    {
      return true;
    }

    WorldTileState tile = tiles.ReadTile(new WorldTileCoordinate(x, y));
    return tile.Active && !tile.Inactive;
  }

  private static bool IsInsideWorld(IWorldTileGrid tiles, int x, int y)
    => x >= 0 && y >= 0 && x < tiles.Width && y < tiles.Height;
}
