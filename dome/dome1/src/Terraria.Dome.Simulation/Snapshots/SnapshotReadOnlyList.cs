using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.Dome.Simulation;

internal sealed class SnapshotReadOnlyList<T> : IReadOnlyList<T>
{
  private readonly T[] _values;

  public SnapshotReadOnlyList(IEnumerable<T> values)
  {
    _values = values?.ToArray() ?? throw new ArgumentNullException(nameof(values));
  }

  public int Count => _values.Length;

  public T this[int index] => _values[index];

  public IEnumerator<T> GetEnumerator()
  {
    return ((IEnumerable<T>)_values).GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}
