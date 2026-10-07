using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;
using RuntimeTileId = NSSLC.WorldGeneration.ID.TileID;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class WiredActuatorTrigger
{
  private const ushort ActiveTileFlag = 0x20;
  private const ushort ActuatorFlag = 0x800;
  private const ushort InactiveTileFlag = 0x40;
  private const int MaximumVisitedWireStates = 1_000_000;
  private readonly HashSet<ushort> _unsupportedWiredDeviceTileTypes = new();

  public IReadOnlyList<ushort> RecognizedUnsupportedWiredDeviceTileTypes =>
    _unsupportedWiredDeviceTileTypes.OrderBy(static tileType => tileType).ToArray();

  public int Trigger(LoadedWorldSession session, TileCoordinate source)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (!IsWithinBounds(session, source))
    {
      throw new ArgumentOutOfRangeException(nameof(source));
    }

    var pending = new Queue<WireState>();
    var visited = new HashSet<WireState>();
    var actuatorChanges = new Dictionary<TileCoordinate, TileCellState>();
    TileCellState sourceTile = session.Storage.TileMap.GetTile(source.X, source.Y);
    byte sourceWires = GetWireMask(sourceTile);
    for (byte color = 1; color <= 8; color <<= 1)
    {
      if ((sourceWires & color) != 0)
      {
        pending.Enqueue(new WireState(source, color));
      }
    }

    while (pending.TryDequeue(out WireState current))
    {
      if (!visited.Add(current))
      {
        continue;
      }
      if (visited.Count > MaximumVisitedWireStates)
      {
        throw new InvalidOperationException(
          $"The logic sensor wire network exceeds {MaximumVisitedWireStates} tile/color states.");
      }

      TileCellState tile = session.Storage.TileMap.GetTile(
        current.Coordinate.X,
        current.Coordinate.Y);
      if ((GetWireMask(tile) & current.Color) == 0)
      {
        continue;
      }

      if (IsUnsupportedWiredDevice(tile.Type))
      {
        _unsupportedWiredDeviceTileTypes.Add(tile.Type);
      }

      if ((tile.TileHeader & (ActiveTileFlag | ActuatorFlag)) ==
          (ActiveTileFlag | ActuatorFlag))
      {
        tile.TileHeader ^= InactiveTileFlag;
        actuatorChanges[current.Coordinate] = tile;
      }

      EnqueueIfConnected(session, pending, current, current.Coordinate.X - 1, current.Coordinate.Y);
      EnqueueIfConnected(session, pending, current, current.Coordinate.X + 1, current.Coordinate.Y);
      EnqueueIfConnected(session, pending, current, current.Coordinate.X, current.Coordinate.Y - 1);
      EnqueueIfConnected(session, pending, current, current.Coordinate.X, current.Coordinate.Y + 1);
    }

    var changedCoordinates = new List<TileCoordinate>(actuatorChanges.Count);
    foreach ((TileCoordinate coordinate, TileCellState updatedTile) in actuatorChanges
      .OrderBy(static entry => entry.Key.Y)
      .ThenBy(static entry => entry.Key.X))
    {
      session.Storage.TileMap.CommitTile(coordinate.X, coordinate.Y, updatedTile);
      changedCoordinates.Add(coordinate);
    }

    WorldStorageOperationResult projection = LegacyWorldTileMapProjection.PublishCommittedTiles(
      session,
      changedCoordinates);
    if (!projection.Succeeded)
    {
      throw new InvalidOperationException(
        $"The wired actuator tiles could not be projected to the runtime: " +
        $"{projection.Failure.Kind} {projection.Failure.Detail}");
    }

    return actuatorChanges.Count;
  }

  private static bool IsUnsupportedWiredDevice(ushort tileType)
  {
    return tileType is
      RuntimeTileId.ClosedDoor or
      RuntimeTileId.OpenDoor or
      RuntimeTileId.Lever or
      RuntimeTileId.Switches or
      RuntimeTileId.Traps or
      RuntimeTileId.Explosives or
      RuntimeTileId.InletPump or
      RuntimeTileId.OutletPump or
      RuntimeTileId.Timers or
      RuntimeTileId.Firework or
      RuntimeTileId.Teleporter or
      RuntimeTileId.MinecartTrack or
      RuntimeTileId.FireworksBox or
      RuntimeTileId.AlphabetStatues or
      RuntimeTileId.FireworkFountain or
      RuntimeTileId.MushroomStatue or
      RuntimeTileId.TrapdoorOpen or
      RuntimeTileId.TrapdoorClosed or
      RuntimeTileId.TallGateClosed or
      RuntimeTileId.TallGateOpen or
      RuntimeTileId.Detonator or
      RuntimeTileId.LogicGateLamp or
      RuntimeTileId.LogicGate or
      RuntimeTileId.WireBulb or
      RuntimeTileId.ProjectilePressurePad or
      RuntimeTileId.GeyserTrap or
      RuntimeTileId.BoulderStatue;
  }

  private static void EnqueueIfConnected(
    LoadedWorldSession session,
    Queue<WireState> pending,
    WireState current,
    int x,
    int y)
  {
    var coordinate = new TileCoordinate(x, y);
    if (!IsWithinBounds(session, coordinate))
    {
      return;
    }

    TileCellState neighbor = session.Storage.TileMap.GetTile(x, y);
    if ((GetWireMask(neighbor) & current.Color) != 0)
    {
      pending.Enqueue(new WireState(coordinate, current.Color));
    }
  }

  private static byte GetWireMask(TileCellState tile)
  {
    byte wires = (byte)((tile.TileHeader & 0x380) >> 7);
    if ((tile.Header & 0x80) != 0)
    {
      wires |= 8;
    }

    return wires;
  }

  private static bool IsWithinBounds(LoadedWorldSession session, TileCoordinate coordinate)
  {
    return (uint)coordinate.X < (uint)session.Storage.TileMap.Width &&
      (uint)coordinate.Y < (uint)session.Storage.TileMap.Height;
  }

  private readonly record struct WireState(TileCoordinate Coordinate, byte Color);
}
