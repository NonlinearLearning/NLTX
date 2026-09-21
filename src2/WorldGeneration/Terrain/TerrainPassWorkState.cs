using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Terrain;

public sealed class TerrainPassWorkState
{
  private readonly double[] _surfaceHeights;
  private readonly ReadOnlyCollection<double> _surfaceHeightsView;
  private readonly ReadOnlyCollection<(double X, double Y)> _circleTestPoints;
  private int _surfaceHistoryIndex;

  public TerrainPassWorkState(int surfaceHistorySize = 12)
  {
    if (surfaceHistorySize <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(surfaceHistorySize));
    }

    _surfaceHeights = new double[surfaceHistorySize];
    _surfaceHeightsView = Array.AsReadOnly(_surfaceHeights);
    _circleTestPoints = new ReadOnlyCollection<(double X, double Y)>(CreateCircleTestPoints());
  }

  public double ChanceOfEntrance { get; private set; } = 0.3333d;

  public double ExtraBuffer { get; } = 1d / Math.Cos(Math.PI / 6d);

  public IReadOnlyList<(double X, double Y)> CircleTestPoints => _circleTestPoints;

  public IReadOnlyList<double> SurfaceHeights => _surfaceHeightsView;

  public int SurfaceHistoryLength => _surfaceHeights.Length;

  public int SurfaceHistoryIndex => _surfaceHistoryIndex;

  public double this[int index]
  {
    get => _surfaceHeights[GetBufferIndex(index)];
    set
    {
      if (!double.IsFinite(value))
      {
        throw new ArgumentOutOfRangeException(nameof(value));
      }

      _surfaceHeights[GetBufferIndex(index)] = value;
    }
  }

  public void SetChanceOfEntrance(double chance)
  {
    if (!double.IsFinite(chance) || chance < 0d || chance > 1d)
    {
      throw new ArgumentOutOfRangeException(nameof(chance));
    }

    ChanceOfEntrance = chance;
  }

  public void SetSurfaceHistoryIndex(int index)
  {
    _surfaceHistoryIndex = NormalizeIndex(index);
  }

  public void AdvanceSurfaceHistory(int offset = 1)
  {
    _surfaceHistoryIndex = NormalizeIndex(_surfaceHistoryIndex + offset);
  }

  public void Clear()
  {
    Array.Clear(_surfaceHeights);
    _surfaceHistoryIndex = 0;
  }

  private int GetBufferIndex(int index)
  {
    if (index < 0 || index >= _surfaceHeights.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    return NormalizeIndex(index + _surfaceHistoryIndex);
  }

  private int NormalizeIndex(int index)
  {
    int normalized = index % _surfaceHeights.Length;
    return normalized < 0 ? normalized + _surfaceHeights.Length : normalized;
  }

  private static (double X, double Y)[] CreateCircleTestPoints()
  {
    const int pointCount = 12;
    (double X, double Y)[] points = new (double X, double Y)[pointCount];
    for (int index = 0; index < pointCount; index++)
    {
      double angle = Math.PI * 2d * index / pointCount;
      points[index] = (Math.Cos(angle), Math.Sin(angle));
    }

    return points;
  }
}
