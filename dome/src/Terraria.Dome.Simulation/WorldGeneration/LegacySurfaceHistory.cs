using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacySurfaceRetargetCommand(int X, double WorldSurface);

public sealed class LegacySurfaceHistory
{
  private readonly double[] _heights;
  private int _index;

  public LegacySurfaceHistory(int size)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
    _heights = new double[size];
  }

  public int Length => _heights.Length;

  public double Get(int index)
  {
    if (index < 0 || index >= _heights.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    return _heights[(index + _index) % _heights.Length];
  }

  public void Record(double height)
  {
    _heights[_index] = height;
    _index = (_index + 1) % _heights.Length;
  }

  public void Retarget(int targetX, double targetHeight, Action<int, double> retargetColumn)
  {
    ArgumentNullException.ThrowIfNull(retargetColumn);
    for (int index = 0; index < _heights.Length / 2; index++)
    {
      if (Get(_heights.Length - 1) <= targetHeight)
      {
        break;
      }

      for (int offset = 0; offset < _heights.Length - index * 2; offset++)
      {
        int historyIndex = _heights.Length - offset - 1;
        double height = Get(historyIndex) - 1.0;
        _heights[(historyIndex + _index) % _heights.Length] = height;
        if (height <= targetHeight)
        {
          break;
        }
      }
    }

    for (int offset = 0; offset < _heights.Length; offset++)
    {
      retargetColumn(targetX - offset, Get(_heights.Length - offset - 1));
    }
  }

  public IReadOnlyList<LegacySurfaceRetargetCommand> PrepareRetargetBatch(
    int targetX,
    double targetHeight)
  {
    List<LegacySurfaceRetargetCommand> commands = new(_heights.Length);
    Retarget(targetX, targetHeight, (x, worldSurface) =>
      commands.Add(new LegacySurfaceRetargetCommand(x, worldSurface)));
    return commands;
  }
}
