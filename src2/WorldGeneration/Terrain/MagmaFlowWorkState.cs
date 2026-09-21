using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Terrain;

public sealed class MagmaFlowWorkState
{
  public const int MaxMagmaIterations = 300;

  private readonly ReadOnlyCollection<(double X, double Y)> _normalisedVectors;
  private MagmaCell[,] _sourceMagmaMap;
  private MagmaCell[,] _targetMagmaMap;

  public MagmaFlowWorkState(int mapWidth = 200, int mapHeight = 200)
  {
    if (mapWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(mapWidth));
    }

    if (mapHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(mapHeight));
    }

    _sourceMagmaMap = new MagmaCell[mapWidth, mapHeight];
    _targetMagmaMap = new MagmaCell[mapWidth, mapHeight];
    _normalisedVectors = new ReadOnlyCollection<(double X, double Y)>(CreateNormalisedVectors());
  }

  public int MapWidth => _sourceMagmaMap.GetLength(0);

  public int MapHeight => _sourceMagmaMap.GetLength(1);

  public int MaximumIterations => MaxMagmaIterations;

  public IReadOnlyList<(double X, double Y)> NormalisedVectors => _normalisedVectors;

  public (double Pressure, double Resistance, bool IsActive) GetSourceCell(int x, int y)
  {
    MagmaCell cell = _sourceMagmaMap[GetX(x), GetY(y)];
    return (cell.Pressure, cell.Resistance, cell.IsActive);
  }

  public (double Pressure, double Resistance, bool IsActive) GetTargetCell(int x, int y)
  {
    MagmaCell cell = _targetMagmaMap[GetX(x), GetY(y)];
    return (cell.Pressure, cell.Resistance, cell.IsActive);
  }

  public void SetSourceCell(int x, int y, double pressure, double resistance, bool isActive)
  {
    _sourceMagmaMap[GetX(x), GetY(y)] = CreateCell(pressure, resistance, isActive);
  }

  public void SetTargetCell(int x, int y, double pressure, double resistance, bool isActive)
  {
    _targetMagmaMap[GetX(x), GetY(y)] = CreateCell(pressure, resistance, isActive);
  }

  public void SwapBuffers()
  {
    (_sourceMagmaMap, _targetMagmaMap) = (_targetMagmaMap, _sourceMagmaMap);
  }

  public void Clear()
  {
    Array.Clear(_sourceMagmaMap);
    Array.Clear(_targetMagmaMap);
  }

  private int GetX(int x)
  {
    if (x < 0 || x >= MapWidth)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    return x;
  }

  private int GetY(int y)
  {
    if (y < 0 || y >= MapHeight)
    {
      throw new ArgumentOutOfRangeException(nameof(y));
    }

    return y;
  }

  private static MagmaCell CreateCell(double pressure, double resistance, bool isActive)
  {
    if (!double.IsFinite(pressure))
    {
      throw new ArgumentOutOfRangeException(nameof(pressure));
    }

    if (!double.IsFinite(resistance))
    {
      throw new ArgumentOutOfRangeException(nameof(resistance));
    }

    return new MagmaCell(pressure, resistance, isActive);
  }

  private static (double X, double Y)[] CreateNormalisedVectors()
  {
    (double X, double Y)[] points = new (double X, double Y)[9];
    int point = 0;
    for (int x = -1; x <= 1; x++)
    {
      for (int y = -1; y <= 1; y++)
      {
        double length = Math.Sqrt(x * x + y * y);
        points[point++] = length == 0d ? (0d, 0d) : (x / length, y / length);
      }
    }

    return points;
  }

  private readonly record struct MagmaCell(
    double Pressure,
    double Resistance,
    bool IsActive);
}
