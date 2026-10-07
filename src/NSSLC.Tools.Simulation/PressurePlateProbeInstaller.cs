using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;
using RuntimeTileId = NSSLC.WorldGeneration.ID.TileID;

namespace Terraria.NonAuthoritative.SimulationHost;

internal static class PressurePlateProbeInstaller
{
  private const ushort RedWire = 0x80;
  private const ushort ActiveTile = 0x20;
  private const ushort Actuator = 0x800;
  private const ushort InactiveTile = 0x40;
  private const ushort WireMask = 0x380;
  private const int UnsupportedTimerOffset = 2;

  public static TileCoordinate Install(
    LoadedWorldSession session,
    RuntimePlayerStore players)
  {
    ArgumentNullException.ThrowIfNull(session);
    ArgumentNullException.ThrowIfNull(players);
    if (players.ActiveCount != 1)
    {
      throw new InvalidOperationException(
        "The pressure-plate probe requires exactly one local player.");
    }

    TileMapStore tileMap = session.Storage.TileMap;
    RuntimePlayerEntity player = players.Players[0];
    Vector2 position = player.Movement.Position;
    int plateX = Math.Clamp(
      (int)MathF.Floor((position.X + RuntimePlayerStore.PlayerWidth * 0.5f) / 16f),
      5,
      tileMap.Width - 6);
    int plateY = Math.Clamp(
      (int)MathF.Floor((position.Y + RuntimePlayerStore.PlayerHeight * 0.5f) / 16f),
      5,
      tileMap.Height - 6);
    int actuatorX = Math.Min(plateX + 5, tileMap.Width - 6);
    var plateAnchor = new TileCoordinate(plateX, plateY);
    var changedCoordinates = new List<TileCoordinate>(actuatorX - plateX + 1);
    for (int x = plateX; x <= actuatorX; x++)
    {
      TileCellState tile = tileMap.GetTile(x, plateY);
      tile.TileHeader = (ushort)(tile.TileHeader & ~(WireMask | Actuator | InactiveTile));
      tile.TileHeader |= RedWire;
      if (x == plateX)
      {
        tile.Type = 135;
        tile.TileHeader |= ActiveTile;
      }
      else if (x == plateX + UnsupportedTimerOffset)
      {
        tile.Type = RuntimeTileId.Timers;
        tile.TileHeader |= ActiveTile;
        tile.FrameX = 0;
        tile.FrameY = 0;
      }
      else if (x == actuatorX)
      {
        tile.Type = 1;
        tile.TileHeader |= ActiveTile | Actuator;
      }

      tileMap.CommitTile(x, plateY, tile);
      changedCoordinates.Add(new TileCoordinate(x, plateY));
    }

    session.Storage.PressurePlates.Replace(new[] { plateAnchor });
    WorldStorageOperationResult registryProjection =
      LegacyWorldPressurePlateProjection.PublishCommittedRegistry(session);
    if (!registryProjection.Succeeded)
    {
      throw new InvalidOperationException(
        $"The pressure-plate probe registry could not be projected: " +
        $"{registryProjection.Failure.Kind} {registryProjection.Failure.Detail}");
    }

    WorldStorageOperationResult projection =
      LegacyWorldTileMapProjection.PublishCommittedTiles(session, changedCoordinates);
    if (!projection.Succeeded)
    {
      throw new InvalidOperationException(
        $"The pressure-plate probe could not be projected: " +
        $"{projection.Failure.Kind} {projection.Failure.Detail}");
    }

    return plateAnchor;
  }
}
