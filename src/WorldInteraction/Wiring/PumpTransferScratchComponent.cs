using System.Collections.ObjectModel;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public sealed class PumpTransferScratchComponent
{
  public const int MaxPumpCount = 20;

  private readonly TileCoordinate[] _inputPumpPositions =
      new TileCoordinate[MaxPumpCount];
  private readonly TileCoordinate[] _outputPumpPositions =
      new TileCoordinate[MaxPumpCount];
  private readonly ReadOnlyCollection<TileCoordinate> _inputPumpPositionsView;
  private readonly ReadOnlyCollection<TileCoordinate> _outputPumpPositionsView;

  public PumpTransferScratchComponent()
  {
    _inputPumpPositionsView = Array.AsReadOnly(_inputPumpPositions);
    _outputPumpPositionsView = Array.AsReadOnly(_outputPumpPositions);
  }

  public IReadOnlyList<TileCoordinate> InputPumpPositions => _inputPumpPositionsView;

  public IReadOnlyList<TileCoordinate> OutputPumpPositions => _outputPumpPositionsView;

  public int InputPumpCount { get; internal set; }

  public int OutputPumpCount { get; internal set; }
}
