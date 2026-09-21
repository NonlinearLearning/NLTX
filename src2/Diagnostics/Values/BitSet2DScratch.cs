namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class BitSet2DScratch
{
  private GridPoint _offset;
  private int _size;
  private bool[] _bits = Array.Empty<bool>();

  public void Reset(GridPoint center, int maxDistance)
  {
    if (maxDistance < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxDistance));
    }

    _size = checked(maxDistance * 2 + 1);
    _offset = new GridPoint(
      center.X - maxDistance,
      center.Y - maxDistance);
    _bits = new bool[checked(_size * _size)];
  }

  public bool Add(GridPoint point)
  {
    int index = ToIndex(point);
    if (_bits[index])
    {
      return false;
    }

    _bits[index] = true;
    return true;
  }

  public bool Contains(GridPoint point)
  {
    return _bits[ToIndex(point)];
  }

  public bool InBounds(GridPoint point)
  {
    int x = point.X - _offset.X;
    int y = point.Y - _offset.Y;
    return x >= 0 && x < _size && y >= 0 && y < _size;
  }

  private int ToIndex(GridPoint point)
  {
    if (!InBounds(point))
    {
      throw new ArgumentOutOfRangeException(nameof(point));
    }

    return (point.Y - _offset.Y) * _size + point.X - _offset.X;
  }
}
