namespace Terraria.World;

/// <summary>
/// Result written back to the requesting entity after a world operation is processed.
/// </summary>
public record struct WorldInteractionResultComponent
{
  public long RequestId { get; private set; }

  public long Tick { get; private set; }

  public WorldInteractionOperation Operation { get; private set; }

  public WorldInteractionResultStatus Status { get; private set; }

  public WorldTileState Tile { get; private set; }

  public string? Failure { get; private set; }

  public bool IsComplete => Status is
      WorldInteractionResultStatus.Succeeded or WorldInteractionResultStatus.Rejected;

  public static WorldInteractionResultComponent Success(
      in WorldInteractionRequestComponent request,
      WorldTileState tile = default)
  {
    return new WorldInteractionResultComponent
    {
      RequestId = request.RequestId,
      Tick = request.Tick,
      Operation = request.Operation,
      Status = WorldInteractionResultStatus.Succeeded,
      Tile = tile
    };
  }

  public static WorldInteractionResultComponent Rejected(
      in WorldInteractionRequestComponent request,
      string failure)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(failure);
    return new WorldInteractionResultComponent
    {
      RequestId = request.RequestId,
      Tick = request.Tick,
      Operation = request.Operation,
      Status = WorldInteractionResultStatus.Rejected,
      Failure = failure
    };
  }
}
