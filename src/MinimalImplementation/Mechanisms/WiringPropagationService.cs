using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Plans Version4 wire colors in source order against one evolving tile view.</summary>
public sealed class WiringPropagationService
{
  private static readonly TileCoordinate[] Directions =
  [
    new TileCoordinate(0, 1),
    new TileCoordinate(0, -1),
    new TileCoordinate(1, 0),
    new TileCoordinate(-1, 0)
  ];

  private readonly MechanismFactsCatalog _facts;

  public WiringPropagationService(MechanismFactsCatalog facts)
  {
    _facts = facts ?? throw new ArgumentNullException(nameof(facts));
  }

  public WiringPropagationPlan Plan(
    WorldSnapshot world,
    HitSwitchPacket request,
    MechanismCooldownService.Preparation? cooldowns = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(request);

    var origin = new TileCoordinate(request.X, request.Y);
    if (!world.Header.Geometry.Contains(origin))
    {
      return WiringPropagationPlan.Rejected("The switch coordinate is outside the world.");
    }

    WorldTile originTile = world.GetTile(origin);
    if (!originTile.Active || !IsSwitch(originTile))
    {
      return WiringPropagationPlan.Rejected(
        "The switch coordinate is not an active Version4 switch.");
    }

    var operation = new WiringTileOperation(world, _facts);
    TileCoordinate topLeft = origin;
    int width = 1;
    int height = 1;
    switch (originTile.Type)
    {
      case 136:
        operation.Write(origin, new WorldTileDelta(
          FrameY: originTile.FrameY == 0 ? (short)18 : (short)0));
        break;
      case 440:
        width = height = 3;
        break;
      case 132:
      case 411:
      case 441:
      case 468:
      case 467:
        int offsetX = -(originTile.FrameX / 18) % 4;
        short frameChange = 36;
        if (offsetX < -1)
        {
          offsetX += 2;
          frameChange = -36;
        }
        topLeft = new TileCoordinate(origin.X + offsetX,
          origin.Y - originTile.FrameY / 18);
        width = height = 2;
        if (originTile.Type is 132 or 411)
        {
          if (originTile.Type == 411)
          {
            return WiringPropagationPlan.Rejected(
              "Switch 411 requires its timed reset owner.");
          }
          operation.ShiftFrames(topLeft, 2, 2, frameChange, 0,
            markSkipped: false, switchTilesOnly: true);
        }
        break;
      case 144:
        if (cooldowns is null || world.WorldEpoch != (ulong)cooldowns.WorldScope)
        {
          return WiringPropagationPlan.Rejected(
            "Timer 144 requires its authoritative timed activation owner.");
        }
        operation.Write(origin, new WorldTileDelta(
          FrameY: originTile.FrameY == 0 ? (short)18 : (short)0));
        if (originTile.FrameY == 0)
        {
          // HitSwitch ignores CheckMech's duplicate/capacity result after switching on.
          cooldowns.Schedule(new Terraria.WorldStorage.TileCoordinate(origin.X, origin.Y), 18000);
        }
        return CapturePlan(operation, origin);
      case 210:
      case 443:
        return WiringPropagationPlan.Rejected(
          $"Switch {originTile.Type} requires its timer or projectile owner.");
    }

    operation.Trip(topLeft, width, height, Directions);
    return CapturePlan(operation, origin);
  }

  internal WiringPropagationPlan PlanTimerTick(
    WorldSnapshot world,
    MechanismCooldownService.Preparation cooldowns)
  {
    if (world.WorldEpoch != (ulong)cooldowns.WorldScope)
    {
      return WiringPropagationPlan.Rejected("The timers belong to a different world.");
    }
    var operation = new WiringTileOperation(world, _facts);
    string? timerRejection = null;
    cooldowns.AdvanceOneTick(AdvanceTimer);
    return timerRejection is null
      ? CapturePlan(operation, default)
      : WiringPropagationPlan.Rejected(timerRejection);

    void AdvanceTimer(MechanismCooldownService.Snapshot timer)
    {
      if (timerRejection is not null || operation.Rejection is not null)
      {
        return;
      }
      var anchor = new TileCoordinate(timer.Anchor.X, timer.Anchor.Y);
      if (anchor.X < 1 || anchor.Y < 1 || anchor.X >= world.Header.Geometry.Width - 1 ||
          anchor.Y >= world.Header.Geometry.Height - 1)
      {
        timerRejection = "An out-of-world mechanism requires the source compaction policy.";
        return;
      }
      int remaining = timer.RemainingTicks;
      WorldTile tile = operation.Read(anchor);
      if (tile.Active && tile.Type == 144)
      {
        if (tile.FrameY == 0)
        {
          remaining = 0;
          cooldowns.TrySetRemainingTicks(timer.Anchor, remaining);
        }
        else
        {
          int period = (tile.FrameX / 18) switch
          {
            0 => 60,
            1 => 180,
            2 => 300,
            3 => 30,
            4 => 15,
            int other => other
          };
          if (Math.IEEERemainder(remaining, period) == 0)
          {
            remaining = 18000;
            cooldowns.TrySetRemainingTicks(timer.Anchor, remaining);
            operation.Trip(anchor, 1, 1, Directions);
          }
        }
      }
      if (remaining > 0)
      {
        return;
      }
      if (tile.Active && tile.Type == 144)
      {
        operation.Write(anchor, new WorldTileDelta(FrameY: 0));
        operation.CaptureTileSquare(anchor, 1, 1);
      }
      if (tile.Active && tile.Type == 411)
      {
        timerRejection = "Switch 411 requires its timed reset and framing owner.";
        return;
      }
      cooldowns.Remove(timer.Anchor);
    }
  }

  private static WiringPropagationPlan CapturePlan(
    WiringTileOperation operation,
    TileCoordinate origin)
  {
    if (operation.Rejection is string rejection)
    {
      return WiringPropagationPlan.Rejected(rejection);
    }
    return new WiringPropagationPlan(
      accepted: true,
      rejection: null,
      operation.Visited,
      operation.CaptureDeltas(),
      operation.TileSquareEffects,
      origin);
  }

  private static bool IsSwitch(WorldTile tile)
  {
    return tile.Type is 135 or 314 or 423 or 428 or 442 or 476 or 440 or 136 or
      210 or 443 or 144 or 441 or 468 or 132 or 411 ||
      tile.Type == 467 && tile.FrameX / 36 == 4;
  }
}

public sealed class WiringPropagationPlan
{
  internal WiringPropagationPlan(
    bool accepted,
    string? rejection,
    IEnumerable<TileCoordinate> visited,
    IReadOnlyDictionary<TileCoordinate, WorldTileDelta> deltas,
    IEnumerable<WiringTileSquareEffect> tileSquareEffects,
    TileCoordinate origin)
  {
    Accepted = accepted;
    Rejection = rejection;
    VisitedTiles = Array.AsReadOnly(visited
      .OrderBy(coordinate => coordinate.Y)
      .ThenBy(coordinate => coordinate.X)
      .ToArray());
    Deltas = new System.Collections.ObjectModel.ReadOnlyDictionary<TileCoordinate, WorldTileDelta>(
      deltas.ToDictionary(pair => pair.Key, pair => pair.Value));
    TileSquareEffects = Array.AsReadOnly(tileSquareEffects.ToArray());
    TileSquareUpdates = Array.AsReadOnly(TileSquareEffects
      .Select(effect => effect.Bounds).Distinct().ToArray());
    Origin = origin;
  }

  public bool Accepted { get; }

  public string? Rejection { get; }

  public TileCoordinate Origin { get; }

  public IReadOnlyList<TileCoordinate> VisitedTiles { get; }

  public IReadOnlyDictionary<TileCoordinate, WorldTileDelta> Deltas { get; }

  public IReadOnlyList<WorldSectionBounds> TileSquareUpdates { get; }

  public IReadOnlyList<WiringTileSquareEffect> TileSquareEffects { get; }

  public static WiringPropagationPlan Rejected(string reason)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reason);
    return new WiringPropagationPlan(
      accepted: false,
      rejection: reason,
      visited: Array.Empty<TileCoordinate>(),
      deltas: new Dictionary<TileCoordinate, WorldTileDelta>(),
      tileSquareEffects: Array.Empty<WiringTileSquareEffect>(),
      origin: default);
  }
}
