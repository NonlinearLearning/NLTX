using System;
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
  private int _inputPumpCount;
  private int _outputPumpCount;

  public PumpTransferScratchComponent()
  {
    _inputPumpPositionsView = Array.AsReadOnly(_inputPumpPositions);
    _outputPumpPositionsView = Array.AsReadOnly(_outputPumpPositions);
  }

  public IReadOnlyList<TileCoordinate> InputPumpPositions => _inputPumpPositionsView;

  public IReadOnlyList<TileCoordinate> OutputPumpPositions => _outputPumpPositionsView;

  public int InputPumpCount
  {
    get => _inputPumpCount;
    internal set => _inputPumpCount = ValidateCount(value);
  }

  public int OutputPumpCount
  {
    get => _outputPumpCount;
    internal set => _outputPumpCount = ValidateCount(value);
  }

  internal bool TryAddInputPump(TileCoordinate coordinate)
  {
    int count = InputPumpCount;
    bool added = TryAddPump(
      coordinate,
      _inputPumpPositions,
      ref count);
    InputPumpCount = count;
    return added;
  }

  internal bool TryAddOutputPump(TileCoordinate coordinate)
  {
    int count = OutputPumpCount;
    bool added = TryAddPump(
      coordinate,
      _outputPumpPositions,
      ref count);
    OutputPumpCount = count;
    return added;
  }

  internal bool Reset()
  {
    bool changed = InputPumpCount != 0 || OutputPumpCount != 0;
    Array.Clear(_inputPumpPositions);
    Array.Clear(_outputPumpPositions);
    InputPumpCount = 0;
    OutputPumpCount = 0;
    return changed;
  }

  private static bool TryAddPump(
    TileCoordinate coordinate,
    TileCoordinate[] positions,
    ref int count)
  {
    if (coordinate.X < 0 || coordinate.Y < 0 ||
        count < 0 || count >= MaxPumpCount)
    {
      return false;
    }

    for (int index = 0; index < count; index++)
    {
      if (positions[index] == coordinate)
      {
        return false;
      }
    }

    positions[count] = coordinate;
    count++;
    return true;
  }

  private static int ValidateCount(int count)
  {
    if (count < 0 || count > MaxPumpCount)
    {
      throw new ArgumentOutOfRangeException(nameof(count));
    }

    return count;
  }
}
