using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.WorldCompatibility.Model;

internal sealed class CompatibilityReadOnlyList<T> : IReadOnlyList<T>
{
  private readonly T[] _values;

  public CompatibilityReadOnlyList(IEnumerable<T> values)
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
