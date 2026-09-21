namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class SegmentedCollectionState<T>
{
  private readonly int _segmentSize;
  private readonly List<T> _items = new();

  public SegmentedCollectionState(int segmentSize = 1024)
  {
    _segmentSize = Math.Max(16, segmentSize);
  }

  public int Count => _items.Count;

  public void PushBack(T value)
  {
    _items.Add(value);
  }

  public T PopFront()
  {
    if (_items.Count == 0)
    {
      throw new InvalidOperationException("The collection is empty.");
    }

    T value = _items[0];
    _items.RemoveAt(0);
    return value;
  }

  public void Clear()
  {
    _items.Clear();
  }
}
