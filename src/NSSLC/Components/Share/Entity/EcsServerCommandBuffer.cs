using Arch.Buffer;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;

namespace Terraria.Entity;

/// <summary>Owns structural Arch changes produced by one ECS server update.</summary>
/// <remarks>
/// Value writes may happen in a domain system when that system owns the component. Entity
/// creation, destruction and component add/remove operations are recorded here and played back
/// at the end of the host-controlled update boundary.
/// </remarks>
public sealed class EcsServerCommandBuffer : IDisposable
{
  private readonly CommandBuffer _buffer;
  private bool _playedBack;
  private bool _disposed;

  public EcsServerCommandBuffer(int initialCapacity = 128)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialCapacity);
    _buffer = new CommandBuffer(initialCapacity);
  }

  public int Size => _buffer.Size;

  public ArchEntity Create(params ComponentType[] types)
  {
    EnsureWritable();
    ArgumentNullException.ThrowIfNull(types);
    return _buffer.Create(types);
  }

  public void Destroy(in ArchEntity entity)
  {
    EnsureWritable();
    _buffer.Destroy(entity);
  }

  public void Set<T>(in ArchEntity entity, in T component)
  {
    EnsureWritable();
    _buffer.Set(entity, component);
  }

  public void Add<T>(in ArchEntity entity, in T component)
  {
    EnsureWritable();
    _buffer.Add(entity, component);
  }

  public void Remove<T>(in ArchEntity entity)
  {
    EnsureWritable();
    _buffer.Remove<T>(entity);
  }

  public void Playback(ArchWorld world)
  {
    EnsureWritable();
    ArgumentNullException.ThrowIfNull(world);
    _buffer.Playback(world, dispose: false);
    _playedBack = true;
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    _buffer.Dispose();
  }

  private void EnsureWritable()
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
    if (_playedBack)
    {
      throw new InvalidOperationException(
          "The ECS structural command buffer has already been played back.");
    }
  }
}
