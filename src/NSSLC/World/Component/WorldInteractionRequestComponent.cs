namespace Terraria.World;

/// <summary>
/// One short-lived request for the authoritative world port.
/// </summary>
/// <remarks>
/// The request contains coordinates and operation data only. It does not contain a tile map,
/// file handle, Arch entity reference or persistence identity.
/// </remarks>
public record struct WorldInteractionRequestComponent
{
  public WorldInteractionRequestComponent(
      long requestId,
      long tick,
      WorldInteractionOperation operation,
      WorldTileCoordinate coordinate,
      ushort tileType = 0,
      byte style = 0)
  {
    if (requestId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(requestId));
    }

    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    RequestId = requestId;
    Tick = tick;
    Operation = operation;
    Coordinate = coordinate;
    TileType = tileType;
    Style = style;
    IsPending = true;
  }

  public long RequestId { get; }

  public long Tick { get; }

  public WorldInteractionOperation Operation { get; }

  public WorldTileCoordinate Coordinate { get; }

  public ushort TileType { get; }

  public byte Style { get; }

  public bool IsPending { get; private set; }

  public static WorldInteractionRequestComponent ReadTile(
      long requestId,
      long tick,
      WorldTileCoordinate coordinate)
  {
    return new WorldInteractionRequestComponent(
        requestId,
        tick,
        WorldInteractionOperation.ReadTile,
        coordinate);
  }

  public static WorldInteractionRequestComponent BreakTile(
      long requestId,
      long tick,
      WorldTileCoordinate coordinate)
  {
    return new WorldInteractionRequestComponent(
        requestId,
        tick,
        WorldInteractionOperation.BreakTile,
        coordinate);
  }

  public static WorldInteractionRequestComponent PlaceTile(
      long requestId,
      long tick,
      WorldTileCoordinate coordinate,
      ushort tileType,
      byte style = 0)
  {
    return new WorldInteractionRequestComponent(
        requestId,
        tick,
        WorldInteractionOperation.PlaceTile,
        coordinate,
        tileType,
        style);
  }

  public void MarkConsumed()
  {
    IsPending = false;
  }
}
