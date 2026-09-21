namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class TileAnimationRegistryComponent
{
  private readonly int _capacity;
  private readonly Dictionary<TileAnimationKey, TileAnimationDefinition> _definitions = new();
  private readonly List<TileAnimationDefinition> _pendingAdditions = new();
  private readonly List<TileAnimationKey> _pendingRemovals = new();

  public TileAnimationRegistryComponent(int capacity)
  {
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    _capacity = capacity;
  }

  public int Count => _definitions.Count;

  public uint Revision { get; private set; }

  internal void QueueAdd(TileAnimationDefinition definition)
  {
    ValidateDefinition(definition);
    EnsurePendingCapacity();
    _pendingAdditions.Add(definition);
  }

  internal void QueueRemove(TileAnimationKey key)
  {
    EnsurePendingCapacity();
    _pendingRemovals.Add(key);
  }

  internal void Commit()
  {
    foreach (TileAnimationKey key in _pendingRemovals)
    {
      _definitions.Remove(key);
    }

    foreach (TileAnimationDefinition definition in _pendingAdditions)
    {
      if (_definitions.ContainsKey(definition.Key) || _definitions.Count >= _capacity)
      {
        throw new InvalidOperationException("The tile animation registry capacity or key is exhausted.");
      }

      _definitions.Add(definition.Key, definition);
    }

    _pendingRemovals.Clear();
    _pendingAdditions.Clear();
    Revision++;
  }

  public bool TryGet(TileAnimationKey key, out TileAnimationDefinition definition)
  {
    return _definitions.TryGetValue(key, out definition);
  }

  private void EnsurePendingCapacity()
  {
    if (_pendingAdditions.Count + _pendingRemovals.Count >= _capacity)
    {
      throw new InvalidOperationException("The pending tile animation registry capacity is exhausted.");
    }
  }

  private static void ValidateDefinition(TileAnimationDefinition definition)
  {
    if (definition.FrameCount <= 0 || definition.TicksPerFrame <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(definition));
    }
  }
}
