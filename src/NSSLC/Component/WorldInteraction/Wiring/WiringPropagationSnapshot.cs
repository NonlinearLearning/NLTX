using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public sealed class WiringPropagationSnapshot
{
  public WiringPropagationSnapshot(
    IReadOnlySet<TileCoordinate> skippedTiles,
    IReadOnlyCollection<TileCoordinate> frontier,
    IReadOnlyCollection<byte> frontierDirections,
    IReadOnlyDictionary<TileCoordinate, byte> tilesToProcess,
    IReadOnlyCollection<TileCoordinate> currentGates,
    IReadOnlyCollection<TileCoordinate> nextGates,
    IReadOnlyCollection<TileCoordinate> lampsToCheck,
    IReadOnlySet<TileCoordinate> completedGates,
    IReadOnlyDictionary<TileCoordinate, byte> pixelBoxTriggers,
    byte? currentWireColor,
    bool isRunning,
    byte currentUser)
    : this(
      skippedTiles,
      frontier,
      frontierDirections,
      tilesToProcess,
      currentGates,
      nextGates,
      lampsToCheck,
      completedGates,
      pixelBoxTriggers,
      Array.Empty<TileCoordinate?>(),
      currentWireColor,
      isRunning,
      currentUser,
      blockPlayerTeleportationForOneIteration: false)
  {
  }

  public WiringPropagationSnapshot(
    IReadOnlySet<TileCoordinate> skippedTiles,
    IReadOnlyCollection<TileCoordinate> frontier,
    IReadOnlyCollection<byte> frontierDirections,
    IReadOnlyDictionary<TileCoordinate, byte> tilesToProcess,
    IReadOnlyCollection<TileCoordinate> currentGates,
    IReadOnlyCollection<TileCoordinate> nextGates,
    IReadOnlyCollection<TileCoordinate> lampsToCheck,
    IReadOnlySet<TileCoordinate> completedGates,
    IReadOnlyDictionary<TileCoordinate, byte> pixelBoxTriggers,
    IReadOnlyList<TileCoordinate?> teleportTargets,
    byte? currentWireColor,
    bool isRunning,
    byte currentUser,
    bool blockPlayerTeleportationForOneIteration)
  {
    ArgumentNullException.ThrowIfNull(teleportTargets);
    SkippedTiles = skippedTiles.ToFrozenSet();
    Frontier = System.Array.AsReadOnly(frontier.ToArray());
    FrontierDirections = System.Array.AsReadOnly(frontierDirections.ToArray());
    TilesToProcess = tilesToProcess.ToFrozenDictionary();
    CurrentGates = System.Array.AsReadOnly(currentGates.ToArray());
    NextGates = System.Array.AsReadOnly(nextGates.ToArray());
    LampsToCheck = System.Array.AsReadOnly(lampsToCheck.ToArray());
    CompletedGates = completedGates.ToFrozenSet();
    PixelBoxTriggers = pixelBoxTriggers.ToFrozenDictionary();
    TeleportTargets = Array.AsReadOnly(teleportTargets.ToArray());
    CurrentWireColor = currentWireColor;
    IsRunning = isRunning;
    CurrentUser = currentUser;
    BlockPlayerTeleportationForOneIteration =
      blockPlayerTeleportationForOneIteration;
  }

  public IReadOnlySet<TileCoordinate> SkippedTiles { get; }

  public IReadOnlyCollection<TileCoordinate> Frontier { get; }

  public IReadOnlyCollection<byte> FrontierDirections { get; }

  public IReadOnlyDictionary<TileCoordinate, byte> TilesToProcess { get; }

  public IReadOnlyCollection<TileCoordinate> CurrentGates { get; }

  public IReadOnlyCollection<TileCoordinate> NextGates { get; }

  public IReadOnlyCollection<TileCoordinate> LampsToCheck { get; }

  public IReadOnlySet<TileCoordinate> CompletedGates { get; }

  public IReadOnlyDictionary<TileCoordinate, byte> PixelBoxTriggers { get; }

  public IReadOnlyList<TileCoordinate?> TeleportTargets { get; }

  public byte? CurrentWireColor { get; }

  public bool IsRunning { get; }

  public byte CurrentUser { get; }

  public bool BlockPlayerTeleportationForOneIteration { get; }
}
