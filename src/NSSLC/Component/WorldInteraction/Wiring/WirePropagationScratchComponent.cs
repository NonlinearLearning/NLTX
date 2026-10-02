using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public sealed class WirePropagationScratchComponent
{
  public const int TeleportTargetCount = 2;

  private readonly HashSet<TileCoordinate> _skippedTiles = new();
  private readonly Queue<TileCoordinate> _frontier = new();
  private readonly Queue<byte> _frontierDirections = new();
  private readonly Dictionary<TileCoordinate, byte> _tilesToProcess = new();
  private readonly Queue<TileCoordinate> _currentGates = new();
  private readonly Queue<TileCoordinate> _nextGates = new();
  private readonly Queue<TileCoordinate> _lampsToCheck = new();
  private readonly HashSet<TileCoordinate> _completedGates = new();
  private readonly Dictionary<TileCoordinate, byte> _pixelBoxTriggers = new();
  private readonly TileCoordinate?[] _teleportTargets =
      new TileCoordinate?[TeleportTargetCount];

  public IReadOnlySet<TileCoordinate> SkippedTiles => _skippedTiles.ToFrozenSet();

  public IReadOnlyCollection<TileCoordinate> Frontier =>
      Array.AsReadOnly(_frontier.ToArray());

  public IReadOnlyCollection<byte> FrontierDirections =>
      Array.AsReadOnly(_frontierDirections.ToArray());

  public IReadOnlyDictionary<TileCoordinate, byte> TilesToProcess =>
      _tilesToProcess.ToFrozenDictionary();

  public IReadOnlyCollection<TileCoordinate> CurrentGates =>
      Array.AsReadOnly(_currentGates.ToArray());

  public IReadOnlyCollection<TileCoordinate> NextGates =>
      Array.AsReadOnly(_nextGates.ToArray());

  public IReadOnlyCollection<TileCoordinate> LampsToCheck =>
      Array.AsReadOnly(_lampsToCheck.ToArray());

  public IReadOnlySet<TileCoordinate> CompletedGates => _completedGates.ToFrozenSet();

  public IReadOnlyDictionary<TileCoordinate, byte> PixelBoxTriggers =>
      _pixelBoxTriggers.ToFrozenDictionary();

  public IReadOnlyList<TileCoordinate?> TeleportTargets =>
      Array.AsReadOnly(_teleportTargets);

  public byte? CurrentWireColor { get; internal set; }

  public bool IsRunning { get; internal set; }

  public bool BlockPlayerTeleportationForOneIteration { get; internal set; }

  public byte CurrentUser { get; internal set; } = byte.MaxValue;

  internal void InitializePropagation()
  {
    ClearPropagationCollections();
    ResetTeleportState();
    CurrentWireColor = null;
    IsRunning = false;
    CurrentUser = byte.MaxValue;
  }

  internal bool BeginPropagation(byte currentUser)
  {
    if (IsRunning)
    {
      return false;
    }

    ClearPropagationCollections();
    ResetTeleportState();
    CurrentWireColor = null;
    IsRunning = true;
    CurrentUser = currentUser;
    return true;
  }

  internal bool SetWireColor(byte wireColor)
  {
    if (!IsRunning || wireColor is < 1 or > 4)
    {
      return false;
    }

    _frontier.Clear();
    _frontierDirections.Clear();
    _tilesToProcess.Clear();
    CurrentWireColor = wireColor;
    return true;
  }

  internal bool TrySkip(TileCoordinate coordinate)
  {
    return _skippedTiles.Add(coordinate);
  }

  internal bool TryQueueTile(TileCoordinate coordinate, byte direction)
  {
    if (!_tilesToProcess.TryAdd(coordinate, direction))
    {
      return false;
    }

    _frontier.Enqueue(coordinate);
    _frontierDirections.Enqueue(direction);
    return true;
  }

  internal bool TryQueueLamp(TileCoordinate coordinate)
  {
    if (_lampsToCheck.Contains(coordinate))
    {
      return false;
    }

    _lampsToCheck.Enqueue(coordinate);
    return true;
  }

  internal bool TryQueueCurrentGate(TileCoordinate coordinate)
  {
    if (_currentGates.Contains(coordinate))
    {
      return false;
    }

    _currentGates.Enqueue(coordinate);
    return true;
  }

  internal bool TryQueueNextGate(TileCoordinate coordinate)
  {
    if (_nextGates.Contains(coordinate))
    {
      return false;
    }

    _nextGates.Enqueue(coordinate);
    return true;
  }

  internal bool TryCompleteGate(TileCoordinate coordinate)
  {
    return _completedGates.Add(coordinate);
  }

  internal bool TryRecordPixelBoxTrigger(
    TileCoordinate coordinate,
    byte trigger)
  {
    if (_pixelBoxTriggers.ContainsKey(coordinate))
    {
      return false;
    }

    _pixelBoxTriggers.Add(coordinate, trigger);
    return true;
  }

  internal bool TryRecordTeleportTarget(TileCoordinate coordinate)
  {
    if (coordinate.X < 0 || coordinate.Y < 0)
    {
      return false;
    }

    if (_teleportTargets[0] is null)
    {
      _teleportTargets[0] = coordinate;
      return true;
    }

    if (_teleportTargets[0] == coordinate || _teleportTargets[1] == coordinate)
    {
      return false;
    }

    if (_teleportTargets[1] is not null)
    {
      return false;
    }

    _teleportTargets[1] = coordinate;
    return true;
  }

  internal bool ResetTeleportState()
  {
    bool changed = _teleportTargets[0] is not null ||
      _teleportTargets[1] is not null ||
      BlockPlayerTeleportationForOneIteration;
    _teleportTargets[0] = null;
    _teleportTargets[1] = null;
    BlockPlayerTeleportationForOneIteration = false;
    return changed;
  }

  internal bool EndPropagation()
  {
    if (!IsRunning)
    {
      return false;
    }

    IsRunning = false;
    CurrentWireColor = null;
    return true;
  }

  internal void ResetPropagation()
  {
    InitializePropagation();
  }

  internal WiringPropagationSnapshot CreateSnapshot()
  {
    return new WiringPropagationSnapshot(
      SkippedTiles,
      Frontier,
      FrontierDirections,
      TilesToProcess,
      CurrentGates,
      NextGates,
      LampsToCheck,
      CompletedGates,
      PixelBoxTriggers,
      TeleportTargets,
      CurrentWireColor,
      IsRunning,
      CurrentUser,
      BlockPlayerTeleportationForOneIteration);
  }

  private void ClearPropagationCollections()
  {
    _skippedTiles.Clear();
    _frontier.Clear();
    _frontierDirections.Clear();
    _tilesToProcess.Clear();
    _currentGates.Clear();
    _nextGates.Clear();
    _lampsToCheck.Clear();
    _completedGates.Clear();
    _pixelBoxTriggers.Clear();
  }
}
