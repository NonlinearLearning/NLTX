namespace Terraria.NpcTownBestiary;

internal sealed class ReadOnlySet<T> : IReadOnlySet<T>
{
  private readonly IReadOnlySet<T> _source;

  public ReadOnlySet(IReadOnlySet<T> source)
  {
    _source = source;
  }

  public int Count => _source.Count;

  public bool Contains(T item)
  {
    return _source.Contains(item);
  }

  public bool IsProperSubsetOf(IEnumerable<T> other)
  {
    return _source.IsProperSubsetOf(other);
  }

  public bool IsProperSupersetOf(IEnumerable<T> other)
  {
    return _source.IsProperSupersetOf(other);
  }

  public bool IsSubsetOf(IEnumerable<T> other)
  {
    return _source.IsSubsetOf(other);
  }

  public bool IsSupersetOf(IEnumerable<T> other)
  {
    return _source.IsSupersetOf(other);
  }

  public bool Overlaps(IEnumerable<T> other)
  {
    return _source.Overlaps(other);
  }

  public bool SetEquals(IEnumerable<T> other)
  {
    return _source.SetEquals(other);
  }

  public IEnumerator<T> GetEnumerator()
  {
    return _source.GetEnumerator();
  }

  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}
