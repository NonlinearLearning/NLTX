using System.Collections.Frozen;
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

  public IReadOnlyCollection<TileCoordinate> Frontier => _frontier.ToArray();

  public IReadOnlyCollection<byte> FrontierDirections => _frontierDirections.ToArray();

  public IReadOnlyDictionary<TileCoordinate, byte> TilesToProcess =>
      _tilesToProcess.ToFrozenDictionary();

  public IReadOnlyCollection<TileCoordinate> CurrentGates => _currentGates.ToArray();

  public IReadOnlyCollection<TileCoordinate> NextGates => _nextGates.ToArray();

  public IReadOnlyCollection<TileCoordinate> LampsToCheck => _lampsToCheck.ToArray();

  public IReadOnlySet<TileCoordinate> CompletedGates => _completedGates.ToFrozenSet();

  public IReadOnlyDictionary<TileCoordinate, byte> PixelBoxTriggers =>
      _pixelBoxTriggers.ToFrozenDictionary();

  public IReadOnlyList<TileCoordinate?> TeleportTargets =>
      Array.AsReadOnly(_teleportTargets);

  public byte? CurrentWireColor { get; internal set; }

  public bool IsRunning { get; internal set; }

  public bool BlockPlayerTeleportationForOneIteration { get; internal set; }
}
