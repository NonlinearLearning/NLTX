namespace Terraria.NonAuthoritative.ContentDefinitions;

public sealed class SetArrayFactoryBoundary
{
  private readonly Queue<bool[]> _boolCache = new();
  private readonly Queue<int[]> _intCache = new();
  private readonly Queue<ushort[]> _ushortCache = new();
  private readonly Queue<float[]> _floatCache = new();
  private readonly object _cacheLock = new();

  public SetArrayFactoryBoundary(int size)
  {
    if (size <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(size));
    }

    Size = size;
  }

  public int Size { get; }

  public bool[] CreateBoolSet(bool defaultState = false, params int[] types)
  {
    ArgumentNullException.ThrowIfNull(types);
    bool[] values = Take(_boolCache, static size => new bool[size]);
    Array.Fill(values, defaultState);
    foreach (int type in types)
    {
      ValidateIndex(type);
      values[type] = !defaultState;
    }

    return values;
  }

  public int[] CreateIntSet(int defaultState = -1, params int[] inputs)
  {
    ArgumentNullException.ThrowIfNull(inputs);
    ValidatePairs(inputs.Length);
    int[] values = Take(_intCache, static size => new int[size]);
    Array.Fill(values, defaultState);
    for (int index = 0; index < inputs.Length; index += 2)
    {
      ValidateIndex(inputs[index]);
      values[inputs[index]] = inputs[index + 1];
    }

    return values;
  }

  public ushort[] CreateUshortSet(ushort defaultState = 0, params ushort[] inputs)
  {
    ArgumentNullException.ThrowIfNull(inputs);
    ValidatePairs(inputs.Length);
    ushort[] values = Take(_ushortCache, static size => new ushort[size]);
    Array.Fill(values, defaultState);
    for (int index = 0; index < inputs.Length; index += 2)
    {
      ValidateIndex(inputs[index]);
      values[inputs[index]] = inputs[index + 1];
    }

    return values;
  }

  public float[] CreateFloatSet(float defaultState = 0f, params float[] inputs)
  {
    ArgumentNullException.ThrowIfNull(inputs);
    ValidatePairs(inputs.Length);
    float[] values = Take(_floatCache, static size => new float[size]);
    Array.Fill(values, defaultState);
    for (int index = 0; index < inputs.Length; index += 2)
    {
      int valueIndex = Convert.ToInt32(inputs[index]);
      if (inputs[index] != valueIndex)
      {
        throw new ArgumentException("Float set indexes must be whole numbers.", nameof(inputs));
      }

      ValidateIndex(valueIndex);
      values[valueIndex] = inputs[index + 1];
    }

    return values;
  }

  public T[] CreateCustomSet<T>(T defaultState, params object[] inputs)
  {
    ArgumentNullException.ThrowIfNull(inputs);
    ValidatePairs(inputs.Length);
    T[] values = new T[Size];
    Array.Fill(values, defaultState);
    for (int index = 0; index < inputs.Length; index += 2)
    {
      int valueIndex = Convert.ToInt32(inputs[index]);
      ValidateIndex(valueIndex);
      values[valueIndex] = (T)Convert.ChangeType(inputs[index + 1], typeof(T));
    }

    return values;
  }

  public void Recycle(bool[] values)
  {
    Return(values, _boolCache);
  }

  public void Recycle(int[] values)
  {
    Return(values, _intCache);
  }

  public void Recycle(ushort[] values)
  {
    Return(values, _ushortCache);
  }

  public void Recycle(float[] values)
  {
    Return(values, _floatCache);
  }

  private T[] Take<T>(Queue<T[]> cache, Func<int, T[]> factory)
  {
    lock (_cacheLock)
    {
      return cache.Count == 0 ? factory(Size) : cache.Dequeue();
    }
  }

  private void Return<T>(T[] values, Queue<T[]> cache)
  {
    ArgumentNullException.ThrowIfNull(values);
    if (values.Length != Size)
    {
      throw new ArgumentException("Only arrays created by this factory may be recycled.", nameof(values));
    }

    lock (_cacheLock)
    {
      cache.Enqueue(values);
    }
  }

  private void ValidateIndex(int index)
  {
    if ((uint)index >= (uint)Size)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }
  }

  private static void ValidatePairs(int length)
  {
    if (length % 2 != 0)
    {
      throw new ArgumentException("Set inputs must contain index/value pairs.");
    }
  }
}

public static class SetArrayFactoryAdapter
{
  public static SetArrayFactoryBoundary Create(int contentCount)
  {
    return new SetArrayFactoryBoundary(contentCount);
  }
}
